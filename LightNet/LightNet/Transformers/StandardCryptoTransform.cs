/*
   Copyright 2015 Tyler Crandall

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle;
using Org.BouncyCastle.Crypto;
using System.IO;

namespace LightNet
{
	public enum StandardCryptoPacketID : byte
	{
		Normal,
		AgreementRequest,
		AgreementResponse,
		PreparedEnc,
		NotifyEnc,
		AgreementError
	}

	public class StandardCryptoTransform : ITransform, IDisposable
	{
		int Disposed;
		CancellationTokenSource CancelSource = new CancellationTokenSource();
		DiffieHellman CommonDH = new DiffieHellman();
		byte Offset = 0;
		Rijndael Rij;
		byte[] NewKey = new byte[32];
		byte[] CurrentKey = new byte[32];
		DateTime CheckForKeyChangeTime = DateTime.UtcNow;
		bool IsTheConnectionInitator = false;
		volatile bool FirstExchange = false;
		volatile bool ActiveExchange = false;

		/// <summary>
		/// It a cycle which is used to indicate how many times key have been changed.
		/// </summary>
		long CryptoCycle = 0;

		public StandardCryptoTransform ()
		{
			Rij.Padding = PaddingMode.PKCS7;
			Rij.BlockSize = 256;
			Rij.KeySize = 256;
			Rij.Mode = CipherMode.CBC;
		}

		public event EventHandler ConnectionFailureEvent;

		public int OffsetUsageCount {
			get {
				return 6;
			}
		}

		#region Outgoing Pair
		ConcurrentQueue<Packet> UntransformedOutgoingPacketQueue = new ConcurrentQueue<Packet>();
		ConcurrentQueue<Packet> TransformedOutgoingPacketQueue = new ConcurrentQueue<Packet>();

		public void EnqueueUntransformedPacket (Packet packet)
		{
			UntransformedOutgoingPacketQueue.Enqueue (packet);
		}

		public Packet DequeueTransformedPacket ()
		{
			var output = new Packet ();
			if (TransformedOutgoingPacketQueue.TryDequeue (out output))
				return output;
			return null;
		}
		#endregion
		#region Incoming Pair
		ConcurrentQueue<Packet> UntransformedIncomingPacketQueue = new ConcurrentQueue<Packet>();
		ConcurrentQueue<Packet> TransformedIncomingPacketQueue = new ConcurrentQueue<Packet>();

		public void EnqueueTransformedPacket (Packet packet)
		{
			TransformedIncomingPacketQueue.Enqueue (packet);
		}

		public Packet DequeueUntransformedPacket ()
		{
			var output = new Packet ();
			if (UntransformedIncomingPacketQueue.TryDequeue (out output))
				return output;
			return null;
		}
		#endregion
		#region Internal Transformer Process
		async Task TransformProcess()
		{
			await Task.Factory.StartNew (delegate {
				while (!CancelSource.IsCancellationRequested)
				{
					Task.WaitAll(new [] {
						EncryptorProcess(),
						DecryptorProcess()
					}, CancelSource.Token);
				}
			});
		}

		async Task EncryptorProcess()
		{
			
		}

		async Task DecryptorProcess()
		{

		}

		void EncryptWithNewKey(ref Packet input)
		{
			using (var output = new MemoryStream ()) {
				Rij.GenerateIV ();
				Rij.Key = NewKey;
				DataUtility.WriteOnlyBytesToStream (Rij.IV, output);
				DataUtility.WriteOnlyBytesToStream (Rij.CreateEncryptor ().TransformFinalBlock
					(input.RawContent, 0, input.RawContent.Length), output);
				Rij.Key = CurrentKey;
				input.RawContent = output.ToArray ();
			}
		}

		/// <summary>
		/// Check the Time and Crypto Cycle as well as Initator Boolean to vertify if it is needed to change the
		/// cryptographic keys.
		/// </summary>
		void CheckIfNeedToChangeKey()
		{
			if (IsTheConnectionInitator || ActiveExchange)
				return;
			
			if (CheckForKeyChangeTime < DateTime.UtcNow && Interlocked.Read (ref CryptoCycle) >= 2 || CryptoCycle < 2) {
				ActiveExchange = true;
				CheckForKeyChangeTime = DateTime.UtcNow.AddSeconds (30);
				SendDHRequest ();
			}
		}

		void SetToNewCryptoKey ()
		{
			lock (CommonDH)
				Array.Copy (CommonDH.Key, Rij.Key, Rij.Key.Length);
		}

		async Task SendDHRequest()
		{
			TransformedOutgoingPacketQueue.Enqueue (new Packet ((byte)StandardCryptoPacketID.AgreementRequest,
				await GenerateRequestDH ()));
		}

		async Task SendDHResponse(byte[] response)
		{
			TransformedOutgoingPacketQueue.Enqueue (new Packet ((byte)StandardCryptoPacketID.AgreementResponse,
				await GenerateResponseDH (response)));
		}

		async Task<byte[]> GenerateRequestDH()
		{
			return await Task.Factory.StartNew<byte[]> (delegate {
				lock (CommonDH)
					return CommonDH.GenerateRequest ();
			});
		}

		async Task<byte[]> GenerateResponseDH(byte[] requestData)
		{
			return await Task.Factory.StartNew<byte[]> (delegate {
				lock (CommonDH)
					return CommonDH.GenerateResponse (requestData);
			});
		}

		async Task GenerateRequestDH(byte[] responseData)
		{
			await Task.Factory.StartNew(delegate {
				lock (CommonDH)
					CommonDH.HandleResponse (responseData);
			});
		}

		void InvokeFailureEvent()
		{
			ConnectionFailureEvent.Invoke (null, new EventArgs ());
		}

		#endregion
		public void Initialize (bool initate, byte offset)
		{
			if (OffsetUsageCount + offset > byte.MaxValue)
				throw new ArgumentOutOfRangeException ("You've used up the available representable identifiers for Transformer Layer.");
			Offset = offset;
			IsTheConnectionInitator = initate;
		}

		public void Dispose()
		{
			if (Interlocked.Exchange (ref Disposed, 1) != 0)
				return;

			CancelSource.Cancel ();

			if (!Object.ReferenceEquals (TransformedIncomingPacketQueue, null))
				TransformedIncomingPacketQueue = null;

			if (!Object.ReferenceEquals (TransformedOutgoingPacketQueue, null))
				TransformedOutgoingPacketQueue = null;
			
			if (!Object.ReferenceEquals (UntransformedIncomingPacketQueue, null))
				UntransformedIncomingPacketQueue = null;
			
			if (!Object.ReferenceEquals (UntransformedOutgoingPacketQueue, null))
				UntransformedOutgoingPacketQueue = null;
		}
	}
}


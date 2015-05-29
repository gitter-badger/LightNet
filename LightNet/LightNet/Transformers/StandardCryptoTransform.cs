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
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle;
using Org.BouncyCastle.Crypto;

namespace LightNet
{
	public enum StandardCryptoPacketID : byte
	{
		AgreementRequest,
		AgreementResponse,
		PreparedEnc,
		NotifyEnc,
		Normal
	}

	public class StandardCryptoTransform : ITransform, IDisposable
	{
		int Disposed;
		CancellationTokenSource CancelSource = new CancellationTokenSource();
		DiffieHellman CommonDH = new DiffieHellman();

		public StandardCryptoTransform ()
		{
		}

		public event EventHandler ConnectionFailureEvent;

		public int OffsetUsageCount {
			get {
				throw new NotImplementedException ();
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

		async Task<byte[]> GenerateRequestDH()
		{
			return await Task.Factory.StartNew<byte[]> (delegate {
				lock (CommonDH)
					return CommonDH.GenerateRequest ();
			});
		}

		void InvokeFailureEvent()
		{
			ConnectionFailureEvent.Invoke (null, new EventArgs ());
		}

		#endregion
		public void OffsetForPacketID (int offset)
		{
			throw new NotImplementedException ();
		}

		public void Dispose()
		{
			if (Interlocked.Exchange (ref Disposed, 1) != 0)
				return;


		}
	}
}


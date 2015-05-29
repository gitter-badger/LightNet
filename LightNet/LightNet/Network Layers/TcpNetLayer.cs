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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LightNet
{
	public class TcpNetLayer : INetLayer, IDisposable
	{
		TcpClient connector = new TcpClient();
		Task InternalProcessing;
		int InternalProcessState;
		CancellationTokenSource CancelSource = new CancellationTokenSource();
		ConcurrentQueue<Packet> OutgoingQueue = new ConcurrentQueue<Packet>();
		ConcurrentQueue<Packet> IncomingQueue = new ConcurrentQueue<Packet>();
		MemoryStream RawIncomingStream = new MemoryStream ();
		long Disposed;

		#region Constants
		const int LengthVariableSize = 2;
		#endregion

		public TcpNetLayer()
		{
			connector = new TcpClient ();
		}

		public TcpNetLayer (TcpClient connection)
		{
			connector = connection;
			Start ();
		}

		public async Task Connect(IPEndPoint ipAddr)
		{
			CheckIfDisposed ();
			await connector.ConnectAsync (ipAddr.Address, ipAddr.Port);
			Start ();
		}

		public void Start()
		{
			CheckIfDisposed ();
			if (Interlocked.Exchange (ref InternalProcessState, 1) != 0)
				return;
			InternalProcessing = InternalProcess ();
		}

		public void Stop()
		{
			CheckIfDisposed ();
			CancelSource.Cancel ();
		}

		public Packet DequeuePacket()
		{
			CheckIfDisposed ();
			var result = new Packet();
			if (IncomingQueue.TryDequeue (out result))
				return result;
			return null;
		}

		public void EnqueuePacket(Packet packet)
		{
			CheckIfDisposed ();
			if (packet.Length > ushort.MaxValue)
				throw new PacketTooBigException (string.Format ("Packet must not exceed {0} bytes long.", ushort.MaxValue));
			OutgoingQueue.Enqueue (packet);
		}

		private void CheckIfDisposed()
		{
			if (Interlocked.Read(ref Disposed) == 1)
				throw new ObjectDisposedException("TcpNetLayer", "TcpNetLayer had been disposed.");
		}

		public void Dispose ()
		{
			if (Interlocked.Exchange (ref Disposed, 1) == 1)
				return;
			CancelSource.Cancel ();
			Task.WaitAll (InternalProcessing);
			InternalProcessing.Dispose ();
			CancelSource.Dispose ();
			RawIncomingStream.Dispose ();
			connector.Close ();
		}

		private Task InternalProcess()
		{
			return Task.Factory.StartNew (delegate {
				while (!CancelSource.IsCancellationRequested) {
					Task.WaitAll ( new [] {
						ProcessIncomingPackets(),
						ProcessOutgoingPackets()
					}, CancelSource.Token);
				}

				Interlocked.Exchange (ref InternalProcessState, 0);
			});
		}

		private async Task ProcessIncomingPackets()
		{
			var netStream = connector.GetStream ();
			var length = 0;
			var buffer = new byte[ushort.MaxValue];
			while (netStream.DataAvailable) {
				length = await netStream.ReadAsync (buffer, 0, buffer.Length);
				if (length <= 0)
					break;
				RawIncomingStream.Write (buffer, 0, length);
			}

			while (RawIncomingStream.Length >= LengthVariableSize) {

				RawIncomingStream.Seek (0, SeekOrigin.Begin);
				var packetLength = DataUtility.ReadUInt16FromStream (RawIncomingStream);

				if (packetLength == 0 || RawIncomingStream.Length - LengthVariableSize < packetLength)
					continue;
				// If Packet Size exceed 65535 bytes, ignore it.
				if (packetLength <= ushort.MaxValue) {
					var newPacketBuffer = new byte[packetLength];
					RawIncomingStream.Read (newPacketBuffer, 0, newPacketBuffer.Length);
					var packet = new Packet (newPacketBuffer);
					IncomingQueue.Enqueue (packet);
				}
				DataUtility.ClearAndCopyMemoryStream (ref RawIncomingStream, (int)packetLength + LengthVariableSize);
			}
		}

		private async Task ProcessOutgoingPackets()
		{
			var outgoingPacket = new Packet ();
			var netStream = connector.GetStream ();
			while (OutgoingQueue.TryDequeue (out outgoingPacket)) {
				var lengthBuffer = DataUtility.GetBytes ((ushort)outgoingPacket.RawContent.Length);
				await netStream.WriteAsync (lengthBuffer, 0, lengthBuffer.Length);
				await netStream.WriteAsync (outgoingPacket.RawContent, 0, outgoingPacket.RawContent.Length);
			}
		}
	}
}


using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace LightNet
{
	internal class DHToAES256Manager : ITransform, IDisposable
	{
		private Rijndael Rij = Rijndael.Create ();

		private ConcurrentQueue<byte[]> DecryptedNormalPackets = new ConcurrentQueue<byte[]> ();
		private volatile bool _IsRunning = false;

		private Thread cryptoThread { get; set; }

		private ConcurrentQueue<byte[]> SendingPackets = new ConcurrentQueue<byte[]> ();
		private DateTime CheckForKeyDate = DateTime.UtcNow;
		private volatile bool FirstExchange = false;
		/// <summary>
		/// The crypto cycle.
		/// 0 = Key is not exchanged...
		/// 1 = DH is exchanged only once. ALL OTHER PACKETS ARE FORBIDDEN FROM EXCHANGING.
		/// 2 = Second DH exchange is encrypted and allowing minimum security. All packets are permitted for exchanges.
		/// </summary>
		private int CryptoCycle = 0;
		/// <summary>
		/// The Diffie-Hellman Module.
		/// This is the primary means of exchanging new encryption keys across the network.
		/// </summary>
		private DiffieHellman CoreDH;
		/// <summary>
		/// First Send Boolean
		/// </summary>
		private volatile bool SendTheNewKeyNow = false;

		public bool IsRunning {
			get {
				return _IsRunning;
			}
		}

		public DHToAES256Manager (IPEndPoint ipendpoint)
		{
			client = new ViewTcpClient (ipendpoint.Address, (ushort)ipendpoint.Port);
			FirstExchange = true;
		}

		public DHToAES256Manager (TcpClient tcpClient)
		{
			client = new ViewTcpClient (tcpClient);
		}

		#region Public Methods

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start ()
		{
			if (_IsRunning)
				return;

			if (cryptoThread != null) {
				if (cryptoThread.ThreadState == ThreadState.Running)
					return;
			}
			_IsRunning = true;
			cryptoThread = new Thread (ThreadProcess);
			cryptoThread.Start ();
		}

		public void NewPublicKey ()
		{
			SendTheNewKeyNow = true;
		}

		/// <summary>
		/// Sends the message.
		/// </summary>
		/// <param name="content">Content.</param>
		public void SendMessage (byte[] content)
		{
			SendingPackets.Enqueue (content);
		}

		/// <summary>
		/// Get Availables packets.
		/// </summary>
		/// <returns>The packets.</returns>
		public int AvailablePackets ()
		{
			int countOfPackets = -1;
			countOfPackets = DecryptedNormalPackets.Count;
			return countOfPackets;
		}

		/// <summary>
		/// Retrieves the packet.
		/// </summary>
		/// <returns>The packet.</returns>
		public byte[] RetrievePacket ()
		{
			byte[] info = null;
			DecryptedNormalPackets.TryDequeue (out info);
			return info;
		}

		#endregion

		#region Private Methods

		private void ThreadProcess ()
		{
			while (_IsRunning &&
				client.IsConnected) {
				Process ();
				Thread.Sleep (1);
			}
		}

		private void SetToNewCryptoKey ()
		{
			Rij.Padding = PaddingMode.PKCS7;
			Rij.BlockSize = 256;
			Rij.KeySize = 256;
			Rij.Mode = CipherMode.CBC;
			Array.Copy (CoreDH.Key, Rij.Key, Rij.Key.Length);
		}

		private void Process ()
		{
			Packet newPacket;
			lock (client) {
				// Send a new request for new key
				if (CheckForKeyDate < DateTime.UtcNow && CryptoCycle >= 2 ||
					SendTheNewKeyNow) {
					SendNewKeyRequest ();
					CheckForKeyDate = DateTime.UtcNow.AddSeconds (30);
				}
				// Recieve Process
				if (client.CountRecievedPacket () > 0) {

					// Pop the Packet, prepare the byte array... then check against the identifier of that packet...
					newPacket = client.DequeueRetrievedPacket ();
					switch (newPacket.TypeOfPacket) {
					case PacketType.Normal:
						{
							if (CryptoCycle < 2) {
								// Software Policy Violation. Disconnect.
								Stop ();
								return;
							} else {
								DecryptedNormalPackets.Enqueue (DecryptBytes (newPacket.Content));
							}
							break;
						}
					case PacketType.NewKey:
						{
							CoreDH = new DiffieHellman ();
							if (CryptoCycle == 0) {
								var sendTo = CoreDH.GenerateResponse (newPacket.Content);
								client.EnqueueSendingPacket (new Packet (PacketType.ReplyExchange, sendTo));
							} else {
								var sendTo = CoreDH.GenerateResponse (DecryptBytes (newPacket.Content));
								client.EnqueueSendingPacket (new Packet (PacketType.ReplyExchange, EncryptBytes (sendTo)));
							}

							SetToNewCryptoKey ();
							CryptoCycle++;
							break;
						}

					case PacketType.ReplyExchange:
						{
							if (CryptoCycle == 0) {
								CoreDH.HandleResponse (newPacket.Content);
							} else {
								CoreDH.HandleResponse (DecryptBytes (newPacket.Content));
							}
							SetToNewCryptoKey ();
							CryptoCycle++;

							if (CryptoCycle < 2) {
								SendNewKeyRequest ();
							}
							break;
						}
					}
				}

				if (FirstExchange) {
					SendFirstKeyRequest ();
					FirstExchange = false;
				}

				if (CryptoCycle >= 2 && SendingPackets.Count > 0) {
					byte[] content = null;
					bool successCheck = SendingPackets.TryDequeue (out content);

					if (!successCheck)
						return;

					content = EncryptBytes (content);
					client.EnqueueSendingPacket (new Packet (PacketType.Normal, content));
				}
			}
		}

		#endregion

		private void SendFirstKeyRequest ()
		{
			CoreDH = new DiffieHellman ();
			var firstPub = CoreDH.GenerateRequest ();
			client.EnqueueSendingPacket (new Packet (PacketType.NewKey, firstPub));
		}

		private void SendNewKeyRequest ()
		{
			CoreDH = new DiffieHellman ();
			var newReq = CoreDH.GenerateRequest ();
			client.EnqueueSendingPacket (new Packet (PacketType.NewKey, EncryptBytes (newReq)));
		}

		private byte[] EncryptBytes (byte[] message)
		{
			if ((message == null) || (message.Length == 0)) {
				return message;
			}
			Rij.GenerateIV ();

			using (MemoryStream outStream = new MemoryStream ()) {
				DataUtility.WriteBytesToStream (Rij.IV, outStream);
				DataUtility.WriteBytesToStream (Rij.CreateEncryptor ().TransformFinalBlock (message, 0, message.Length), outStream);
				return outStream.ToArray ();
			}
		}

		private byte[] DecryptBytes (byte[] message)
		{
			if ((message == null) || (message.Length == 0)) {
				return message;
			}
			byte[] restOfData = null;
			using (MemoryStream inStream = new MemoryStream (message)) {
				Rij.IV = DataUtility.ReadBytesFromStream (inStream);
				restOfData = DataUtility.ReadBytesFromStream (inStream);
			}
			var result = Rij.CreateDecryptor ().TransformFinalBlock (restOfData, 0, restOfData.Length);
			return result;
		}
	}
}


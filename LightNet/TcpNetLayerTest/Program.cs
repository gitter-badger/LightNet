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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LightNet;

namespace TcpNetLayerTest
{
	class MainClass
	{
		/// <summary>
		/// This aspect of the program takes too long for Unit Testing, so it's best to be made seperately for testing.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main (string[] args)
		{
			var tcpNetLayerClient = new TcpNetLayer ();
			var listener = new TcpListener (IPAddress.Loopback, 8833);
			listener.Start ();
			tcpNetLayerClient.Connect (new IPEndPoint (IPAddress.Loopback, 8833));
			var waitProcess = listener.AcceptTcpClientAsync ();
			Task.WaitAll (waitProcess);
			var connectedClientLayer = new TcpNetLayer (waitProcess.Result);
			#region Round 1
			Console.Write("Round 1... ");
			tcpNetLayerClient.EnqueuePacket (new Packet (1, new byte[] { 55 }));
			Packet packet;
			while (true) {
				packet = connectedClientLayer.DequeuePacket ();
				if (packet != null)
					break;
			}

			if (packet.ID == 1 && packet.RawContent.SequenceEqual (new byte[] { 55 }))
				Console.WriteLine ("Success");
			else
				Console.WriteLine ("Failed");
			#endregion
			#region Round 2
			Console.Write("Round 2... ");
			var shortBuffer = new byte[] { 255, 255, 1, 253, 189};
			tcpNetLayerClient.EnqueuePacket (new Packet (255, shortBuffer));
			while (true) {
				packet = connectedClientLayer.DequeuePacket ();
				if (packet != null)
					break;
			}

			if (packet.ID == 255 && packet.RawContent.SequenceEqual (shortBuffer))
				Console.WriteLine ("Success");
			else
				Console.WriteLine ("Failed");
			#endregion
			Console.ReadLine ();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LightNet
{

    /// <summary>
    /// Base Network Layer for Individual Client Connection (No Encryption)
    /// </summary>
    public class NetworkLayer : IDisposable
    {
        int _AttemptPacketID = -1;
        int _AttemptLength = -1;
        long AvailablePackets = 0;

        ConcurrentQueue<Packet> RecievedPackets = new ConcurrentQueue<Packet>();

        public void ProcessIncomingStream(NetworkStream stream)
        {
            if (!stream.DataAvailable)
            {
                return;
            }

            if (_AttemptPacketID == -1)
            {
                _AttemptPacketID = stream.ReadByte();
                if (_AttemptPacketID == -1)
                    return;
            }


        }

        void ProcessPacketFromID(NetworkStream stream)
        {
            if (_AttemptPacketID < 0)
            {
                // Break Connection
            }
            switch ((PacketID)_AttemptPacketID)
            {
                case PacketID.ServiceAdd:
                    {

                        break;
                    }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

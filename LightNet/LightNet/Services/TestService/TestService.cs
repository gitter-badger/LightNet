using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightNet.Services.TestService
{
    public class TestService : Service
    {
        ByteServiceStream EasyStream;
        int IdIndex = int.MinValue;

        public TestService()
        {
            EasyStream = new ByteServiceStream(
                typeof(TestMessage)
            );
        }

        public void SendNewMessage(string message)
        {
            var uniqueID = Interlocked.Increment(ref IdIndex);
            EasyStream.EnqueueMessage(new TestMessage(UniqueId, message));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Return null when unable to recieve object</returns>
        public TestMessage GetNewMessage()
        {
            return EasyStream.AttemptDequeueMessage();
        }

        public override void RecieveMessage(byte[] input)
        {
            EasyStream.Read(input);
        }

        public override bool Available()
        {
            return EasyStream.Avaliable();
        }

        public override byte[] SendMessage()
        {
            return EasyStream.Write(65535);
        }
    }

    public struct TestMessage
    {
        public int ID;
        public string Message;

        public TestMessage(int id, string msg)
        {
            ID = id;
            Message = msg;
        }
    }
}

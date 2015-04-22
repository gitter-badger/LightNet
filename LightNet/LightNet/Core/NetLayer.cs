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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightNet
{
    // For now, it's implemented for TCP connections until abstraction of this can be decided.
    public class NetLayer
    {
        CancellationTokenSource cancelSource = new CancellationTokenSource();
        CancellationTokenSource serverCancelSource = new CancellationTokenSource();
        ConcurrentQueue<byte[]> OutgoingMessages = new ConcurrentQueue<byte[]>();
        ConcurrentQueue<byte[]> IncomingMessages = new ConcurrentQueue<byte[]>();
        int IsDisposed;
        int LoginClearance;
        int ServerInterlock = 0;
        Task ServerProcessTask;
        Task ClientProcessTask;
        TcpListener Listener = null;
        ConcurrentDictionary<IPEndPoint, TcpClient> Clients = new ConcurrentDictionary<IPEndPoint, TcpClient>();

        public NetLayer()
        {
            ServerProcessTask = new Task(ServerProcess);
            ClientProcessTask = new Task(MultiClientProcess);
            ClientProcessTask.Start();
        }

        public async Task<byte[]> DequeueIncomingPackets()
        {
            return await Task.Factory.StartNew<byte[]>(delegate()
            {
                if (cancelSource.IsCancellationRequested)
                    return new byte[0];
                var data = new byte[0];
                while (!IncomingMessages.TryDequeue(out data))
                {
                    if (cancelSource.IsCancellationRequested)
                        return data;
                }
                return data;
            });
        }

        public async Task EnqueueOutgoingPackets(ServiceMessage[] contents)
        {
            await Task.Factory.StartNew(delegate()
            {
                if (contents.Length == 0 || cancelSource.IsCancellationRequested)
                    return;
                foreach (var content in contents)
                {
                    if (cancelSource.IsCancellationRequested)
                        return;
                    OutgoingMessages.Enqueue(content.ToBinary());
                }
            });
        }

        public void CreateNewKeyExchange()
        {
            return; // Does nothing.
        }

        public async Task Connect(IPEndPoint hostAddress)
        {
            await Task.Factory.StartNew(delegate()
            {
                TcpClient client = new TcpClient();
                client.ConnectAsync(hostAddress.Address, hostAddress.Port).Wait();
                if (!client.Connected) return;
                while (!Clients.TryAdd(hostAddress, client))
                {
                    if (cancelSource.IsCancellationRequested)
                        return;
                }
            });
        }

        public async Task Disconnect(IPEndPoint hostAddress)
        {
            if (cancelSource.IsCancellationRequested || !Clients.ContainsKey(hostAddress))
                return;
            await Task.Factory.StartNew(delegate()
            {
                TcpClient client;
                while (!Clients.TryRemove(hostAddress, out client))
                {
                    if (cancelSource.IsCancellationRequested)
                        return;
                }
                client.Close();
            });
        }

        public IPEndPoint[] GetActiveEndPoints()
        {
            return Clients.Keys.ToArray();
        }

        public void Listen(IPEndPoint hostAddress, int connectionLimit)
        {
            var orig = Interlocked.Exchange(ref ServerInterlock, 1);
            if (orig == 1)
                return;

            Listener = new TcpListener(hostAddress);
            if (connectionLimit > 0)
                Listener.Start(connectionLimit);
            else
                Listener.Start();
            ServerProcessTask.Start();
        }

        public void StopListening()
        {
            serverCancelSource.Cancel();
        }

        public void SetMinimumUserClearance(int Limit)
        {
            Interlocked.Exchange(ref LoginClearance, Limit);
        }

        public void Dispose()
        {
            var original = Interlocked.Exchange(ref IsDisposed, 1);
            if (original == 1)
                return;
            cancelSource.Cancel();
        }

        void ServerProcess()
        {
            lock (Listener)
                while (!serverCancelSource.IsCancellationRequested)
                {
                    Task<TcpClient> runningTask = Listener.AcceptTcpClientAsync();
                    runningTask.Start();
                    Task.WaitAny(new[] { runningTask }, serverCancelSource.Token);
                    TcpClient newClient = runningTask.Result;
                    if (newClient == null)
                        continue;
                    if (!newClient.Connected)
                        continue;

                    while (!Clients.TryAdd((IPEndPoint)newClient.Client.RemoteEndPoint, newClient))
                    {
                        if (serverCancelSource.IsCancellationRequested)
                            break;
                    }
                }
            Listener.Stop();
            Interlocked.Exchange(ref ServerInterlock, 0);
        }

        void MultiClientProcess()
        {
            while (!cancelSource.IsCancellationRequested)
            {
                var ClientTasks = new List<Task>();
                var enumerate = Clients.GetEnumerator();
                while (enumerate.MoveNext())
                {
                    var taskForClient = SingleClientProcess(enumerate.Current.Value, enumerate.Current.Key);
                    taskForClient.Start();
                    ClientTasks.Add(taskForClient);
                }

                Task.WaitAll(ClientTasks.ToArray(), cancelSource.Token);
                ClientTasks.Clear();
            }
        }

        async Task SingleClientProcess(TcpClient client, IPEndPoint address)
        {
            await Task.Factory.StartNew(delegate()
            {
                
            });
        }
    }
}

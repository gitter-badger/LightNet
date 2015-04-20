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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
namespace LightNet
{
    public class LightNetManager : IDisposable
    {
        #region Private Fields
        NetLayer netLayer;
        Task IncomingServiceMessageProcessingTask;
        Task OutgoingServiceMessageProcessingTask;
        CancellationTokenSource cancelSource = new CancellationTokenSource();
        volatile bool IsDisposed;
        ServiceMessageBalancer balancer = new ServiceMessageBalancer();
        #endregion
        #region Constructor
        public LightNetManager(NetLayer layer)
        {
            netLayer = layer;
            IncomingServiceMessageProcessingTask = new Task(IncomingServiceMessageProcessing, TaskCreationOptions.LongRunning);
            OutgoingServiceMessageProcessingTask = new Task(OutgoingServiceMessageProcessing, TaskCreationOptions.LongRunning);
        }
        #endregion
        #region Destructor
        ~LightNetManager()
        {
            Dispose();
        }
        #endregion
        #region Public Methods
        public void Start()
        {
            IncomingServiceMessageProcessingTask.Start();
            OutgoingServiceMessageProcessingTask.Start();
        }

        public void Stop()
        {
            cancelSource.Cancel();
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            cancelSource.Cancel();
            Task.WaitAll(IncomingServiceMessageProcessingTask, OutgoingServiceMessageProcessingTask);

            IncomingServiceMessageProcessingTask.Dispose();
            OutgoingServiceMessageProcessingTask.Dispose();
            netLayer.Dispose();
        }

        public async Task AddService(Service service, ServicePriority priority)
        {
            await balancer.AddService(service, priority);
        }

        public async Task AddService(Service service)
        {
            await balancer.AddService(service, ServicePriority.Med);
        }

        public async Task RemoveService(Service service)
        {
            await balancer.RemoveService(service);
        }
        #endregion

        internal async void IncomingServiceMessageProcessing()
        {
            while (!cancelSource.IsCancellationRequested)
            {
                var rawContent = await netLayer.DequeueIncomingPackets();

                if (rawContent.Length < 5)
                    continue;

                await balancer.ProcessIncomingMessage(new ServiceMessage(rawContent));
            }
        }

        internal async void OutgoingServiceMessageProcessing()
        {
            while (!cancelSource.IsCancellationRequested)
            {
                var outgoingMessages = await balancer.ProcessOutgoingMessage();
                await netLayer.EnqueueOutgoingPackets(outgoingMessages);
            }
        }
    }
}


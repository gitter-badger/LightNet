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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightNet
{
    /// <summary>
    /// Handle multiple groups of services in different priorities while asynchronously support
    /// shifting priority, adding services, and removal of services as well as processing incoming and outgoing
    /// service messages seamlessly.
    /// </summary>
    internal class ServiceMessageBalancer : IDisposable
    {
        #region Private Fields
        ConcurrentDictionary<int, Service> ServiceDirectory = new ConcurrentDictionary<int, Service>();
        ConcurrentDictionary<int, ServicePriority> ServiceIDToServicePriority = new ConcurrentDictionary<int, ServicePriority>();
        List<int> HighPriorityBatch = new List<int>();
        List<int> MedPriorityBatch = new List<int>();
        List<int> LowPriorityBatch = new List<int>();

        int ServiceID;
        CancellationTokenSource cancelSource = new CancellationTokenSource();
        bool IsDisposed;
        volatile int _highPritoryBatchLimit = 20;
        volatile int _medPritoryBatchLimit = 15;
        volatile int _lowPritoryBatchLimit = 10;
        int _lockProcess;
        int _highIndex = 0;
        int _medIndex = 0;
        int _lowIndex = 0;
        #endregion
        #region Constructor
        public ServiceMessageBalancer()
        {

        }
        #endregion
        #region Public Properties
        public int HighPritoryBatchLimit
        {
            get
            {
                return _highPritoryBatchLimit;
            }
            internal set
            {
                _highPritoryBatchLimit = value;
            }
        }
        public int MedPritoryBatchLimit
        {
            get
            {
                return _medPritoryBatchLimit;
            }
            internal set
            {
                _medPritoryBatchLimit = value;
            }
        }
        public int LowPritoryBatchLimit
        {
            get
            {
                return _lowPritoryBatchLimit;
            }
            internal set
            {
                _lowPritoryBatchLimit = value;
            }
        }
        #endregion
        #region Public Methods
        public async Task AddService(Service service, ServicePriority priority)
        {
            if (service.ServiceID != 0)
                return; // Shouldn't add the same service instance twice.
            var uniqueServiceID = Interlocked.Increment(ref ServiceID);
            var original = Interlocked.Exchange(ref service._serviceID, uniqueServiceID);
            await Task.Factory.StartNew(
                delegate()
                {
                    try
                    {
                        while (!ServiceDirectory.TryAdd(uniqueServiceID, service))
                        {
                            if (ServiceDirectory.ContainsKey(uniqueServiceID) ||
                                cancelSource.IsCancellationRequested)
                                return;
                        }

                        while (!ServiceDirectory.TryAdd(uniqueServiceID, service))
                        {
                            if (cancelSource.IsCancellationRequested) return;
                            if (ServiceIDToServicePriority.ContainsKey(uniqueServiceID))
                            {
                                ServiceIDToServicePriority[uniqueServiceID] = priority;
                                break;
                            }
                        }

                        switch (priority)
                        {
                            case ServicePriority.High:
                                {
                                    lock (HighPriorityBatch)
                                        HighPriorityBatch.Add(uniqueServiceID);
                                    break;
                                }
                            case ServicePriority.Med:
                                {
                                    lock (MedPriorityBatch)
                                        MedPriorityBatch.Add(uniqueServiceID);
                                    break;
                                }
                            case ServicePriority.Low:
                                {
                                    lock (LowPriorityBatch)
                                        LowPriorityBatch.Add(uniqueServiceID);
                                    break;
                                }
                        }
                    }
                    catch (Exception)
                    {
                        // For now, suppress error
                    }
                }
            );
        }

        public async Task RemoveService(Service service)
        {
            var tempID = Interlocked.Exchange(ref service._serviceID, 0);
            if (tempID == 0)
                return;

            await Task.Factory.StartNew(
                delegate()
                {
                    try
                    {
                        Service temp = default(Service);
                        while (!ServiceDirectory.TryRemove(tempID, out temp))
                        {
                            if (ServiceDirectory.ContainsKey(tempID) ||
                                cancelSource.IsCancellationRequested)
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // For now, suppress error
                    }
                }
            );
        }

        public async Task SetServicePriority(Service service, ServicePriority priority)
        {
            await Task.Factory.StartNew(delegate()
            {
                if (!ServiceIDToServicePriority.ContainsKey(service.ServiceID))
                {
                    AddService(service, priority).Wait();
                    return;
                }
                while (!ServiceIDToServicePriority.TryAdd(service.ServiceID, priority))
                {
                    if (cancelSource.IsCancellationRequested)
                        return;
                    if (ServiceIDToServicePriority.ContainsKey(service.ServiceID))
                    {
                        ServiceIDToServicePriority[service.ServiceID] = priority;
                        return;
                    }
                }
            });
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            cancelSource.Cancel();

            ServiceDirectory.Clear();

            IsDisposed = true;
        }

        public async Task ProcessIncomingMessage(ServiceMessage message)
        {
            await Task.Factory.StartNew(delegate() { ServiceDirectory[message.ServiceID].RecieveMessage(message.Data); });
        }

        public async Task<ServiceMessage[]> ProcessOutgoingMessage()
        {
            var orig = Interlocked.Exchange(ref _lockProcess, 1);
            if (orig == 1)
                return new ServiceMessage[0];

            return await Task.Factory.StartNew<ServiceMessage[]>(delegate()
            {
                var bagOfMessages = new List<ServiceMessage>();
                    Func<List<int>, int, int, bool> ProcessPriorityBatch = new Func<List<int>, int, int, bool>(delegate(List<int> batch, int batchIndex, int batchLimit)
                        {
                            lock (batch)
                                for (int I = 0; I < batchLimit; I++)
                                {
                                    if (cancelSource.IsCancellationRequested)
                                        return false;

                                    var index = Interlocked.Increment(ref batchIndex);
                                    if (index >= batch.Count)
                                        index = Interlocked.Increment(ref batchIndex);

                                    if (index >= batch.Count)
                                        return false;
                                    var currentServiceID = batch[index];
                                    var currentService = ServiceDirectory[currentServiceID];
                                    if (currentService.Available)
                                        bagOfMessages.Add(new ServiceMessage(currentServiceID, currentService.SendMessage()));
                                }
                            return true;
                        }
                    );

                    ProcessPriorityBatch(HighPriorityBatch, _highIndex, _highPritoryBatchLimit);
                    ProcessPriorityBatch(MedPriorityBatch, _medIndex, _medPritoryBatchLimit);
                    ProcessPriorityBatch(LowPriorityBatch, _lowIndex, _lowPritoryBatchLimit);

                    Interlocked.Exchange(ref _lockProcess, 0); // Unlock the process
                    return bagOfMessages.ToArray();
                });
        }
        #endregion
    }
}

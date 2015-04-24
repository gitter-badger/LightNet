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

namespace LightNet
{
	public abstract class Service
	{
        /// <summary>
        /// This is an unsafe integer that holds the ID for this service.
        /// Please use ServiceID property instead.
        /// </summary>
        internal int _serviceID;

        /// <summary>
        /// Assigned ServiceID
        /// This is changed when you add and remove Service to and from LightNet manager.
        /// </summary>
        public int ServiceID
        {
            get
            {
                return _serviceID;
            }
        }

        /// <summary>
        /// This indictate whether or not if this service can handle multiple clients at once or not.
        /// </summary>
        public bool IsADomainService;

        /// <summary>
        /// Reserved Methods for Network Manager to recieve message from it's recipients
        /// </summary>
        /// <param name="input"></param>
		public abstract void RecieveMessage(byte[] input);

        /// <summary>
        /// Reserved Methods for Network Manager allowing Network Manager to acknowledge that Service have messages that needed
        /// to be sent out to it's outgoing recipients
        /// Note: It is advised to keep this method simple and efficient as this can potentially slow down network processing.
        /// </summary>
        public abstract bool Available { get; }

		/// <summary>
		/// Reserved Methods for NetworkManager to send message out to outgoing recipients
		/// </summary>
		/// <returns>The message.</returns>
        public abstract byte[] SendMessage();
	}
}


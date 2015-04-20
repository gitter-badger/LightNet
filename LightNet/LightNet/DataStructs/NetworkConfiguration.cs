﻿/*
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
using System.Net;

namespace LightNet
{
	public sealed class NetworkConfiguration
	{
		public IPAddress DestinationIPAddress;
		public ushort DestinationPort;
		public uint MinimumSymmetricKeyStrength = 512;
		/// <summary>
		/// Begin another key exchange cycle per 'this' amount of exchanges
		/// </summary>
		public uint KeyCycle = 10000;
	}
}

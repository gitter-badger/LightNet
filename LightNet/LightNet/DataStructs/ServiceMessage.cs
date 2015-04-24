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
using System.IO;
using System.Net;

namespace LightNet
{
    public sealed class ServiceMessage
    {
        IPEndPoint AddressTo;
        IPEndPoint AddressFrom;
        User Member;
        public int ServiceID;
        public byte[] Data;

        public ServiceMessage(int serviceid, byte[] data)
        {
            ServiceID = serviceid;
            Data = data;
        }

        public ServiceMessage(byte[] content)
        {
            if (content.Length < 5)
            {
                throw new Exception("Content is not valid, require at least 5 bytes long for ServiceID and the content.");
            }
            using (var memStream = new MemoryStream(content))
            {
                ServiceID = DataUtility.ReadInt32FromStream(memStream);
                Data = DataUtility.ReadOnlyBytesFromStream(memStream);
            }
        }

        public byte[] ToBinary()
        {
            using (var data = new MemoryStream())
            {
                DataUtility.WriteInt32ToStream(ServiceID, data);
                DataUtility.WriteOnlyBytesToStream(Data, data);
                return data.ToArray();
            }
        }
    }
}
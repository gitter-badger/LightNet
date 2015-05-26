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

namespace LightNet
{
    public class LoginPacket : Packet
    {
        public new byte[] RawContent
        {
            get
            {
                return _RawContent;
            }
            set
            {
                _RawContent = value;
            }
        }

        public ulong UserID
        {
            get { return DataUtility.GetUInt64(_RawContent.Take(8).ToArray()); }
            set { Array.ConstrainedCopy(DataUtility.GetBytes(value), 0, _RawContent, 0, 8); }
        }
        public byte[] PasswordHash
        {
            get { return _RawContent.Skip(8).ToArray(); }
            set { Array.ConstrainedCopy(value, 0, _RawContent, 8, 64); }
        }

        public LoginPacket(byte[] content)
        {
            _ID = PacketID.Login;
            if (content.Length > 71)
                _RawContent = content;
            else
                _RawContent = new byte[72];
        }

        public LoginPacket(ulong userid, byte[] passwordHash)
        {
            _ID = PacketID.Login;
            _RawContent = new byte[72];
            UserID = userid;
            PasswordHash = passwordHash;
        }
    }
}

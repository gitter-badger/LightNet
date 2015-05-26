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

namespace LightNet
{
    public class LoginReplyPacket : Packet
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

        public UserLoginStatus UserLoginStatus
        {
            get { return (UserLoginStatus)_RawContent[0]; }
            set { RawContent[0] = (byte)value; }
        }

        public LoginReplyPacket(byte[] content)
        {
            RawContent = content;
            if (RawContent.Length < 1)
                RawContent = new byte[1];
        }

        public LoginReplyPacket(UserLoginStatus status)
        {
            _ID = PacketID.LoginReply;
            _RawContent = new byte[1];
            UserLoginStatus = status;
        }
    }

    public enum UserLoginStatus : byte
    {
        Success = 0,
        Failed = 1,
        Rejected = 2 // Meaning Login is Reserved for Specific Computers
    }
}

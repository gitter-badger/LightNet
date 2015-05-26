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
    public class Packet
    {
        protected PacketID _ID;
        protected byte[] _RawContent;

        public PacketID ID { get { return _ID; } set { _ID = value; } }
        public byte[] RawContent { get { return _RawContent; } set { _RawContent = value; } }

        public Packet()
        {

        }

        public Packet(PacketID id, byte[] content)
        {
            _ID = id;
            _RawContent = content;
        }
    }
}

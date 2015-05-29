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

namespace LightNet
{
	public class Packet
	{
		protected byte _ID;
		protected byte[] _RawContent;

		public byte ID {
			get {
				if (_RawContent.Length > 0)
					return _RawContent [0];
				else
					return 0;
			} 
		}

		public byte[] RawContent { get { return _RawContent; } set { _RawContent = value; } }

		public int Length { get { return _RawContent.Length; } }

		public Packet ()
		{
			_RawContent = new byte[0];
		}

		public Packet (byte[] content)
		{
			_RawContent = (byte[])content.Clone ();
		}

		public Packet (byte id, byte[] content)
		{
			_RawContent = new byte[content.Length + 1];
			_RawContent [0] = id;
			Array.Copy (content, 0, _RawContent, 1, content.Length);
		}
	}
}

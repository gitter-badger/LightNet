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
	/// <summary>
	/// Interface for Transforming Packet
	/// For use with Cryptography and Scrambling
	/// Can also be used for compression
	/// </summary>
	public interface ITransform
	{
		/// <summary>
		/// Returns how many representable states the Transformer requires for Packet ID.
		/// </summary>
		/// <value>The offset usage count.</value>
		int OffsetUsageCount {get;}

		// For Outgoing Packet Transformation
		void EnqueueUntransformedPacket(Packet packet);
		Packet DequeueTransformedPacket();

		// For Incoming Packet Transformation
		void EnqueueTransformedPacket(Packet packet);
		Packet DequeueUntransformedPacket();

		/// <summary>
		/// Initialize the Transformer by setting wheither or not if client initate the connection
		/// or accepted the connection. And to assign which range of identifiers the transformer will use.
		/// </summary>
		/// <param name="initate">Set to true if initated connection, otherwise false</param>
		/// <param name="offset">Set offset for which range of identifiers to use.</param>
		void Initialize(bool initate, byte offset);
		/// <summary>
		/// Occurs when connection failure occured.
		/// Ex. Message Authenication Code Vertified That Something Has Been Tampered With.
		/// </summary>
		event EventHandler ConnectionFailureEvent;

		void Dispose();
	}
}


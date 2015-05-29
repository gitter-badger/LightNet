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

using Salar.Bois;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightNet
{
    /// <summary>
    /// Byte Service Stream is designed to handle objects being broken up in several packets and reconstruct it from network stream.
    /// It is also recommended that all service message objects are kept below 65,535 bytes int to allow batch balancing to do it's
    /// job efficiently. It also should be noted that this is designed ONLY for Services, not to be used for any internal part of the
    /// network manager.
    /// </summary>
	public class ByteServiceStream
	{
		Dictionary<byte, Type> IDToType = new Dictionary<byte, Type> ();
		Dictionary<Type, byte> TypeToID = new Dictionary<Type, byte> ();
		BoisSerializer Serializer = new BoisSerializer();
		MemoryStream InStream = new MemoryStream ();
		MemoryStream OutStream = new MemoryStream ();
        byte _attemptedTypeID;
        int _attemptedLength;
        bool _attempted;

		/// <summary>
		/// Initializes a new instance of the <see cref="LightNet.ByteServiceStream"/> class.
		/// </summary>
		/// <param name='MessageObjects'>
		/// Initialize the service stream by loading the types of message objects (Use the typeof keyword)
		/// </param>
		public ByteServiceStream (params Type[] MessageObjects)
		{
			byte idRoll = 0;
			foreach (var obj in MessageObjects) {
				IDToType.Add (idRoll, obj);
				TypeToID.Add (obj, idRoll);
				idRoll++;
			}

			Serializer.Initialize (MessageObjects);
		}

		/// <summary>
		/// Enqueues the message.
		/// </summary>
		/// <param name='data'>
		/// This will serialize the Data object to stream
		/// </param>
		public void EnqueueMessage (object data)
		{
			var buffer = new MemoryStream ();
			// Check The Type of Enqueued Object
			byte TypeID = TypeToID [data.GetType ()];
			// Write the ID to the stream
			DataUtility.WriteUInt8ToStream (TypeID, buffer);
			// Inner Memory Stream
			var innerBuffer = new MemoryStream ();
			// Serialize the data into stream
            Serializer.Serialize(data, innerBuffer);
			// Write that Stream
			DataUtility.WriteBytesToStream (innerBuffer.ToArray (), buffer);
			// Lock the OutStream and enqueue the stream into output
			lock (OutStream) {
				OutStream.Seek (0, SeekOrigin.End);
				DataUtility.WriteOnlyBytesToStream (OutStream, buffer.ToArray ());
			}
		}

		public object AttemptDequeueMessage ()
		{
			lock (InStream) {
				if (_attempted) {
					if (InStream.Length < _attemptedLength)
						return null;
				} else {
					if (InStream.Length < 5)
						return null;
					InStream.Seek (0, SeekOrigin.Begin);
					_attemptedTypeID = DataUtility.ReadUInt8FromStream (InStream);
					_attemptedLength = DataUtility.ReadInt32FromStream (InStream);
					_attempted = true;
					if (InStream.Length < _attemptedLength) {
						return null;
					}
				}
				InStream.Seek (5, SeekOrigin.Begin);
                var genericDeserializerMethod = typeof(BoisSerializer).GetMethods().Where(I => I.Name == "Deserialize").
                    First(I => I.GetParameters()[0].ParameterType == typeof(Stream)).MakeGenericMethod(IDToType[_attemptedTypeID]);
                object output = genericDeserializerMethod.Invoke(Serializer, new[] { InStream });
                DataUtility.ClearAndCopyMemoryStream(ref InStream, 5 + _attemptedLength);
                return output;
			}
		}

		public bool Avaliable ()
		{
			lock (OutStream)
				return OutStream.Length > 0;
		}

		public byte[] Write (int size)
		{
			var data = new byte[size];
			lock (OutStream) {
				OutStream.Seek (0, SeekOrigin.Begin);
				var fixLength = OutStream.Read (data, 0, data.Length);
				Array.Resize (ref data, fixLength);

				DataUtility.ClearAndCopyMemoryStream (ref OutStream, data.Length);
				return data;
			}
		}

		public void Read (byte[] data)
		{
			lock (InStream) {
				InStream.Seek (0, SeekOrigin.End);
				InStream.Write (data, 0, data.Length);
			}
		}
	}
}


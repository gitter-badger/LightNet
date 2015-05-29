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
using Mpir.NET;
using SuperInteger;
using Org.BouncyCastle.Security;
using LightNet;

namespace LightNet
{
	// It turn out that in C with GMP library can generate 8192 bit key within 1.3 seconds
	// While in IntX library, it take ~500 seconds
	/// <summary>
	/// Represents the Diffie-Hellman algorithm.
	/// </summary>
	public class DiffieHellman : IDisposable
	{

		
		#region - Util -

		SecureRandom RandomGenerator = new SecureRandom ();

		#endregion

		
		#region - Fields -

		/// <summary>
		/// The number of bytes to generate.
		/// </summary>
		int bytes = 2048;
		/// <summary>
		/// The shared prime.
		/// </summary>
		dynamic p;
		/// <summary>
		/// The shared base.
		/// </summary>
		dynamic g;
		/// <summary>
		/// The final key.
		/// </summary>
		dynamic S;
		/// <summary>
		/// The secret number
		/// </summary>
		int a;

		
		#endregion

		#region - Properties -

		/// <summary>
		/// Gets the final key to use for encryption.
		/// </summary>
		public byte[] Key {
			get;
			set;
		}

		
		#endregion

		
		#region - Ctor -

		public DiffieHellman ()
		{
		}

		public DiffieHellman (int bytesize)
		{
			bytes = bytesize;
		}

		~DiffieHellman ()
		{
			Dispose (false);
		}

		
		#endregion

		
		#region - Implementation Methods -

		mpz_t W_GeneratePrime ()
		{
			int limit = bytes;
			if (limit < 4)
				limit = 4;
			var raw = new byte[limit];
			RandomGenerator.NextBytes (raw);
			var newInt = new mpz_t (raw, 1);
			return newInt;
		}

		Integer L_GeneratePrime ()
		{
			int limit = bytes;
			if (limit < 4)
				limit = 4;
			var raw = new byte[limit];
			RandomGenerator.NextBytes (raw);
			var newInt = new Integer (raw);
			return newInt;
		}

		/// <summary>
		/// Generates a request packet.
		/// </summary>
		/// <returns></returns>
		public byte[] GenerateRequest ()
		{
			var currentPlatform = OSCheck.RunningPlatform ();
			if (currentPlatform == OSCheck.Platform.Mac) {
				throw new PlatformNotSupportedException ("Mac OSX is not a supported operating system for this.");
			}
			// Generate the parameters.
			var raw = new byte[bytes];
			var memStream = new MemoryStream ();
			RandomGenerator.NextBytes (raw);
			a = RandomGenerator.Next ((bytes) / 4 * 3, bytes);
			if (currentPlatform == OSCheck.Platform.Windows) {
				p = W_GeneratePrime ();
				g = new mpz_t (raw, 1);
				var A = g.Power (a);
				A = A.Mod (p);
				// Get Raw Integer Data
				var gData = (g as mpz_t).ToByteArray (1);
				var pData = (p as mpz_t).ToByteArray (1);
				var AData = A.ToByteArray (1);

				// Write Length to Stream
				DataUtility.WriteBytesToStream (gData, memStream);
				DataUtility.WriteBytesToStream (pData, memStream);
				DataUtility.WriteBytesToStream (AData, memStream);
			} else if (currentPlatform == OSCheck.Platform.Linux) {
				p = L_GeneratePrime ();
				g = new Integer ();
				g.FromBytes (raw);
				var A = g.Pow (a);
				A %= p;
				// Get Raw Integer Data
				var gData = g.ToBytes ();
				var pData = p.ToBytes ();
				var AData = A.ToBytes ();

				// Write Length to Stream
				DataUtility.WriteBytesToStream (gData, memStream);
				DataUtility.WriteBytesToStream (pData, memStream);
				DataUtility.WriteBytesToStream (AData, memStream);
			}
			var finalDataSend = memStream.ToArray ();
			memStream.Dispose ();
			return finalDataSend;
		}

		void W_GetKeyData ()
		{
			Key = S.ToByteArray (1);
		}

		void L_GetKeyData ()
		{
			Key = S.ToBytes ();
		}

		/// <summary>
		/// Generate a response packet.
		/// </summary>
		/// <param name="request">The string representation of the request.</param>
		/// <returns></returns>
		public byte[] GenerateResponse (byte[] request)
		{
			var currentPlatform = OSCheck.RunningPlatform ();
			if (currentPlatform == OSCheck.Platform.Mac) {
				throw new PlatformNotSupportedException ("Mac OSX is not a supported operating system for this.");
			}

			var instream = new MemoryStream (request);
			var gData = DataUtility.ReadBytesFromStream (instream);
			var pData = DataUtility.ReadBytesFromStream (instream);
			var AData = DataUtility.ReadBytesFromStream (instream);
			byte[] BData = null;

			if (currentPlatform == OSCheck.Platform.Windows) {
				g = new mpz_t (gData, 1);
				p = new mpz_t (pData, 1);
				var A = new mpz_t (AData, 1);

				// Generate the parameters.
				a = RandomGenerator.Next (bytes);
				var B = g.Power (a);
				B = B.Mod (p);

				// Get Raw IntX Data
				BData = B.ToByteArray (1);

				// Got the key!!! HOORAY!
				S = A.Power (a);
				S = S.Mod (p);
				W_GetKeyData ();
			} else if (currentPlatform == OSCheck.Platform.Linux) {
				g = new Integer ();
				g.FromBytes (gData);
				p = new Integer ();
				p.FromBytes (pData);
				var A = new Integer (AData);

				// Generate the parameters.
				a = RandomGenerator.Next (bytes);
				var B = g.Pow (a);
				B %= p;

				// Get Raw IntX Data
				BData = B.ToBytes ();

				// Got the key!!! HOORAY!
				S = A.Pow (a);
				S %= p;
				L_GetKeyData ();
			}
			return BData;
		}

		/// <summary>
		/// Generates the key after a response is received.
		/// </summary>
		/// <param name="response">The string representation of the response.</param>
		public void HandleResponse (byte[] response)
		{
			var currentPlatform = OSCheck.RunningPlatform ();
			if (currentPlatform == OSCheck.Platform.Mac) {
				throw new PlatformNotSupportedException ("Mac OSX is not a supported operating system for this.");
			}
			if (currentPlatform == OSCheck.Platform.Windows) {
				var B = new mpz_t (response, 1);

				S = B.Power (a);
				S = (S as mpz_t).Mod ((p as mpz_t));
				W_GetKeyData ();
			} else if (currentPlatform == OSCheck.Platform.Linux) {
				var B = new Integer (response);
				S = B.Pow (a);
				S %= p;
				L_GetKeyData ();
			}
		}


		
		#endregion

		
		#region IDisposable Members

		public void Dispose ()
		{
            Dispose(true);
            GC.SuppressFinalize(this);
		}
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (S != null)
                {
                    S.Dispose();
                    S = null;
                }
                
                if (p != null)
                {
                    p.Dispose();
                    p = null;
                }

                if (g != null)
                {
                    g.Dispose();
                    g = null;
                }
                
                if (Key != null)
                    Key = null;
            }
        }
		
		#endregion
	
	}
}


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
using System.Text;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace LightNet
{
    /// <summary>
    /// Data Utility is used to ensure that the data being written into Network Stream are corrected for Endian
    /// and improve code readability and reusability when attempting to debug.
    /// </summary>
    public static class DataUtility
    {
        #region Stream Based Solution
        /// <summary>
        /// Writes the signed byte to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteInt8ToStream(sbyte input, Stream output)
        {
            output.WriteByte((byte)input);
        }

        /// <summary>
        /// Writes the signed byte to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteInt8ToStream(Stream output, sbyte input)
        {
            output.WriteByte((byte)input);
        }

        /// <summary>
        /// Reads the signed byte from stream.
        /// </summary>
        /// <returns>The signed byte from stream.</returns>
        /// <param name="input">Input.</param>
        public static sbyte ReadInt8FromStream(Stream input)
        {
            return (sbyte)input.ReadByte();
        }

        /// <summary>
        /// Writes the int16 to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteInt16ToStream(short input, Stream output)
        {
            var data = BitConverter.GetBytes(input);
            ConvertForBigEndian(data);
            output.Write(data, 0, 2);
        }

        /// <summary>
        /// Writes the int16 to stream.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="input">Input.</param>
        public static void WriteInt16ToStream(Stream output, short input)
        {
            WriteInt16ToStream(input, output);
        }

        /// <summary>
        /// Reads the int16 from stream.
        /// </summary>
        /// <returns>The int16 from stream.</returns>
        /// <param name="input">Input.</param>
        public static short ReadInt16FromStream(Stream input)
        {
            var data = new byte[2];
            input.Read(data, 0, 2);
            ConvertForBigEndian(data);
            return BitConverter.ToInt16(data, 0);
        }

        /// <summary>
        /// Writes the int32 to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteInt32ToStream(int input, Stream output)
        {
            var data = BitConverter.GetBytes(input);
            ConvertForBigEndian(data);
            output.Write(data, 0, 4);
        }

        /// <summary>
        /// Writes the int32 to stream.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="input">Input.</param>
        public static void WriteInt32ToStream(Stream output, int input)
        {
            WriteInt32ToStream(input, output);
        }

        /// <summary>
        /// Reads the int32 from stream.
        /// </summary>
        /// <returns>The int32 from stream.</returns>
        /// <param name="input">Input.</param>
        public static int ReadInt32FromStream(Stream input)
        {
            var data = new byte[4];
            input.Read(data, 0, 4);
            ConvertForBigEndian(data);
            return BitConverter.ToInt32(data, 0);
        }

        /// <summary>
        /// Writes the int64 to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteInt64ToStream(long input, Stream output)
        {
            var data = BitConverter.GetBytes(input);
            ConvertForBigEndian(data);
            output.Write(data, 0, 8);
        }

        /// <summary>
        /// Writes the int64 to stream.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="input">Input.</param>
        public static void WriteInt64ToStream(Stream output, long input)
        {
            WriteInt64ToStream(input, output);
        }
        /// <summary>
        /// Reads the int64 from stream.
        /// </summary>
        /// <returns>The int64 from stream.</returns>
        /// <param name="input">Input.</param>
        public static long ReadInt64FromStream(Stream input)
        {
            var data = new byte[8];
            input.Read(data, 0, 8);
            ConvertForBigEndian(data);
            return BitConverter.ToInt64(data, 0);
        }

        /// <summary>
        /// Writes the unsigned byte to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteUInt8ToStream(byte input, Stream output)
        {
            output.WriteByte(input);
        }

        /// <summary>
        /// Writes the unsigned byte to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteUInt8ToStream(Stream output, byte input)
        {
            output.WriteByte(input);
        }

        /// <summary>
        /// Reads the unsigned byte from stream.
        /// </summary>
        /// <returns>The Uint8 from stream.</returns>
        /// <param name="input">Input.</param>
        public static byte ReadUInt8FromStream(Stream input)
        {
            return (byte)input.ReadByte();
        }

        /// <summary>
        /// Writes the Uint16 to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteUInt16ToStream(ushort input, Stream output)
        {
            var data = BitConverter.GetBytes(input);
            ConvertForBigEndian(data);
            output.Write(data, 0, 2);
        }

        /// <summary>
        /// Writes the Uint16 to stream.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="input">Input.</param>
        public static void WriteUInt16ToStream(Stream output, ushort input)
        {
            WriteUInt16ToStream(input, output);
        }

        /// <summary>
        /// Reads the Uint16 from stream.
        /// </summary>
        /// <returns>The Uint16 from stream.</returns>
        /// <param name="input">Input.</param>
        public static ushort ReadUInt16FromStream(Stream input)
        {
            var data = new byte[2];
            input.Read(data, 0, 2);
            ConvertForBigEndian(data);
            return BitConverter.ToUInt16(data, 0);
        }

        /// <summary>
        /// Writes the Uint32 to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteUInt32ToStream(uint input, Stream output)
        {
            var data = BitConverter.GetBytes(input);
            ConvertForBigEndian(data);
            output.Write(data, 0, 4);
        }

        /// <summary>
        /// Writes the Uint32 to stream.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="input">Input.</param>
        public static void WriteUInt32ToStream(Stream output, uint input)
        {
            WriteUInt32ToStream(input, output);
        }

        /// <summary>
        /// Reads the Uint32 from stream.
        /// </summary>
        /// <returns>The Uint32 from stream.</returns>
        /// <param name="input">Input.</param>
        public static uint ReadUInt32FromStream(Stream input)
        {
            var data = new byte[4];
            input.Read(data, 0, 4);
            ConvertForBigEndian(data);
            return BitConverter.ToUInt32(data, 0);
        }

        /// <summary>
        /// Writes the Uint64 to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteUInt64ToStream(ulong input, Stream output)
        {
            var data = BitConverter.GetBytes(input);
            ConvertForBigEndian(data);
            output.Write(data, 0, 8);
        }

        /// <summary>
        /// Writes the Uint64 to stream.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="input">Input.</param>
        public static void WriteUInt64ToStream(Stream output, ulong input)
        {
            WriteUInt64ToStream(input, output);
        }

        /// <summary>
        /// Reads the Uint64 from stream.
        /// </summary>
        /// <returns>The Uint64 from stream.</returns>
        /// <param name="input">Input.</param>
        public static ulong ReadUInt64FromStream(Stream input)
        {
            var data = new byte[8];
            input.Read(data, 0, 8);
            ConvertForBigEndian(data);
            return BitConverter.ToUInt64(data, 0);
        }

        /// <summary>
        /// Writes the UTF8 string to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="output">Output.</param>
        public static void WriteUTF8StringToStream(string input, Stream output)
        {
            var data = Encoding.UTF8.GetBytes(input);
            var len = BitConverter.GetBytes((uint)data.Length);
            ConvertForBigEndian(len);
            output.Write(len, 0, len.Length);
            output.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes the UTF8 string to stream.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="input">Input.</param>
        public static void WriteUTF8StringToStream(Stream output, string input)
        {
            WriteUTF8StringToStream(input, output);
        }

        /// <summary>
        /// Reads the UTF8 string from stream.
        /// </summary>
        /// <returns>The UTF8 string from stream.</returns>
        /// <param name="input">Input.</param>
        public static string ReadUTF8StringFromStream(Stream input)
        {
            var rawUInt32 = new byte[4];
            input.Read(rawUInt32, 0, rawUInt32.Length);
            ConvertForBigEndian(rawUInt32);
            var Length = BitConverter.ToUInt32(rawUInt32, 0);
            var rawTextData = new byte[Length];
            input.Read(rawTextData, 0, (int)Length);
            return Encoding.UTF8.GetString(rawTextData);
        }

        /// <summary>
        /// Writes bytes to stream.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="input">Input.</param>
        public static void WriteBytesToStream(byte[] data, Stream input)
        {
            var len = BitConverter.GetBytes((uint)data.Length);
            ConvertForBigEndian(len);
            input.Write(len, 0, 4);
            input.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes bytes to stream.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="data">Data.</param>
        public static void WriteBytesToStream(Stream input, byte[] data)
        {
            WriteBytesToStream(data, input);
        }

        /// <summary>
        /// Reads bytes from stream.
        /// </summary>
        /// <returns>The bytes from stream.</returns>
        /// <param name="input">Input.</param>
        public static byte[] ReadBytesFromStream(Stream input)
        {
            var rawUInt32 = new byte[4];
            input.Read(rawUInt32, 0, rawUInt32.Length);
            ConvertForBigEndian(rawUInt32);
            var Length = BitConverter.ToUInt32(rawUInt32, 0);
            var data = new byte[Length];
            input.Read(data, 0, data.Length);
            return data;
        }

        /// <summary>
        /// Reads only bytes without any length prefix from stream.
        /// </summary>
        /// <returns>The only bytes from stream.</returns>
        /// <param name="size">Size.</param>
        /// <param name="input">Input.</param>
        public static byte[] ReadOnlyBytesFromStream(int size, Stream input)
        {
            var data = new byte[size];
            input.Read(data, 0, data.Length);
            return data;
        }

        /// <summary>
        /// Reads only bytes without any length prefix from stream.
        /// </summary>
        /// <returns>The only bytes from stream.</returns>
        /// <param name="input">Input.</param>
        /// <param name="size">Size.</param>
        public static byte[] ReadOnlyBytesFromStream(Stream input, int size)
        {
            return ReadOnlyBytesFromStream(size, input);
        }

        /// <summary>
        /// Reads only bytes without any length prefix from stream.
        /// </summary>
        /// <returns>The only bytes from stream.</returns>
        /// <param name="input">Input.</param>
        public static byte[] ReadOnlyBytesFromStream(Stream input)
        {
            var data = new byte[input.Length - input.Position];
            if (data.Length == 0)
                return data;
            input.Read(data, 0, data.Length);
            return data;
        }

        /// <summary>
        /// Writes only bytes without any length prefix to stream.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="output">Output.</param>
        public static void WriteOnlyBytesToStream(byte[] data, Stream output)
        {
            output.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes only bytes without any length prefix to stream.
        /// </summary>
        /// <param name="output">Output.</param>
        /// <param name="data">Data.</param>
        public static void WriteOnlyBytesToStream(Stream output, byte[] data)
        {
            WriteOnlyBytesToStream(data, output);
        }
        #endregion
        #region Buffer Based Solution
        public static byte[] GetBytes(byte[] UnmanagedBuffer)
        {
            return ConvertForBigEndian(UnmanagedBuffer);
        }

        public static byte[] GetBytes(short input)
        {
            return ConvertForBigEndian(BitConverter.GetBytes(input));
        }

        public static byte[] GetBytes(ushort input)
        {
            return ConvertForBigEndian(BitConverter.GetBytes(input));
        }

        public static byte[] GetBytes(int input)
        {
            return ConvertForBigEndian(BitConverter.GetBytes(input));
        }

        public static byte[] GetBytes(uint input)
        {
            return ConvertForBigEndian(BitConverter.GetBytes(input));
        }

        public static byte[] GetBytes(long input)
        {
            return ConvertForBigEndian(BitConverter.GetBytes(input));
        }

        public static byte[] GetBytes(ulong input)
        {
            return ConvertForBigEndian(BitConverter.GetBytes(input));
        }

        public static byte[] GetBytes(float input)
        {
            return ConvertForBigEndian(BitConverter.GetBytes(input));
        }

        public static byte[] GetBytes(decimal input)
        {
            var data = new byte[16];
            var result = decimal.GetBits(input);
            Array.Copy(data, BitConverter.GetBytes(result[0]), 4);
            Array.Copy(data, 4, BitConverter.GetBytes(result[1]), 0, 4);
            Array.Copy(data, 8, BitConverter.GetBytes(result[2]), 0, 4);
            Array.Copy(data, 12, BitConverter.GetBytes(result[3]), 0, 4);
            return ConvertForBigEndian(data);
        }

        public static byte[] GetBytes(double input)
        {
            return ConvertForBigEndian(BitConverter.GetBytes(input));
        }

        public static byte[] GetBytes(char[] input)
        {
            return ConvertForBigEndian(Encoding.Unicode.GetBytes(input));
        }

        public static byte[] GetBytes(string input)
        {
            return ConvertForBigEndian(Encoding.Unicode.GetBytes(input));
        }

        public static short GetInt16(byte[] input)
        {
            return BitConverter.ToInt16(ConvertForBigEndian(input), 0);
        }

        public static ushort GetUInt16(byte[] input)
        {
            return BitConverter.ToUInt16(ConvertForBigEndian(input), 0);
        }

        public static int GetInt32(byte[] input)
        {
            return BitConverter.ToInt32(ConvertForBigEndian(input), 0);
        }

        public static uint GetUInt32(byte[] input)
        {
            return BitConverter.ToUInt32(ConvertForBigEndian(input), 0);
        }

        public static long GetInt64(byte[] input)
        {
            return BitConverter.ToInt64(ConvertForBigEndian(input), 0);
        }

        public static ulong GetUInt64(byte[] input)
        {
            return BitConverter.ToUInt64(ConvertForBigEndian(input), 0);
        }

        public static float GetFloat(byte[] input)
        {
            return BitConverter.ToSingle(ConvertForBigEndian(input), 0);
        }

        public static double GetDouble(byte[] input)
        {
            return BitConverter.ToDouble(ConvertForBigEndian(input), 0);
        }

        public static decimal GetDecimal(byte[] input)
        {
            var buffer = ConvertForBigEndian(input);
            return new decimal(new int[] {
                BitConverter.ToInt32(buffer, 0),
                BitConverter.ToInt32(buffer, 4),
                BitConverter.ToInt32(buffer, 8),
                BitConverter.ToInt32(buffer, 12)
            });
        }

        public static char[] GetCharArray(byte[] input)
        {
            return Encoding.Unicode.GetChars(ConvertForBigEndian(input));
        }

        public static string GetString(byte[] input)
        {
            return Encoding.Unicode.GetString(ConvertForBigEndian(input));
        }
        #endregion
        #region Misc Solution
        /// <summary>
        /// XORs between 2 buffers.
        /// </summary>
        /// <returns>The buffers.</returns>
        /// <param name="massBuffer1">Mass buffer1.</param>
        /// <param name="massBuffer2">Mass buffer2.</param>
        public static byte[] XORBuffers(byte[] Buffer1, byte[] Buffer2)
        {
            for (int I = 0; I < Buffer1.Length && I < Buffer2.Length; I++)
                Buffer1[I] ^= Buffer2[I];
            return Buffer1;
        }

        /// <summary>
        /// Generates an insecure key. Use at your own risk.
        /// </summary>
        /// <returns>The new key.</returns>
        public static byte[] GenerateNewKey(int size)
        {
            var FirstTempBuffer = new byte[size];
            var SecondTempBuffer = new byte[size];

            var randomGen = new Random();
            randomGen.NextBytes(FirstTempBuffer);
            randomGen.Next();
            randomGen.NextBytes(SecondTempBuffer);

            return DataUtility.XORBuffers(FirstTempBuffer, SecondTempBuffer);
        }

        /// <summary>
        /// Generates a SHA512 Hash from string
        /// </summary>
        /// <returns>The hash.</returns>
        /// <param name="password">Password.</param>
        public static byte[] GenerateHash(string password)
        {
            using (var sha = SHA512.Create())
                return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// Cycle the byte forward
        /// </summary>
        /// <returns>The forward.</returns>
        /// <param name="input">Input.</param>
        /// <param name="offset">Offset.</param>
        public static byte CycleForward(byte input, byte offset)
        {
            var result = input + offset;
            if (result > byte.MaxValue)
                result -= byte.MaxValue;
            return (byte)result;
        }

        /// <summary>
        /// Cycle the byte backward
        /// </summary>
        /// <returns>The backward.</returns>
        /// <param name="input">Input.</param>
        /// <param name="offset">Offset.</param>
        public static byte CycleBackward(byte input, byte offset)
        {
            var result = input - offset;
            if (result < byte.MinValue)
                result += byte.MaxValue;
            return (byte)result;
        }

        /// <summary>
        /// This ensure that Endian arrangement are corrected for network stream. If CPU is little endian, the byte arrangement
        /// will be converted to Big Endian to ensure that information can be read by all devices.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ConvertForBigEndian(byte[] input)
        {

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }
            return input;
        }

		/// <summary>
		/// Clears certain length of buffer and copy the remaining buffer to a new memory stream on the same reference.
		/// </summary>
		/// <param name="memStream">Mem stream.</param>
		/// <param name="length">Length.</param>
		public static void ClearAndCopyMemoryStream (ref MemoryStream memStream, int length)
		{
			var BufferedCopy = new byte[memStream.Length - length];
			if (BufferedCopy.Length == 0) {
				memStream.Dispose ();
				memStream = new MemoryStream ();
				return;
			}

			Array.Copy (memStream.ToArray (), length, BufferedCopy, 0, BufferedCopy.Length);
			memStream.Dispose ();
			memStream = new MemoryStream ();
			memStream.Write (BufferedCopy, 0, BufferedCopy.Length);
		}
        #endregion
    }
}


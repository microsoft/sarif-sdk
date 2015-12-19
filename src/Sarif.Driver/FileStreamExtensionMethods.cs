// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.CodeAnalysisDriver
{
    public static class FileStreamExtensionMethods
    {
        public static string ReadString(this FileStream stream, int padTo)
        {
            long startPosition = stream.Position;

            int byteRead;
            int byteCount = 0;

            while ((byteRead = stream.ReadByte()) > 0)
            {
                byteCount++;
            };

            stream.Seek(startPosition, SeekOrigin.Begin);
            byte[] abString = stream.ReadToArray(byteCount);

            int padding = padTo - (byteCount + 1) % padTo;
            if (padding > 0)
            {
                stream.Seek(padding + 1, SeekOrigin.Current);
            }

            return System.Text.Encoding.UTF8.GetString(abString);
        }

        public static UInt16 ReadUInt16(this FileStream stream)
        {
            return BitConverter.ToUInt16(stream.ReadToArray(2), 0);
        }

        public static Int32 ReadInt32(this FileStream stream)
        {
            return BitConverter.ToInt32(stream.ReadToArray(4), 0);
        }

        public static Int64 ReadInt64(this FileStream stream)
        {
            return BitConverter.ToInt64(stream.ReadToArray(8), 0);
        }

        public static byte[] ReadToArray(this FileStream stream, int length)
        {
            byte[] bytes = new byte[length];
            int dataRead = stream.Read(bytes, 0, length);
            if (dataRead != length)
            {
                throw new InvalidDataException("Invalid data length read. Expected " + length + " read " + dataRead);
            }

            return bytes;
        }
    }
}

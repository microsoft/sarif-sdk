// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;


namespace Microsoft.CodeAnalysis.Sarif
{
    public class FileEncoding
    {
        static FileEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        public static bool IsTextualData(byte[] bytes)
        {
            return IsTextualData(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Detects if the provided byte array contains textual data. The heuristic applied
        /// is to allow .NET to attempt to decode the byte array as a sequence of characters.
        /// If that decoding generates does not generate a Unicode replacement character, 
        /// the data is classified as binary rather than text.
        /// </summary>
        /// <param name="bytes">The raw data expressed as bytes.</param>
        /// <param name="start">The starting position to being classification.</param>
        /// <param name="count">The maximal count of characters to decode.</param>
        public static bool IsTextualData(byte[] bytes, int start, int count)
        {

            if (count < bytes.Length)
            {
                var span = new ReadOnlySpan<byte>(bytes, start, count);
                bytes = span.ToArray();
            }

            return IsUtf8Text(bytes);
        }

        /// <summary>
        /// Detects whether the provided byte array contains only UTF8 textual characters.
        /// </summary>
        internal static bool IsUtf8Text(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];

                // Control characters
                if (b < 0x20)
                {
                    return false;
                }

                // Multi-byte sequence
                if ((b & 0b11100000) == 0b11000000)
                {
                    if (i + 1 >= bytes.Length)
                    {
                        return false;
                    }

                    int b2 = bytes[i + 1];
                    if ((b2 & 0b11000000) != 0b10000000)
                    {
                        return false;
                    }

                    i++;
                    continue;
                }

                if ((b & 0b11110000) == 0b11100000)
                {
                    if (i + 2 >= bytes.Length)
                    {
                        return false;
                    }

                    int b2 = bytes[i + 1];
                    if ((b2 & 0b11000000) != 0b10000000)
                    {
                        return false;
                    }

                    int b3 = bytes[i + 2];
                    if ((b3 & 0b11000000) != 0b10000000)
                    {
                        return false;
                    }

                    i += 2;
                    continue;
                }

                if ((b & 0b11111000) == 0b11110000)
                {
                    if (i + 3 >= bytes.Length)
                    {
                        return false;
                    }

                    int b2 = bytes[i + 1];
                    if ((b2 & 0b11000000) != 0b10000000)
                    {
                        return false;
                    }

                    int b3 = bytes[i + 2];
                    if ((b3 & 0b11000000) != 0b10000000)
                    {
                        return false;
                    }

                    int b4 = bytes[i + 3];
                    if ((b4 & 0b11000000) != 0b10000000)
                    {
                        return false;
                    }

                    i += 3;
                    continue;
                }
            }

            return true;
        }
    }
}

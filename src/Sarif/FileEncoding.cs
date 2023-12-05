// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class FileEncoding
    {
        private static Encoding Windows1252;

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
            Windows1252 = Windows1252 ?? Encoding.GetEncoding(1252);
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            
            if (start >= bytes.Length)
            {
                throw new ArgumentException($"Buffer size ({bytes.Length}) not valid for start ({start}) argument.");
            }

            foreach (Encoding encoding in new[] { Encoding.UTF8, Windows1252, Encoding.UTF32 })
            {
                using var reader = new StreamReader(new MemoryStream(bytes), encoding, detectEncodingFromByteOrderMarks: true);
                reader.BaseStream.Position = start;

                bool isTextual = true;

                for (int i = start; i < count; i++)
                {
                    int ch = reader.Read();

                    if (ch == -1)
                    {
                        break;
                    }

                    // Unicode REPLACEMENT CHARACTER (U+FFFD)
                    if (ch == 65533 || (reader.BaseStream.Position > 1 && ch == '\0'))
                    {
                        isTextual = false;
                        break;
                    }
                }

                if (isTextual)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

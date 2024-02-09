// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
        /// is to first examine the data for any control characters (<0x20). If none whatsoever
        /// are found, the data is classified as textual. Otherwise, if the inspected bytes
        /// decode successfully as either Unicode or UTF32, the data is classified as textual.
        /// This helper does not currently properly handled detected UnicodeBigEndian data.
        /// </summary>
        /// <param name="bytes">The raw data expressed as bytes.</param>
        /// <param name="start">The starting position to being classification.</param>
        /// <param name="count">The maximal count of bytes to decode.</param>
        public static bool IsTextualData(byte[] bytes, int start, int count)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));

            if (start >= bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start), $"Buffer size ({bytes.Length}) not valid for start ({start}) argument.");
            }

            // Ensure count % 4 == 0 to guarantee we do not attempt a misaligned decoding
            // at the end of the buffer, under all tested encodings.
            count = (count / 4) * 4;

            Windows1252 = Windows1252 ?? Encoding.GetEncoding(1252);

            bool containsControlCharacters = false;

            for (int i = 0; i < count; i++)
            {
                containsControlCharacters |= bytes[i] < 0x20;
            }

            if (!containsControlCharacters)
            {
                return true;
            }

            foreach (Encoding encoding in new[] { Encoding.UTF32, Encoding.Unicode })
            {
                bool encodingSucceeded = true;

                char[] chars = encoding.GetChars(bytes, start, count);
                foreach (char c in chars)
                {
                    if (c == 0xfffd)
                    {
                        encodingSucceeded = false;
                        break;
                    }
                }

                if (encodingSucceeded)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

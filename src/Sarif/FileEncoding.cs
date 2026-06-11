// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
        /// Detects if the provided byte array contains textual data, defined as the
        /// inverse of <see cref="IsBinaryData(byte[])"/>.
        /// </summary>
        /// <param name="bytes">The raw data expressed as bytes.</param>
        public static bool IsTextualData(byte[] bytes)
        {
            return !IsBinaryData(bytes);
        }

        /// <summary>
        /// Detects if the provided byte array contains textual data, defined as the
        /// inverse of <see cref="IsBinaryData(byte[], int, int)"/>.
        /// </summary>
        /// <param name="bytes">The raw data expressed as bytes.</param>
        /// <param name="start">The starting position to begin classification.</param>
        /// <param name="count">The maximal count of bytes to inspect.</param>
        public static bool IsTextualData(byte[] bytes, int start, int count)
        {
            return !IsBinaryData(bytes, start, count);
        }

        /// <summary>
        /// Detects if the provided byte array contains binary data.
        /// </summary>
        /// <param name="bytes">The raw data expressed as bytes.</param>
        public static bool IsBinaryData(byte[] bytes)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            return IsBinaryData(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Detects if the provided byte array contains binary data. The heuristic applied
        /// first checks for a Unicode byte-order mark (BOM) at the start of the inspected
        /// window; its presence classifies the data as textual (UTF-16/UTF-32 content is
        /// otherwise indistinguishable from binary by content alone, because its ASCII
        /// code points carry NUL padding bytes). Absent a BOM, the data is classified as
        /// binary if any NUL (0x00) byte occurs within the first 8000 bytes of the window.
        /// This is the same NUL-scan heuristic that Git and GitHub apply when distinguishing
        /// binary from textual content.
        /// </summary>
        /// <param name="bytes">The raw data expressed as bytes.</param>
        /// <param name="start">The starting position to begin classification.</param>
        /// <param name="count">The maximal count of bytes to inspect.</param>
        public static bool IsBinaryData(byte[] bytes, int start, int count)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));

            if (start >= bytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start), $"Buffer size ({bytes.Length}) not valid for start ({start}) argument.");
            }

            int available = Math.Min(count, bytes.Length - start);
            int scanLength = Math.Min(available, 8000);

            ReadOnlySpan<byte> scanRange = bytes.AsSpan(start, scanLength);

            if (StartsWithTextBom(scanRange))
            {
                return false;
            }

            return scanRange.IndexOf<byte>(0) != -1;
        }

        /// <summary>
        /// Detects a UTF-16 or UTF-32 byte-order mark at the start of the provided span.
        /// The four-byte UTF-32 BOMs are checked before the two-byte UTF-16 LE BOM because
        /// the UTF-32 LE BOM (FF FE 00 00) begins with the UTF-16 LE BOM (FF FE). The UTF-8
        /// BOM is intentionally not recognized here: UTF-8 text never contains a NUL byte,
        /// so the NUL scan already classifies it as textual without a BOM fast-path.
        /// </summary>
        private static bool StartsWithTextBom(ReadOnlySpan<byte> span)
        {
            if (span.Length >= 4)
            {
                if (span[0] == 0xFF && span[1] == 0xFE && span[2] == 0x00 && span[3] == 0x00) { return true; } // UTF-32 LE
                if (span[0] == 0x00 && span[1] == 0x00 && span[2] == 0xFE && span[3] == 0xFF) { return true; } // UTF-32 BE
            }

            if (span.Length >= 2)
            {
                if (span[0] == 0xFF && span[1] == 0xFE) { return true; } // UTF-16 LE
                if (span[0] == 0xFE && span[1] == 0xFF) { return true; } // UTF-16 BE
            }

            return false;
        }
    }
}

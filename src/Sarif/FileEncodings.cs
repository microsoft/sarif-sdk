// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class FileEncoding
    {
        /// <summary>
        public static bool CheckForTextualData(byte[] rawData, out Encoding encoding)
        {
            return CheckForTextualData(rawData, 0, rawData.Length, out encoding);
        }

        /// <summary>
        /// Detects if contains textual data.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        public static bool CheckForTextualData(byte[] rawData, int start, int count, out Encoding encoding)
        {
            encoding = null;

            string decoded = new StreamReader(new MemoryStream(rawData)).ReadToEnd();

            for (int i = 0; i < decoded.Length; i++)
            {
                if (decoded[i] == (char)65533)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

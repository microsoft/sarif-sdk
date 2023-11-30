#region *   License     *
/*
    SimpleHelpers - FileEncoding   

    Copyright © 2014 Khalid Salomão

    Permission is hereby granted, free of charge, to any person
    obtaining a copy of this software and associated documentation
    files (the “Software”), to deal in the Software without
    restriction, including without limitation the rights to use,
    copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the
    Software is furnished to do so, subject to the following
    conditions:

    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
    HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
    FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
    OTHER DEALINGS IN THE SOFTWARE. 

    License: http://www.opensource.org/licenses/mit-license.php
    Website: https://github.com/khalidsalomao/SimpleHelpers.Net
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class FileEncoding
    {
        /// <summary>
        /// Detects if contains textual data.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
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

            if (rawData.Length < count || count < 4 || start + 1 >= count)
            {
                return true;
            }

            if (CheckForByteOrderMark(rawData, out encoding, start))
            {
                return true;
            }

            // http://stackoverflow.com/questions/910873/how-can-i-determine-if-a-file-is-binary-or-text-in-c
            // http://www.gnu.org/software/diffutils/manual/html_node/Binary.html
            // count the number od null bytes sequences
            // considering only sequences of 2 0s: "\0\0" or control characters below 10
            int nullSequences = 0;
            int controlSequences = 0;
            for (int i = start + 1; i < count; i++)
            {
                if (rawData[i - 1] == 0 && rawData[i] == 0)
                {
                    if (++nullSequences > 1)
                    {
                        break;
                    }
                }
                else if (rawData[i - 1] == 0 && rawData[i] < 10)
                {
                    ++controlSequences;
                }
            }

            // is text if there is no null byte sequences or less than 10% of the buffer has control characters
            return nullSequences == 0 && (controlSequences <= (rawData.Length / 10));
        }

        /// <summary>
        /// Detects if data has bytes order mark to indicate its encoding for textual data.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        public static bool CheckForByteOrderMark(byte[] rawData, out Encoding encoding, int start = 0)
        {
            encoding = null;
            if (rawData.Length - start < 4)
            {
                return false;
            }

            // Detect encoding correctly (from Rick Strahl's blog)
            // http://www.west-wind.com/weblog/posts/2007/Nov/28/Detecting-Text-Encoding-for-StreamReader
            if (rawData[start] == 0xef && rawData[start + 1] == 0xbb && rawData[start + 2] == 0xbf)
            {
                encoding = Encoding.UTF8;
                return true;
            }
            else if (rawData[start] == 0xfe && rawData[start + 1] == 0xff)
            {
                encoding = Encoding.Unicode;
                return true;
            }
            else if (rawData[start] == 0 && rawData[start + 1] == 0 && rawData[start + 2] == 0xfe && rawData[start + 3] == 0xff)
            {
                encoding = Encoding.UTF32;
                return true;
            }
            else if (rawData[start] == 0x2b && rawData[start + 1] == 0x2f && rawData[start + 2] == 0x76)
            {
                encoding = Encoding.UTF7;
                return true;
            }

            return false;
        }
    }
}

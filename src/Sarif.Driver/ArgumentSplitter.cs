// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>This class contains argument splitting functionality.</summary>
    public static class ArgumentSplitter
    {
        private enum WhitespaceMode
        {
            Ignore,
            PartOfArgument,
            EndArgument
        }

        /// <summary>
        /// Mimics CommandLineToArgvW's argument splitting behavior, plus bug fixes.
        /// </summary>
        /// <param name="input">The command line to split into arguments.</param>
        /// <returns>The values of the arguments supplied in the input.</returns>
        public static List<string> CommandLineToArgvW(string input)
        {
            // This function mimics CommandLineToArgvW's escaping behavior, documented here:
            // http://msdn.microsoft.com/en-us/library/windows/desktop/bb776391.aspx

            //
            // We used to P/Invoke to the real CommandLineToArgvW, but re-implement it here
            // as a workaround for the following:
            // 
            // * CommandLineToArgvW does not treat newlines as whitespace (twcsec-tfs01 bug # 17291)
            // * CommandLineToArgvW returns the executable name for the empty string, not the empty set
            // * CommandLineToArgvW chokes on leading whitespace (twcsec-tfs01 bug# 17378)
            //
            // and as a result of the above we expect to find more nasty edge cases in the future.
            //

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            WhitespaceMode whitespaceMode = WhitespaceMode.Ignore;
            int slashCount = 0;

            var result = new List<string>();
            var sb = new StringBuilder();

            foreach (char c in input)
            {
                if (whitespaceMode == WhitespaceMode.Ignore && Char.IsWhiteSpace(c))
                {
                    // Purposely do nothing
                }
                else if (whitespaceMode == WhitespaceMode.EndArgument && Char.IsWhiteSpace(c))
                {
                    AddSlashes(sb, ref slashCount);
                    EmitArgument(result, sb);
                    whitespaceMode = WhitespaceMode.Ignore;
                }
                else if (c == '\\')
                {
                    ++slashCount;
                    if (whitespaceMode == WhitespaceMode.Ignore)
                    {
                        whitespaceMode = WhitespaceMode.EndArgument;
                    }
                }
                else if (c == '\"')
                {
                    bool quoteIsEscaped = (slashCount & 1) == 1;
                    slashCount >>= 1; // Using >> to avoid C# bankers rounding
                    // 2n backslashes followed by a quotation mark produce n slashes followed by a quotation mark
                    AddSlashes(sb, ref slashCount);

                    if (quoteIsEscaped)
                    {
                        sb.Append(c);
                    }
                    else if (whitespaceMode == WhitespaceMode.PartOfArgument)
                    {
                        whitespaceMode = WhitespaceMode.EndArgument;
                    }
                    else
                    {
                        whitespaceMode = WhitespaceMode.PartOfArgument;
                    }
                }
                else
                {
                    AddSlashes(sb, ref slashCount);
                    sb.Append(c);
                    if (whitespaceMode == WhitespaceMode.Ignore)
                    {
                        whitespaceMode = WhitespaceMode.EndArgument;
                    }
                }
            }

            AddSlashes(sb, ref slashCount);
            if (sb.Length != 0)
            {
                EmitArgument(result, sb);
            }

            return result;
        }

        private static void EmitArgument(List<string> result, StringBuilder sb)
        {
            result.Add(sb.ToString());
            sb.Clear();
        }

        private static void AddSlashes(StringBuilder sb, ref int slashCount)
        {
            sb.Append('\\', slashCount);
            slashCount = 0;
        }
    }
}

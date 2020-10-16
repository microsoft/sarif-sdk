// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public abstract class LogReader<TLog> where TLog : class
    {
        public TLog ReadLog(string input)
        {
            return ReadLog(input, Encoding.UTF8);
        }

        public TLog ReadLog(string input, Encoding encoding)
        {
            return ReadLog(new MemoryStream(encoding.GetBytes(input)));
        }

        public abstract TLog ReadLog(Stream input);
    }
}
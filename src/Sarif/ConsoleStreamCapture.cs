// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ConsoleStreamCapture : IConsoleCapture
    {
        private StringBuilder sb;
        private StreamReader reader;

        public string Text => sb?.ToString();

        public Task<string> Capture(StreamReader reader, CancellationToken cancellationToken)
        {
            this.reader = reader;
            return Task.Run(ReadAll, cancellationToken);
        }

        private string ReadAll()
        {
            this.sb ??= new StringBuilder();
            sb.Length = 0;

            int ch;
            while ((ch = reader.Read()) != -1)
            {
                sb.Append((char)ch);
            }

            return sb.ToString();
        }
    }
}

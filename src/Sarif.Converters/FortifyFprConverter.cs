// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyFprConverter : ToolFileConverterBase
    {
        /// <summary>Initializes a new instance of the <see cref="FortifyFprConverter"/> class.</summary>
        public FortifyFprConverter()
        {
        }

        /// <summary>
        /// Interface implementation for converting a stream in Fortify FPR format to a stream in
        /// SARIF format.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="input">Stream in Fortify FPR format.</param>
        /// <param name="output">Stream in SARIF format.</param>
        public override void Convert(Stream input, IResultLogWriter output)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var tool = new Tool
            {
                Name = "Fortify"
            };


            ReadFpr(input);
            output.Initialize(id: null, correlationId: null);

            output.WriteTool(tool);

            output.OpenResults();
            output.CloseResults();
        }

        private void ReadFpr(Stream input)
        {
            using (ZipArchive fprArchive = new ZipArchive(input))
            {
                using (Stream auditStream = OpenAuditStream(fprArchive))
                {
                    Console.WriteLine("CanRead: " + auditStream.CanRead);
                }
            }
        }

        private static Stream OpenAuditStream(ZipArchive fprArchive)
        {
            ZipArchiveEntry auditEntry = fprArchive.Entries.Single(e => e.FullName.Equals("audit.fvdl"));
            return auditEntry.Open();
        }
    }
}

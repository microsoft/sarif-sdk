// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyFprConverter : ToolFileConverterBase
    {
        private readonly NameTable _nameTable;
        private readonly FortifyFprStrings _strings;
        private Invocation _invocation;

        /// <summary>Initializes a new instance of the <see cref="FortifyFprConverter"/> class.</summary>
        public FortifyFprConverter()
        {
            _nameTable = new NameTable();
            _strings = new FortifyFprStrings(_nameTable);
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

            _invocation = new Invocation();

            ParseFprFile(input);

            output.Initialize(id: null, correlationId: null);

            output.WriteTool(tool);
            output.WriteInvocation(_invocation);

            output.OpenResults();
            output.CloseResults();
        }

        private void ParseFprFile(Stream input)
        {
            using (ZipArchive fprArchive = new ZipArchive(input))
            {
                using (Stream auditStream = OpenAuditStream(fprArchive))
                {
                    ParseAuditStream(auditStream);
                }
            }
        }

        private static Stream OpenAuditStream(ZipArchive fprArchive)
        {
            ZipArchiveEntry auditEntry = fprArchive.Entries.Single(e => e.FullName.Equals("audit.fvdl"));
            return auditEntry.Open();
        }

        private void ParseAuditStream(Stream auditStream)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreWhitespace = true,
                NameTable = _nameTable,
                XmlResolver = null
            };

            using (XmlReader reader = XmlReader.Create(auditStream, settings))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (Ref.Equal(reader.LocalName, _strings.CommandLine))
                        {
                            ParseCommandLineArguments(reader);
                        }
                        else if (Ref.Equal(reader.LocalName, _strings.MachineInfo))
                        {
                            ParseMachineInfo(reader);
                        }
                    }
                }
            }
        }

        private void ParseCommandLineArguments(XmlReader reader)
        {
            var sb = new StringBuilder();
            reader.MoveToElement();
            reader.Read();
            while (!reader.EOF && Ref.Equal(reader.LocalName, _strings.Argument))
            {
                string argument = reader.ReadElementContentAsString();
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(argument);
                reader.MoveToElement();
            }

            _invocation.CommandLine = sb.ToString();
        }

        private void ParseMachineInfo(XmlReader reader)
        {
            reader.Read();
            while (!reader.EOF && !Ref.Equal(reader.LocalName, _strings.MachineInfo))
            {
                if (Ref.Equal(reader.LocalName, _strings.Hostname))
                {
                    _invocation.Machine = reader.ReadElementContentAsString();
                }
                else if (Ref.Equal(reader.LocalName, _strings.Username))
                {
                    _invocation.Account = reader.ReadElementContentAsString();
                }
                else
                {
                    reader.Read();
                }
            }
        }
    }
}

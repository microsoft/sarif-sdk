// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyFprConverter : ToolFileConverterBase
    {
        private const string ErrorCodePrefix = "FPR";

        private readonly NameTable _nameTable;
        private readonly FortifyFprStrings _strings;

        private XmlReader _reader;
        private Invocation _invocation;
        private string _automationId;
        private List<Notification> _toolNotifications = new List<Notification>();

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
            _toolNotifications.Clear();

            ParseFprFile(input);

            output.Initialize(id: null, automationId: _automationId);

            output.WriteTool(tool);
            output.WriteInvocation(_invocation);

            output.OpenResults();
            output.CloseResults();

            if (_toolNotifications.Any())
            {
                output.WriteToolNotifications(_toolNotifications);
            }
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

            using (_reader = XmlReader.Create(auditStream, settings))
            {
                while (_reader.Read())
                {
                    if (_reader.IsStartElement())
                    {
                        if (Ref.Equal(_reader.LocalName, _strings.Build))
                        {
                            ParseBuild();
                        }
                        else if (Ref.Equal(_reader.LocalName, _strings.CommandLine))
                        {
                            ParseCommandLineArguments();
                        }
                        else if (Ref.Equal(_reader.LocalName, _strings.Errors))
                        {
                            ParseErrors();
                        }
                        else if (Ref.Equal(_reader.LocalName, _strings.MachineInfo))
                        {
                            ParseMachineInfo();
                        }
                    }
                }
            }
        }

        private void ParseBuild()
        {
            _reader.Read();
            while (!_reader.EOF && !Ref.Equal(_reader.LocalName, _strings.Build))
            {
                if (Ref.Equal(_reader.LocalName, _strings.BuildId))
                {
                    _automationId = _reader.ReadElementContentAsString();
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private void ParseCommandLineArguments()
        {
            var sb = new StringBuilder();
            _reader.Read();
            while (!_reader.EOF && Ref.Equal(_reader.LocalName, _strings.Argument))
            {
                string argument = _reader.ReadElementContentAsString();
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(argument);
                _reader.MoveToElement();
            }

            _invocation.CommandLine = sb.ToString();
        }

        private void ParseErrors()
        {
            _reader.Read();
            while (!_reader.EOF && !Ref.Equal(_reader.LocalName, _strings.Errors))
            {
                if (Ref.Equal(_reader.LocalName, _strings.Error))
                {
                    string errorCode = _reader.GetAttribute(_strings.Code);
                    string message = _reader.ReadElementContentAsString();

                    _toolNotifications.Add(new Notification
                    {
                        Id = ErrorCodePrefix + errorCode,
                        Level = NotificationLevel.Error,
                        Message = message
                    });
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private void ParseMachineInfo()
        {
            _reader.Read();
            while (!_reader.EOF && !Ref.Equal(_reader.LocalName, _strings.MachineInfo))
            {
                if (Ref.Equal(_reader.LocalName, _strings.Hostname))
                {
                    _invocation.Machine = _reader.ReadElementContentAsString();
                }
                else if (Ref.Equal(_reader.LocalName, _strings.Username))
                {
                    _invocation.Account = _reader.ReadElementContentAsString();
                }
                else
                {
                    _reader.Read();
                }
            }
        }
    }
}

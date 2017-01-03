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
        private const string FortifyExecutable = "[REMOVED]insourceanalyzer.exe";

        private readonly NameTable _nameTable;
        private readonly FortifyFprStrings _strings;

        private XmlReader _reader;
        private Invocation _invocation;
        private string _automationId;
        private List<Result> _results = new List<Result>();
        private List<Notification> _toolNotifications = new List<Notification>();
        private Dictionary<string, FileData> _fileDictionary = new Dictionary<string, FileData>();
        private Dictionary<string, string> _classToMessageDictionary = new Dictionary<string, string>();

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
            _results.Clear();
            _toolNotifications.Clear();
            _fileDictionary.Clear();
            _classToMessageDictionary.Clear();

            ParseFprFile(input);
            AddMessagesToResults();

            output.Initialize(id: null, automationId: _automationId);

            output.WriteTool(tool);
            output.WriteInvocation(_invocation);

            if (_fileDictionary.Any())
            {
                output.WriteFiles(_fileDictionary);
            }

            output.OpenResults();
            output.WriteResults(_results);
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
                    if (AtStartOfNonEmpty(_strings.Build))
                    {
                        ParseBuild();
                    }
                    else if (AtStartOfNonEmpty(_strings.Vulnerabilities))
                    {
                        ParseVulnerabilities();
                    }
                    else if (AtStartOfNonEmpty(_strings.Description))
                    {
                        ParseDescription();
                    }
                    else if (AtStartOfNonEmpty(_strings.CommandLine))
                    {
                        ParseCommandLine();
                    }
                    else if (AtStartOfNonEmpty(_strings.Errors))
                    {
                        ParseErrors();
                    }
                    else if (AtStartOfNonEmpty(_strings.MachineInfo))
                    {
                        ParseMachineInfo();
                    }
                }
            }
        }

        private void ParseBuild()
        {
            _reader.Read();
            while (!AtEndOf(_strings.Build))
            {
                if (AtStartOfNonEmpty(_strings.BuildId))
                {
                    _automationId = _reader.ReadElementContentAsString();
                }
                else if (AtStartOfNonEmpty(_strings.SourceFiles))
                {
                    ParseSourceFiles();
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private void ParseSourceFiles()
        {
            _reader.Read();
            while (!AtEndOf(_strings.SourceFiles))
            {
                if (AtStartOfNonEmpty(_strings.File))
                {
                    ParseFile();
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private void ParseFile()
        {
            string fileType = _reader.GetAttribute(_strings.TypeAttribute);
            int length = 0;
            string sizeAttribute = _reader.GetAttribute(_strings.SizeAttribute);
            if (sizeAttribute != null)
            {
                if (!int.TryParse(sizeAttribute, out length))
                {
                    length = 0;
                }
            }

            string fileName = null;
            _reader.Read();
            while (!AtEndOf(_strings.File))
            {
                if (AtStartOfNonEmpty(_strings.Name))
                {
                    fileName = _reader.ReadElementContentAsString();
                }
                else
                {
                    _reader.Read();
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                _fileDictionary.Add(
                    fileName,
                    new FileData
                    {
                        MimeType = FprTypeToMimeType(fileType),
                        Length = length
                    });
            }
        }

        private string FprTypeToMimeType(string fileType)
        {
            switch (fileType)
            {
                case "xml":
                    return "text/xml";

                case "tsql":
                    return "text/x-sql";

                default:
                    return "unknown/" + fileType;
            }
        }

        private void ParseVulnerabilities()
        {
            _reader.Read();
            while (!AtEndOf(_strings.Vulnerabilities))
            {
                if (AtStartOfNonEmpty(_strings.Vulnerability))
                {
                    ParseVulnerability();
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private void ParseVulnerability()
        {
            var result = new Result();
            _reader.Read();
            while (!AtEndOf(_strings.Vulnerability))
            {
                if (AtStartOfNonEmpty(_strings.ClassId))
                {
                    result.RuleId = _reader.ReadElementContentAsString();
                }

                _reader.Read();
            }

            _results.Add(result);
        }

        private void ParseDescription()
        {
            string classId = _reader.GetAttribute(_strings.ClassIdAttribute);
            string @abstract = null;
            _reader.Read();
            while (!AtEndOf(_strings.Description))
            {
                if (AtStartOfNonEmpty(_strings.Abstract))
                {
                    @abstract = _reader.ReadElementContentAsString();
                }
                else
                {
                    _reader.Read();
                }
            }

            _classToMessageDictionary.Add(classId, @abstract);
        }

        private void ParseCommandLine()
        {
            var sb = new StringBuilder(FortifyExecutable);
            _reader.Read();
            while (!AtEndOf(_strings.CommandLine))
            {
                if (AtStartOfNonEmpty(_strings.Argument))
                {
                    string argument = _reader.ReadElementContentAsString();
                    sb.Append(' ');
                    sb.Append(argument);
                    _reader.MoveToElement();
                }
                else
                {
                    _reader.Read();
                }
            }

            _invocation.CommandLine = sb.ToString();
        }

        private void ParseErrors()
        {
            _reader.Read();
            while (!AtEndOf(_strings.Errors))
            {
                if (AtStartOfNonEmpty(_strings.Error))
                {
                    string errorCode = _reader.GetAttribute(_strings.CodeAttribute);
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
            while (!AtEndOf(_strings.MachineInfo))
            {
                if (AtStartOfNonEmpty(_strings.Hostname))
                {
                    _invocation.Machine = _reader.ReadElementContentAsString();
                }
                else if (AtStartOfNonEmpty(_strings.Username))
                {
                    _invocation.Account = _reader.ReadElementContentAsString();
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private bool AtStartOfNonEmpty(String elementName)
        {
            return AtStartOf(elementName) && !_reader.IsEmptyElement;
        }

        private bool AtStartOf(string elementName)
        {
            return !_reader.EOF &&
                (_reader.NodeType == XmlNodeType.Element && Ref.Equal(_reader.LocalName, elementName));
        }

        private bool AtEndOf(string elementName)
        {
            return _reader.EOF ||
                (_reader.NodeType == XmlNodeType.EndElement && Ref.Equal(_reader.LocalName, elementName));
        }

        private void AddMessagesToResults()
        {
            foreach (Result result in _results)
            {
                string message;
                if (_classToMessageDictionary.TryGetValue(result.RuleId, out message))
                {
                    result.Message = message;
                }
            }
        }
    }
}

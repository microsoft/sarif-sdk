// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class CppCheckConverter : ToolFileConverterBase
    {
        private readonly NameTable _nameTable;
        private readonly CppCheckStrings _strings;

        /// <summary>Initializes a new instance of the <see cref="CppCheckConverter"/> class.</summary>
        public CppCheckConverter()
        {
            _nameTable = new NameTable();
            _strings = new CppCheckStrings(_nameTable);
        }

        /// <summary>
        /// Interface implementation that takes a CppChecker log stream and converts its data to a SARIF json stream.
        /// Read in CppChecker data from an input stream and write Result objects.
        /// </summary>
        /// <param name="input">Stream of a CppChecker log</param>
        /// <param name="output">SARIF json stream of the converted CppChecker log</param>
        /// <param name="loggingOptions">Logging options that configure output.</param>
        public override void Convert(Stream input, IResultLogWriter output, LoggingOptions loggingOptions)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Ignore,
                NameTable = _nameTable,
                XmlResolver = null
            };

            using (XmlReader xmlReader = XmlReader.Create(input, settings))
            {
                ProcessCppCheckLog(xmlReader, output, loggingOptions);
            }
        }

        private void ProcessCppCheckLog(XmlReader reader, IResultLogWriter output, LoggingOptions loggingOptions)
        {
            reader.ReadStartElement(_strings.Results);

            if (!StringReference.AreEqual(reader.LocalName, _strings.CppCheck))
            {
                throw reader.CreateException(ConverterResources.CppCheckCppCheckElementMissing);
            }

            string version = reader.GetAttribute(_strings.Version);

            if (version != null && !version.IsSemanticVersioningCompatible())
            {
                // This logic only fixes up simple cases, such as being passed
                // 1.66, where Semantic Versioning 2.0 requires 1.66.0. Also
                // strips Revision member if passed a complete .NET version.
                Version dotNetVersion;
                if (Version.TryParse(version, out dotNetVersion))
                {
                    version = 
                        Math.Max(0, dotNetVersion.Major) + "." + 
                        Math.Max(0, dotNetVersion.Minor) + "." + 
                        Math.Max(0, dotNetVersion.Build);
                }
            }

            if (String.IsNullOrWhiteSpace(version))
            {
                throw reader.CreateException(ConverterResources.CppCheckCppCheckElementMissing);
            }

            reader.Skip(); // <cppcheck />

            if (!StringReference.AreEqual(reader.LocalName, _strings.Errors))
            {
                throw reader.CreateException(ConverterResources.CppCheckErrorsElementMissing);
            }

            var results = new List<Result>();
            if (reader.IsEmptyElement)
            {
                reader.Skip(); // <errors />
            }
            else
            {
                int errorsDepth = reader.Depth;
                reader.Read(); // <errors>

                while (reader.Depth > errorsDepth)
                {
                    var parsedError = CppCheckError.Parse(reader, _strings);
                    results.Add(parsedError.ToSarifIssue());
                }

                reader.ReadEndElement(); // </errors>
            }

            reader.ReadEndElement(); // </results>

            var tool = new Tool
            {
                Name = "CppCheck",
                Version = version,
            };

            var fileInfoFactory = new FileInfoFactory(uri => MimeType.Cpp, loggingOptions);
            Dictionary<string, FileData> fileDictionary = fileInfoFactory.Create(results);

            output.Initialize(id: null, automationId: null);

            output.WriteTool(tool);
            if (fileDictionary != null && fileDictionary.Count > 0) { output.WriteFiles(fileDictionary); }

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();
        }
    }
}

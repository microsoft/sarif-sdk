// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Xml;
using Microsoft.CodeAnalysis.Driver;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Converters
{
    internal class CppCheckConverter : IToolFileConverter
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
        /// Interface implementation that takes a CppChecker log stream and converts its data to a OES json format stream.
        /// Read in CppChecker data from an input stream and write Result objects.
        /// </summary>
        /// <param name="input">Stream of a CppChecker log</param>
        /// <param name="output">OES json stream of the converted CppChecker log</param>
        public void Convert(Stream input, IResultLogWriter output)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Ignore,
                NameTable = _nameTable
            };

            using (XmlReader xmlReader = XmlReader.Create(input, settings))
            {
                ProcessCppCheckLog(xmlReader, output);
            }
        }

        private void ProcessCppCheckLog(XmlReader reader, IResultLogWriter issueWriter)
        {
            reader.ReadStartElement(_strings.Results);

            if (!Ref.Equal(reader.LocalName, _strings.CppCheck))
            {
                throw reader.CreateException(SarifResources.CppCheckCppCheckElementMissing);
            }

            string version = reader.GetAttribute(_strings.Version);
            if (String.IsNullOrWhiteSpace(version))
            {
                throw reader.CreateException(SarifResources.CppCheckCppCheckElementMissing);
            }

            issueWriter.WriteToolAndRunInfo(new ToolInfo
            {
                Name = "CppCheck",
                Version = Version.Parse(version)
            }, null);

            reader.Skip(); // <cppcheck />

            if (!Ref.Equal(reader.LocalName, _strings.Errors))
            {
                throw reader.CreateException(SarifResources.CppCheckErrorsElementMissing);
            }

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
                    issueWriter.WriteResult(parsedError.ToSarifIssue());
                }

                reader.ReadEndElement(); // </errors>
            }

            reader.ReadEndElement(); // </results>
        }
    }
}

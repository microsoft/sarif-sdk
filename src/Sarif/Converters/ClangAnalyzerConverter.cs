﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class ClangAnalyzerConverter : IToolFileConverter
    {
        private IList<object> _files = null;

        /// <summary>Convert a Clang plist report into the SARIF format.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="input">CLang log file stream.</param>
        /// <param name="output">Result log writer.</param>
        public void Convert(Stream input, IResultLogWriter output)
        {
            // ToDo remove this comment after all issues are resolved.
            // Rodney is tasked with bringing Clang analyzer results into the SARIF fold.
            // Once this work is complete, he can close the following task:
            // http://twcsec-tfs01:8080/tfs/DefaultCollection/SecDevTools/_workitems#_a=edit&id=13409
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }


            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                settings.DtdProcessing = DtdProcessing.Ignore;

                ToolInfo toolInfo = new ToolInfo();
                toolInfo.Name = "Clang";
                output.WriteToolAndRunInfo(toolInfo, null);

                using (XmlReader xmlReader = XmlReader.Create(input, settings))
                {
                    XmlNodeType nodeType = xmlReader.MoveToContent();
                    xmlReader.ReadStartElement(ClangSchemaStrings.PlistName);
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        using (System.Object pListReader = xmlReader.ReadSubtree())
                        {
                            this.ReadPlist(pListReader, output);
                        }
                    }
                }
            }
            finally
            {
                _files = null;
            }
        }

        private static IDictionary<string, object> FindDictionary(IDictionary<string, object> dictionary, string key)
        {
            object getObject;
            Dictionary<string, object> value = null;

            if (dictionary.TryGetValue(key, out getObject))
            {
                value = getObject as Dictionary<string, object>;
            }

            return value ?? new Dictionary<string, object>();
        }

        private static int FindInt(IDictionary<string, object> dictionary, string key)
        {
            object getObject;
            string value = null;
            int returnValue = 0;

            if (dictionary.TryGetValue(key, out getObject))
            {
                value = getObject as string;
                if (!int.TryParse(value, out returnValue))
                {
                    throw new InvalidDataException("Expected an int value for " + key + " found : " + value);
                }
            }

            return returnValue;
        }

        private static string FindString(IDictionary<string, object> dictionary, string key)
        {
            object getObject;
            string value = null;

            if (dictionary.TryGetValue(key, out getObject))
            {
                value = getObject as string;
            }

            return value ?? string.Empty;
        }

        private void LogIssue(IDictionary<string, object> issueData, IResultLogWriter output)
        {
            if (issueData != null)
            {
                string description = FindString(issueData, "description");
                string category = FindString(issueData, "category");
                string issueType = FindString(issueData, "type");
                string issueContextKind = FindString(issueData, "issue_context_kind");
                string issueContext = FindString(issueData, "issue_context");
                string issueHash = FindString(issueData, "issue_hash");
                int issueLine = 0;
                int issueColumn = 0;
                string fileName = null;

                IDictionary<string, object> location = FindDictionary(issueData, "location");
                if (location != null)
                {
                    issueLine = FindInt(location, "line");
                    issueColumn = FindInt(location, "col");
                    int fileNumber = FindInt(location, "file");
                    if (_files != null && fileNumber < _files.Count)
                    {
                        fileName = _files[fileNumber] as string;
                    }
                }

                Result result = new Result
                {
                    FullMessage = category + " : " + description,
                    ShortMessage = issueType,
                    Locations = new[]
                    {
                        new Location
                        {
                            AnalysisTarget = new[]
                            {
                                new PhysicalLocationComponent
                                {
                                    Uri = new Uri(fileName, UriKind.RelativeOrAbsolute),
                                    MimeType = MimeType.DetermineFromFileExtension(fileName),
                                    Region = new Region()
                                    {
                                        StartLine = issueLine,
                                        StartColumn = issueColumn
                                    }
                                }
                            }
                        }
                    }
                };

                output.WriteResult(result);
            }
        }

        private static IList<object> ReadArray(XmlReader xmlReader)
        {
            List<object> list = new List<object>();
            bool readerMoved = false; // ReadElementContentAsString moves the reader so prevent double moves.

            xmlReader.Read(); // Read past the "array" element start.

            while (readerMoved || xmlReader.Read())
            {
                readerMoved = false;

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {
                        case ClangSchemaStrings.StringName:
                        case ClangSchemaStrings.IntegerName:
                        case ClangSchemaStrings.RealName:
                        case ClangSchemaStrings.DataName:
                        case ClangSchemaStrings.DateName:
                            {
                                string value = xmlReader.ReadElementContentAsString();
                                readerMoved = true;
                                list.Add(value);
                                break;
                            }

                        case ClangSchemaStrings.ArrayName:
                            {
                                using (System.Object subTreeReader = xmlReader.ReadSubtree())
                                {
                                    IList<object> array = ReadArray(subTreeReader);
                                    list.Add(array);
                                }
                                break;
                            }

                        case ClangSchemaStrings.DictionaryName:
                            {
                                using (System.Object subTreeReader = xmlReader.ReadSubtree())
                                {
                                    IDictionary<string, object> dictionary = ReadDictionary(subTreeReader);
                                    list.Add(dictionary);
                                }
                                break;
                            }
                    }
                }

                if (xmlReader.NodeType == XmlNodeType.EndElement && (xmlReader.Name == ClangSchemaStrings.ArrayName))
                {
                    break;
                }
            }

            return list;
        }

        private static IDictionary<string, object> ReadDictionary(XmlReader xmlReader)
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            string keyName = string.Empty;
            bool readerMoved = false;       // ReadElementContentAsString reads to next element

            xmlReader.Read();               // read past the dictionary element;
            while (readerMoved || xmlReader.Read())
            {
                readerMoved = false;

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {
                        case ClangSchemaStrings.KeyName:
                            {
                                keyName = xmlReader.ReadElementContentAsString();
                                readerMoved = true;
                                break;
                            }

                        case ClangSchemaStrings.StringName:
                        case ClangSchemaStrings.IntegerName:
                        case ClangSchemaStrings.RealName:
                        case ClangSchemaStrings.DataName:
                        case ClangSchemaStrings.DateName:
                            {
                                if (string.IsNullOrEmpty(keyName))
                                {
                                    throw new InvalidDataException("Expected key value before dictionary data.");
                                }

                                string value = xmlReader.ReadElementContentAsString();
                                readerMoved = true;
                                dictionary.Add(keyName, value);
                                keyName = string.Empty;
                                break;
                            }

                        case ClangSchemaStrings.ArrayName:
                            {
                                if (string.IsNullOrEmpty(keyName))
                                {
                                    throw new InvalidDataException("Expected key value before dictionary data.");
                                }

                                using (System.Object subTreeReader = xmlReader.ReadSubtree())
                                {
                                    IList<object> array = ReadArray(subTreeReader);
                                    dictionary.Add(keyName, array);
                                    keyName = string.Empty;
                                }
                                break;
                            }

                        case ClangSchemaStrings.DictionaryName:
                            {
                                if (string.IsNullOrEmpty(keyName))
                                {
                                    throw new InvalidDataException("Expected key value before dictionary data.");
                                }

                                using (System.Object subTreeReader = xmlReader.ReadSubtree())
                                {
                                    IDictionary<string, object> child = ReadDictionary(subTreeReader);
                                    dictionary.Add(keyName, child);
                                    keyName = string.Empty;
                                }
                                break;
                            }
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == ClangSchemaStrings.DictionaryName)
                {
                    break;
                }
            }

            return dictionary;
        }

        private void ReadPlistDictionary(XmlReader xmlReader, IResultLogWriter output)
        {
            string keyName = string.Empty;
            bool readerMoved = false;       // ReadElementContentAsString reads to next element

            xmlReader.Read();               // read past the dictionary element;
            while (readerMoved || xmlReader.Read())
            {
                readerMoved = false;

                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {
                        case ClangSchemaStrings.KeyName:
                            {
                                keyName = xmlReader.ReadElementContentAsString();
                                readerMoved = true;
                                break;
                            }

                        case ClangSchemaStrings.StringName:
                            {
                                if (string.IsNullOrEmpty(keyName))
                                {
                                    throw new InvalidDataException("Expected key value before dictionary data.");
                                }

                                string value = xmlReader.ReadElementContentAsString();
                                readerMoved = true;
                                keyName = string.Empty;
                                break;
                            }

                        case ClangSchemaStrings.ArrayName:
                            {
                                if (string.IsNullOrEmpty(keyName))
                                {
                                    throw new InvalidDataException("Expected key value before dictionary data.");
                                }

                                using (System.Object subTreeReader = xmlReader.ReadSubtree())
                                {
                                    if (keyName.Equals("files"))
                                    {
                                        _files = ReadArray(subTreeReader);
                                    }

                                    if (keyName.Equals("diagnostics"))
                                    {
                                        ReadDiagnostics(subTreeReader, output);
                                    }

                                    keyName = string.Empty;
                                }
                                break;
                            }
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == ClangSchemaStrings.DictionaryName)
                {
                    break;
                }
            }
        }

        private void ReadDiagnostics(XmlReader xmlReader, IResultLogWriter output)
        {
            xmlReader.Read(); // Read past the "array" element start.

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name.Equals(ClangSchemaStrings.DictionaryName))
                    {
                        using (System.Object subTreeReader = xmlReader.ReadSubtree())
                        {
                            IDictionary<string, object> dictionary = ReadDictionary(subTreeReader);
                            this.LogIssue(dictionary, output);
                        }
                    }
                }

                if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == ClangSchemaStrings.ArrayName)
                {
                    break;
                }
            }
        }

        private void ReadPlist(XmlReader xmlReader, IResultLogWriter output)
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name.Equals(ClangSchemaStrings.DictionaryName))
                    {
                        using (System.Object subTreeReader = xmlReader.ReadSubtree())
                        {
                            this.ReadPlistDictionary(subTreeReader, output);
                        }
                    }
                }
            }
        }

        private static class ClangSchemaStrings
        {
            public const string ArrayName = "array";
            public const string DataName = "data";
            public const string DateName = "date";
            public const string DictionaryName = "dict";
            public const string IntegerName = "integer";
            public const string KeyName = "key";
            public const string PlistName = "plist";
            public const string RealName = "real";
            public const string StringName = "string";
            public const string VersionName = "version";
        }
    }
}
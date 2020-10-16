// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class ClangAnalyzerConverter : ToolFileConverterBase
    {
        private IList<object> _files = null;

        public override string ToolName => "Clang Analyzer";

        /// <summary>Convert a Clang plist report into the SARIF format.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="input">CLang log file stream.</param>
        /// <param name="output">Result log writer.</param>
        /// <param name="dataToInsert">Optionally emitted properties that should be written to log.</param>
        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            // ToDo remove this comment after all issues are resolved.
            // Rodney is tasked with bringing Clang analyzer results into the SARIF fold.
            // Once this work is complete, he can close the following task:
            // http://twcsec-tfs01:8080/tfs/DefaultCollection/SecDevTools/_workitems#_a=edit&id=13409
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            try
            {
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    IgnoreWhitespace = true,
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null
                };

                var results = new List<Result>();

                using (XmlReader xmlReader = XmlReader.Create(input, settings))
                {
                    xmlReader.MoveToContent();
                    xmlReader.ReadStartElement(ClangSchemaStrings.PlistName);
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        using (XmlReader pListReader = xmlReader.ReadSubtree())
                        {
                            this.ReadPlist(pListReader, results);
                        }
                    }
                }

                PersistResults(output, results);
            }
            finally
            {
                _files = null;
            }
        }

        private static IDictionary<string, object> FindDictionary(IDictionary<string, object> dictionary, string key)
        {
            Dictionary<string, object> value = null;

            if (dictionary.TryGetValue(key, out object getObject))
            {
                value = getObject as Dictionary<string, object>;
            }

            return value ?? new Dictionary<string, object>();
        }

        private static int FindInt(IDictionary<string, object> dictionary, string key)
        {
            string value = null;
            int returnValue = 0;

            if (dictionary.TryGetValue(key, out object getObject))
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
            string value = null;

            if (dictionary.TryGetValue(key, out object getObject))
            {
                value = getObject as string;
            }

            return value ?? string.Empty;
        }

        private Result CreateResult(IDictionary<string, object> issueData)
        {
            if (issueData != null)
            {
                // Used for Result.FullMessage 
                string description = FindString(issueData, "description");

                // Used as rule id. 
                string issueType = FindString(issueData, "type");

                // This data persisted to result property bag
                string category = FindString(issueData, "category");
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
                        fileName = (string)_files[fileNumber];
                    }
                }

                var result = new Result
                {
                    RuleId = issueType,
                    Message = new Message { Text = description },
                    Locations = new List<Location>
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    Uri = new Uri(fileName, UriKind.RelativeOrAbsolute)
                                },
                                Region = new Region()
                                {
                                    StartLine = issueLine,
                                    StartColumn = issueColumn
                                }
                            }
                        }
                    },
                };

                result.SetProperty("category", category);
                result.SetProperty("issue_context_kind", issueContextKind);
                result.SetProperty("issueContext", issueContext);
                result.SetProperty("issueHash", issueHash);

                return result;
            }
            else
            {
                return null;
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
                            using (XmlReader subTreeReader = xmlReader.ReadSubtree())
                            {
                                IList<object> array = ReadArray(subTreeReader);
                                list.Add(array);
                            }
                            break;
                        }

                        case ClangSchemaStrings.DictionaryName:
                        {
                            using (XmlReader subTreeReader = xmlReader.ReadSubtree())
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

                            using (XmlReader subTreeReader = xmlReader.ReadSubtree())
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

                            using (XmlReader subTreeReader = xmlReader.ReadSubtree())
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

        private void ReadPlistDictionary(XmlReader xmlReader, IList<Result> results)
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

                            xmlReader.ReadElementContentAsString();
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

                            using (XmlReader subTreeReader = xmlReader.ReadSubtree())
                            {
                                if (keyName.Equals("files"))
                                {
                                    _files = ReadArray(subTreeReader);
                                }

                                if (keyName.Equals("diagnostics"))
                                {
                                    ReadDiagnostics(subTreeReader, results);
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

        private void ReadDiagnostics(XmlReader xmlReader, IList<Result> results)
        {
            xmlReader.Read(); // Read past the "array" element start.

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name.Equals(ClangSchemaStrings.DictionaryName))
                    {
                        using (XmlReader subTreeReader = xmlReader.ReadSubtree())
                        {
                            IDictionary<string, object> dictionary = ReadDictionary(subTreeReader);
                            Result result = this.CreateResult(dictionary);
                            if (result != null)
                            {
                                results.Add(result);
                            }
                        }
                    }
                }

                if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == ClangSchemaStrings.ArrayName)
                {
                    break;
                }
            }
        }

        private void ReadPlist(XmlReader xmlReader, IList<Result> results)
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name.Equals(ClangSchemaStrings.DictionaryName))
                    {
                        using (XmlReader subTreeReader = xmlReader.ReadSubtree())
                        {
                            this.ReadPlistDictionary(subTreeReader, results);
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
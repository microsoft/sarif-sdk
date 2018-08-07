﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyFprConverter : ToolFileConverterBase
    {
        private const string FortifyToolName = "HP Fortify Static Code Analyzer";
        private const string FortifyExecutable = "[REMOVED]insourceanalyzer.exe";
        private const string ReplacementTokenFormat = "<Replace key=\"{0}\"/>";
        private const string EmbeddedLinkFormat = "[{0}](1)";

        private readonly NameTable _nameTable;
        private readonly FortifyFprStrings _strings;
        private readonly string[] SupportedReplacementTokens = new[] { "PrimaryLocation.file", "PrimaryLocation.line" };

        private XmlReader _reader;
        private Invocation _invocation;
        private string _runId;
        private string _automationId;
        private List<Result> _results = new List<Result>();
        private List<Notification> _toolNotifications;
        private Dictionary<string, FileData> _fileDictionary;
        private Dictionary<string, IRule> _ruleDictionary;
        private Dictionary<ThreadFlowLocation, string> _tflToNodeIdDictionary;
        private Dictionary<ThreadFlowLocation, string> _tflToSnippetIdDictionary;
        private Dictionary<Location, string> _locationToSnippetIdDictionary;
        private Dictionary<Result, string> _resultToSnippetIdDictionary;
        private Dictionary<Result, Dictionary<string, string>> _resultToReplacementDefinitionDictionary;
        private Dictionary<string, Location> _nodeIdToLocationDictionary;
        private Dictionary<string, Region> _snippetIdToRegionDictionary;

        /// <summary>Initializes a new instance of the <see cref="FortifyFprConverter"/> class.</summary>
        public FortifyFprConverter()
        {
            _nameTable = new NameTable();
            _strings = new FortifyFprStrings(_nameTable);

            _results = new List<Result>();
            _toolNotifications = new List<Notification>();
            _fileDictionary = new Dictionary<string, FileData>();
            _ruleDictionary = new Dictionary<string, IRule>();
            _tflToNodeIdDictionary = new Dictionary<ThreadFlowLocation, string>();
            _tflToSnippetIdDictionary = new Dictionary<ThreadFlowLocation, string>();
            _locationToSnippetIdDictionary = new Dictionary<Location, string>();
            _resultToSnippetIdDictionary = new Dictionary<Result, string>();
            _resultToReplacementDefinitionDictionary = new Dictionary<Result, Dictionary<string, string>>();
            _nodeIdToLocationDictionary = new Dictionary<string, Location>();
            _snippetIdToRegionDictionary = new Dictionary<string, Region>();
        }

        /// <summary>
        /// Interface implementation for converting a stream in Fortify FPR format to a stream in
        /// SARIF format.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="input">Stream in Fortify FPR format.</param>
        /// <param name="output">Stream in SARIF format.</param>
        /// <param name="dataToInsert">Optionally emitted properties that should be written to log.</param>
        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
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
                Name = FortifyToolName
            };

            _invocation = new Invocation();
            _results.Clear();
            _toolNotifications.Clear();
            _fileDictionary.Clear();
            _ruleDictionary.Clear();
            _tflToNodeIdDictionary.Clear();
            _tflToSnippetIdDictionary.Clear();
            _locationToSnippetIdDictionary.Clear();
            _resultToSnippetIdDictionary.Clear();
            _resultToReplacementDefinitionDictionary.Clear();
            _nodeIdToLocationDictionary.Clear();
            _snippetIdToRegionDictionary.Clear();

            ParseFprFile(input);
            AddMessagesToResults();
            AddSnippetsToResults();
            AddNodeLocationsToThreadFlowLocations();
            AddSnippetsToThreadFlowLocations();

            var run = new Run()
            {
                InstanceGuid = _runId,
                AutomationLogicalId = _automationId,
                Tool = tool,
                Invocations = new[] { _invocation }
            };

            output.Initialize(run);

            (output as ResultLogJsonWriter).WriteInvocations(run.Invocations);

            if (_fileDictionary.Any())
            {
                output.WriteFiles(_fileDictionary);
            }

            output.OpenResults();
            output.WriteResults(_results);
            output.CloseResults();

            if (_ruleDictionary.Any())
            {
                output.WriteRules(_ruleDictionary);
            }

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
                    if (AtStartOfNonEmpty(_strings.Uuid))
                    {
                        ParseUuid();
                    }
                    // Note: CreatedTS is an empty element (it has only attributes),
                    // so we can't call AtStartOfNonEmpty here.
                    else if (AtStartOf(_strings.CreatedTimestamp))
                    {
                        ParseCreatedTimestamp();
                    }
                    else if (AtStartOfNonEmpty(_strings.Build))
                    {
                        ParseBuild();
                    }
                    else if (AtStartOfNonEmpty(_strings.Vulnerabilities))
                    {
                        ParseVulnerabilities();
                    }
                    else if (AtStartOfNonEmpty(_strings.Description))
                    {
                        ParseRuleFromDescription();
                    }
                    else if (AtStartOfNonEmpty(_strings.UnifiedNodePool))
                    {
                        ParseNodes();
                    }
                    else if (AtStartOfNonEmpty(_strings.Snippets))
                    {
                        ParseSnippets();
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

        private void ParseUuid()
        {
            _runId = _reader.ReadElementContentAsString();
        }

        private void ParseCreatedTimestamp()
        {
            string date = _reader.GetAttribute(_strings.DateAttribute);
            string time = _reader.GetAttribute(_strings.TimeAttribute);
            if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(time))
            {
                string dateTime = date + "T" + time;
                _invocation.StartTime = DateTime.Parse(dateTime, CultureInfo.InvariantCulture);
            }

            // Step past the empty element.
            _reader.Read();
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
            int length = 0;
            string sizeAttribute = _reader.GetAttribute(_strings.SizeAttribute);
            if (sizeAttribute != null)
            {
                int.TryParse(sizeAttribute, out length);
            }

            string encoding = _reader.GetAttribute(_strings.EncodingAttribute);

            string fileName = null;
            _reader.Read();
            while (!AtEndOf(_strings.File))
            {
                if (AtStartOfNonEmpty(_strings.Name))
                {
                    fileName = UriHelper.MakeValidUri(_reader.ReadElementContentAsString());
                }
                else
                {
                    _reader.Read();
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                var fileData = new FileData
                {
                    Encoding = encoding,
                    MimeType = MimeType.DetermineFromFileExtension(fileName),
                    Length = length
                };

                _fileDictionary.Add(fileName, fileData);
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
            var result = new Result
            {
                Locations = new List<Location>(),
                RelatedLocations = new List<Location>(),
                CodeFlows = new []
                {
                    SarifUtilities.CreateSingleThreadedCodeFlow()
                }
            };

            _reader.Read();
            while (!AtEndOf(_strings.Vulnerability))
            {
                if (AtStartOfNonEmpty(_strings.ClassId))
                {
                    result.RuleId = _reader.ReadElementContentAsString();
                }
                else if (AtStartOfNonEmpty(_strings.ReplacementDefinitions))
                {
                    ParseReplacementDefinitions(result);
                }
                else if (AtStartOfNonEmpty(_strings.Trace))
                {
                    ParseLocationFromTrace(result);
                }

                _reader.Read();
            }

            _results.Add(result);
        }

        private void ParseLocationFromTrace(Result result)
        {
            CodeFlow codeFlow = result.CodeFlows.First();
            int step = 0;
            string nodeLabel = null;
            string lastNodeId = null;

            _reader.Read();

            while (!AtEndOf(_strings.Trace))
            {
                if (AtStartOf(_strings.NodeRef))
                {
                    string nodeId = _reader.GetAttribute(_strings.IdAttribute);

                    if (!string.IsNullOrWhiteSpace(nodeId))
                    {
                        var tfl = new ThreadFlowLocation
                        {
                            Step = ++step
                        };

                        _tflToNodeIdDictionary.Add(tfl, nodeId);
                        codeFlow.ThreadFlows[0].Locations.Add(tfl);
                    }

                    _reader.Read();
                }
                else if (AtStartOf(_strings.Node))
                {
                    nodeLabel = _reader.GetAttribute(_strings.LabelAttribute);
                    _reader.Read();
                }
                else if (AtStartOf(_strings.SourceLocation))
                {
                    // Note: SourceLocation is an empty element (it has only attributes),
                    // so we can't call AtStartOfNonEmpty here.

                    string snippetId = _reader.GetAttribute(_strings.SnippetAttribute);
                    PhysicalLocation physicalLocation = ParsePhysicalLocationFromSourceInfo();

                    // Step past the empty SourceLocation element.
                    _reader.Read();

                    // If we don't have a label, get the <Action> value
                    if (string.IsNullOrWhiteSpace(nodeLabel))
                    {
                        nodeLabel = _reader.ReadElementContentAsString();
                    }

                    var tfl = new ThreadFlowLocation
                    {
                        Step = ++step,
                        Location = new Location
                        {
                            Message = new Message
                            {
                                Text = nodeLabel
                            },
                            PhysicalLocation = physicalLocation
                        }
                    };

                    // Remember the id of the snippet associated with this location.
                    // We'll use it to fill the snippet text when we read the Snippets element later on.
                    if (!string.IsNullOrEmpty(snippetId))
                    {
                        _tflToSnippetIdDictionary.Add(tfl, snippetId);
                    }

                    codeFlow.ThreadFlows[0].Locations.Add(tfl);

                    // Keep track of the snippet associated with the last location in the
                    // CodeFlow; that's the snippet that we'll associate with the Result
                    // as a whole.
                    lastNodeId = snippetId;
                }
                else
                {
                    _reader.Read();
                }
            }

            if (codeFlow.ThreadFlows[0].Locations.Any())
            {
                result.Locations.Add(new Location
                {
                    // TODO: Confirm that the traces are ordered chronologically
                    // (so that we really do want to use the last one as the
                    // overall result location).
                    PhysicalLocation = codeFlow.ThreadFlows[0].Locations.Last().Location?.PhysicalLocation.DeepClone()
                });
                result.RelatedLocations.Add(new Location
                {
                    // Links embedded in the result message refer to related physicalLocation.id
                    PhysicalLocation = codeFlow.ThreadFlows[0].Locations.Last().Location?.PhysicalLocation.DeepClone()
                });

                result.RelatedLocations.Last().PhysicalLocation.Id = 1;

                if (!string.IsNullOrEmpty(lastNodeId))
                {
                    _resultToSnippetIdDictionary.Add(result, lastNodeId);
                }
            }
        }

        private void ParseReplacementDefinitions(Result result)
        {
            var replacements = new Dictionary<string, string>();
            _reader.Read();

            while (!AtEndOf(_strings.ReplacementDefinitions))
            {
                if (_reader.Name == _strings.Def && !AtEndOf(_strings.Def))
                {
                    string key = _reader.GetAttribute(_strings.KeyAttribute);
                    string value = _reader.GetAttribute(_strings.ValueAttribute);
                    replacements.Add(key, value);
                }

                _reader.Read();
            }

            if (replacements.Any())
            {
                _resultToReplacementDefinitionDictionary.Add(result, replacements);
            }
        }

        private PhysicalLocation ParsePhysicalLocationFromSourceInfo()
        {
            string path = _reader.GetAttribute(_strings.PathAttribute);

            return new PhysicalLocation
            {
                FileLocation = new FileLocation
                {
                    Uri = new Uri(path, UriKind.Relative)
                },
                Region = ParseRegion()
            };
        }

        private Region ParseRegion()
        {
            int startLine = 0;
            string lineAttr = _reader.GetAttribute(_strings.LineAttribute);
            if (lineAttr != null)
            {
                int.TryParse(lineAttr, out startLine);
            }

            int endLine = 0;
            string linelEndAttr = _reader.GetAttribute(_strings.LineEndAttribute);
            if (linelEndAttr != null)
            {
                int.TryParse(linelEndAttr, out endLine);
            }

            int startColumn = 0;
            string colStartAttr = _reader.GetAttribute(_strings.ColStartAttribute);
            if (colStartAttr != null)
            {
                int.TryParse(colStartAttr, out startColumn);
            }

            int endColumn = 0;
            string colEndAttr = _reader.GetAttribute(_strings.ColEndAttribute);
            if (colEndAttr != null)
            {
                int.TryParse(colEndAttr, out endColumn);
            }

            return new Region
            {
                StartLine = startLine,
                EndLine = endLine,
                StartColumn = startColumn,
                EndColumn = endColumn
            };
        }

        private void ParseRuleFromDescription()
        {
            var rule = new Rule
            {
                Id = _reader.GetAttribute(_strings.ClassIdAttribute)
            };

            _reader.Read();
            while (!AtEndOf(_strings.Description))
            {
                if (AtStartOfNonEmpty(_strings.Abstract))
                {
                    string content = _reader.ReadElementContentAsString();
                    rule.ShortDescription = new Message { Text = FortifyUtilities.ParseFormattedContentText(content) };
                }
                else if (AtStartOfNonEmpty(_strings.Explanation))
                {
                    string content = _reader.ReadElementContentAsString();
                    rule.FullDescription = new Message { Text = FortifyUtilities.ParseFormattedContentText(content) };
                }
                else if (AtStartOfNonEmpty(_strings.CustomDescription))
                {
                    // Skip the custom description block
                    while (!AtEndOf(_strings.CustomDescription))
                    {
                        _reader.Read();
                    }
                }
                else
                {
                    _reader.Read();
                }
            }

            _ruleDictionary.Add(rule.Id, rule);
        }

        private void ParseNodes()
        {
            _reader.Read();
            while (!AtEndOf(_strings.UnifiedNodePool))
            {
                if (AtStartOfNonEmpty(_strings.Node))
                {
                    ParseNode();
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private void ParseNode()
        {
            string nodeId = _reader.GetAttribute(_strings.IdAttribute);
            _reader.Read();
            while (!AtEndOf(_strings.Node))
            {
                if (AtStartOf(_strings.SourceLocation))
                {
                    PhysicalLocation physicalLocation = ParsePhysicalLocationFromSourceInfo();

                    string snippetId = _reader.GetAttribute(_strings.SnippetAttribute);

                    // Step past the empty SourceLocation element.
                    _reader.Read();

                    // Get the node text from the Action element
                    string nodeLabel = _reader.ReadElementContentAsString();

                    // Create the location
                    var location = new Location
                    {
                        Message = new Message
                        {
                            Text = nodeLabel
                        },
                        PhysicalLocation = physicalLocation
                    };

                    _nodeIdToLocationDictionary.Add(nodeId, location);
                    _locationToSnippetIdDictionary.Add(location, snippetId);
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private void ParseSnippets()
        {
            _reader.Read();
            while (!AtEndOf(_strings.Snippets))
            {
                if (AtStartOfNonEmpty(_strings.Snippet))
                {
                    ParseSnippet();
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private void ParseSnippet()
        {
            string snippetId = _reader.GetAttribute(_strings.IdAttribute);
            int startLine = 0;
            int endLine = 0;
            string text = null;

            _reader.Read();

            while (!AtEndOf(_strings.Snippet))
            {
                if (AtStartOfNonEmpty(_strings.StartLine))
                {
                    string value = _reader.ReadElementContentAsString();
                    int.TryParse(value, out startLine);
                }
                else if (AtStartOfNonEmpty(_strings.EndLine))
                {
                    string value = _reader.ReadElementContentAsString();
                    int.TryParse(value, out endLine);
                }
                else if (AtStartOfNonEmpty(_strings.Text))
                {
                    text = _reader.ReadElementContentAsString();
                }
                else
                {
                    _reader.Read();
                }
            }

            Region region = new Region
            {
                StartLine = startLine,
                EndLine = endLine
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                region.Snippet = new FileContent { Text = text };
            }

            _snippetIdToRegionDictionary.Add(snippetId, region);
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
                        Id = errorCode,
                        Level = NotificationLevel.Error,
                        Message = new Message { Text = message }
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
                else if (AtStartOfNonEmpty(_strings.Platform))
                {
                    _invocation.SetProperty("Platform", _reader.ReadElementContentAsString());
                }
                else
                {
                    _reader.Read();
                }
            }
        }

        private bool AtStartOfNonEmpty(string elementName)
        {
            return AtStartOf(elementName) && !_reader.IsEmptyElement;
        }

        private bool AtStartOf(string elementName)
        {
            return !_reader.EOF &&
                (_reader.NodeType == XmlNodeType.Element && StringReference.AreEqual(_reader.LocalName, elementName));
        }

        private bool AtEndOf(string elementName)
        {
            return _reader.EOF ||
                (_reader.NodeType == XmlNodeType.EndElement && StringReference.AreEqual(_reader.LocalName, elementName));
        }

        private void AddMessagesToResults()
        {
            foreach (Result result in _results)
            {
                IRule rule;
                if (_ruleDictionary.TryGetValue(result.RuleId, out rule))
                {
                    Message message = rule.ShortDescription ?? rule.FullDescription;
                    Dictionary<string, string> replacements = null;

                    if (_resultToReplacementDefinitionDictionary.TryGetValue(result, out replacements))
                    {
                        string messageText = message?.Text;
                        foreach (string key in replacements.Keys)
                        {
                            string value = replacements[key];

                            if (SupportedReplacementTokens.Contains(key))
                            {
                                // Replace the token with an embedded hyperlink
                                messageText = messageText.Replace(string.Format(ReplacementTokenFormat, key),
                                                                  string.Format(EmbeddedLinkFormat, value));
                            }
                            else
                            {
                                // Replace the token with plain text
                                messageText = messageText.Replace(string.Format(ReplacementTokenFormat, key), value);
                            }
                        }

                        message = message.DeepClone();
                        message.Text = messageText;
                    }

                    result.Message = message;
                }
            }
        }

        private void AddSnippetsToResults()
        {
            foreach (Result result in _results)
            {
                string snippetId;
                Region region;

                if (result.Locations?[0]?.PhysicalLocation?.Region != null &&
                    _resultToSnippetIdDictionary.TryGetValue(result, out snippetId) &&
                    _snippetIdToRegionDictionary.TryGetValue(snippetId, out region) &&
                    !string.IsNullOrWhiteSpace(region.Snippet.Text))
                {
                    result.Locations[0].PhysicalLocation.Region = region;
                }
            }
        }

        /// <summary>
        /// Locates the region (including snippet text) for threadFlowLocations that
        /// were created from Node elements within a Trace
        /// </summary>
        private void AddSnippetsToThreadFlowLocations()
        {
            foreach (Result result in _results)
            {
                if (result.CodeFlows != null)
                {
                    foreach (CodeFlow codeFlow in result.CodeFlows)
                    {
                        if (codeFlow.ThreadFlows[0].Locations != null)
                        {
                            foreach (ThreadFlowLocation tfl in codeFlow.ThreadFlows[0].Locations)
                            {
                                string snippetId;
                                Region region = null;

                                if (_tflToSnippetIdDictionary.TryGetValue(tfl, out snippetId) &&
                                    _snippetIdToRegionDictionary.TryGetValue(snippetId, out region))
                                {
                                    tfl.Location.PhysicalLocation.Region = region;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Locates the location and region (including snippet) for threadFlowLocations
        /// that were created from NodeRef elements
        /// </summary>
        private void AddNodeLocationsToThreadFlowLocations()
        {
            foreach (Result result in _results)
            {
                if (result.CodeFlows != null)
                {
                    foreach (CodeFlow codeFlow in result.CodeFlows)
                    {
                        if (codeFlow.ThreadFlows[0].Locations != null)
                        {
                            foreach (ThreadFlowLocation tfl in codeFlow.ThreadFlows[0].Locations)
                            {
                                string nodeId;
                                string snippetId;
                                Region region = null;
                                Location location = null;

                                if (_tflToNodeIdDictionary.TryGetValue(tfl, out nodeId) &&
                                    _nodeIdToLocationDictionary.TryGetValue(nodeId, out location) &&
                                    _locationToSnippetIdDictionary.TryGetValue(location, out snippetId) &&
                                    _snippetIdToRegionDictionary.TryGetValue(snippetId, out region))
                                {
                                    location.PhysicalLocation.Region = region;
                                    tfl.Location = location;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
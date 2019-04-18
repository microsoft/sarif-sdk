// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyFprConverter : ToolFileConverterBase
    {
        private const string FortifyExecutable = "[REMOVED]insourceanalyzer.exe";
        private const string FileLocationUriBaseId = "SRCROOT";
        private const string ReplacementTokenFormat = "<Replace key=\"{0}\"/>";
        private const string EmbeddedLinkFormat = "[{0}](1)";

        private readonly NameTable _nameTable;
        private readonly FortifyFprStrings _strings;
        private readonly string[] SupportedReplacementTokens = new[] { "PrimaryLocation.file", "PrimaryLocation.line" };
        private readonly Dictionary<string, List<string>> ActionTypeToLocationKindMap = new Dictionary<string, List<string>>
        {
            { "InCall", new List<string> { "call", "function" } },
            { "InOutCall", new List<string> { "call", "function", "return" , "function"} },
            { "BranchTaken", new List<string> { "branch", "true" } },
            { "BranchNotTaken", new List<string> { "branch", "false" } },
            { "Return", new List<string> { "return", "function" } },
            { "EndScope", new List<string> { "exit", "scope" } },
            { "Assign", new List<string> { "acquire", "resource" } },
            { "Read", new List<string> { "acquire", "resource" } }
        };
        private readonly ToolComponent CweToolComponent = new ToolComponent
        {
            Name = "CWE",
            Guid = "2B841697-D0DE-45DD-9F19-1EEE1312429",
            Organization = "MITRE",
            ShortDescription = new MultiformatMessageString
            {
                Text = "The MITRE Common Weakness Enumeration"
            }
        };

        private XmlReader _reader;
        private Invocation _invocation;
        private string _runId;
        private string _automationId;
        private string _originalUriBasePath;
        private int _currentFileIndex = 0;
        private List<Result> _results = new List<Result>();
        private Dictionary<Uri, Tuple<Artifact, int>> _files;
        private List<ReportingDescriptor> _rules;
        private Dictionary<string, int> _ruleIdToIndexMap;
        private HashSet<string> _cweIds;
        private Dictionary<ThreadFlowLocation, string> _tflToNodeIdDictionary;
        private Dictionary<ThreadFlowLocation, string> _tflToSnippetIdDictionary;
        private Dictionary<Location, string> _locationToSnippetIdDictionary;
        private Dictionary<Result, string> _resultToSnippetIdDictionary;
        private Dictionary<Result, Dictionary<string, string>> _resultToReplacementDefinitionDictionary;
        private Dictionary<string, Location> _nodeIdToLocationDictionary;
        private Dictionary<string, string> _nodeIdToActionTypeDictionary;
        private Dictionary<string, Region[]> _snippetIdToRegionsDictionary;

        /// <summary>Initializes a new instance of the <see cref="FortifyFprConverter"/> class.</summary>
        public FortifyFprConverter()
        {
            _nameTable = new NameTable();
            _strings = new FortifyFprStrings(_nameTable);

            _results = new List<Result>();
            _files = new Dictionary<Uri, Tuple<Artifact, int>>();
            _rules = new List<ReportingDescriptor>();
            _ruleIdToIndexMap = new Dictionary<string, int>();
            _cweIds = new HashSet<string>();
            _tflToNodeIdDictionary = new Dictionary<ThreadFlowLocation, string>();
            _tflToSnippetIdDictionary = new Dictionary<ThreadFlowLocation, string>();
            _locationToSnippetIdDictionary = new Dictionary<Location, string>();
            _resultToSnippetIdDictionary = new Dictionary<Result, string>();
            _resultToReplacementDefinitionDictionary = new Dictionary<Result, Dictionary<string, string>>();
            _nodeIdToLocationDictionary = new Dictionary<string, Location>();
            _nodeIdToActionTypeDictionary = new Dictionary<string, string>();
            _snippetIdToRegionsDictionary = new Dictionary<string, Region[]>();
        }

        public override string ToolName => "HP Fortify Static Code Analyzer";

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

            _invocation = new Invocation();
            _invocation.ToolExecutionNotifications = new List<Notification>();
            _results.Clear();
            _files.Clear();
            _rules.Clear();
            _ruleIdToIndexMap.Clear();
            _cweIds.Clear();
            _tflToNodeIdDictionary.Clear();
            _tflToSnippetIdDictionary.Clear();
            _locationToSnippetIdDictionary.Clear();
            _resultToSnippetIdDictionary.Clear();
            _resultToReplacementDefinitionDictionary.Clear();
            _nodeIdToLocationDictionary.Clear();
            _nodeIdToActionTypeDictionary.Clear();
            _snippetIdToRegionsDictionary.Clear();

            ParseFprFile(input);
            AddMessagesToResults();
            AddSnippetsToResults();
            AddNodeLocationsToThreadFlowLocations();
            AddSnippetsToThreadFlowLocations();

            var run = new Run()
            {
                AutomationDetails = new RunAutomationDetails
                {
                    Guid = _runId,
                    Id = _automationId + "/"
                },
                Artifacts = _files.OrderBy(d => d.Value.Item2)
                                  .Select(p => p.Value)
                                  .Select(t => t.Item1)
                                  .ToList() as IList<Artifact>,
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = ToolName,
                        Rules = _rules,
                        SupportedTaxonomies = new List<ToolComponentReference>
                        {
                            new ToolComponentReference
                            {
                                Name = "CWE",
                                Index = 0,
                                Guid = "2B841697-D0DE-45DD-9F19-1EEE1312429"
                            }
                        }
                    }
                },
                Taxonomies = new List<ToolComponent>
                {
                    CweToolComponent
                },
                Invocations = new[] { _invocation },
            };

            if (_cweIds.Count > 0)
            {
                run.Taxonomies[0].Taxa = _cweIds.Select(c => new ReportingDescriptor { Id = c }).ToList();
            }

            if (!string.IsNullOrWhiteSpace(_originalUriBasePath))
            {
                if (_originalUriBasePath.StartsWith("/") &&
                    _invocation.GetProperty("Platform") == "Linux")
                {
                    _originalUriBasePath = "file:/" + _originalUriBasePath;
                }

                if (Uri.TryCreate(_originalUriBasePath, UriKind.Absolute, out Uri uri))
                {
                    run.OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>
                    {
                        { FileLocationUriBaseId, new ArtifactLocation { Uri = uri } }
                    };
                }
            }

            PersistResults(output, _results, run);
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
                        ParseRuleDescriptions();
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
                    else if (AtStartOf(_strings.RuleInfo))
                    {
                        ParseRuleInfo();
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
                _invocation.StartTimeUtc = DateTime.Parse(dateTime, CultureInfo.InvariantCulture);
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
                else if (AtStartOfNonEmpty(_strings.SourceBasePath))
                {
                    _originalUriBasePath = _reader.ReadElementContentAsString();
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
                Uri uri = new Uri(fileName, UriKind.RelativeOrAbsolute);
                var fileData = new Artifact
                {
                    Encoding = encoding,
                    MimeType = MimeType.DetermineFromFileExtension(fileName),
                    Length = length,
                    Location = new ArtifactLocation
                    { 
                        Uri = uri,
                        UriBaseId = uri.IsAbsoluteUri ? null : FileLocationUriBaseId
                    }
                };

                _files.Add(uri, new Tuple<Artifact, int>(fileData, _currentFileIndex++));
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
                RelatedLocations = new List<Location>(),
                CodeFlows = new List<CodeFlow>()
            };

            _reader.Read();
            ReportingDescriptor rule = null;
            int ruleIndex;

            while (!AtEndOf(_strings.Vulnerability))
            {
                if (AtStartOfNonEmpty(_strings.ClassId))
                {
                    // Get the rule GUID from the ClassId element.
                    string ruleId = _reader.ReadElementContentAsString();

                    if (_ruleIdToIndexMap.ContainsKey(ruleId))
                    {
                        // We previously created the rule, so just get the index and rule object.
                        ruleIndex = _ruleIdToIndexMap[ruleId];
                        rule = _rules[ruleIndex];
                    }
                    else
                    {
                        // Create a new rule and add it to the list and index map.
                        rule = new ReportingDescriptor
                        {
                            Guid = ruleId
                        };

                        ruleIndex = _rules.Count;
                        _rules.Add(rule);
                        _ruleIdToIndexMap[ruleId] = ruleIndex;
                    }

                    result.RuleIndex = ruleIndex;
                }
                else if (AtStartOfNonEmpty(_strings.Kingdom))
                {
                    rule.SetProperty(_strings.Kingdom, _reader.ReadElementContentAsString());
                }
                else if (AtStartOfNonEmpty(_strings.Type))
                {
                    rule.SetProperty(_strings.Type, _reader.ReadElementContentAsString());
                }
                else if (AtStartOfNonEmpty(_strings.Subtype))
                {
                    rule.SetProperty(_strings.Subtype, _reader.ReadElementContentAsString());
                }
                else if (AtStartOfNonEmpty(_strings.InstanceSeverity))
                {
                    if (double.TryParse(_reader.ReadElementContentAsString(), out double rank))
                    {
                        result.Rank = rank;
                    }
                }
                else if (AtStartOfNonEmpty(_strings.ReplacementDefinitions))
                {
                    ParseReplacementDefinitions(result);
                }
                else if (AtStartOfNonEmpty(_strings.Trace))
                {
                    ParseLocationsFromTraces(result);
                }
                else
                {
                    _reader.Read();
                }
            }

            _results.Add(result);
        }

        private void ParseLocationsFromTraces(Result result)
        {
            CodeFlow codeFlow = null;
            string nodeLabel = null;
            string lastNodeId = null;
            bool? isDefault = null;

            while (!AtEndOf(_strings.Unified))
            {
                if (AtStartOf(_strings.Trace))
                {
                    codeFlow = SarifUtilities.CreateSingleThreadedCodeFlow();
                    result.CodeFlows.Add(codeFlow);

                    while (!AtEndOf(_strings.Trace))
                    {
                        if (AtStartOf(_strings.NodeRef))
                        {
                            string nodeId = _reader.GetAttribute(_strings.IdAttribute);

                            if (!string.IsNullOrWhiteSpace(nodeId))
                            {
                                var tfl = new ThreadFlowLocation();
                                _tflToNodeIdDictionary.Add(tfl, nodeId);
                                codeFlow.ThreadFlows[0].Locations.Add(tfl);
                            }

                            _reader.Read();
                        }
                        else if (AtStartOf(_strings.Node))
                        {
                            if (isDefault == null)
                            {
                                // We haven't found the default node yet, so check this one.
                                string isDefaultValue = _reader.GetAttribute(_strings.IsDefaultAttribute);

                                if (!string.IsNullOrWhiteSpace(isDefaultValue)
                                    && bool.TryParse(isDefaultValue, out bool val)
                                    && val == true)
                                {
                                    // This is the default, set the flag so we know to add a result location.
                                    isDefault = val;
                                }
                            }

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

                            string actionType = null;
                            if (AtStartOf(_strings.Action))
                            {
                                actionType = _reader.GetAttribute(_strings.TypeAttribute);
                                actionType = actionType ?? string.Empty; // We use empty string to indicates there is an
                                                                         // Action element without a type attribute.

                                // If we don't have a label, get the <Action> value.
                                if (string.IsNullOrWhiteSpace(nodeLabel))
                                {
                                    nodeLabel = _reader.ReadElementContentAsString();
                                }
                            }

                            if (actionType == string.Empty)
                            {
                                if (codeFlow.ThreadFlows[0].Locations.Count > 0)
                                {
                                    // If there is no type attribute on the Action element, we treat
                                    // it as a note about the prior node.
                                    ThreadFlowLocation tfl = codeFlow.ThreadFlows[0].Locations.Last();

                                    // Annotate the location with the Action text.
                                    if (tfl?.Location != null)
                                    {
                                        tfl.Location.Annotations = new List<Region>();
                                        Region region = physicalLocation.Region;
                                        region.Message = new Message
                                        {
                                            Text = nodeLabel
                                        };
                                        tfl.Location.Annotations.Add(region);
                                    }
                                }
                            }
                            else
                            {
                                var location = new Location
                                {
                                    PhysicalLocation = physicalLocation
                                };

                                if (isDefault == true)
                                {
                                    result.Locations = new List<Location>();
                                    result.Locations.Add(location.DeepClone());
                                    result.RelatedLocations.Add(location.DeepClone());

                                    // Keep track of the snippet associated with the default location.
                                    // That's the snippet that we'll associate with the result.
                                    lastNodeId = snippetId;

                                    isDefault = false; // This indicates we have already found the default node.
                                }

                                var tfl = new ThreadFlowLocation
                                {
                                    Kinds = ConvertActionTypeToLocationKinds(actionType),
                                    Location = location
                                };

                                if (!string.IsNullOrWhiteSpace(nodeLabel))
                                {
                                    tfl.Location.Message = new Message
                                    {
                                        Text = nodeLabel
                                    };
                                }

                                // Remember the id of the snippet associated with this location.
                                // We'll use it to fill the snippet text when we read the Snippets element later on.
                                if (!string.IsNullOrEmpty(snippetId))
                                {
                                    _tflToSnippetIdDictionary.Add(tfl, snippetId);
                                }

                                codeFlow.ThreadFlows[0].Locations.Add(tfl);
                            }
                        }
                        else
                        {
                            _reader.Read();
                        }
                    }
                }
                else
                {
                    _reader.Read();
                }
            }

            if (result.RelatedLocations.Any())
            {
                Location relatedLocation = result.RelatedLocations.Last();

                if (relatedLocation != null)
                {
                    relatedLocation.Id = 1;
                }
            }

            if (!string.IsNullOrEmpty(lastNodeId))
            {
                _resultToSnippetIdDictionary.Add(result, lastNodeId);
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

            var uri = new Uri(path, UriKind.RelativeOrAbsolute);
            int index = _files[uri].Item2;
            return new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Index = index
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
                StartColumn = startColumn,
                EndLine = endLine > startLine ? endLine : 0,
                EndColumn = endColumn
            };
        }

        private void ParseRuleInfo()
        {
            while (!AtEndOf(_strings.RuleInfo))
            {
                _reader.Read();

                if (AtStartOfNonEmpty(_strings.Rule))
                {
                    string ruleId = _reader.GetAttribute(_strings.IdAttribute);
                    int ruleIndex = _ruleIdToIndexMap[ruleId];
                    ReportingDescriptor rule = _rules[ruleIndex];

                    _reader.Read();

                    if (AtStartOfNonEmpty(_strings.MetaInfo))
                    {
                        while (!AtEndOf(_strings.MetaInfo))
                        {
                            _reader.Read();

                            string groupName = _reader.GetAttribute(_strings.NameAttribute);

                            switch (groupName)
                            {
                                case "altcategoryCWE":
                                    string nodeValue = _reader.ReadElementContentAsString();

                                    if (!string.IsNullOrWhiteSpace(nodeValue))
                                    {
                                        string[] parts = nodeValue.Split(',');

                                        foreach (string part in parts)
                                        {
                                            // Format: CWE ID xxx
                                            string cweId = part.Substring(part.LastIndexOf(' ') + 1);
                                            _cweIds.Add(cweId);
                                            
                                            rule.Relationships = rule.Relationships ?? new List<ReportingDescriptorRelationship>();
                                            rule.Relationships.Add(new ReportingDescriptorRelationship
                                            {
                                                Target = new ReportingDescriptorReference
                                                {
                                                    Id = cweId,
                                                    ToolComponent = new ToolComponentReference
                                                    {
                                                        Name = CweToolComponent.Name,
                                                        Guid = CweToolComponent.Guid
                                                    }
                                                },
                                                Kinds = new List<string>
                                                {
                                                    "relevant"
                                                }
                                            });
                                        }
                                    }

                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void ParseRuleDescriptions()
        {
            string ruleId = _reader.GetAttribute(_strings.ClassIdAttribute);
            int ruleIndex = _ruleIdToIndexMap[ruleId];
            ReportingDescriptor rule = _rules[ruleIndex];

            _reader.Read();
            while (!AtEndOf(_strings.Description))
            {
                if (AtStartOfNonEmpty(_strings.Abstract))
                {
                    string content = _reader.ReadElementContentAsString();
                    rule.ShortDescription = new MultiformatMessageString { Text = FortifyUtilities.ParseFormattedContentText(content) };
                }
                else if (AtStartOfNonEmpty(_strings.Explanation))
                {
                    string content = _reader.ReadElementContentAsString();
                    rule.FullDescription = new MultiformatMessageString { Text = FortifyUtilities.ParseFormattedContentText(content) };
                }
                else if (AtStartOfNonEmpty(_strings.CustomDescription))
                {
                    // Skip the custom description block.
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

                    // Get the node text and type attribute from the Action element.
                    string actionType = _reader.GetAttribute(_strings.TypeAttribute);
                    string nodeLabel = _reader.ReadElementContentAsString();

                    // Create the location.
                    var location = new Location
                    {
                        Message = new Message
                        {
                            Text = nodeLabel
                        },
                        PhysicalLocation = physicalLocation
                    };

                    _nodeIdToLocationDictionary.Add(nodeId, location);
                    _nodeIdToActionTypeDictionary.Add(nodeId, actionType);
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
            // Format: <guid>#<file path>:<start line>:<end line>
            string snippetId = _reader.GetAttribute(_strings.IdAttribute);
            int snippetStartLine = 0;
            int snippetEndLine = 0;
            int regionStartLine = 0;
            int regionEndLine = 0;

            string[] parts = snippetId.Split(':');

            int.TryParse(parts[parts.Length - 2], out regionStartLine);
            int.TryParse(parts[parts.Length - 1], out regionEndLine);
            string text = null;

            _reader.Read();

            while (!AtEndOf(_strings.Snippet))
            {
                if (AtStartOfNonEmpty(_strings.StartLine))
                {
                    string value = _reader.ReadElementContentAsString();
                    int.TryParse(value, out snippetStartLine);
                }
                else if (AtStartOfNonEmpty(_strings.EndLine))
                {
                    string value = _reader.ReadElementContentAsString();
                    int.TryParse(value, out snippetEndLine);
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

            Region region = null;
            Region contextRegion = null;

            if (!string.IsNullOrWhiteSpace(text))
            {
                region = new Region
                {
                    StartLine = regionStartLine,
                    EndLine = regionEndLine > regionStartLine ? regionEndLine : 0
                };

                contextRegion = new Region
                {
                    StartLine = snippetStartLine,
                    EndLine = snippetEndLine > snippetStartLine ? snippetEndLine : 0,
                    Snippet = new ArtifactContent
                    {
                        Text = text
                    }
                };

                using (StringReader reader = new StringReader(text))
                {
                    // Read down to the first line we want to include.
                    for (int i = 0; i < regionStartLine - snippetStartLine; i++)
                    {
                        reader.ReadLine();
                    }

                    var sb = new StringBuilder();

                    // Gather the lines we want.
                    for (int i = 0; i <= regionEndLine - regionStartLine; i++)
                    {
                        sb.AppendLine(reader.ReadLine());
                    }

                    // Trim the trailing line break.
                    text = sb.ToString().TrimEnd(new[] { '\r', '\n' });
                }

                region.Snippet = new ArtifactContent { Text = text };
            }

            // Regions[0] => physicalLocation.region
            // Regions[1] => physicalLocation.contextRegion
            _snippetIdToRegionsDictionary.Add(snippetId, new[] { region, contextRegion });
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

                    _invocation.ToolExecutionNotifications.Add(new Notification
                    {
                        Descriptor = new ReportingDescriptorReference
                        {
                            Id = errorCode
                        },
                        Level = FailureLevel.Error,
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

        private List<string> ConvertActionTypeToLocationKinds(string actionType)
        {
            List<string> kinds;

            if (!ActionTypeToLocationKindMap.TryGetValue(actionType, out kinds))
            {
                kinds = new List<string> { "unknown" };
            }

            return kinds;
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
                ReportingDescriptor rule = _rules[result.RuleIndex];
                Message message = new Message();

                if (_resultToReplacementDefinitionDictionary.TryGetValue(result, out Dictionary<string, string> replacements))
                {
                    string messageText = (rule.ShortDescription ?? rule.FullDescription)?.Text;
                    foreach (string key in replacements.Keys)
                    {
                        string value = replacements[key];

                        if (SupportedReplacementTokens.Contains(key))
                        {
                            // Replace the token with an embedded hyperlink.
                            messageText = messageText.Replace(string.Format(ReplacementTokenFormat, key),
                                                              string.Format(EmbeddedLinkFormat, value));
                        }
                        else
                        {
                            // Replace the token with plain text.
                            messageText = messageText.Replace(string.Format(ReplacementTokenFormat, key), value);
                        }
                    }
                    message.Text = messageText;
                }
                result.Message = message;
            }
        }

        private void AddSnippetsToResults()
        {
            foreach (Result result in _results)
            {
                if (result.Locations?[0]?.PhysicalLocation?.Region != null &&
                    _resultToSnippetIdDictionary.TryGetValue(result, out string snippetId) &&
                    _snippetIdToRegionsDictionary.TryGetValue(snippetId, out Region[] regions) &&
                    !string.IsNullOrWhiteSpace(regions[0]?.Snippet.Text))
                {
                    // Regions[0] => physicalLocation.region
                    // Regions[1] => physicalLocation.contextRegion
                    result.Locations[0].PhysicalLocation.Region = regions[0];
                    result.Locations[0].PhysicalLocation.ContextRegion = regions[1];
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
                                if (_tflToSnippetIdDictionary.TryGetValue(tfl, out string snippetId) &&
                                    _snippetIdToRegionsDictionary.TryGetValue(snippetId, out Region[] regions))
                                {
                                    // Regions[0] => physicalLocation.region
                                    // Regions[1] => physicalLocation.contextRegion
                                    tfl.Location.PhysicalLocation.Region = regions[0];
                                    tfl.Location.PhysicalLocation.ContextRegion = regions[1];
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
                                if (_tflToNodeIdDictionary.TryGetValue(tfl, out string nodeId) &&
                                    _nodeIdToLocationDictionary.TryGetValue(nodeId, out Location location) &&
                                    _nodeIdToActionTypeDictionary.TryGetValue(nodeId, out string actionType) &&
                                    _locationToSnippetIdDictionary.TryGetValue(location, out string snippetId))
                                {
                                    if (!string.IsNullOrEmpty(snippetId) && _snippetIdToRegionsDictionary.TryGetValue(snippetId, out Region[] regions))
                                    {
                                        // Regions[0] => physicalLocation.region
                                        // Regions[1] => physicalLocation.contextRegion
                                        location.PhysicalLocation.Region = regions[0];
                                        location.PhysicalLocation.ContextRegion = regions[1];
                                    }

                                    tfl.Location = location;
                                    tfl.Kinds = ConvertActionTypeToLocationKinds(actionType);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
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

        /// <summary>
        ///  Represents a Snippet in the Fortify format converted to Sarif types.
        /// </summary>
        private class Snippet
        {
            public Region Region { get; set; }
            public Region ContextRegion { get; set; }

            public Snippet(Region region, Region contextRegion)
            {
                this.Region = region;
                this.ContextRegion = contextRegion;
            }

            public void ApplyTo(PhysicalLocation physicalLocation)
            {
                physicalLocation.Region = this.Region;
                physicalLocation.ContextRegion = this.ContextRegion;
            }
        }

        /// <summary>
        ///  Represents a Node in the Fortify format converted to Sarif types
        /// </summary>
        private class Node
        {
            public ThreadFlowLocation ThreadFlowLocation { get; set; }
            public string SnippetId { get; set; }

            public Node(ThreadFlowLocation tfl, string snippetId)
            {
                this.ThreadFlowLocation = tfl;
                this.SnippetId = snippetId;
            }
        }

        private XmlReader _reader;
        private Invocation _invocation;
        private string _runId;
        private string _automationId;
        private string _originalUriBasePath;
        private int _currentFileIndex = 0;
        private List<Result> _results = new List<Result>();

        private Dictionary<string, string> _currentResultReplacementDictionary;

        private HashSet<string> _cweIds;
        private Dictionary<Uri, Tuple<Artifact, int>> _files;
        private List<ReportingDescriptor> _rules;
        private Dictionary<string, int> _ruleIdToIndexMap;

        private Dictionary<string, Node> _nodeDictionary;
        private Dictionary<string, Snippet> _snippetDictionary;

        /// <summary>Initializes a new instance of the <see cref="FortifyFprConverter"/> class.</summary>
        public FortifyFprConverter()
        {
            _nameTable = new NameTable();
            _strings = new FortifyFprStrings(_nameTable);
            _currentResultReplacementDictionary = new Dictionary<string, string>();

            _results = new List<Result>();
            _files = new Dictionary<Uri, Tuple<Artifact, int>>();
            _rules = new List<ReportingDescriptor>();
            _ruleIdToIndexMap = new Dictionary<string, int>();
            _cweIds = new HashSet<string>();
            _nodeDictionary = new Dictionary<string, Node>();
            _snippetDictionary = new Dictionary<string, Snippet>();
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
            _invocation.ExecutionSuccessful = true;
            _results.Clear();
            _files.Clear();
            _rules.Clear();
            _ruleIdToIndexMap.Clear();
            _cweIds.Clear();
            _nodeDictionary.Clear();
            _snippetDictionary.Clear();

            ParseFprFile(input);

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

        private void ParseFprFile(Stream fprFileStream)
        {
            // Parse everything except vulnerabilities (building maps to write Results as-we-go next pass)
            ParseAuditStream_PassOne(OpenAuditFvdlReader(fprFileStream));

            // Add Snippets to NodePool Nodes which referenced them (Snippets appear after the NodePool in Fortify files)
            AddSnippetsToNodes();

            // Parse the Vulnerabilities, writing as we go
            ParseAuditStream_PassTwo(OpenAuditFvdlReader(fprFileStream));
        }

        private XmlReader OpenAuditFvdlReader(Stream fprFileStream)
        {
            ZipArchive fprArchive = new ZipArchive(fprFileStream);
            ZipArchiveEntry auditEntry = fprArchive.Entries.Single(e => e.FullName.Equals("audit.fvdl"));

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreWhitespace = true,
                NameTable = _nameTable,
                XmlResolver = null
            };

            return XmlReader.Create(auditEntry.Open(), settings);
        }

        private void ParseAuditStream_PassOne(XmlReader reader)
        {
            // Pass One: Everything except vulnerabilities
            using (_reader = reader)
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

        private void ParseAuditStream_PassTwo(XmlReader reader)
        {
            // Second Pass: Parse Vulnerabilities only and write as-you-go
            using (_reader = reader)
            {
                while (_reader.Read())
                {
                    if (AtStartOfNonEmpty(_strings.Vulnerabilities))
                    {
                        ParseVulnerabilities();
                        break;
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
            _currentResultReplacementDictionary.Clear();

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
                    rule = FindOrCreateRule(ruleId, out ruleIndex);

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

            // Set Result location including any Replacement Dictionary replacements
            AddMessagesToResult(result);

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
                                if (_nodeDictionary.TryGetValue(nodeId, out Node node))
                                {
                                    codeFlow.ThreadFlows[0].Locations.Add(node.ThreadFlowLocation);
                                }
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

                                // Apply the Snippet, if any, to the new ThreadFlowLocation
                                if (!string.IsNullOrEmpty(snippetId))
                                {
                                    AddSnippetToThreadFlowLocation(tfl, snippetId);
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
                AddSnippetsToResult(result, lastNodeId);
            }
        }

        private void ParseReplacementDefinitions(Result result)
        {
            _currentResultReplacementDictionary.Clear();
            _reader.Read();

            while (!AtEndOf(_strings.ReplacementDefinitions))
            {
                if (_reader.Name == _strings.Def && !AtEndOf(_strings.Def))
                {
                    string key = _reader.GetAttribute(_strings.KeyAttribute);
                    string value = _reader.GetAttribute(_strings.ValueAttribute);
                    _currentResultReplacementDictionary.Add(key, value);
                }

                _reader.Read();
            }
        }

        private PhysicalLocation ParsePhysicalLocationFromSourceInfo()
        {
            string path = _reader.GetAttribute(_strings.PathAttribute);

            PhysicalLocation location = new PhysicalLocation()
            {
                Region = ParseRegion()
            };

            var uri = new Uri(path, UriKind.RelativeOrAbsolute);
            if (_files.TryGetValue(uri, out var entry))
            {
                location.ArtifactLocation = new ArtifactLocation() { Index = entry.Item2 };
            }
            else
            {
                location.ArtifactLocation = new ArtifactLocation() { Uri = uri };
            }

            return location;
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
                    ReportingDescriptor rule = FindOrCreateRule(ruleId, out int ruleIndex);

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
            ReportingDescriptor rule = FindOrCreateRule(ruleId, out int ruleIndex);

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

                    // Convert to Sarif types
                    Node node = new Node(
                        new ThreadFlowLocation()
                        {
                            Kinds = ConvertActionTypeToLocationKinds(actionType),
                            Location = new Location()
                            {
                                Message = new Message() { Text = nodeLabel },
                                PhysicalLocation = physicalLocation
                            }
                        },
                        snippetId
                    );

                    _nodeDictionary.Add(nodeId, node);
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

            _snippetDictionary.Add(snippetId, new Snippet(region, contextRegion));
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

        private ReportingDescriptor FindOrCreateRule(string ruleId, out int ruleIndex)
        {
            if (_ruleIdToIndexMap.TryGetValue(ruleId, out ruleIndex))
            {
                return _rules[ruleIndex];
            }
            else
            {
                ReportingDescriptor rule = new ReportingDescriptor() { Guid = ruleId };

                ruleIndex = _rules.Count;
                _rules.Add(rule);
                _ruleIdToIndexMap[ruleId] = ruleIndex;

                return rule;
            }
        }

        private List<string> ConvertActionTypeToLocationKinds(string actionType)
        {
            List<string> kinds;

            if (actionType == null || !ActionTypeToLocationKindMap.TryGetValue(actionType, out kinds))
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

        private void AddMessagesToResult(Result result)
        {
            ReportingDescriptor rule = _rules[result.RuleIndex];
            Message message = new Message();

            string messageText = (rule.ShortDescription ?? rule.FullDescription)?.Text;

            if (_currentResultReplacementDictionary != null)
            {
                foreach (string key in _currentResultReplacementDictionary.Keys)
                {
                    string value = _currentResultReplacementDictionary[key];

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
            }

            message.Text = messageText;
            result.Message = message;
        }

        private void AddSnippetsToResult(Result result, string snippetId)
        {
            if (result.Locations?[0]?.PhysicalLocation?.Region != null &&
                _snippetDictionary.TryGetValue(snippetId, out Snippet snippet) &&
                !string.IsNullOrWhiteSpace(snippet.Region?.Snippet.Text))
            {
                snippet.ApplyTo(result.Locations[0].PhysicalLocation);
            }
        }

        private void AddSnippetToThreadFlowLocation(ThreadFlowLocation tfl, string snippetId)
        {
            if (_snippetDictionary.TryGetValue(snippetId, out Snippet snippet))
            {
                snippet.ApplyTo(tfl.Location.PhysicalLocation);
            }
        }

        private void AddSnippetsToNodes()
        {
            foreach (Node node in _nodeDictionary.Values)
            {
                if (!String.IsNullOrEmpty(node.SnippetId) && _snippetDictionary.TryGetValue(node.SnippetId, out Snippet snippet))
                {
                    snippet.ApplyTo(node.ThreadFlowLocation.Location.PhysicalLocation);
                }
            }
        }
    }
}
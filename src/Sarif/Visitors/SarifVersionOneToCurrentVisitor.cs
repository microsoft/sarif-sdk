// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Utilities = Microsoft.CodeAnalysis.Sarif.Visitors.SarifTransformerUtilities;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SarifVersionOneToCurrentVisitor : SarifRewritingVisitorVersionOne
    {
        private static readonly SarifVersion FromSarifVersion = SarifVersion.OneZeroZero;
        private static readonly string FromPropertyBagPrefix =
            Utilities.PropertyBagTransformerItemPrefixes[FromSarifVersion];

        private Run _currentRun;
        private RunVersionOne _currentV1Run;
        private int _threadFlowLocationNestingLevel;
        private IDictionary<string, string> _v1KeyToFullyQualifiedNameMap;
        private IDictionary<LogicalLocation, int> _v2LogicalLocationToIndexMap;
        private IDictionary<string, LogicalLocation> _v1KeyToV2LogicalLocationMap;
        private IDictionary<string, string> _v1fullyQualifiedNameToDecoratedNameMap;

        public SarifLog SarifLog { get; private set; }

        public bool EmbedVersionOneContentInPropertyBag { get; set; }

        public override SarifLogVersionOne VisitSarifLogVersionOne(SarifLogVersionOne v1SarifLog)
        {
            _v2LogicalLocationToIndexMap = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);
            _v1KeyToV2LogicalLocationMap = new Dictionary<string, LogicalLocation>();

            SarifLog = new SarifLog(SarifVersion.Current.ConvertToSchemaUri(),
                                    SarifVersion.Current,
                                    new List<Run>(),
                                    properties: null);

            foreach (RunVersionOne v1Run in v1SarifLog.Runs)
            {
                SarifLog.Runs.Add(CreateRun(v1Run));
            }

            return null;
        }

        internal CodeFlow CreateCodeFlow(CodeFlowVersionOne v1CodeFlow)
        {
            CodeFlow codeFlow = null;

            if (v1CodeFlow != null)
            {
                codeFlow = new CodeFlow
                {
                    Message = CreateMessage(v1CodeFlow.Message),
                    Properties = v1CodeFlow.Properties
                };

                if (v1CodeFlow.Locations != null && v1CodeFlow.Locations.Count > 0)
                {
                    _threadFlowLocationNestingLevel = 0;
                    int executionOrder = 0;
                    var threadFlowDictionary = new Dictionary<int, ThreadFlow>();

                    foreach (AnnotatedCodeLocationVersionOne v1CodeLocation in v1CodeFlow.Locations)
                    {
                        ThreadFlow threadFlow;
                        int threadId = v1CodeLocation.ThreadId;

                        if (!threadFlowDictionary.TryGetValue(threadId, out threadFlow))
                        {
                            threadFlow = new ThreadFlow
                            {
                                Id = threadId.ToString(CultureInfo.InvariantCulture),
                                Locations = new List<ThreadFlowLocation>()
                            };
                            threadFlowDictionary.Add(threadId, threadFlow);
                        }

                        ThreadFlowLocation tfl = CreateThreadFlowLocation(v1CodeLocation);
                        tfl.ExecutionOrder = ++executionOrder;
                        threadFlow.Locations.Add(tfl);
                    }

                    codeFlow.ThreadFlows = threadFlowDictionary.Values.ToList();
                }
            }

            return codeFlow;
        }

        internal ThreadFlowLocation CreateThreadFlowLocation(AnnotatedCodeLocationVersionOne v1AnnotatedCodeLocation)
        {
            ThreadFlowLocation threadFlowLocation = null;

            if (v1AnnotatedCodeLocation != null)
            {
                threadFlowLocation = new ThreadFlowLocation
                {
                    Importance = Utilities.CreateThreadFlowLocationImportance(v1AnnotatedCodeLocation.Importance),
                    Location = CreateLocation(v1AnnotatedCodeLocation),
                    Module = v1AnnotatedCodeLocation.Module,
                    NestingLevel = _threadFlowLocationNestingLevel,
                    Properties = v1AnnotatedCodeLocation.Properties,
                    State = v1AnnotatedCodeLocation.State,
                };

                if (v1AnnotatedCodeLocation.Kind == AnnotatedCodeLocationKindVersionOne.Call)
                {
                    _threadFlowLocationNestingLevel++;
                }
                else if (v1AnnotatedCodeLocation.Kind == AnnotatedCodeLocationKindVersionOne.CallReturn)
                {
                    _threadFlowLocationNestingLevel--;
                }
            }

            return threadFlowLocation;
        }

        internal ExceptionData CreateExceptionData(ExceptionDataVersionOne v1ExceptionData)
        {
            ExceptionData exceptionData = null;

            if (v1ExceptionData != null)
            {
                exceptionData = new ExceptionData
                {
                    InnerExceptions = v1ExceptionData.InnerExceptions?.Select(CreateExceptionData).ToList(),
                    Kind = v1ExceptionData.Kind,
                    Message = v1ExceptionData.Message?.ToMessage(),
                    Stack = CreateStack(v1ExceptionData.Stack)
                };
            }

            return exceptionData;
        }

        internal FileChange CreateFileChange(FileChangeVersionOne v1FileChange)
        {
            FileChange fileChange = null;

            if (v1FileChange != null)
            {
                fileChange = new FileChange
                {
                    FileLocation = CreateFileLocation(v1FileChange),
                    Replacements = v1FileChange.Replacements?.Select(CreateReplacement).ToList()
                };
            }

            return fileChange;
        }

        internal FileData CreateFileData(FileDataVersionOne v1FileData)
        {
            FileData fileData = null;

            if (v1FileData != null)
            {
                fileData = new FileData
                {
                    Hashes = BuildHashesDictionary(v1FileData.Hashes),
                    Length = v1FileData.Length,
                    MimeType = v1FileData.MimeType,
                    Offset = v1FileData.Offset,
                    ParentKey = v1FileData.ParentKey,
                    Properties = v1FileData.Properties
                };

                if (v1FileData.Uri != null)
                {
                    fileData.FileLocation = new FileLocation
                    {
                        Uri = v1FileData.Uri,
                        UriBaseId = v1FileData.UriBaseId
                    };
                }

                if (v1FileData.Contents != null)
                {
                    fileData.Contents = new FileContent();

                    if (MimeType.IsTextualMimeType(v1FileData.MimeType))
                    {
                        fileData.Contents.Text = SarifUtilities.DecodeBase64String(v1FileData.Contents);
                    }
                    else
                    {
                        fileData.Contents.Binary = v1FileData.Contents;
                    }
                }
            }

            return fileData;
        }

        private IDictionary<string, string> BuildHashesDictionary(IList<HashVersionOne> hashes)
        {
            if (hashes == null) { return null; }

            var v2Hashes = new Dictionary<string, string>();

            foreach (HashVersionOne v1Hash in hashes)
            {
                v2Hashes[Utilities.AlgorithmKindNameMap[v1Hash.Algorithm]] = v1Hash.Value;
            }

            return v2Hashes;
        }

        internal FileLocation CreateFileLocation(Uri uri, string uriBaseId)
        {
            FileLocation fileLocation = null;

            if (uri != null)
            {
                fileLocation = new FileLocation
                {
                    Uri = uri,
                    UriBaseId = uriBaseId
                };
            }

            return fileLocation;
        }

        internal FileLocation CreateFileLocation(PhysicalLocationVersionOne v1PhysicalLocation)
        {
            return CreateFileLocation(v1PhysicalLocation?.Uri, v1PhysicalLocation?.UriBaseId);
        }

        internal FileLocation CreateFileLocation(FileChangeVersionOne v1FileChange)
        {
            return CreateFileLocation(v1FileChange?.Uri, v1FileChange?.UriBaseId);
        }

        internal Fix CreateFix(FixVersionOne v1Fix)
        {
            Fix fix = null;

            if (v1Fix != null)
            {
                fix = new Fix()
                {
                    Description = CreateMessage(v1Fix.Description),
                    FileChanges = v1Fix.FileChanges?.Select(CreateFileChange).ToList()
                };
            }

            return fix;
        }

        internal KeyValuePair<string, string> CreateHash(HashVersionOne v1Hash)
        {
            if (v1Hash == null) { return new KeyValuePair<string, string>(); }

            string algorithm;
            if (!Utilities.AlgorithmKindNameMap.TryGetValue(v1Hash.Algorithm, out algorithm))
            {
                algorithm = v1Hash.Algorithm.ToString().ToLowerInvariant();
            }

            return new KeyValuePair<string, string>(algorithm, v1Hash.Value);
        }

        internal Invocation CreateInvocation(InvocationVersionOne v1Invocation,
                                             IList<NotificationVersionOne> v1ToolNotifications,
                                             IList<NotificationVersionOne> v1ConfigurationNotifications)
        {
            Invocation invocation = CreateInvocation(v1Invocation);
            IList<Notification> toolNotifications = v1ToolNotifications?.Select(CreateNotification).ToList();
            IList<Notification> configurationNotifications = v1ConfigurationNotifications?.Select(CreateNotification).ToList(); ;

            if (toolNotifications?.Count > 0 || configurationNotifications?.Count > 0)
            {
                if (invocation == null)
                {
                    invocation = new Invocation();
                }

                invocation.ToolNotifications = toolNotifications;
                invocation.ConfigurationNotifications = configurationNotifications;
            }

            return invocation;
        }

        internal Invocation CreateInvocation(InvocationVersionOne v1Invocation)
        {
            Invocation invocation = null;

            if (v1Invocation != null)
            {
                invocation = new Invocation
                {
                    Account = v1Invocation.Account,
                    CommandLine = v1Invocation.CommandLine,
                    EndTimeUtc = v1Invocation.EndTime,
                    EnvironmentVariables = v1Invocation.EnvironmentVariables,
                    Machine = v1Invocation.Machine,
                    ProcessId = v1Invocation.ProcessId,
                    Properties = v1Invocation.Properties,
                    ResponseFiles = CreateResponseFilesList(v1Invocation.ResponseFiles),
                    StartTimeUtc = v1Invocation.StartTime,
                };

                if (!string.IsNullOrWhiteSpace(v1Invocation.FileName))
                {
                    invocation.ExecutableLocation = new FileLocation
                    {
                        Uri = new Uri(v1Invocation.FileName, UriKind.RelativeOrAbsolute)
                    };
                }

                if (!string.IsNullOrWhiteSpace(v1Invocation.WorkingDirectory))
                {
                    invocation.WorkingDirectory = new FileLocation
                    {
                        Uri = new Uri(v1Invocation.WorkingDirectory, UriKind.RelativeOrAbsolute)
                    };
                }
            }

            return invocation;
        }

        internal Location CreateLocation(LocationVersionOne v1Location)
        {
            Location location = null;

            if (v1Location != null)
            {
                location = new Location
                {
                    FullyQualifiedLogicalName = v1Location.FullyQualifiedLogicalName ?? v1Location.LogicalLocationKey,
                    PhysicalLocation = CreatePhysicalLocation(v1Location.ResultFile),
                    Properties = v1Location.Properties
                };

                if (!string.IsNullOrWhiteSpace(location.FullyQualifiedLogicalName))
                {
                    if (_v1KeyToV2LogicalLocationMap.TryGetValue(location.FullyQualifiedLogicalName, out LogicalLocation logicalLocation))
                    {
                        _v2LogicalLocationToIndexMap.TryGetValue(logicalLocation, out int index);

                        if (!string.IsNullOrEmpty(logicalLocation.DecoratedName))
                        {
                            logicalLocation.DecoratedName = v1Location.DecoratedName;
                            _v2LogicalLocationToIndexMap[logicalLocation] = index;
                        }

                        location.LogicalLocationIndex = index;
                    }
                }
            }

            return location;
        }

        internal Location CreateLocation(AnnotatedCodeLocationVersionOne v1AnnotatedCodeLocation)
        {
            Location location = null;

            if (v1AnnotatedCodeLocation != null)
            {
                location = new Location
                {
                    Annotations = v1AnnotatedCodeLocation.Annotations?.SelectMany(a => a.Locations,
                                                                                 (a, pl) => CreateRegion(v1AnnotatedCodeLocation.PhysicalLocation,
                                                                                                         pl,
                                                                                                         a.Message))
                                                                      .Where(r => r != null)
                                                                      .ToList(),
                    FullyQualifiedLogicalName = v1AnnotatedCodeLocation.LogicalLocationKey ?? v1AnnotatedCodeLocation.FullyQualifiedLogicalName,
                    Message = CreateMessage(v1AnnotatedCodeLocation.Message),
                    PhysicalLocation = CreatePhysicalLocation(v1AnnotatedCodeLocation.PhysicalLocation),
                    Properties = v1AnnotatedCodeLocation.Properties
                };

                string logicalLocationKey = v1AnnotatedCodeLocation.LogicalLocationKey ?? v1AnnotatedCodeLocation.FullyQualifiedLogicalName;

                if (!string.IsNullOrWhiteSpace(logicalLocationKey))
                {
                    if (_v1KeyToV2LogicalLocationMap.TryGetValue(logicalLocationKey, out LogicalLocation logicalLocation))
                    {
                        _v2LogicalLocationToIndexMap.TryGetValue(logicalLocation, out int index);
                        location.LogicalLocationIndex = index;
                    }
                }

                if (!string.IsNullOrWhiteSpace(v1AnnotatedCodeLocation.Snippet))
                {
                    if (location.PhysicalLocation == null)
                    {
                        location.PhysicalLocation = new PhysicalLocation();
                    }

                    if (location.PhysicalLocation.Region == null)
                    {
                        location.PhysicalLocation.Region = new Region();
                    }

                    location.PhysicalLocation.Region.Snippet = new FileContent
                    {
                        Text = v1AnnotatedCodeLocation.Snippet
                    };
                }
            }

            return location;
        }

        /// <summary>
        /// This overload of CreateLocation is used by CreateStackFrame to assemble
        /// a location object from a bunch of individual properties.
        /// </summary>
        internal Location CreateLocation(string fullyQualifiedLogicalName,
                                         string logicalLocationKey,
                                         string message,
                                         Uri uri,
                                         string uriBaseId,
                                         int column,
                                         int line)
        {
            var location = new Location
            {
                Message = CreateMessage(message)
            };

            logicalLocationKey = logicalLocationKey ?? fullyQualifiedLogicalName;

            // Retrieve logical location so that we can acquire the index
            LogicalLocation logicalLocation;
            _v1KeyToV2LogicalLocationMap.TryGetValue(logicalLocationKey, out logicalLocation);            

            location.FullyQualifiedLogicalName = fullyQualifiedLogicalName ?? logicalLocation?.FullyQualifiedName;

            int logicalLocationIndex;
            if (logicalLocation == null || !_v2LogicalLocationToIndexMap.TryGetValue(logicalLocation, out logicalLocationIndex))
            {
                logicalLocationIndex = -1;
            }

            location.LogicalLocationIndex = logicalLocationIndex;

            if (uri != null)
            {
                location.PhysicalLocation = new PhysicalLocation
                {
                    FileLocation = CreateFileLocation(uri, uriBaseId),
                    Region = CreateRegion(column, line)
                };
            }

            return location;
        }

        internal LogicalLocation CreateLogicalLocation(LogicalLocationVersionOne v1LogicalLocation, string fullyQualifiedName)
        {
            LogicalLocation logicalLocation = null;

            int parentIndex = -1;

            if (!string.IsNullOrEmpty(v1LogicalLocation.ParentKey) &&
                _v1KeyToV2LogicalLocationMap.TryGetValue(v1LogicalLocation.ParentKey, out LogicalLocation parentLogicalLocation))
            {
                _v2LogicalLocationToIndexMap.TryGetValue(parentLogicalLocation, out parentIndex);
            }

            _v1fullyQualifiedNameToDecoratedNameMap.TryGetValue(fullyQualifiedName, out string decoratedName);

            if (v1LogicalLocation != null)
            {
                logicalLocation = new LogicalLocation
                {
                    Kind = v1LogicalLocation.Kind,
                    Name = v1LogicalLocation.Name,
                    FullyQualifiedName = fullyQualifiedName != v1LogicalLocation.Name ? fullyQualifiedName : null,
                    DecoratedName = decoratedName,
                    ParentIndex = parentIndex
                };
            }

            return logicalLocation;
        }

        internal string GetLogicalLocationName(string fullyQualifiedLogicalName)
        {
            if (string.IsNullOrWhiteSpace(fullyQualifiedLogicalName))
            {
                throw new ArgumentNullException(nameof(fullyQualifiedLogicalName));
            }

            return fullyQualifiedLogicalName.Split(Utilities.DefaultFullyQualifiedNameDelimiters,
                                                   StringSplitOptions.RemoveEmptyEntries).Last();
        }

        internal Message CreateMessage(string text)
        {
            Message message = null;

            if (text != null)
            {
                message = new Message
                {
                    Text = text
                };
            }

            return message;
        }

        internal Notification CreateNotification(NotificationVersionOne v1Notification)
        {
            Notification notification = null;

            if (v1Notification != null)
            {
                notification = new Notification
                {
                    Exception = CreateExceptionData(v1Notification.Exception),
                    Id = v1Notification.Id,
                    Level = Utilities.CreateNotificationLevel(v1Notification.Level),
                    Message = CreateMessage(v1Notification.Message),
                    PhysicalLocation = CreatePhysicalLocation(v1Notification.PhysicalLocation),
                    Properties = v1Notification.Properties,
                    RuleId = v1Notification.RuleId,
                    ThreadId = v1Notification.ThreadId,
                    TimeUtc = v1Notification.Time
                };
            }

            return notification;
        }

        internal Replacement CreateReplacement(ReplacementVersionOne v1Replacement)
        {
            Replacement replacement = null;

            if (v1Replacement != null)
            {
                replacement = new Replacement();
                
                if (v1Replacement.InsertedBytes != null)
                {
                    replacement.InsertedContent = new FileContent
                    {
                        Binary = v1Replacement.InsertedBytes
                    };
                }

                replacement.DeletedRegion = new Region
                {
                    ByteLength = v1Replacement.DeletedLength,
                    ByteOffset = v1Replacement.Offset
                };
            }

            return replacement;
        }

        internal IList<FileLocation> CreateResponseFilesList(IDictionary<string, string> responseFileToContentsDictionary)
        {
            List<FileLocation> fileLocations = null;

            if (responseFileToContentsDictionary != null)
            {
                fileLocations = new List<FileLocation>();

                foreach (string key in responseFileToContentsDictionary.Keys)
                {
                    var fileLocation = new FileLocation
                    {
                        Uri = new Uri(key, UriKind.RelativeOrAbsolute)
                    };
                    fileLocations.Add(fileLocation);

                    if (_currentRun != null && !string.IsNullOrWhiteSpace(responseFileToContentsDictionary[key]))
                    {
                        // We have contents, so mention this file in _currentRun.files
                        if (_currentRun.Files == null)
                        {
                            _currentRun.Files = new Dictionary<string, FileData>();
                        }

                        if (!_currentRun.Files.ContainsKey(key))
                        {
                            _currentRun.Files.Add(key, new FileData());
                        }

                        FileData responseFile = _currentRun.Files[key];

                        responseFile.Contents = new FileContent
                        {
                            Text = responseFileToContentsDictionary[key]
                        };
                        responseFile.FileLocation = fileLocation;
                    }
                }
            }

            return fileLocations;
        }

        internal PhysicalLocation CreatePhysicalLocation(PhysicalLocationVersionOne v1PhysicalLocation)
        {
            PhysicalLocation physicalLocation = null;

            if (v1PhysicalLocation != null)
            {
                physicalLocation = new PhysicalLocation
                {
                    FileLocation = CreateFileLocation(v1PhysicalLocation),
                    Region = CreateRegion(v1PhysicalLocation.Region)
                };
            }

            return physicalLocation;
        }

        internal Region CreateRegion(RegionVersionOne v1Region)
        {
            Region region = null;

            if (v1Region != null)
            {
                region = new Region
                {
                    ByteLength = v1Region.Length,
                    ByteOffset = v1Region.Offset,
                    EndColumn = v1Region.EndColumn,
                    EndLine = v1Region.EndLine,
                    StartColumn = v1Region.StartColumn,
                    StartLine = v1Region.StartLine
                };

                bool startIsTextBased = v1Region.StartLine > 0;
                bool endIsTextBased = v1Region.EndLine > 0 || v1Region.EndColumn > 0;

                if (startIsTextBased && endIsTextBased && v1Region.EndColumn == 0)
                {
                    region.EndColumn = v1Region.StartColumn;
                }
            }

            return region;
        }

        internal Region CreateRegion(PhysicalLocationVersionOne v1AnnotationLocation, PhysicalLocationVersionOne v1PhysicalLocation, string message)
        {
            Region region = null;

            // In SARIF v1, a location could have annotations that referred to files other than the location's own file.
            // That made no sense. In SARIF v2, a location can only be annotated with regions in the same file.
            // So only copy the v1 annotations that refer to the same file as the location.
            if (v1PhysicalLocation != null && v1AnnotationLocation.Uri == v1PhysicalLocation.Uri)
            {
                region = CreateRegion(v1PhysicalLocation.Region);
                region.Message = CreateMessage(message);
            }

            return region;
        }

        internal Region CreateRegion(int startColumn, int startLine, int endColumn = 0, int endLine = 0, int length = 0, int offset = 0)
        {
            Region region = null;

            if (startColumn > 0 || startLine > 0 || endColumn > 0 || endLine > 0 || length > 0 || offset > 0)
            {
                region = new Region
                {
                    ByteLength = length,
                    ByteOffset = offset,
                    EndColumn = endColumn > 0 ? endColumn : startColumn,
                    EndLine = endLine,
                    StartColumn = startColumn,
                    StartLine = startLine
                };
            }

            return region;
        }

        internal Result CreateResult(ResultVersionOne v1Result)
        {
            Result result = null;

            if (v1Result != null)
            {
                result = new Result
                {
                    BaselineState = Utilities.CreateBaselineState(v1Result.BaselineState),
                    CodeFlows = v1Result.CodeFlows?.Select(CreateCodeFlow).ToList(),
                    Fixes = v1Result.Fixes?.Select(CreateFix).ToList(),
                    InstanceGuid = v1Result.Id,
                    Level = Utilities.CreateResultLevel(v1Result.Level),
                    Locations = v1Result.Locations?.Select(CreateLocation).ToList(),
                    Message = CreateMessage(v1Result.Message),
                    Properties = v1Result.Properties,
                    RelatedLocations = v1Result.RelatedLocations?.Select(CreateLocation).ToList(),
                    Stacks = v1Result.Stacks?.Select(CreateStack).ToList(),
                    SuppressionStates = Utilities.CreateSuppressionStates(v1Result.SuppressionStates)
                };

                // The spec says that analysisTarget is required only if it differs from the result file.
                if (v1Result.Locations?[0]?.AnalysisTarget?.Uri != v1Result.Locations?[0]?.ResultFile?.Uri)
                {
                    result.AnalysisTarget = CreateFileLocation(v1Result.Locations[0].AnalysisTarget);
                }

                if (v1Result.RuleKey == null)
                {
                    result.RuleId = v1Result.RuleId;
                }
                else
                {
                    if (v1Result.RuleId == null)
                    {
                        result.RuleId = v1Result.RuleKey;
                    }
                    else
                    {
                        if (v1Result.RuleId == v1Result.RuleKey)
                        {
                            result.RuleId = v1Result.RuleId;
                        }
                        else
                        {
                            result.RuleId = v1Result.RuleKey;

                            if (_currentRun.Resources == null)
                            {
                                _currentRun.Resources = new Resources();
                            }

                            if (_currentRun.Resources.Rules == null)
                            {
                                _currentRun.Resources.Rules = new Dictionary<string, Rule>();
                            }

                            IDictionary<string, Rule> rules = _currentRun.Resources.Rules;

                            if (!rules.ContainsKey(v1Result.RuleKey))
                            {
                                Rule rule = new Rule() { Id = v1Result.RuleId };
                                rules.Add(v1Result.RuleKey, rule);
                            }

                            Debug.Assert(rules[v1Result.RuleKey].Id == v1Result.RuleId);
                        }
                    }
                }
                
                if (v1Result.FormattedRuleMessage != null)
                {
                    if (result.Message == null)
                    {
                        result.Message = new Message() { MessageId = v1Result.FormattedRuleMessage.FormatId };
                    }

                    result.Message.Arguments = v1Result.FormattedRuleMessage.Arguments;
                }

                if (!string.IsNullOrWhiteSpace(v1Result.ToolFingerprintContribution))
                {
                    result.PartialFingerprints = new Dictionary<string, string>
                    {
                        { "Fingerprint", v1Result.ToolFingerprintContribution }
                    };
                }

                if (!string.IsNullOrWhiteSpace(v1Result.Snippet))
                {
                    if (result.Locations == null)
                    {
                        result.Locations = new List<Location>();
                    }

                    if (result.Locations.Count == 0)
                    {
                        result.Locations.Add(new Location());
                    }

                    if (result.Locations[0].PhysicalLocation == null)
                    {
                        result.Locations[0].PhysicalLocation = new PhysicalLocation();
                    }

                    if (result.Locations[0].PhysicalLocation.Region == null)
                    {
                        result.Locations[0].PhysicalLocation.Region = new Region();
                    }

                    result.Locations[0].PhysicalLocation.Region.Snippet = new FileContent
                    {
                        Text = v1Result.Snippet
                    };
                }
            }

            return result;
        }

        internal Rule CreateRule(RuleVersionOne v1Rule)
        {
            Rule rule = null;

            if (v1Rule != null)
            {
                rule = new Rule
                {
                    FullDescription = CreateMessage(v1Rule.FullDescription),
                    HelpUri = v1Rule.HelpUri,
                    Id = v1Rule.Id,
                    MessageStrings = v1Rule.MessageFormats,
                    Name = CreateMessage(v1Rule.Name),
                    Properties = v1Rule.Properties,
                    ShortDescription = CreateMessage(v1Rule.ShortDescription)
                };

                RuleConfigurationDefaultLevel level = Utilities.CreateRuleConfigurationDefaultLevel(v1Rule.DefaultLevel);

                if (v1Rule.Configuration == RuleConfigurationVersionOne.Enabled ||
                    level != RuleConfigurationDefaultLevel.Warning)
                {
                    rule.Configuration = new RuleConfiguration
                    {
                        DefaultLevel = level,
                        Enabled = v1Rule.Configuration == RuleConfigurationVersionOne.Enabled
                    };
                }
            }

            return rule;
        }

        internal Run CreateRun(RunVersionOne v1Run)
        {
            Run run = null;

            if (v1Run != null)
            {
                if (v1Run.TryGetProperty("sarifv2/run", out run))
                {
                    return run;
                }
                else
                {
                    _currentV1Run = v1Run;

                    RunAutomationDetails id = null;
                    RunAutomationDetails[] aggregateIds = null;

                    if (v1Run.Id != null || v1Run.StableId != null)
                    {
                        id = new RunAutomationDetails
                        {
                            InstanceGuid = v1Run.Id,
                            InstanceId = v1Run.StableId != null ? v1Run.StableId + "/" : null
                        };
                    }

                    if (v1Run.AutomationId != null)
                    {
                        aggregateIds = new[] { new RunAutomationDetails { InstanceId = v1Run.AutomationId + "/" } };
                    }

                    run = new Run()
                    {
                        Id = id,
                        AggregateIds = aggregateIds,
                        BaselineInstanceGuid = v1Run.BaselineId,
                        Properties = v1Run.Properties,
                        Tool = CreateTool(v1Run.Tool),
                        ColumnKind = ColumnKind.Utf16CodeUnits
                    };

                    _currentRun = run;

                    if (v1Run.Rules != null)
                    {
                        run.Resources = new Resources
                        {
                            Rules = new Dictionary<string, Rule>()
                        };

                        foreach (var pair in v1Run.Rules)
                        {
                            run.Resources.Rules.Add(pair.Key, CreateRule(pair.Value));
                        }
                    }

                    if (v1Run.Files != null)
                    {
                        run.Files = new Dictionary<string, FileData>();

                        foreach (var pair in v1Run.Files)
                        {
                            run.Files.Add(pair.Key, CreateFileData(pair.Value));
                        }
                    }

                    // Pass 1 over results. In this phase, we're simply collecting fully qualified names that
                    // may be duplicated in the logical locations dictionary. We're doing this so that we 
                    // can properly construct the v2 logical instances in the converted array (i.e., we can't
                    // populate the v2 logicalLocation.FullyQualifiedName property in cases where the 
                    // v1 key is a synthesized value and not actually the fully qualified name)
                    var visitor = new VersionOneLogicalLocationKeyToFullyQualifiedNameMappingVisitor();
                    visitor.VisitRunVersionOne(v1Run);

                    _v1KeyToFullyQualifiedNameMap = visitor.LogicalLocationKeyToFullyQualifiedNameMap;
                    _v1fullyQualifiedNameToDecoratedNameMap = visitor.FullyQualifiedNameToDecoratedNameMap;

                    if (v1Run.LogicalLocations != null)
                    {
                        run.LogicalLocations = new List<LogicalLocation>();
                        HashSet<string> populatedKeys = new HashSet<string>();

                        foreach (KeyValuePair<string, LogicalLocationVersionOne> pair in v1Run.LogicalLocations)
                        {
                            PopulateLogicalLocation(
                                run, 
                                v1Run.LogicalLocations,
                                _v1fullyQualifiedNameToDecoratedNameMap,
                                _v1KeyToFullyQualifiedNameMap,
                                pair.Key, 
                                pair.Value,
                                populatedKeys);
                        }
                    }

                    // Even if there is no v1 invocation, there may be notifications
                    // in which case we will need a v2 invocation to contain them
                    Invocation invocation = CreateInvocation(v1Run.Invocation,
                                                             v1Run.ToolNotifications,
                                                             v1Run.ConfigurationNotifications);

                    if (invocation != null)
                    {
                        run.Invocations = new List<Invocation>()
                        {
                            invocation
                        };
                    }

                    if (v1Run.Results != null)
                    {
                        run.Results = new List<Result>();

                        foreach (ResultVersionOne v1Result in v1Run.Results)
                        {
                            Result result = CreateResult(v1Result);
                            run.Results.Add(result);
                        }
                    }

                    // Stash the entire v1 run in this v2 run's property bag
                    if (EmbedVersionOneContentInPropertyBag)
                    {
                        run.SetProperty($"{FromPropertyBagPrefix}/run", v1Run);
                    }
                }
            }

            _currentRun = null;

            return run;
        }

        private void PopulateLogicalLocation(
            Run v2Run, 
            IDictionary<string, LogicalLocationVersionOne> v1LogicalLocations,
            IDictionary<string, string> fullyQualifiedNameToDecoratedNameMap,
            IDictionary<string, string> keyToFullyQualifiedNameMap,
            string logicalLocationKey, 
            LogicalLocationVersionOne v1LogicalLocation, 
            HashSet<string> populatedKeys)
        {
            // We saw and populated this one previously, because it was a parent to 
            // a logical location that we encountered earlier
            if (populatedKeys.Contains(logicalLocationKey)) { return; }

            if (v1LogicalLocation.ParentKey != null && !populatedKeys.Contains(v1LogicalLocation.ParentKey))
            {
                // Ensure that any parent has been populated 
                PopulateLogicalLocation(
                    v2Run, v1LogicalLocations,
                    fullyQualifiedNameToDecoratedNameMap,
                    keyToFullyQualifiedNameMap,
                    v1LogicalLocation.ParentKey,
                    v1LogicalLocations[v1LogicalLocation.ParentKey],
                    populatedKeys);
            }

            string fullyQualifiedName;

            if (!keyToFullyQualifiedNameMap.TryGetValue(logicalLocationKey, out fullyQualifiedName))
            {
                // If we don't find a remapping, the dictionary key itself comprises
                // the fully qualified name.
                fullyQualifiedName = logicalLocationKey;
            }

            // Create the logical location from the v1 version
            LogicalLocation logicalLocation = CreateLogicalLocation(v1LogicalLocation, fullyQualifiedName);

            // Remember the index that is associated with the new logical location
            _v2LogicalLocationToIndexMap[logicalLocation] = v2Run.LogicalLocations.Count;

            // Store the old v1 look-up key for the new logical location
            // We will use this to generate the index when we walk results
            // v1 key -> logical location -> logical location index
            _v1KeyToV2LogicalLocationMap[logicalLocationKey] = logicalLocation;

            v2Run.LogicalLocations.Add(logicalLocation);

            populatedKeys.Add(logicalLocationKey);
        }

        internal Stack CreateStack(StackVersionOne v1Stack)
        {
            Stack stack = null;

            if (v1Stack != null)
            {
                stack = new Stack
                {
                    Message = CreateMessage(v1Stack.Message),
                    Properties = v1Stack.Properties,
                    Frames = v1Stack.Frames?.Select(CreateStackFrame).ToList()
                };
            }

            return stack;
        }

        internal StackFrame CreateStackFrame(StackFrameVersionOne v1StackFrame)
        {
            StackFrame stackFrame = null;

            if (v1StackFrame != null)
            {
                stackFrame = new StackFrame
                {
                    Address = v1StackFrame.Address,
                    Module = v1StackFrame.Module,
                    Offset = v1StackFrame.Offset,
                    Parameters = v1StackFrame.Parameters,
                    Properties = v1StackFrame.Properties,
                    ThreadId = v1StackFrame.ThreadId
                };
            }

            stackFrame.Location = CreateLocation(v1StackFrame.FullyQualifiedLogicalName,
                                                 v1StackFrame.LogicalLocationKey,
                                                 v1StackFrame.Message,
                                                 v1StackFrame.Uri,
                                                 v1StackFrame.UriBaseId,
                                                 v1StackFrame.Column,
                                                 v1StackFrame.Line);

            return stackFrame;
        }

        internal Tool CreateTool(ToolVersionOne v1Tool)
        {
            Tool tool = null;

            if (v1Tool != null)
            {
                tool = new Tool()
                {
                    DottedQuadFileVersion = v1Tool.FileVersion,
                    FullName = v1Tool.FullName,
                    Language = v1Tool.Language,
                    Name = v1Tool.Name,
                    Properties = v1Tool.Properties,
                    SarifLoggerVersion = v1Tool.SarifLoggerVersion,
                    SemanticVersion = v1Tool.SemanticVersion,
                    Version = v1Tool.Version
                };
            }

            return tool;
        }
    }
}

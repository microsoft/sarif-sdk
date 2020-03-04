// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        private int _threadFlowLocationNestingLevel;
        private IDictionary<string, int> _v1FileKeytoV2IndexMap;
        private IDictionary<string, int> _v1RuleKeyToV2IndexMap;

        private IDictionary<string, string> _v1KeyToFullyQualifiedNameMap;
        private IDictionary<LogicalLocation, int> _v2LogicalLocationToIndexMap;
        private IDictionary<string, LogicalLocation> _v1KeyToV2LogicalLocationMap;
        private IDictionary<string, string> _v1LogicalLocationKeyToDecoratedNameMap;

        public SarifLog SarifLog { get; private set; }

        public bool EmbedVersionOneContentInPropertyBag { get; set; }

        public override SarifLogVersionOne VisitSarifLogVersionOne(SarifLogVersionOne v1SarifLog)
        {
            _v2LogicalLocationToIndexMap = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);
            _v1KeyToV2LogicalLocationMap = new Dictionary<string, LogicalLocation>();

            SarifLog = new SarifLog(SarifVersion.Current.ConvertToSchemaUri(),
                                    SarifVersion.Current,
                                    new List<Run>(),
                                    null,
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
                        int threadId = v1CodeLocation.ThreadId;

                        if (!threadFlowDictionary.TryGetValue(threadId, out ThreadFlow threadFlow))
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
                    State = v1AnnotatedCodeLocation.State.ConvertToMultiformatMessageStringsDictionary(),
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
                    Message = v1ExceptionData.Message,
                    Stack = CreateStack(v1ExceptionData.Stack)
                };
            }

            return exceptionData;
        }

        internal ArtifactChange CreateFileChange(FileChangeVersionOne v1FileChange)
        {
            ArtifactChange fileChange = null;

            if (v1FileChange != null)
            {
                fileChange = new ArtifactChange
                {
                    ArtifactLocation = CreateFileLocation(v1FileChange),
                    Replacements = v1FileChange.Replacements?.Select(CreateReplacement).ToList()
                };
            }

            return fileChange;
        }

        internal Artifact CreateFileData(FileDataVersionOne v1FileData, string key)
        {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }

            Artifact fileData = null;

            if (v1FileData != null)
            {
                string parentKey = v1FileData.ParentKey;
                int parentIndex = parentKey == null
                    ? -1
                    : _v1FileKeytoV2IndexMap[parentKey];

                fileData = new Artifact
                {
                    Hashes = v1FileData.Hashes?.Select(CreateHash).ToDictionary(p => p.Key, p => p.Value),
                    Length = v1FileData.Length == 0 ? -1 : v1FileData.Length,
                    MimeType = v1FileData.MimeType,
                    Offset = v1FileData.Offset,
                    ParentIndex = parentIndex,
                    Properties = v1FileData.Properties
                };

                fileData.Location = ArtifactLocation.CreateFromFilesDictionaryKey(key, parentKey);
                fileData.Location.UriBaseId = v1FileData.UriBaseId;

                if (v1FileData.Contents != null)
                {
                    fileData.Contents = new ArtifactContent();

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

        internal ArtifactLocation CreateFileLocation(Uri uri, string uriBaseId)
        {
            ArtifactLocation fileLocation = null;

            if (uri != null)
            {
                if (_v1FileKeytoV2IndexMap.TryGetValue(uri.OriginalString, out int fileIndex))
                {
                    fileLocation = _currentRun.Artifacts[fileIndex].Location;
                }
                else
                {
                    fileLocation = new ArtifactLocation
                    {
                        Uri = uri,
                        UriBaseId = uriBaseId
                    };
                }
            }

            return fileLocation;
        }

        internal ArtifactLocation CreateFileLocation(PhysicalLocationVersionOne v1PhysicalLocation)
        {
            return CreateFileLocation(v1PhysicalLocation?.Uri, v1PhysicalLocation?.UriBaseId);
        }

        internal ArtifactLocation CreateFileLocation(FileChangeVersionOne v1FileChange)
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
                    ArtifactChanges = v1Fix.FileChanges?.Select(CreateFileChange).ToList()
                };
            }

            return fix;
        }

        internal KeyValuePair<string, string> CreateHash(HashVersionOne v1Hash)
        {
            if (v1Hash == null) { return new KeyValuePair<string, string>(); }

            if (!Utilities.AlgorithmKindNameMap.TryGetValue(v1Hash.Algorithm, out string algorithm))
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

                invocation.ToolExecutionNotifications = toolNotifications;
                invocation.ToolConfigurationNotifications = configurationNotifications;
                invocation.ExecutionSuccessful = true;
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
                    ExecutionSuccessful = true
                };

                if (!string.IsNullOrWhiteSpace(v1Invocation.FileName))
                {
                    invocation.ExecutableLocation = new ArtifactLocation
                    {
                        Uri = new Uri(v1Invocation.FileName, UriKind.RelativeOrAbsolute)
                    };
                }

                if (!string.IsNullOrWhiteSpace(v1Invocation.WorkingDirectory))
                {
                    invocation.WorkingDirectory = new ArtifactLocation
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

            string key = v1Location.LogicalLocationKey ?? v1Location.FullyQualifiedLogicalName;

            if (v1Location != null)
            {
                location = new Location
                {
                    PhysicalLocation = CreatePhysicalLocation(v1Location.ResultFile ?? v1Location.AnalysisTarget),
                    Properties = v1Location.Properties
                };

                if (!string.IsNullOrWhiteSpace(v1Location.FullyQualifiedLogicalName))
                {
                    location.LogicalLocation = new LogicalLocation
                    {
                        FullyQualifiedName = v1Location.FullyQualifiedLogicalName
                    };
                }

                if (!string.IsNullOrWhiteSpace(location.LogicalLocation?.FullyQualifiedName))
                {
                    if (_v1KeyToV2LogicalLocationMap.TryGetValue(key, out LogicalLocation logicalLocation))
                    {
                        _v2LogicalLocationToIndexMap.TryGetValue(logicalLocation, out int index);

                        if (!string.IsNullOrEmpty(logicalLocation.DecoratedName))
                        {
                            logicalLocation.DecoratedName = v1Location.DecoratedName;
                            _v2LogicalLocationToIndexMap[logicalLocation] = index;
                        }

                        location.LogicalLocation.Index = index;
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
                    Message = CreateMessage(v1AnnotatedCodeLocation.Message),
                    PhysicalLocation = CreatePhysicalLocation(v1AnnotatedCodeLocation.PhysicalLocation),
                    Properties = v1AnnotatedCodeLocation.Properties
                };

                if (!string.IsNullOrWhiteSpace(v1AnnotatedCodeLocation.FullyQualifiedLogicalName))
                {
                    location.LogicalLocation = new LogicalLocation
                    {
                        FullyQualifiedName = v1AnnotatedCodeLocation.FullyQualifiedLogicalName
                    };
                }

                string logicalLocationKey = v1AnnotatedCodeLocation.LogicalLocationKey ?? v1AnnotatedCodeLocation.FullyQualifiedLogicalName;

                if (!string.IsNullOrWhiteSpace(logicalLocationKey))
                {
                    if (_v1KeyToV2LogicalLocationMap.TryGetValue(logicalLocationKey, out LogicalLocation logicalLocation))
                    {
                        _v2LogicalLocationToIndexMap.TryGetValue(logicalLocation, out int index);

                        if (location.LogicalLocation == null)
                        {
                            location.LogicalLocation = new LogicalLocation();
                        }

                        location.LogicalLocation.Index = index;
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

                    location.PhysicalLocation.Region.Snippet = new ArtifactContent
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
                                         int line,
                                         int address,
                                         int offset)
        {
            var location = new Location
            {
                Message = CreateMessage(message)
            };

            logicalLocationKey = logicalLocationKey ?? fullyQualifiedLogicalName;

            // Retrieve logical location so that we can acquire the index
            _v1KeyToV2LogicalLocationMap.TryGetValue(logicalLocationKey, out LogicalLocation logicalLocation);

            location.LogicalLocation = new LogicalLocation
            {
                FullyQualifiedName = fullyQualifiedLogicalName ?? logicalLocation?.FullyQualifiedName
            };

            if (logicalLocation == null || !_v2LogicalLocationToIndexMap.TryGetValue(logicalLocation, out int logicalLocationIndex))
            {
                logicalLocationIndex = -1;
            }

            location.LogicalLocation.Index = logicalLocationIndex;

            if (uri != null)
            {
                location.PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = CreateFileLocation(uri, uriBaseId),
                    Region = CreateRegion(column, line)
                };
            }

            // https://github.com/Microsoft/sarif-sdk/issues/1469
            // TODO: Ensure stackFrame.address and offset properties are being transformed correctly during v1 -> v2 conversion and vice-versa

            //if (address > 0 || offset > 0)
            //{
            //    if (location.PhysicalLocation == null)
            //    {
            //        location.PhysicalLocation = new PhysicalLocation();
            //    }

            //    location.PhysicalLocation.Address = new Address
            //    {
            //        AbsoluteAddress = address,
            //        OffsetFromParent = offset
            //    };
            //}
            return location;
        }

        internal LogicalLocation CreateLogicalLocation(LogicalLocationVersionOne v1LogicalLocation, string fullyQualifiedName, string logicalLocationKey)
        {
            LogicalLocation logicalLocation = null;

            int parentIndex = -1;

            if (!string.IsNullOrEmpty(v1LogicalLocation.ParentKey) &&
                _v1KeyToV2LogicalLocationMap.TryGetValue(v1LogicalLocation.ParentKey, out LogicalLocation parentLogicalLocation))
            {
                _v2LogicalLocationToIndexMap.TryGetValue(parentLogicalLocation, out parentIndex);
            }

            _v1LogicalLocationKeyToDecoratedNameMap.TryGetValue(logicalLocationKey, out string decoratedName);

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

        internal MultiformatMessageString CreateMultiformatMessageString(string text)
        {
            MultiformatMessageString multiformatMessageString = null;

            if (text != null)
            {
                multiformatMessageString = new MultiformatMessageString
                {
                    Text = text
                };
            }

            return multiformatMessageString;
        }

        internal Notification CreateNotification(NotificationVersionOne v1Notification)
        {
            Notification notification = null;

            if (v1Notification != null)
            {
                notification = new Notification
                {
                    Exception = CreateExceptionData(v1Notification.Exception),
                    Level = Utilities.CreateFailureLevel(v1Notification.Level),
                    Message = CreateMessage(v1Notification.Message),
                    Locations = CreateLocations(v1Notification.PhysicalLocation),
                    Properties = v1Notification.Properties,
                    ThreadId = v1Notification.ThreadId,
                    TimeUtc = v1Notification.Time
                };

                if (!string.IsNullOrWhiteSpace(v1Notification.Id))
                {
                    notification.Descriptor = new ReportingDescriptorReference
                    {
                        Id = v1Notification.Id,
                    };
                }

                if (!string.IsNullOrWhiteSpace(v1Notification.RuleId))
                {
                    notification.AssociatedRule = new ReportingDescriptorReference
                    {
                        Id = v1Notification.RuleId,
                    };
                }
            }

            return notification;
        }

        private List<Location> CreateLocations(PhysicalLocationVersionOne v1PhysicalLocation)
        {
            List<Location> locations = null;

            if (v1PhysicalLocation != null)
            {
                locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = CreatePhysicalLocation(v1PhysicalLocation)
                    }
                };
            }

            return locations;
        }

        internal Replacement CreateReplacement(ReplacementVersionOne v1Replacement)
        {
            Replacement replacement = null;

            if (v1Replacement != null)
            {
                replacement = new Replacement();

                if (v1Replacement.InsertedBytes != null)
                {
                    replacement.InsertedContent = new ArtifactContent
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

        internal IList<ArtifactLocation> CreateResponseFilesList(IDictionary<string, string> responseFileToContentsDictionary)
        {
            List<ArtifactLocation> fileLocations = null;

            if (responseFileToContentsDictionary != null)
            {
                fileLocations = new List<ArtifactLocation>();

                foreach (string key in responseFileToContentsDictionary.Keys)
                {
                    // If the response file is mentioned in Run.Files, use the FileLocation
                    // object from there (which, conveniently, already has the FileIndex property
                    // set); otherwise create a new FileLocation.
                    ArtifactLocation fileLocation = null;
                    Artifact responseFile = null;
                    bool existsInRunFiles = _v1FileKeytoV2IndexMap.TryGetValue(key, out int responseFileIndex);
                    if (existsInRunFiles)
                    {
                        responseFile = _currentRun.Artifacts[responseFileIndex];
                        fileLocation = responseFile.Location;
                    }
                    else
                    {
                        fileLocation = new ArtifactLocation
                        {
                            Uri = new Uri(key, UriKind.RelativeOrAbsolute)
                        };
                    }

                    // If this response file has contents, add it to Run.Files, if it
                    // isn't already there.
                    string responseFileText = responseFileToContentsDictionary[key];
                    if (!string.IsNullOrWhiteSpace(responseFileText))
                    {
                        if (!existsInRunFiles)
                        {
                            _currentRun.Artifacts = _currentRun.Artifacts ?? new List<Artifact>();
                            fileLocation.Index = _currentRun.Artifacts.Count;

                            responseFile = new Artifact
                            {
                                Location = fileLocation
                            };

                            _currentRun.Artifacts.Add(responseFile);
                        }

                        // At this point, responseFile is guaranteed to be initialized and to exist
                        // in Run.Files, either because it previously existed in Run.Files and we
                        // obtained it above, or because it didn't exist and we just created it and
                        // added it to Run.Files. Either way, we can now add the content.
                        responseFile.Contents = new ArtifactContent
                        {
                            Text = responseFileText
                        };
                    }

                    fileLocations.Add(fileLocation);
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
                    ArtifactLocation = CreateFileLocation(v1PhysicalLocation),
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

                if (region.ByteLength <= 0)
                {
                    region.ByteOffset = -1;
                }

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

                if (region.ByteLength <= 0)
                {
                    region.ByteOffset = -1;
                }
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
                    Guid = v1Result.Id,
                    Level = Utilities.CreateFailureLevel(v1Result.Level),
                    Kind = Utilities.CreateResultKind(v1Result.Level),
                    Locations = v1Result.Locations?.Select(CreateLocation).ToList(),
                    Message = CreateMessage(v1Result.Message),
                    Properties = v1Result.Properties,
                    RelatedLocations = v1Result.RelatedLocations?.Select(CreateLocation).ToList(),
                    Stacks = v1Result.Stacks?.Select(CreateStack).ToList(),
                    Suppressions = Utilities.CreateSuppressions(v1Result.SuppressionStates)
                };

                // The v2 spec says that analysisTarget is required only if it differs from the result location.
                // On the other hand, the v1 spec says that if the result is found in the file that the tool
                // was instructed to scan, then analysisTarget should be present and resultFile should be
                // absent -- so we should _not_ populate the v2 analysisTarget in this case.
                LocationVersionOne v1Location = v1Result.Locations?.FirstOrDefault();
                if (v1Location?.ResultFile != null && v1Location.AnalysisTarget?.Uri != v1Location.ResultFile.Uri)
                {
                    result.AnalysisTarget = CreateFileLocation(v1Result.Locations[0].AnalysisTarget);
                }

                result.RuleId = v1Result.RuleId;

                string ruleKey = v1Result.RuleKey ?? v1Result.RuleId;
                result.RuleIndex = GetRuleIndexForRuleKey(ruleKey, _v1RuleKeyToV2IndexMap);

                if (v1Result.FormattedRuleMessage != null)
                {
                    if (result.Message == null)
                    {
                        result.Message = new Message() { Id = v1Result.FormattedRuleMessage.FormatId };
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

                    result.Locations[0].PhysicalLocation.Region.Snippet = new ArtifactContent
                    {
                        Text = v1Result.Snippet
                    };
                }
            }

            return result;
        }

        internal ReportingDescriptor CreateRule(RuleVersionOne v1Rule)
        {
            ReportingDescriptor rule = null;

            if (v1Rule != null)
            {
                rule = new ReportingDescriptor
                {
                    FullDescription = CreateMultiformatMessageString(v1Rule.FullDescription),
                    HelpUri = v1Rule.HelpUri,
                    Id = v1Rule.Id,
                    MessageStrings = v1Rule.MessageFormats.ConvertToMultiformatMessageStringsDictionary(),
                    Name = v1Rule.Name,
                    Properties = v1Rule.Properties,
                    ShortDescription = CreateMultiformatMessageString(v1Rule.ShortDescription)
                };

                FailureLevel level = Utilities.CreateReportingConfigurationDefaultLevel(v1Rule.DefaultLevel);

                rule.DefaultConfiguration = new ReportingConfiguration
                {
                    Level = level,
                    Enabled = v1Rule.Configuration != RuleConfigurationVersionOne.Disabled
                };
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
                    _v1FileKeytoV2IndexMap = CreateFileKeyToIndexMapping(v1Run.Files);
                    _v1RuleKeyToV2IndexMap = CreateRuleKeyToIndexMapping(v1Run.Rules);

                    RunAutomationDetails id = null;
                    RunAutomationDetails[] aggregateIds = null;

                    if (v1Run.Id != null || v1Run.StableId != null)
                    {
                        id = new RunAutomationDetails
                        {
                            Guid = v1Run.Id,
                            Id = v1Run.StableId != null ? v1Run.StableId + "/" : null
                        };
                    }

                    if (v1Run.AutomationId != null)
                    {
                        aggregateIds = new[] { new RunAutomationDetails { Id = v1Run.AutomationId + "/" } };
                    }

                    run = new Run()
                    {
                        AutomationDetails = id,
                        RunAggregates = aggregateIds,
                        BaselineGuid = v1Run.BaselineId,
                        Properties = v1Run.Properties,
                        Language = v1Run.Tool?.Language ?? "en-US",
                        Tool = CreateTool(v1Run.Tool),
                        ColumnKind = ColumnKind.Utf16CodeUnits
                    };

                    _currentRun = run;

                    if (v1Run.Rules != null)
                    {
                        run.Tool.Driver.Rules = new List<ReportingDescriptor>();

                        foreach (KeyValuePair<string, RuleVersionOne> pair in v1Run.Rules)
                        {
                            run.Tool.Driver.Rules.Add(CreateRule(pair.Value));
                        }
                    }

                    if (v1Run.Files != null)
                    {
                        run.Artifacts = new List<Artifact>();

                        foreach (KeyValuePair<string, FileDataVersionOne> pair in v1Run.Files)
                        {
                            FileDataVersionOne fileDataVersionOne = pair.Value;
                            if (fileDataVersionOne.Uri == null)
                            {
                                fileDataVersionOne.Uri = new Uri(pair.Key, UriKind.RelativeOrAbsolute);
                            }

                            run.Artifacts.Add(CreateFileData(fileDataVersionOne, pair.Key));
                        }
                    }

                    if (v1Run.LogicalLocations != null)
                    {
                        // Pass 1 over results. In this phase, we're simply collecting fully qualified names that
                        // may be duplicated in the logical locations dictionary. We're doing this so that we 
                        // can properly construct the v2 logical instances in the converted array (i.e., we can't
                        // populate the v2 logicalLocation.FullyQualifiedName property in cases where the 
                        // v1 key is a synthesized value and not actually the fully qualified name)
                        var visitor = new VersionOneLogicalLocationKeyToLogicalLocationDataVisitor();
                        visitor.VisitRunVersionOne(v1Run);

                        _v1KeyToFullyQualifiedNameMap = visitor.LogicalLocationKeyToFullyQualifiedNameMap;
                        _v1LogicalLocationKeyToDecoratedNameMap = visitor.LogicalLocationKeyToDecoratedNameMap;

                        run.LogicalLocations = new List<LogicalLocation>();
                        HashSet<string> populatedKeys = new HashSet<string>();

                        foreach (KeyValuePair<string, LogicalLocationVersionOne> pair in v1Run.LogicalLocations)
                        {
                            PopulateLogicalLocation(
                                run,
                                v1Run.LogicalLocations,
                                _v1LogicalLocationKeyToDecoratedNameMap,
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

        private static IDictionary<string, int> CreateFileKeyToIndexMapping(IDictionary<string, FileDataVersionOne> v1Files)
        {
            var v1FileKeyToV2IndexMap = new Dictionary<string, int>();

            if (v1Files != null)
            {
                int index = 0;
                foreach (KeyValuePair<string, FileDataVersionOne> entry in v1Files)
                {
                    v1FileKeyToV2IndexMap[entry.Key] = index++;
                }
            }

            return v1FileKeyToV2IndexMap;
        }

        private static IDictionary<string, int> CreateRuleKeyToIndexMapping(IDictionary<string, RuleVersionOne> v1Rules)
        {
            var v1RuleKeyToV2IndexMap = new Dictionary<string, int>();

            if (v1Rules != null)
            {
                int index = 0;
                foreach (KeyValuePair<string, RuleVersionOne> entry in v1Rules)
                {
                    v1RuleKeyToV2IndexMap[entry.Key] = index++;
                }
            }

            return v1RuleKeyToV2IndexMap;
        }

        private int GetRuleIndexForRuleKey(string ruleKey, IDictionary<string, int> v1RuleKeyToV2IndexMap)
        {
            if (ruleKey == null || !v1RuleKeyToV2IndexMap.TryGetValue(ruleKey, out int index))
            {
                index = -1;
            }

            return index;
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

            if (!keyToFullyQualifiedNameMap.TryGetValue(logicalLocationKey, out string fullyQualifiedName))
            {
                // If we don't find a remapping, the dictionary key itself comprises
                // the fully qualified name.
                fullyQualifiedName = logicalLocationKey;
            }

            // Create the logical location from the v1 version
            LogicalLocation logicalLocation = CreateLogicalLocation(v1LogicalLocation, fullyQualifiedName, logicalLocationKey);

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
                    Module = v1StackFrame.Module,
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
                                                 v1StackFrame.Line,
                                                 v1StackFrame.Address,
                                                 v1StackFrame.Offset);

            return stackFrame;
        }

        private const string DottedQuadFileVersionPattern = @"[0-9]+(\.[0-9]+){3}";
        private static readonly Regex s_dottedQuadFileVersionRegex = SarifUtilities.RegexFromPattern(DottedQuadFileVersionPattern);

        internal Tool CreateTool(ToolVersionOne v1Tool)
        {
            Tool tool = null;

            if (v1Tool != null)
            {
                // The SARIF v1 spec does not specify a format for tool.fileVersion (although on the
                // Windows platform, the spec anticipated that it would be of the form n.n.n.n where
                // each n is one or more decimal digits).
                //
                // The SARIF v2 spec is more prescriptive: it defines a property toolComponent.dottedQuadFileVersion
                // with exactly that format. So if the v1 file contained a tool.fileVersion in any other
                // format, the best we can do is put it in the property bag.
                string dottedQuadFileVersion = null;

                if (v1Tool.FileVersion != null &&
                    s_dottedQuadFileVersionRegex.IsMatch(v1Tool.FileVersion))
                {
                    dottedQuadFileVersion = v1Tool.FileVersion;
                }

                var driver = new ToolComponent
                {
                    DottedQuadFileVersion = dottedQuadFileVersion,
                    FullName = v1Tool.FullName,
                    Name = v1Tool.Name,
                    Properties = v1Tool.Properties,
                    SemanticVersion = v1Tool.SemanticVersion,
                    Version = v1Tool.Version
                };

                if (dottedQuadFileVersion == null && v1Tool.FileVersion != null)
                {
                    driver.SetProperty("sarifv1/toolFileVersion", v1Tool.FileVersion);
                }

                tool = new Tool
                {
                    Driver = driver
                };
            }

            return tool;
        }
    }
}

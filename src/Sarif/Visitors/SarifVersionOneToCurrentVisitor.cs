// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SarifVersionOneToCurrentVisitor : SarifRewritingVisitorVersionOne
    {
        private static readonly SarifVersion FromSarifVersion = SarifVersion.OneZeroZero;
        private static readonly string FromPropertyBagPrefix =
            SarifTransformerUtilities.PropertyBagTransformerItemPrefixes[FromSarifVersion];

        private Run _currentRun = null;

        public SarifLog SarifLog { get; private set; }

        public override SarifLogVersionOne VisitSarifLogVersionOne(SarifLogVersionOne v1SarifLog)
        {
            SarifLog = new SarifLog(SarifVersion.TwoZeroZero.ConvertToSchemaUri(),
                                    SarifVersion.TwoZeroZero,
                                    new List<Run>());

            foreach (RunVersionOne v1Run in v1SarifLog.Runs)
            {
                SarifLog.Runs.Add(CreateRun(v1Run));
            }

            return null;
        }

       internal ExceptionData CreateExceptionData(ExceptionDataVersionOne v1ExceptionData)
        {
            ExceptionData exceptionData = null;

            if (v1ExceptionData != null)
            {
                exceptionData = new ExceptionData
                {
                    InnerExceptions = SarifTransformerUtilities.TransformList<ExceptionDataVersionOne, ExceptionData>(
                                                                    v1ExceptionData.InnerExceptions,
                                                                    CreateExceptionData),
                    Kind = v1ExceptionData.Kind,
                    Message = v1ExceptionData.Message
                };

                if (v1ExceptionData.Stack != null)
                {
                    exceptionData.Stack = CreateStack(v1ExceptionData.Stack);
                }
            }

            return exceptionData;
        }

        internal FileData CreateFileData(FileDataVersionOne v1FileData)
        {
            FileData fileData = null;

            if (v1FileData != null)
            {
                fileData = new FileData
                {
                    Hashes = SarifTransformerUtilities.TransformList<HashVersionOne, Hash>(
                                                            v1FileData.Hashes,
                                                            CreateHash),
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

                    if (SarifTransformerUtilities.TextMimeTypes.Contains(v1FileData.MimeType))
                    {
                        fileData.Contents.Text = SarifUtilities.DecodeBase64Utf8String(v1FileData.Contents);
                    }
                    else
                    {
                        fileData.Contents.Binary = v1FileData.Contents;
                    }
                }
            }

            return fileData;
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

        internal Hash CreateHash(HashVersionOne v1Hash)
        {
            Hash hash = null;

            if (v1Hash != null)
            {
                string algorithm;
                if (!SarifTransformerUtilities.AlgorithmKindNameMap.TryGetValue(v1Hash.Algorithm, out algorithm))
                {
                    algorithm = v1Hash.Algorithm.ToString().ToLowerInvariant();
                }

                hash = new Hash
                {
                    Algorithm = algorithm,
                    Value = v1Hash.Value
                };
            }

            return hash;
        }

        internal Invocation CreateInvocation(InvocationVersionOne v1Invocation,
                                           IList<NotificationVersionOne> v1ToolNotifications,
                                           IList<NotificationVersionOne> v1ConfigurationNotifications)
        {
            Invocation invocation = CreateInvocation(v1Invocation);
            IList<Notification> toolNotifications = SarifTransformerUtilities.TransformList<NotificationVersionOne, Notification>(
                                                                                    v1ToolNotifications,
                                                                                    CreateNotification);
            IList<Notification> configurationNotifications = SarifTransformerUtilities.TransformList<NotificationVersionOne, Notification>(
                                                                                    v1ConfigurationNotifications,
                                                                                    CreateNotification); ;

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
                    EndTime = v1Invocation.EndTime,
                    EnvironmentVariables = v1Invocation.EnvironmentVariables,
                    Machine = v1Invocation.Machine,
                    ProcessId = v1Invocation.ProcessId,
                    Properties = v1Invocation.Properties,
                    ResponseFiles = CreateResponseFilesList(v1Invocation.ResponseFiles),
                    StartTime = v1Invocation.StartTime,
                    WorkingDirectory = v1Invocation.WorkingDirectory
                };

                if (!string.IsNullOrWhiteSpace(v1Invocation.FileName))
                {
                    invocation.ExecutableLocation = new FileLocation
                    {
                        Uri = new Uri(v1Invocation.FileName, UriKind.RelativeOrAbsolute)
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
                    DecoratedName = v1Location.DecoratedName,
                    FullyQualifiedLogicalName = v1Location.FullyQualifiedLogicalName,
                    LogicalLocationKey = v1Location.LogicalLocationKey,
                    //PhysicalLocation = TransformPhysicalLocationVersionOne(v1Location.ResultFile),
                    Properties = v1Location.Properties
                };

                //if (location.Properties == null)
                //{
                //    location.Properties = new Dictionary<string, SerializedPropertyInfo>();
                //}

                //location.SetProperty("AnalysisTarget",
                //                     new SerializedPropertyInfo(JsonConvert.SerializeObject(v1Location.AnalysisTarget), false));

                //if (node.AnalysisTarget != null)
                //{
                //    if (location.Properties == null)
                //    {
                //        location.Properties = new Dictionary<string, SerializedPropertyInfo>();
                //    }

                //    location.Properties.Add("AnalysisTarget",
                //                            new SerializedPropertyInfo(JsonConvert.SerializeObject(node.AnalysisTarget), false));
                //}
            }

            return location;
        }

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

            if (!string.IsNullOrWhiteSpace(fullyQualifiedLogicalName))
            {
                if (!string.IsNullOrWhiteSpace(logicalLocationKey))
                {
                    location.FullyQualifiedLogicalName = logicalLocationKey;

                    //if (_currentRun.LogicalLocations == null)
                }
                else
                {
                    location.FullyQualifiedLogicalName = fullyQualifiedLogicalName;
                }
            }

            location.PhysicalLocation = new PhysicalLocation
            {
                FileLocation = CreateFileLocation(uri, uriBaseId)
            };

            if (column > 0 || line > 0)
            {
                location.PhysicalLocation.Region = new Region
                {
                    StartColumn = column,
                    StartLine = line
                };
            }

            return location;
        }

        internal LogicalLocation CreateLogicalLocation(LogicalLocationVersionOne v1LogicalLocation)
        {
            LogicalLocation logicalLocation = null;

            if (v1LogicalLocation != null)
            {
                logicalLocation = new LogicalLocation
                {
                    Kind = v1LogicalLocation.Kind,
                    Name = v1LogicalLocation.Name,
                    ParentKey = v1LogicalLocation.ParentKey
                };
            }

            return logicalLocation;
        }

        internal LogicalLocation CreateLogicalLocation(string name, string logicalLocationKey, string fulyyQualifiedName)
        {
            return null;
        }

        internal string AddLogicalLocation()
        {
            if (_currentRun.LogicalLocations == null)
            {
                _currentRun.LogicalLocations = new Dictionary<string, LogicalLocation>();
            }

            string logicalLocationKey = "";


            return logicalLocationKey;
        }

        internal Message CreateMessage(string text)
        {
            Message message = null;

            if (!string.IsNullOrWhiteSpace(text))
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
                    Level = SarifTransformerUtilities.CreateNotificationLevel(v1Notification.Level),
                    Message = CreateMessage(v1Notification.Message),
                    Properties = v1Notification.Properties,
                    RuleId = v1Notification.RuleId,
                    ThreadId = v1Notification.ThreadId,
                    Time = v1Notification.Time
                };
            }

            return notification;
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
                    // Id = ??? this is used for embedded links, need to understand if we can infer that
                    // According to the spec, this needs to be unique across results
                    FileLocation = CreateFileLocation(v1PhysicalLocation.Uri, v1PhysicalLocation.UriBaseId),
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
                region = CreateRegion(v1Region.StartColumn,
                                      v1Region.StartLine,
                                      v1Region.EndColumn,
                                      v1Region.EndLine,
                                      v1Region.Length,
                                      v1Region.Offset);
            }

            return region;
        }

        internal Region CreateRegion(int startColumn, int startLine, int endColumn, int endLine, int length, int offset)
        {
            Region region = new Region
            {
                EndColumn = endColumn,
                EndLine = endLine,
                Length = length,
                Offset = offset,
                StartColumn = startColumn,
                StartLine = startLine
            };

            return region;
        }

        internal Rule CreateRule(RuleVersionOne v1Rule)
        {
            Rule rule = null;

            if (v1Rule != null)
            {
                rule = new Rule
                {
                    FullDescription = CreateMessage(v1Rule.FullDescription),
                    HelpLocation = CreateFileLocation(v1Rule.HelpUri, null),
                    Id = v1Rule.Id,
                    MessageStrings = v1Rule.MessageFormats,
                    Name = CreateMessage(v1Rule.Name),
                    Properties = v1Rule.Properties,
                    ShortDescription = CreateMessage(v1Rule.ShortDescription)
                };

                RuleConfigurationDefaultLevel level = SarifTransformerUtilities.CreateRuleConfigurationDefaultLevel(v1Rule.DefaultLevel);

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
                run = new Run()
                {
                    Architecture = v1Run.Architecture,
                    AutomationId = v1Run.AutomationId,
                    BaselineId = v1Run.BaselineId,
                    Id = v1Run.Id,
                    Properties = v1Run.Properties,
                    Results = new List<Result>(),
                    StableId = v1Run.StableId,
                    Tool = CreateTool(v1Run.Tool)
                };

                _currentRun = run;

                if (v1Run.Files != null)
                {
                    run.Files = new Dictionary<string, FileData>();

                    foreach (var pair in v1Run.Files)
                    {
                        run.Files.Add(pair.Key, CreateFileData(pair.Value));
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

                if (v1Run.LogicalLocations != null)
                {
                    run.LogicalLocations = new Dictionary<string, LogicalLocation>();

                    foreach (var pair in v1Run.LogicalLocations)
                    {
                        run.LogicalLocations.Add(pair.Key, CreateLogicalLocation(pair.Value));
                    }
                }

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
            }

            return run;
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
                    Frames = SarifTransformerUtilities.TransformList<StackFrameVersionOne, StackFrame>(
                                                            v1Stack.Frames,
                                                            CreateStackFrame)
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
                    FileVersion = v1Tool.FileVersion,
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

        internal void AddLogicalLocation()
        { }
    }
}

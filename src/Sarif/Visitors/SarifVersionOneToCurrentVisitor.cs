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

        public static ExceptionData CreateExceptionData(ExceptionDataVersionOne v1ExceptionData)
        {
            ExceptionData exceptionData = null;

            if (v1ExceptionData != null)
            {
                exceptionData = new ExceptionData
                {
                    Kind = v1ExceptionData.Kind,
                    Message = v1ExceptionData.Message
                };

                if (v1ExceptionData.InnerExceptions != null)
                {
                    exceptionData.InnerExceptions = new List<ExceptionData>();

                    foreach (ExceptionDataVersionOne edvo in v1ExceptionData.InnerExceptions)
                    {
                        exceptionData.InnerExceptions.Add(CreateExceptionData(edvo));
                    }
                }

                if (v1ExceptionData.Stack != null)
                {
                    exceptionData.Stack = CreateStack(v1ExceptionData.Stack);
                }
            }

            return exceptionData;
        }

        public static FileData CreateFileData(FileDataVersionOne v1FileData)
        {
            FileData fileData = null;

            if (v1FileData != null)
            {
                fileData = new FileData
                {
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

                if (v1FileData.Hashes != null)
                {
                    fileData.Hashes = new List<Hash>();

                    foreach (HashVersionOne hash in v1FileData.Hashes)
                    {
                        fileData.Hashes.Add(CreateHash(hash));
                    }
                }

                if (v1FileData.Tags.Count > 0)
                {
                    fileData.Tags.UnionWith(v1FileData.Tags);
                }
            }

            return fileData;
        }

        public static Hash CreateHash(HashVersionOne v1Hash)
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

        public static Invocation CreateInvocation(InvocationVersionOne v1Invocation,
                                           IEnumerable<NotificationVersionOne> v1ToolNotifications,
                                           IEnumerable<NotificationVersionOne> v1ConfigurationNotifications,
                                           Run run)
        {
            Invocation invocation = CreateInvocation(v1Invocation, run);
            IList<Notification> toolNotifications = CreateNotificationsList(v1ToolNotifications);
            IList<Notification> configurationNotifications = CreateNotificationsList(v1ConfigurationNotifications);

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

        public static Invocation CreateInvocation(InvocationVersionOne v1Invocation, Run run)
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
                    ResponseFiles = CreateResponseFilesList(v1Invocation.ResponseFiles, run),
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

        public static LogicalLocation CreateLogicalLocation(LogicalLocationVersionOne v1LogicalLocation)
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

        public static IList<Notification> CreateNotificationsList(IEnumerable<NotificationVersionOne> v1Notifications)
        {
            List<Notification> notifications = null;

            if (v1Notifications != null)
            {
                notifications = new List<Notification>();

                foreach (NotificationVersionOne notification in v1Notifications)
                {
                    notifications.Add(CreateNotification(notification));
                }
            }

            return notifications;
        }

        public static Notification CreateNotification(NotificationVersionOne v1Notification)
        {
            Notification notification = null;

            if (v1Notification != null)
            {
                notification = new Notification
                {
                    Exception = CreateExceptionData(v1Notification.Exception),
                    Id = v1Notification.Id,
                    Level = SarifTransformerUtilities.CreateNotificationLevel(v1Notification.Level),
                    Properties = v1Notification.Properties,
                    RuleId = v1Notification.RuleId,
                    RuleKey = v1Notification.RuleKey,
                    ThreadId = v1Notification.ThreadId,
                    Time = v1Notification.Time
                };

                if (!string.IsNullOrWhiteSpace(v1Notification.Message))
                {
                    notification.Message = new Message
                    {
                        Text = v1Notification.Message
                    };
                }
            }

            return notification;
        }

        private static IList<FileLocation> CreateResponseFilesList(IDictionary<string, string> responseFileToContentsDictionary, Run run)
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

                    if (run != null && !string.IsNullOrWhiteSpace(responseFileToContentsDictionary[key]))
                    {
                        // We have contents, so mention this file in run.files
                        if (run.Files == null)
                        {
                            run.Files = new Dictionary<string, FileData>();
                        }

                        if (!run.Files.ContainsKey(key))
                        {
                            run.Files.Add(key, new FileData());
                        }

                        FileData responseFile = run.Files[key];

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

        public static Rule CreateRule(RuleVersionOne v1Rule)
        {
            Rule rule = null;

            if (v1Rule != null)
            {
                rule = new Rule
                {
                    Id = v1Rule.Id,
                    MessageStrings = v1Rule.MessageFormats,
                    Properties = v1Rule.Properties
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

                if (!string.IsNullOrWhiteSpace(v1Rule.Name))
                {
                    rule.Name = new Message
                    {
                        Text = v1Rule.Name
                    };
                }

                if (!string.IsNullOrWhiteSpace(v1Rule.FullDescription))
                {
                    rule.FullDescription = new Message
                    {
                        Text = v1Rule.FullDescription
                    };
                }

                if (!string.IsNullOrWhiteSpace(v1Rule.ShortDescription))
                {
                    rule.ShortDescription = new Message
                    {
                        Text = v1Rule.ShortDescription
                    };
                }

                if (v1Rule.HelpUri != null)
                {
                    rule.HelpLocation = new FileLocation
                    {
                        Uri = v1Rule.HelpUri
                    };
                }

                if (v1Rule.Tags.Count > 0)
                {
                    rule.Tags.UnionWith(v1Rule.Tags);
                }
            }

            return rule;
        }

        public static Run CreateRun(RunVersionOne v1Run)
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
                                                         v1Run.ConfigurationNotifications,
                                                         run);

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

                if (v1Run.Tags.Count > 0)
                {
                    run.Tags.UnionWith(v1Run.Tags);
                }
            }

            return run;
        }

        public static Stack CreateStack(StackVersionOne v1Stack)
        {
            Stack stack = null;

            if (v1Stack != null)
            {
                stack = new Stack
                {
                    Properties = v1Stack.Properties
                };

                if (!string.IsNullOrWhiteSpace(v1Stack.Message))
                {
                    stack.Message = new Message
                    {
                        Text = v1Stack.Message
                    };
                }

                if (v1Stack.Frames != null)
                {
                    stack.Frames = new List<StackFrame>();

                    foreach (StackFrameVersionOne v1StackFrame in v1Stack.Frames)
                    {
                        stack.Frames.Add(CreateStackFrame(v1StackFrame));
                    }
                }

                if (v1Stack.Tags.Count > 0)
                {
                    stack.Tags.UnionWith(v1Stack.Tags);
                }
            }

            return stack;
        }

        public static StackFrame CreateStackFrame(StackFrameVersionOne v1StackFrame)
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

                if (!string.IsNullOrWhiteSpace(v1StackFrame.Message))
                {
                    // No Message property in v2, stash it in the property bag
                    stackFrame.SetProperty($"{FromPropertyBagPrefix}/message", v1StackFrame.Message);
                }

                if (v1StackFrame.Tags.Count > 0)
                {
                    stackFrame.Tags.UnionWith(v1StackFrame.Tags);
                }
            }

            return stackFrame;
        }

        public static Tool CreateTool(ToolVersionOne v1Tool)
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

                if (v1Tool.Tags.Count > 0)
                {
                    tool.Tags.UnionWith(v1Tool.Tags);
                }
            }

            return tool;
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Utilities = Microsoft.CodeAnalysis.Sarif.Visitors.SarifTransformerUtilities;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SarifCurrentToVersionOneVisitor : SarifRewritingVisitor
    {
        private static readonly SarifVersion FromSarifVersion = SarifVersion.TwoZeroZero;
        private static readonly string FromPropertyBagPrefix =
            Utilities.PropertyBagTransformerItemPrefixes[FromSarifVersion];

        private RunVersionOne _currentRun = null;
        private Run _currentV2Run = null;

        public SarifLogVersionOne SarifLogVersionOne { get; private set; }

        public override SarifLog VisitSarifLog(SarifLog v2SarifLog)
        {
            SarifLogVersionOne = new SarifLogVersionOne(SarifVersionVersionOne.OneZeroZero.ConvertToSchemaUri(),
                                                        SarifVersionVersionOne.OneZeroZero,
                                                        new List<RunVersionOne>());

            foreach (Run v2Run in v2SarifLog.Runs)
            {
                SarifLogVersionOne.Runs.Add(CreateRun(v2Run));
            }

            return null;
        }

        internal ExceptionDataVersionOne CreateExceptionData(ExceptionData v2ExceptionData)
        {
            ExceptionDataVersionOne exceptionData = null;

            if (v2ExceptionData != null)
            {
                exceptionData = new ExceptionDataVersionOne
                {
                    InnerExceptions = v2ExceptionData.InnerExceptions?.Select(CreateExceptionData).ToList(),
                    Kind = v2ExceptionData.Kind,
                    Message = v2ExceptionData.Message,
                    Stack = CreateStack(v2ExceptionData.Stack)
                };
            }

            return exceptionData;
        }

        internal FileChangeVersionOne CreateFileChange(FileChange v2FileChange)
        {
            FileChangeVersionOne fileChange = null;

            if (v2FileChange != null)
            {
                string encodingName = GetFileEncodingName(v2FileChange.FileLocation?.Uri);
                Encoding encoding = GetFileEncoding(encodingName);

                try
                {
                    fileChange = new FileChangeVersionOne
                    {
                        Replacements = v2FileChange.Replacements?.Select(r => CreateReplacement(r, encoding)).ToList(),
                        Uri = v2FileChange.FileLocation?.Uri,
                        UriBaseId = v2FileChange.FileLocation?.UriBaseId
                    };
                }
                catch (UnknownEncodingException ex)
                {
                    // Set the unknown encoding name so the caller can provide useful reporting
                    ex.EncodingName = encodingName;
                    throw ex;
                }
            }

            return fileChange;
        }

        private string GetFileEncodingName(Uri uri)
        {
            string encodingName = null;
            IDictionary<string, FileData> filesDictionary = _currentV2Run.Files;

            FileData fileData;
            if (uri != null &&
                filesDictionary != null &&
                filesDictionary.TryGetValue(uri.OriginalString, out fileData))
            {
                encodingName = fileData.Encoding;
            }

            return encodingName;
        }

        private Encoding GetFileEncoding(string encodingName)
        {
            Encoding encoding = null;

            try
            {
                encoding = Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException) { }

            return encoding;
        }

        internal FileDataVersionOne CreateFileData(FileData v2FileData)
        {
            FileDataVersionOne fileData = null;

            if (v2FileData != null)
            {
                fileData = new FileDataVersionOne
                {
                    Hashes = v2FileData.Hashes?.Select(CreateHash).ToList(),
                    Length = v2FileData.Length,
                    MimeType = v2FileData.MimeType,
                    Offset = v2FileData.Offset,
                    ParentKey = v2FileData.ParentKey,
                    Properties = v2FileData.Properties,
                    Uri = v2FileData.FileLocation?.Uri,
                    UriBaseId = v2FileData.FileLocation?.UriBaseId
                };

                if (v2FileData.Contents != null)
                {
                    fileData.Contents = Utilities.TextMimeTypes.Contains(v2FileData.MimeType) ?
                        SarifUtilities.GetUtf8Base64String(v2FileData.Contents.Text) :
                        v2FileData.Contents.Binary;
                }
            }

            return fileData;
        }

        internal FixVersionOne CreateFix(Fix v2Fix)
        {
            FixVersionOne fix = null;

            if (v2Fix != null)
            {
                try
                {
                    fix = new FixVersionOne()
                    {
                        Description = v2Fix.Description?.Text,
                        FileChanges = v2Fix.FileChanges?.Select(CreateFileChange).ToList()
                    };
                }
                catch (UnknownEncodingException)
                {
                    // A replacement in this fix specifies plain text, but the file's
                    // encoding is unknown or unsupported, so we refuse to transform the fix.
                    return null;
                }
            }

            return fix;
        }

        internal HashVersionOne CreateHash(Hash v2Hash)
        {
            HashVersionOne hash = null;

            if (v2Hash != null)
            {
                AlgorithmKindVersionOne algorithm;
                if (!Utilities.AlgorithmNameKindMap.TryGetValue(v2Hash.Algorithm, out algorithm))
                {
                    algorithm = AlgorithmKindVersionOne.Unknown;
                }

                hash = new HashVersionOne
                {
                    Algorithm = algorithm,
                    Value = v2Hash.Value
                };
            }

            return hash;
        }

        internal InvocationVersionOne CreateInvocation(Invocation v2Invocation)
        {
            InvocationVersionOne invocation = null;

            if (v2Invocation != null)
            {
                invocation = new InvocationVersionOne
                {
                    Account = v2Invocation.Account,
                    CommandLine = v2Invocation.CommandLine,
                    EndTime = v2Invocation.EndTime,
                    EnvironmentVariables = v2Invocation.EnvironmentVariables,
                    FileName = v2Invocation.ExecutableLocation?.Uri?.OriginalString,
                    Machine = v2Invocation.Machine,
                    ProcessId = v2Invocation.ProcessId,
                    Properties = v2Invocation.Properties,
                    ResponseFiles = CreateResponseFilesDictionary(v2Invocation.ResponseFiles),
                    StartTime = v2Invocation.StartTime,
                    WorkingDirectory = v2Invocation.WorkingDirectory
                };

                if (v2Invocation.ConfigurationNotifications != null)
                {
                    if (_currentRun.ConfigurationNotifications == null)
                    {
                        _currentRun.ConfigurationNotifications = new List<NotificationVersionOne>();
                    }

                    IEnumerable<NotificationVersionOne> notifications = v2Invocation.ConfigurationNotifications.Select(CreateNotification);
                    _currentRun.ConfigurationNotifications = _currentRun.ConfigurationNotifications.Union(notifications).ToList();
                }

                if (v2Invocation.ToolNotifications != null)
                {
                    if (_currentRun.ToolNotifications == null)
                    {
                        _currentRun.ToolNotifications = new List<NotificationVersionOne>();
                    }

                    List<NotificationVersionOne> notifications = v2Invocation.ToolNotifications.Select(CreateNotification).ToList();
                    _currentRun.ToolNotifications = _currentRun.ToolNotifications.Union(notifications).ToList();
                }
            }

            return invocation;
        }

        internal LogicalLocationVersionOne CreateLogicalLocation(LogicalLocation v2LogicalLocation)
        {
            LogicalLocationVersionOne logicalLocation = null;

            if (v2LogicalLocation != null)
            {
                logicalLocation = new LogicalLocationVersionOne
                {
                    Kind = v2LogicalLocation.Kind,
                    Name = v2LogicalLocation.Name,
                    ParentKey = v2LogicalLocation.ParentKey
                };
            }

            return logicalLocation;
        }

        internal NotificationVersionOne CreateNotification(Notification v2Notification)
        {
            NotificationVersionOne notification = null;

            if (v2Notification != null)
            {
                notification = new NotificationVersionOne
                {
                    Exception = CreateExceptionData(v2Notification.Exception),
                    Id = v2Notification.Id,
                    Level = Utilities.CreateNotificationLevelVersionOne(v2Notification.Level),
                    Message = v2Notification.Message?.Text,
                    PhysicalLocation = CreatePhysicalLocation(v2Notification.PhysicalLocation),
                    Properties = v2Notification.Properties,
                    RuleId = v2Notification.RuleId,
                    ThreadId = v2Notification.ThreadId,
                    Time = v2Notification.Time
                };
            }

            return notification;
        }

        internal PhysicalLocationVersionOne CreatePhysicalLocation(PhysicalLocation v2PhysicalLocation)
        {
            PhysicalLocationVersionOne physicalLocation = null;

            if (v2PhysicalLocation != null)
            {
                physicalLocation = new PhysicalLocationVersionOne
                {
                    Region = CreateRegion(v2PhysicalLocation.Region, v2PhysicalLocation.FileLocation?.Uri),
                    Uri = v2PhysicalLocation.FileLocation?.Uri,
                    UriBaseId = v2PhysicalLocation.FileLocation?.UriBaseId
                };
            }

            return physicalLocation;
        }

        internal RegionVersionOne CreateRegion(Region v2Region, Uri uri)
        {
            RegionVersionOne region = null;

            if (v2Region != null && (v2Region.StartColumn > 0 ||
                                     v2Region.StartLine > 0 ||
                                     v2Region.EndColumn > 0 ||
                                     v2Region.EndLine > 0 ||
                                     v2Region.ByteLength > 0 ||
                                     v2Region.ByteOffset > 0 ||
                                     v2Region.CharLength > 0 ||
                                     v2Region.CharOffset > 0))
            {
                region = new RegionVersionOne
                {
                    Length = v2Region.ByteLength,
                    Offset = v2Region.ByteOffset,
                    EndColumn = v2Region.EndColumn,
                    EndLine = v2Region.EndLine,
                    StartColumn = v2Region.StartColumn > 0 ? v2Region.StartColumn : 1,
                    StartLine = v2Region.StartLine
                };

                if ((v2Region.CharLength > 0 || v2Region.CharOffset > 0) ||
                    (v2Region.StartLine > 0 && v2Region.StartColumn > 0 && v2Region.EndColumn == 0))
                {
                    // We will need the source file content to determine values needed for the v1 region

                    string failureReason = null;
                    string localPath = uri?.LocalPath ?? "(null file path)";
                    TextReader reader = null;

                    try
                    {
                        Encoding encoding;
                        reader = GetFileTextReader(uri, out encoding);

                        if (reader == null)
                        {
                            failureReason = $"File '{localPath}' could not be found, or access was denied";
                        }
                        else if (encoding == null)
                        {
                            failureReason = $"Encoding could not be determined or is not supported for file '{localPath}'";
                        }
                        else
                        {
                            if (v2Region.CharLength > 0 || v2Region.CharOffset > 0)
                            {
                                // We need to determine byte length & offset

                                // Read the first <charOffset> characters
                                char[] buffer = new char[v2Region.CharOffset];
                                int count = reader.Read(buffer, 0, buffer.Length);

                                if (count == v2Region.CharOffset)
                                {
                                    region.Offset = SarifUtilities.GetByteLength(buffer, encoding);

                                    // Read the next <charLength> characters
                                    buffer = new char[v2Region.CharLength];
                                    count = reader.Read(buffer, 0, buffer.Length);

                                    if (count == v2Region.CharLength)
                                    {
                                        region.Length = SarifUtilities.GetByteLength(buffer, encoding);
                                    }
                                    else
                                    {
                                        failureReason = $"Attempted to read {v2Region.CharLength} characters, but only read {count} from file '{localPath}'";
                                    }
                                }
                                else
                                {
                                    failureReason = $"Attempted to read {v2Region.CharOffset} characters, but only read {count} from file '{localPath}'";
                                }
                            }
                            else if (v2Region.StartLine > 0 && v2Region.StartColumn > 0 && v2Region.EndColumn == 0)
                            {
                                // It's not an insertion point, so we need to find the line ending to meet v1 spec

                                // Read down to the line preceding the line where the region begins
                                for (int i = 0; i < v2Region.StartLine - 1; reader.ReadLine(), i++) { }

                                // Read the line where the region begins
                                string sourceLine = reader.ReadLine();

                                region.EndColumn = sourceLine.Length;
                            }
                        }
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Dispose();
                        }
                    }

                    if (failureReason != null)
                    {
                        // If we get here, we were unable to determine byte length/offset, so we have to warn the caller
                        // TODO: add a warning to the list
                    }
                }
            }

            return region;
        }

        public TextReader GetFileTextReader(Uri uri, out Encoding encoding)
        {
            TextReader reader = null;
            encoding = null;

            if (uri != null && _currentV2Run.Files != null)
            {
                FileData fileData;
                if (_currentV2Run.Files.TryGetValue(uri.OriginalString, out fileData))
                {
                    // Determine the encoding
                    string encodingName = null;

                    if (fileData.Contents?.Text != null)
                    {
                        // Embedded text shall be UTF-8 encoded
                        encoding = Encoding.UTF8;
                    }
                    else
                    {
                        encodingName = fileData.Encoding ?? _currentV2Run.DefaultFileEncoding;
                        encoding = GetFileEncoding(encodingName);
                    }

                    if (encoding != null)
                    {
                        try
                        {
                            if (fileData.Contents != null)
                            {
                                // Embedded text file content
                                string content = fileData.Contents.Text ?? SarifUtilities.DecodeBase64String(fileData.Contents.Binary, encoding);
                                reader = new StringReader(content);
                            }
                            else if (uri.IsAbsoluteUri &&
                                     uri.Scheme == Uri.UriSchemeFile &&
                                     File.Exists(uri.LocalPath))
                            {
                                // External source file
                                reader = new StreamReader(uri.LocalPath, encoding);
                            }
                        }
                        catch (IOException) { }
                    }
                }
            }

            return reader;
        }

        internal ReplacementVersionOne CreateReplacement(Replacement v2Replacement, Encoding encoding)
        {
            ReplacementVersionOne replacement = null;

            if (v2Replacement != null)
            {
                replacement = new ReplacementVersionOne();
                FileContent insertedContent = v2Replacement.InsertedContent;

                if (insertedContent != null)
                {
                    if (insertedContent.Binary != null)
                    {
                        replacement.InsertedBytes = insertedContent.Binary;
                    }
                    else if (insertedContent.Text != null)
                    {
                        if (encoding != null)
                        {
                            replacement.InsertedBytes = SarifUtilities.GetBase64String(insertedContent.Text, encoding);
                        }
                        else
                        {
                            // The encoding is null or not supported on the current platform
                            throw new UnknownEncodingException();
                        }
                    }
                }

                replacement.DeletedLength = v2Replacement.DeletedRegion.ByteLength;
                replacement.Offset = v2Replacement.DeletedRegion.ByteOffset;
            }

            return replacement;
        }

        internal Dictionary<string, string> CreateResponseFilesDictionary(IList<FileLocation> v2ResponseFilesList)
        {
            Dictionary<string, string> responseFiles = null;

            if (v2ResponseFilesList != null)
            {
                responseFiles = new Dictionary<string, string>();

                foreach (FileLocation fileLocation in v2ResponseFilesList)
                {
                    string key = fileLocation.Uri.OriginalString;
                    string fileContent = null;
                    FileData responseFile;

                    if (_currentV2Run.Files != null && _currentV2Run.Files.TryGetValue(key, out responseFile))
                    {
                        fileContent = responseFile.Contents?.Text;
                    }

                    responseFiles.Add(key, fileContent);
                }
            }

            return responseFiles;
        }

        internal ResultVersionOne CreateResult(Result v2Result)
        {
            ResultVersionOne result = null;

            if (v2Result != null)
            {
                result = new ResultVersionOne
                {
                    BaselineState = Utilities.CreateBaselineStateVersionOne(v2Result.BaselineState),
                    Fixes = v2Result.Fixes?.Select(CreateFix).ToList(),
                    Id = v2Result.InstanceGuid,
                    Level = Utilities.CreateResultLevelVersionOne(v2Result.Level),
                    Message = v2Result.Message?.Text,
                    Properties = v2Result.Properties,
                    Snippet = v2Result.Locations?[0]?.PhysicalLocation?.Region?.Snippet?.Text,
                    Stacks = v2Result.Stacks?.Select(CreateStack).ToList(),
                };

                if (result.Fixes != null)
                {
                    // Null Fixes will be present in the case of unsupported encoding
                    (result.Fixes as List<FixVersionOne>).RemoveAll(f => f == null);

                    if (result.Fixes.Count == 0)
                    {
                        result.Fixes = null;
                    }
                }

                if (_currentV2Run.Resources?.Rules != null)
                {
                    IDictionary<string, Rule> rules = _currentV2Run.Resources.Rules;
                    Rule v2Rule;

                    if (v2Result.RuleId != null &&
                        rules.TryGetValue(v2Result.RuleId, out v2Rule) &&
                        v2Rule.Id != v2Result.RuleId)
                    {
                        result.RuleId = v2Rule.Id;
                        result.RuleKey = v2Result.RuleId;
                    }
                    else
                    {
                        result.RuleId = v2Result.RuleId;
                    }
                }
                else
                {
                    result.RuleId = v2Result.RuleId;
                }

                if (!string.IsNullOrWhiteSpace(v2Result.RuleMessageId))
                {
                    result.FormattedRuleMessage = new FormattedRuleMessageVersionOne
                    {
                        Arguments = v2Result.Message?.Arguments,
                        FormatId = v2Result.RuleMessageId
                    };
                }
            }

            return result;
        }

        internal RuleVersionOne CreateRule(Rule v2Rule)
        {
            RuleVersionOne rule = null;

            if (v2Rule != null)
            {
                rule = new RuleVersionOne
                {
                    FullDescription = v2Rule.FullDescription?.Text,
                    HelpUri = v2Rule.HelpLocation?.Uri,
                    Id = v2Rule.Id,
                    MessageFormats = v2Rule.MessageStrings,
                    Name = v2Rule.Name?.Text,
                    Properties = v2Rule.Properties,
                    ShortDescription = v2Rule.ShortDescription?.Text
                };

                if (v2Rule.Configuration != null)
                {
                    rule.Configuration = v2Rule.Configuration.Enabled ?
                            RuleConfigurationVersionOne.Enabled :
                            RuleConfigurationVersionOne.Disabled;
                    rule.DefaultLevel = Utilities.CreateResultLevelVersionOne(v2Rule.Configuration.DefaultLevel);
                }
            }

            return rule;
        }

        internal RunVersionOne CreateRun(Run v2Run)
        {
            RunVersionOne run = null;

            if (v2Run != null)
            {
                if (v2Run.TryGetProperty("sarifv1/run", out run))
                {
                    return run;
                }
                else
                {
                    _currentV2Run = v2Run;

                    // We need to create the run before we start working on children
                    // because some of them will need to refer to _currentRun
                    run = new RunVersionOne();
                    _currentRun = run;

                    run.Architecture = v2Run.Architecture;
                    run.AutomationId = v2Run.AutomationLogicalId;
                    run.BaselineId = v2Run.BaselineInstanceGuid;
                    run.Files = v2Run.Files?.ToDictionary(v => v.Key, v => CreateFileData(v.Value));
                    run.Id = v2Run.InstanceGuid;
                    run.Invocation = CreateInvocation(v2Run.Invocations?[0]);
                    run.LogicalLocations = v2Run.LogicalLocations?.ToDictionary(v => v.Key, v => CreateLogicalLocation(v.Value));
                    run.Properties = v2Run.Properties;
                    run.Results = new List<ResultVersionOne>();
                    run.Rules = v2Run.Resources?.Rules?.ToDictionary(v => v.Key, v => CreateRule(v.Value));
                    run.StableId = v2Run.LogicalId;
                    run.Tool = CreateTool(v2Run.Tool);

                    foreach (Result v2Result in v2Run.Results)
                    {
                        run.Results.Add(CreateResult(v2Result));
                    }

                    // Stash the entire v2 run in this v1 run's property bag
                    run.SetProperty($"{FromPropertyBagPrefix}/run", v2Run);
                }
            }

            return run;
        }

        internal StackVersionOne CreateStack(Stack v2Stack)
        {
            StackVersionOne stack = null;

            if (v2Stack != null)
            {
                stack = new StackVersionOne
                {
                    Message = v2Stack.Message?.Text,
                    Properties = v2Stack.Properties,
                    Frames = v2Stack.Frames?.Select(CreateStackFrame).ToList()
                };
            }

            return stack;
        }

        internal StackFrameVersionOne CreateStackFrame(StackFrame v2StackFrame)
        {
            StackFrameVersionOne stackFrame = null;

            if (v2StackFrame != null)
            {
                stackFrame = new StackFrameVersionOne
                {
                    Address = v2StackFrame.Address,
                    Module = v2StackFrame.Module,
                    Offset = v2StackFrame.Offset,
                    Parameters = v2StackFrame.Parameters,
                    Properties = v2StackFrame.Properties,
                    ThreadId = v2StackFrame.ThreadId
                };

                Location location = v2StackFrame.Location;
                if (location != null)
                {
                    string fqln = location.FullyQualifiedLogicalName;

                    if (_currentV2Run.LogicalLocations != null &&
                        _currentV2Run.LogicalLocations.ContainsKey(fqln) &&
                        !string.IsNullOrWhiteSpace(_currentV2Run.LogicalLocations[fqln].FullyQualifiedName))
                    {
                        stackFrame.FullyQualifiedLogicalName = _currentV2Run.LogicalLocations[fqln].FullyQualifiedName;
                        stackFrame.LogicalLocationKey = fqln;
                    }
                    else
                    {
                        stackFrame.FullyQualifiedLogicalName = fqln;
                    }

                    stackFrame.Message = location.Message?.Text;

                    PhysicalLocation physicalLocation = location.PhysicalLocation;
                    if (physicalLocation != null)
                    {
                        stackFrame.Column = physicalLocation.Region?.StartColumn ?? 0;
                        stackFrame.Line = physicalLocation.Region?.StartLine ?? 0;
                        stackFrame.Uri = physicalLocation.FileLocation?.Uri;
                        stackFrame.UriBaseId = physicalLocation.FileLocation?.UriBaseId;
                    }
                }
            }

            return stackFrame;
        }

        internal ToolVersionOne CreateTool(Tool v2Tool)
        {
            ToolVersionOne tool = null;

            if (v2Tool != null)
            {
                tool = new ToolVersionOne()
                {
                    FileVersion = v2Tool.FileVersion,
                    FullName = v2Tool.FullName,
                    Language = v2Tool.Language,
                    Name = v2Tool.Name,
                    Properties = v2Tool.Properties,
                    SarifLoggerVersion = v2Tool.SarifLoggerVersion,
                    SemanticVersion = v2Tool.SemanticVersion,
                    Version = v2Tool.Version
                };
            }

            return tool;
        }
    }
}

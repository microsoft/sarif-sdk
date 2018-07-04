// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
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

        internal AnnotatedCodeLocationVersionOne CreateAnnotatedCodeLocation(Location v2Location)
        {
            AnnotatedCodeLocationVersionOne annotatedCodeLocation = null;

            if (v2Location != null)
            {
                annotatedCodeLocation = new AnnotatedCodeLocationVersionOne
                {
                    Annotations = v2Location.Annotations?.Select(CreateAnnotation).ToList(),
                    FullyQualifiedLogicalName = v2Location.FullyQualifiedLogicalName,
                    LogicalLocationKey = v2Location.FullyQualifiedLogicalName,
                    Message = v2Location.Message?.Text,
                    PhysicalLocation = CreatePhysicalLocation(v2Location.PhysicalLocation),
                    Snippet = v2Location.PhysicalLocation?.Region?.Snippet?.Text
                };
            }

            return annotatedCodeLocation;
        }

        internal AnnotatedCodeLocationVersionOne CreateAnnotatedCodeLocation(CodeFlowLocation v2CodeFlowLocation)
        {
            AnnotatedCodeLocationVersionOne annotatedCodeLocation = null;

            if (v2CodeFlowLocation != null)
            {
                annotatedCodeLocation = CreateAnnotatedCodeLocation(v2CodeFlowLocation.Location);
                annotatedCodeLocation = annotatedCodeLocation ?? new AnnotatedCodeLocationVersionOne();

                annotatedCodeLocation.Importance = Utilities.CreateAnnotatedCodeLocationImportance(v2CodeFlowLocation.Importance);
                annotatedCodeLocation.Module = v2CodeFlowLocation.Module;
                annotatedCodeLocation.Properties = v2CodeFlowLocation.Properties;
                annotatedCodeLocation.State = v2CodeFlowLocation.State;
                annotatedCodeLocation.Step = v2CodeFlowLocation.Step;
            }

            return annotatedCodeLocation;
        }

        internal IList<AnnotatedCodeLocationVersionOne> CreateAnnotatedCodeLocation(ThreadFlow v2ThreadFlow)
        {
            List<AnnotatedCodeLocationVersionOne> annotatedCodeLocationList = null;

            if (v2ThreadFlow?.Locations?.Count > 0)
            {
                annotatedCodeLocationList = v2ThreadFlow.Locations.Select(CreateAnnotatedCodeLocation).ToList();

                if (annotatedCodeLocationList.Count == v2ThreadFlow.Locations.Count)
                {
                    for (int i = 0; i < annotatedCodeLocationList.Count - 1; i++)
                    {
                        if (v2ThreadFlow.Locations[i].NestingLevel > v2ThreadFlow.Locations[i + 1].NestingLevel)
                        {
                            annotatedCodeLocationList[i].Kind = AnnotatedCodeLocationKindVersionOne.CallReturn;
                        }
                        else if (v2ThreadFlow.Locations[i].NestingLevel < v2ThreadFlow.Locations[i + 1].NestingLevel)
                        {
                            annotatedCodeLocationList[i].Kind = AnnotatedCodeLocationKindVersionOne.Call;
                        }
                    }
                }
            }

            return annotatedCodeLocationList;
        }

        internal AnnotationVersionOne CreateAnnotation(Region v2Region)
        {
            AnnotationVersionOne annotation = null;

            if (v2Region != null)
            {
                annotation = new AnnotationVersionOne
                {
                    Locations = new List<PhysicalLocationVersionOne>
                    {
                        CreatePhysicalLocation(v2Region)
                    },
                    Message = v2Region.Message?.Text
                };
            }

            return annotation;
        }

        internal CodeFlowVersionOne CreateCodeFlow(CodeFlow v2CodeFlow)
        {
            CodeFlowVersionOne codeFlow = null;

            if (v2CodeFlow != null)
            {
                codeFlow = new CodeFlowVersionOne
                {
                    Message = v2CodeFlow.Message?.Text,
                    Properties = v2CodeFlow.Properties
                };

                if (v2CodeFlow.ThreadFlows?.Count > 0)
                {
                    codeFlow.Locations = new List<AnnotatedCodeLocationVersionOne>();

                    foreach (ThreadFlow tf in v2CodeFlow.ThreadFlows)
                    {
                        (codeFlow.Locations as List<AnnotatedCodeLocationVersionOne>).AddRange(CreateAnnotatedCodeLocation(tf));
                    }
                }
            }

            return codeFlow;
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

        internal LocationVersionOne CreateLocation(Location v2Location)
        {
            LocationVersionOne location = null;

            if (v2Location != null)
            {
                location = new LocationVersionOne
                {
                    FullyQualifiedLogicalName = v2Location.FullyQualifiedLogicalName,
                    Properties = v2Location.Properties,
                    ResultFile = CreatePhysicalLocation(v2Location.PhysicalLocation)
                };

                if (!string.IsNullOrWhiteSpace(v2Location.FullyQualifiedLogicalName) &&
                    _currentV2Run.LogicalLocations?.ContainsKey(v2Location.FullyQualifiedLogicalName) == true)
                {
                    location.DecoratedName = _currentV2Run.LogicalLocations[v2Location.FullyQualifiedLogicalName].DecoratedName;
                }
            }

            return location;
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

        internal PhysicalLocationVersionOne CreatePhysicalLocation(FileLocation v2FileLocation)
        {
            PhysicalLocationVersionOne physicalLocation = null;

            if (v2FileLocation != null)
            {
                physicalLocation = new PhysicalLocationVersionOne
                {
                    Uri = v2FileLocation.Uri,
                    UriBaseId = v2FileLocation.UriBaseId
                };
            }

            return physicalLocation;
        }

        internal PhysicalLocationVersionOne CreatePhysicalLocation(Region v2Region)
        {
            PhysicalLocationVersionOne physicalLocation = null;

            if (v2Region != null)
            {
                physicalLocation = new PhysicalLocationVersionOne
                {
                    Region = CreateRegion(v2Region, uri: null)
                };
            }

            return physicalLocation;
        }

        internal RegionVersionOne CreateRegion(Region v2Region, Uri uri)
        {
            RegionVersionOne region = null;

            if (v2Region != null)
            {
                region = new RegionVersionOne();

                if (v2Region.StartLine > 0 ||
                    v2Region.EndLine > 0 ||
                    v2Region.StartColumn > 0 ||
                    v2Region.EndColumn > 0 ||
                    v2Region.CharOffset > 0 ||
                    v2Region.CharLength > 0)
                {
                    if (v2Region.StartLine > 0)
                    {
                        // The start of the region is described by line/column
                        region.StartLine = v2Region.StartLine;
                        region.StartColumn = v2Region.StartColumn > 0
                            ? v2Region.StartColumn
                            : 1;
                    }
                    else
                    {
                        // The start of the region is described by character offset

                        if (v2Region.CharOffset == 0)
                        {
                            region.Offset = 0;
                        }
                        else
                        {
                            // Try to get the byte offset using the file encoding and contents
                            region.Offset = ConvertCharOffsetToByteOffset(v2Region.CharOffset, uri);
                        }
                    }

                    if (v2Region.CharLength > 0)
                    {
                        // The end of the region is described by character length
                        // Try to get the byte length using the file encoding and contents
                        region.Length = GetRegionByteLength(v2Region, uri);
                    }
                    else if (v2Region.EndLine > 0)
                    {
                        region.EndLine = v2Region.EndLine;

                        if (v2Region.EndColumn > 0)
                        {
                            region.EndColumn = v2Region.EndColumn;
                        }
                        else
                        {
                            // Try to get the end column using the file encoding and contents
                            region.EndColumn = GetRegionEndColumn(v2Region, uri);
                        }
                    }
                    else if (v2Region.EndColumn > 0)
                    {
                        region.EndColumn = v2Region.EndColumn;
                    }
                    else
                    {
                        // THIS IS A PROBLEM. IF ALL THE "END" PROPERTIES ARE MISSING,
                        // IT MEANS "THE REST OF THE StartLine". BUT IF CHARLENGTH IS
                        // PRESENT BUT 0, IT MEANS "INSERTION POINT". AND WE CAN'T TELL
                        // THE DIFFERENCE.
                        // TODO: Issue #932

                        // Assume it's an insertion point
                        region.EndLine = region.EndColumn = region.Length = 0;
                    }
                }
                else
                {
                    // There are no text-related properties. Therefore either the region is
                    // described entirely by binary-related properties, or the region is an
                    // insertion point at the start of the file.
                    region.Length = v2Region.ByteLength;
                    region.Offset = v2Region.ByteOffset;
                }
            }

            return region;
        }

        private int ConvertCharOffsetToByteOffset(int charOffset, Uri uri)
        {
            int byteOffset = 0;
            Encoding encoding;

            using (StreamReader reader = GetFileStreamReader(uri, out encoding))
            {
                if (reader != null)
                {
                    char[] buffer = new char[charOffset];

                    // Read everything up to charOffset
                    if (reader.ReadBlock(buffer, 0, buffer.Length) > 0)
                    {
                        byteOffset = encoding.GetByteCount(buffer, 0, buffer.Length);
                    }
                }
            }

            return byteOffset;
        }

        private int GetRegionByteLength(Region v2Region, Uri uri)
        {
            int byteLength = 0;
            Encoding encoding;

            using (StreamReader reader = GetFileStreamReader(uri, out encoding))
            {
                if (reader != null)
                {
                    if (v2Region.StartLine > 0) // Use line and column 
                    {
                        string sourceLine = null;

                        // Read down to startLine (null return means EOF)
                        for (int i = 1; i <= v2Region.StartLine && sourceLine != null; sourceLine = reader.ReadLine(), i++) { }

                        if (sourceLine != null && sourceLine.Length <= v2Region.StartColumn)
                        {
                            // Since we read past startColumn, we need to back up using the base stream
                            Stream stream = reader.BaseStream;
                            stream.Position -= v2Region.StartColumn - 1;

                            // Read the next charLength characters
                            char[] buffer = new char[v2Region.CharLength];
                            reader.Read(buffer, 0, buffer.Length);

                            byteLength = encoding.GetByteCount(buffer);
                        }
                    }
                    else // Use charOffset
                    {
                        // Read the first charOffset characters
                        char[] buffer = new char[v2Region.CharOffset];
                        reader.Read(buffer, 0, buffer.Length);

                        // Read the next charLength characters  
                        buffer = new char[v2Region.CharLength];
                        reader.Read(buffer, 0, buffer.Length);

                        byteLength = encoding.GetByteCount(buffer);
                    }
                }
            }

            return byteLength;
        }

        private int GetRegionEndColumn(Region v2Region, Uri uri)
        {
            int endColumn = 0;
            Encoding encoding;

            using (StreamReader reader = GetFileStreamReader(uri, out encoding))
            {
                if (reader != null)
                {
                    string sourceLine = null;

                    // Read down to endLine (null return means EOF)
                    for (int i = 1; i <= v2Region.EndLine && sourceLine != null; sourceLine = reader.ReadLine(), i++) { }

                    if (sourceLine != null && sourceLine.Length < v2Region.EndColumn)
                    {
                        endColumn = sourceLine.Length;
                    }
                }
            }

            return endColumn;
        }

        private Stream GetContentStream(Uri uri, out Encoding encoding)
        {
            Stream stream = null;
            encoding = null;
            string failureReason = null;

            if (uri != null && _currentV2Run.Files != null)
            {
                FileData fileData;
                if (_currentV2Run.Files.TryGetValue(uri.OriginalString, out fileData))
                {
                    if (fileData.Contents != null && (fileData.Contents.Text != null || fileData.Contents.Binary != null))
                    {
                        // We need the encoding because the content might have been transposed to UTF-8
                        string encodingName = fileData.Encoding ?? _currentV2Run.DefaultFileEncoding;
                        encoding = GetFileEncoding(encodingName);

                        if (encoding != null)
                        {
                            // Embedded text file content

                            byte[] content = !string.IsNullOrWhiteSpace(fileData.Contents.Text) ?
                                encoding.GetBytes(fileData.Contents.Text) :
                                Convert.FromBase64String(fileData.Contents.Binary);
                            stream = new MemoryStream(content);
                        }
                        else
                        {
                            failureReason = $"Encoding for embedded file '{uri.OriginalString}' could not be determined";
                        }
                    }
                    else if (uri.IsAbsoluteUri &&
                             uri.Scheme == Uri.UriSchemeFile &&
                             File.Exists(uri.LocalPath))
                    {
                        // External source file

                        try
                        {
                            stream = new FileStream(uri.LocalPath, FileMode.Open);
                        }
                        catch (FileNotFoundException ex)
                        {
                            failureReason = $"File '{uri.LocalPath}' could not be found: {ex.ToString()}";
                        }
                        catch (IOException ex)
                        {
                            failureReason = $"File '{uri.LocalPath}' could not be read: {ex.ToString()}";
                        }
                        catch (SecurityException ex)
                        {
                            failureReason = $"File '{uri.LocalPath}' could not be accessed: {ex.ToString()}";
                        }
                    }
                    
                }
            }

            if (stream == null && failureReason == null)
            {
                failureReason = $"File '{uri.LocalPath}' could not be opened";
            }

            if (failureReason != null)
            {
                // If we get here, we were unable to determine region character offset, so we have to warn the caller
                // TODO: add a warning to the list
            }

            return stream;
        }

        private StreamReader GetFileStreamReader(Uri uri, out Encoding encoding)
        {
            StreamReader reader = null;
            encoding = null;

            Stream contentStream = GetContentStream(uri, out encoding);
            if (contentStream != null)
            {
                // If we found embedded content, encoding was specified on the file object
                // If the content is in an extenal file, we let the StreamReader detect the encoding
                reader = encoding != null ?
                    new StreamReader(contentStream, encoding, false) :
                    new StreamReader(contentStream);
                encoding = encoding ?? reader.CurrentEncoding;
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
                    CodeFlows = v2Result.CodeFlows?.Select(CreateCodeFlow).ToList(),
                    Fixes = v2Result.Fixes?.Select(CreateFix).ToList(),
                    Id = v2Result.InstanceGuid,
                    Level = Utilities.CreateResultLevelVersionOne(v2Result.Level),
                    Locations = v2Result.Locations?.Select(CreateLocation).ToList(),
                    Message = v2Result.Message?.Text,
                    Properties = v2Result.Properties,
                    RelatedLocations = v2Result.RelatedLocations?.Select(CreateAnnotatedCodeLocation).ToList(),
                    Snippet = v2Result.Locations?[0]?.PhysicalLocation?.Region?.Snippet?.Text,
                    Stacks = v2Result.Stacks?.Select(CreateStack).ToList(),
                    SuppressionStates = Utilities.CreateSuppressionStatesVersionOne(v2Result.SuppressionStates)
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

                if (v2Result.AnalysisTarget != null)
                {
                    foreach (LocationVersionOne location in result.Locations)
                    {
                        location.AnalysisTarget = CreatePhysicalLocation(v2Result.AnalysisTarget);
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

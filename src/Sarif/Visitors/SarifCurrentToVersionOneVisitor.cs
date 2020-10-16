// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Utilities = Microsoft.CodeAnalysis.Sarif.Visitors.SarifTransformerUtilities;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class SarifCurrentToVersionOneVisitor : SarifRewritingVisitor
    {
        private static readonly SarifVersion FromSarifVersion = SarifVersion.Current;
        private static readonly string FromPropertyBagPrefix =
            Utilities.PropertyBagTransformerItemPrefixes[FromSarifVersion];

        private Run _currentV2Run = null;
        private RunVersionOne _currentRun = null;

        // To understand the purpose of these fields, see the comment on CreateFileKeyIndexMappings.
        private IDictionary<string, int> _v1FileKeyToV2IndexMap;
        private IDictionary<int, string> _v2FileIndexToV1KeyMap;

        // To understand the purpose of this field, see the comment on CreateRuleIndexToKeyMapping.
        private IDictionary<int, string> _v2RuleIndexToV1KeyMap;

        public bool EmbedVersionTwoContentInPropertyBag { get; set; }

        public SarifLogVersionOne SarifLogVersionOne { get; private set; }

        public override SarifLog VisitSarifLog(SarifLog v2SarifLog)
        {
            SarifLogVersionOne = new SarifLogVersionOne(SarifVersionVersionOne.OneZeroZero.ConvertToSchemaUri(),
                                                        SarifVersionVersionOne.OneZeroZero,
                                                        new List<RunVersionOne>());

            foreach (Run v2Run in v2SarifLog.Runs)
            {
                SarifLogVersionOne.Runs.Add(CreateRunVersionOne(v2Run));
            }

            return null;
        }

        internal AnnotatedCodeLocationVersionOne CreateAnnotatedCodeLocationVersionOne(Location v2Location)
        {
            AnnotatedCodeLocationVersionOne annotatedCodeLocation = null;

            if (v2Location != null)
            {
                annotatedCodeLocation = new AnnotatedCodeLocationVersionOne
                {
                    Annotations = v2Location.Annotations?.Select(CreateAnnotationVersionOne).ToList(),
                    FullyQualifiedLogicalName = v2Location.LogicalLocation?.FullyQualifiedName,
                    Message = v2Location.Message?.Text,
                    PhysicalLocation = CreatePhysicalLocationVersionOne(v2Location.PhysicalLocation),
                    Snippet = v2Location.PhysicalLocation?.Region?.Snippet?.Text
                };
            }

            return annotatedCodeLocation;
        }

        internal AnnotatedCodeLocationVersionOne CreateAnnotatedCodeLocationVersionOne(ThreadFlowLocation v2ThreadFlowLocation)
        {
            AnnotatedCodeLocationVersionOne annotatedCodeLocation = null;

            if (v2ThreadFlowLocation != null)
            {
                annotatedCodeLocation = CreateAnnotatedCodeLocationVersionOne(v2ThreadFlowLocation.Location);
                annotatedCodeLocation = annotatedCodeLocation ?? new AnnotatedCodeLocationVersionOne();

                annotatedCodeLocation.Importance = Utilities.CreateAnnotatedCodeLocationImportance(v2ThreadFlowLocation.Importance);
                annotatedCodeLocation.Module = v2ThreadFlowLocation.Module;
                annotatedCodeLocation.Properties = v2ThreadFlowLocation.Properties;
                annotatedCodeLocation.State = ConvertToV1MessageStringsDictionary(v2ThreadFlowLocation.State);
                annotatedCodeLocation.Step = v2ThreadFlowLocation.ExecutionOrder;
            }

            return annotatedCodeLocation;
        }

        internal AnnotationVersionOne CreateAnnotationVersionOne(Region v2Region)
        {
            AnnotationVersionOne annotation = null;

            if (v2Region != null)
            {
                annotation = new AnnotationVersionOne
                {
                    Locations = new List<PhysicalLocationVersionOne>
                    {
                        CreatePhysicalLocationVersionOne(v2Region)
                    },
                    Message = v2Region.Message?.Text
                };
            }

            return annotation;
        }

        internal ExceptionDataVersionOne CreateExceptionDataVersionOne(ExceptionData v2ExceptionData)
        {
            ExceptionDataVersionOne exceptionData = null;

            if (v2ExceptionData != null)
            {
                exceptionData = new ExceptionDataVersionOne
                {
                    InnerExceptions = v2ExceptionData.InnerExceptions?.Select(CreateExceptionDataVersionOne).ToList(),
                    Kind = v2ExceptionData.Kind,
                    Message = v2ExceptionData.Message,
                    Stack = CreateStackVersionOne(v2ExceptionData.Stack)
                };
            }

            return exceptionData;
        }

        internal FileChangeVersionOne CreateFileChangeVersionOne(ArtifactChange v2FileChange)
        {
            FileChangeVersionOne fileChange = null;

            if (v2FileChange != null)
            {
                string encodingName = GetFileEncodingName(v2FileChange.ArtifactLocation?.Uri);
                Encoding encoding = GetFileEncoding(encodingName);

                try
                {
                    fileChange = new FileChangeVersionOne
                    {
                        Replacements = v2FileChange.Replacements?.Select(r => CreateReplacementVersionOne(r, encoding)).ToList(),
                        Uri = v2FileChange.ArtifactLocation?.Uri,
                        UriBaseId = v2FileChange.ArtifactLocation?.UriBaseId
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
            IList<Artifact> files = _currentV2Run.Artifacts;

            if (uri != null &&
                files != null
                && _v1FileKeyToV2IndexMap != null
                && _v1FileKeyToV2IndexMap.TryGetValue(uri.OriginalString, out int index))
            {
                encodingName = files[index].Encoding;
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

        private FileDataVersionOne CreateFileDataVersionOne(Artifact v2FileData)
        {
            FileDataVersionOne fileData = null;

            if (v2FileData != null)
            {
                int parentIndex = v2FileData.ParentIndex;
                string parentKey = parentIndex == -1
                    ? null
                    : _v2FileIndexToV1KeyMap?[parentIndex];

                fileData = new FileDataVersionOne
                {
                    Hashes = CreateHashVersionOneListFromV2Hashes(v2FileData.Hashes),
                    Length = v2FileData.Length == -1 ? 0 : v2FileData.Length,
                    MimeType = v2FileData.MimeType,
                    Offset = v2FileData.Offset,
                    ParentKey = parentKey,
                    Properties = v2FileData.Properties,
                    Uri = v2FileData.Location?.Uri,
                    UriBaseId = v2FileData.Location?.UriBaseId
                };

                if (v2FileData.Contents != null)
                {
                    fileData.Contents = MimeType.IsTextualMimeType(v2FileData.MimeType) ?
                        SarifUtilities.GetUtf8Base64String(v2FileData.Contents.Text) :
                        v2FileData.Contents.Binary;
                }
            }

            return fileData;
        }

        internal FixVersionOne CreateFixVersionOne(Fix v2Fix)
        {
            FixVersionOne fix = null;

            if (v2Fix != null)
            {
                try
                {
                    fix = new FixVersionOne()
                    {
                        Description = v2Fix.Description?.Text,
                        FileChanges = v2Fix.ArtifactChanges?.Select(CreateFileChangeVersionOne).ToList()
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

        internal IList<HashVersionOne> CreateHashVersionOneListFromV2Hashes(IDictionary<string, string> v2Hashes)
        {
            if (v2Hashes == null) { return null; }

            var v1Hashes = new List<HashVersionOne>();

            foreach (string key in v2Hashes.Keys)
            {
                // If TryGetValue fails here, algorithm will be assigned the value of 0, which is 'Unknown'
                Utilities.AlgorithmNameKindMap.TryGetValue(key, out AlgorithmKindVersionOne algorithm);

                v1Hashes.Add(new HashVersionOne { Algorithm = algorithm, Value = v2Hashes[key] });
            }

            return v1Hashes;
        }

        internal InvocationVersionOne CreateInvocationVersionOne(Invocation v2Invocation)
        {
            InvocationVersionOne invocation = null;

            if (v2Invocation != null)
            {
                invocation = new InvocationVersionOne
                {
                    Account = v2Invocation.Account,
                    CommandLine = v2Invocation.CommandLine,
                    EndTime = v2Invocation.EndTimeUtc,
                    EnvironmentVariables = v2Invocation.EnvironmentVariables,
                    FileName = v2Invocation.ExecutableLocation?.Uri?.OriginalString,
                    Machine = v2Invocation.Machine,
                    ProcessId = v2Invocation.ProcessId,
                    Properties = v2Invocation.Properties,
                    ResponseFiles = CreateResponseFilesDictionary(v2Invocation.ResponseFiles),
                    StartTime = v2Invocation.StartTimeUtc,
                    WorkingDirectory = v2Invocation.WorkingDirectory?.Uri?.OriginalString
                };

                if (v2Invocation.ToolConfigurationNotifications != null)
                {
                    if (_currentRun.ConfigurationNotifications == null)
                    {
                        _currentRun.ConfigurationNotifications = new List<NotificationVersionOne>();
                    }

                    IEnumerable<NotificationVersionOne> notifications = v2Invocation.ToolConfigurationNotifications.Select(CreateNotificationVersionOne);
                    _currentRun.ConfigurationNotifications = _currentRun.ConfigurationNotifications.Union(notifications).ToList();
                }

                if (v2Invocation.ToolExecutionNotifications != null)
                {
                    if (_currentRun.ToolNotifications == null)
                    {
                        _currentRun.ToolNotifications = new List<NotificationVersionOne>();
                    }

                    List<NotificationVersionOne> notifications = v2Invocation.ToolExecutionNotifications.Select(CreateNotificationVersionOne).ToList();
                    _currentRun.ToolNotifications = _currentRun.ToolNotifications.Union(notifications).ToList();
                }
            }

            return invocation;
        }

        internal LocationVersionOne CreateLocationVersionOne(Location v2Location)
        {
            LocationVersionOne location = null;

            if (v2Location != null)
            {
                location = new LocationVersionOne
                {
                    FullyQualifiedLogicalName = v2Location.LogicalLocation?.FullyQualifiedName,
                    Properties = v2Location.Properties,
                    ResultFile = CreatePhysicalLocationVersionOne(v2Location.PhysicalLocation)
                };

                if (!string.IsNullOrWhiteSpace(v2Location.LogicalLocation?.FullyQualifiedName))
                {
                    if (v2Location.LogicalLocation.Index != -1)
                    {
                        location.DecoratedName = _currentV2Run.LogicalLocations[v2Location.LogicalLocation.Index].DecoratedName;
                    }
                }
            }

            return location;
        }

        internal LogicalLocationVersionOne CreateLogicalLocationVersionOne(LogicalLocation v2LogicalLocation)
        {
            LogicalLocationVersionOne logicalLocation = null;
            string parentKey = null;

            if (_currentV2Run.LogicalLocations != null && v2LogicalLocation.ParentIndex > -1)
            {
                parentKey = _currentV2Run.LogicalLocations[v2LogicalLocation.ParentIndex].FullyQualifiedName;
            }

            if (v2LogicalLocation != null)
            {
                logicalLocation = new LogicalLocationVersionOne
                {
                    Kind = v2LogicalLocation.Kind,
                    Name = v2LogicalLocation.Name,
                    ParentKey = parentKey
                };
            }

            return logicalLocation;
        }

        internal NotificationVersionOne CreateNotificationVersionOne(Notification v2Notification)
        {
            NotificationVersionOne notification = null;

            if (v2Notification != null)
            {
                notification = new NotificationVersionOne
                {
                    Exception = CreateExceptionDataVersionOne(v2Notification.Exception),
                    Id = v2Notification.Descriptor?.Id,
                    Level = Utilities.CreateNotificationLevelVersionOne(v2Notification.Level),
                    Message = v2Notification.Message?.Text,
                    PhysicalLocation = CreatePhysicalLocationVersionOne(v2Notification.Locations?[0].PhysicalLocation),
                    Properties = v2Notification.Properties,
                    RuleId = v2Notification.AssociatedRule?.Id,
                    ThreadId = v2Notification.ThreadId,
                    Time = v2Notification.TimeUtc
                };
            }

            return notification;
        }

        internal PhysicalLocationVersionOne CreatePhysicalLocationVersionOne(PhysicalLocation v2PhysicalLocation)
        {
            PhysicalLocationVersionOne physicalLocation = null;

            if (v2PhysicalLocation != null)
            {
                physicalLocation = new PhysicalLocationVersionOne
                {
                    Region = CreateRegionVersionOne(v2PhysicalLocation.Region, v2PhysicalLocation.ArtifactLocation?.Uri),
                    Uri = v2PhysicalLocation.ArtifactLocation?.Uri,
                    UriBaseId = v2PhysicalLocation.ArtifactLocation?.UriBaseId
                };
            }

            return physicalLocation;
        }

        internal PhysicalLocationVersionOne CreatePhysicalLocationVersionOne(ArtifactLocation v2FileLocation)
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

        internal PhysicalLocationVersionOne CreatePhysicalLocationVersionOne(Region v2Region)
        {
            PhysicalLocationVersionOne physicalLocation = null;

            if (v2Region != null)
            {
                physicalLocation = new PhysicalLocationVersionOne
                {
                    Region = CreateRegionVersionOne(v2Region, uri: null)
                };
            }

            return physicalLocation;
        }

        internal RegionVersionOne CreateRegionVersionOne(Region v2Region, Uri uri)
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
                        // Try to get the byte offset using the file encoding and contents
                        region.Offset = ConvertCharOffsetToByteOffset(v2Region.CharOffset, uri);
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
                            // In v2, if endColumn is missing, then the region extends to the end
                            // of the line (exclusive of any newline sequence). Use the file contents
                            // and encoding to determine how many columns are in the end line.
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

        internal string CreateToolFingerprintContributionVersionOne(IDictionary<string, string> v2PartialFingerprints)
        {
            string toolFingerprintContribution = null;

            if (v2PartialFingerprints?.Keys.Count > 0)
            {
                // V1 only supports one of what v2 refers to as "partial fingerprints". We arbitrarily take the
                // one with the "smallest" key.
                string smallestKey = v2PartialFingerprints.Keys.OrderBy(k => k).First();
                toolFingerprintContribution = v2PartialFingerprints[smallestKey];
            }

            return toolFingerprintContribution;
        }

        private int ConvertCharOffsetToByteOffset(int charOffset, Uri uri)
        {
            int byteOffset = 0;

            using (StreamReader reader = GetFileStreamReader(uri, out Encoding encoding))
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

            using (StreamReader reader = GetFileStreamReader(uri, out Encoding encoding))
            {
                if (reader != null)
                {
                    if (v2Region.StartLine > 0) // Use line and column 
                    {
                        string sourceLine = string.Empty;

                        // Read down to startLine (null return means EOF)
                        for (int i = 1; i <= v2Region.StartLine && sourceLine != null; sourceLine = reader.ReadLine(), i++) { }

                        if (sourceLine != null)
                        {
                            int startColumn = v2Region.StartColumn > 0
                                                ? v2Region.StartColumn
                                                : 1;

                            if (sourceLine.Length > startColumn)
                            {
                                // Since we read past startColumn, we need to back up using the base stream
                                Stream stream = reader.BaseStream;
                                stream.Position -= encoding.GetByteCount(sourceLine.Substring(startColumn - 1));
                            }

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

            using (StreamReader reader = GetFileStreamReader(uri, out Encoding encoding))
            {
                if (reader != null)
                {
                    string sourceLine = string.Empty;

                    // Read down to endLine (null return means EOF)
                    for (int i = 1; i <= v2Region.EndLine && sourceLine != null; sourceLine = reader.ReadLine(), i++) { }

                    if (sourceLine != null)
                    {
                        endColumn = sourceLine.Length >= v2Region.EndColumn
                            ? v2Region.EndColumn
                            : sourceLine.Length + 1;
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
            IList<Artifact> files = _currentV2Run.Artifacts;

            if (uri != null && files != null && _v1FileKeyToV2IndexMap != null)
            {
                if (_v1FileKeyToV2IndexMap.TryGetValue(uri.OriginalString, out int index))
                {
                    Artifact fileData = files[index];

                    // We need the encoding because the content might have been transcoded to UTF-8
                    string encodingName = fileData.Encoding ?? _currentV2Run.DefaultEncoding;
                    encoding = GetFileEncoding(encodingName);

                    if (encoding != null)
                    {
                        if (fileData.Contents?.Binary != null)
                        {
                            // Embedded binary file content

                            byte[] content = Convert.FromBase64String(fileData.Contents.Binary);
                            stream = new MemoryStream(content);
                        }
                        else if (fileData.Contents?.Text != null)
                        {
                            // Embedded text file content

                            byte[] content = encoding.GetBytes(fileData.Contents.Text);
                            stream = new MemoryStream(content);
                        }
                        else if (uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeFile && File.Exists(uri.LocalPath))
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
                            catch (UnauthorizedAccessException ex)
                            {
                                failureReason = $"File '{uri.LocalPath}' could not be accessed: {ex.ToString()}";
                            }
                        }
                    }
                    else
                    {
                        failureReason = $"Encoding for file '{uri.OriginalString}' could not be determined";
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

            Stream contentStream = GetContentStream(uri, out encoding);
            if (contentStream != null && encoding != null)
            {
                reader = new StreamReader(contentStream, encoding, detectEncodingFromByteOrderMarks: false);
            }

            return reader;
        }

        internal ReplacementVersionOne CreateReplacementVersionOne(Replacement v2Replacement, Encoding encoding)
        {
            ReplacementVersionOne replacement = null;

            if (v2Replacement != null)
            {
                replacement = new ReplacementVersionOne();
                ArtifactContent insertedContent = v2Replacement.InsertedContent;

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

        internal Dictionary<string, string> CreateResponseFilesDictionary(IList<ArtifactLocation> v2ResponseFilesList)
        {
            Dictionary<string, string> responseFiles = null;

            if (v2ResponseFilesList != null)
            {
                responseFiles = new Dictionary<string, string>();

                foreach (ArtifactLocation fileLocation in v2ResponseFilesList)
                {
                    string key = fileLocation.Uri.OriginalString;
                    if (_v1FileKeyToV2IndexMap != null && _v1FileKeyToV2IndexMap.TryGetValue(key, out int responseFileIndex))
                    {
                        Artifact responseFile = _currentV2Run.Artifacts[responseFileIndex];
                        responseFiles.Add(key, responseFile.Contents?.Text);
                    }
                }
            }

            return responseFiles;
        }

        internal ResultVersionOne CreateResultVersionOne(Result v2Result)
        {
            ResultVersionOne result = null;

            if (v2Result != null)
            {
                result = new ResultVersionOne
                {
                    BaselineState = Utilities.CreateBaselineStateVersionOne(v2Result.BaselineState),
                    Fixes = v2Result.Fixes?.Select(CreateFixVersionOne).ToList(),
                    Id = v2Result.Guid,
                    Level = Utilities.CreateResultLevelVersionOne(v2Result.Level, v2Result.Kind),
                    Locations = v2Result.Locations?.Select(CreateLocationVersionOne).ToList(),
                    Message = v2Result.Message?.Text,
                    Properties = v2Result.Properties,
                    RelatedLocations = v2Result.RelatedLocations?.Select(CreateAnnotatedCodeLocationVersionOne).ToList(),
                    Snippet = v2Result.Locations?[0]?.PhysicalLocation?.Region?.Snippet?.Text,
                    Stacks = v2Result.Stacks?.Select(CreateStackVersionOne).ToList(),
                    SuppressionStates = Utilities.CreateSuppressionStatesVersionOne(v2Result.Suppressions),
                    ToolFingerprintContribution = CreateToolFingerprintContributionVersionOne(v2Result.PartialFingerprints)
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
                        location.AnalysisTarget = CreatePhysicalLocationVersionOne(v2Result.AnalysisTarget);
                    }
                }

                result.RuleId = v2Result.RuleId;
                string ruleKey = GetV1RuleKeyFromV2Index(v2Result.RuleIndex, _v2RuleIndexToV1KeyMap);

                // If the rules dictionary key is the same as the rule id, don't set result.RuleKey;
                // leave it null. This way, we don't unnecessarily persist ruleKey in the v1 SARIF file.
                // That is, we persist
                //
                //   "ruleId": "TST0001"
                //
                // instead of
                //
                //   "ruleId": "TST0001",
                //   "ruleKey": "TST0001"
                //
                if (ruleKey != result.RuleId)
                {
                    result.RuleKey = ruleKey;
                }

                if (!string.IsNullOrWhiteSpace(v2Result.Message?.Id))
                {
                    result.FormattedRuleMessage = new FormattedRuleMessageVersionOne
                    {
                        Arguments = v2Result.Message?.Arguments,
                        FormatId = v2Result.Message.Id
                    };
                }
            }

            return result;
        }

        internal static RuleVersionOne CreateRuleVersionOne(ReportingDescriptor v2ReportingDescriptor)
        {
            RuleVersionOne rule = null;

            if (v2ReportingDescriptor != null)
            {
                rule = new RuleVersionOne
                {
                    FullDescription = v2ReportingDescriptor.FullDescription?.Text,
                    HelpUri = v2ReportingDescriptor.HelpUri,
                    Id = v2ReportingDescriptor.Id,
                    MessageFormats = ConvertToV1MessageStringsDictionary(v2ReportingDescriptor.MessageStrings),
                    Name = v2ReportingDescriptor.Name,
                    Properties = v2ReportingDescriptor.Properties,
                    ShortDescription = v2ReportingDescriptor.ShortDescription?.Text
                };

                if (v2ReportingDescriptor.DefaultConfiguration != null)
                {
                    rule.Configuration = v2ReportingDescriptor.DefaultConfiguration.Enabled ?
                            RuleConfigurationVersionOne.Enabled :
                            RuleConfigurationVersionOne.Disabled;
                    rule.DefaultLevel = Utilities.CreateResultLevelVersionOne(v2ReportingDescriptor.DefaultConfiguration.Level);
                }
            }

            return rule;
        }

        private static IDictionary<string, string> ConvertToV1MessageStringsDictionary(IDictionary<string, MultiformatMessageString> v2MessageStringsDictionary)
        {
            return v2MessageStringsDictionary?.ToDictionary(
                keyValuePair => keyValuePair.Key,
                keyValuePair => keyValuePair.Value.Text);
        }

        internal RunVersionOne CreateRunVersionOne(Run v2Run)
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

                    CreateFileKeyIndexMappings(v2Run.Artifacts, out _v1FileKeyToV2IndexMap, out _v2FileIndexToV1KeyMap);
                    _v2RuleIndexToV1KeyMap = CreateV2RuleIndexToV1KeyMapping(v2Run.Tool.Driver.Rules);

                    run.BaselineId = v2Run.BaselineGuid;
                    run.Files = CreateFileDataVersionOneDictionary();
                    run.Id = v2Run.AutomationDetails?.Id;
                    run.AutomationId = v2Run.RunAggregates?.FirstOrDefault()?.Id;

                    run.StableId = v2Run.AutomationDetails?.InstanceIdLogicalComponent();

                    run.Invocation = CreateInvocationVersionOne(v2Run.Invocations?[0]);
                    run.LogicalLocations = CreateLogicalLocationVersionOneDictionary(v2Run.LogicalLocations);
                    run.Properties = v2Run.Properties;
                    run.Results = new List<ResultVersionOne>();

                    run.Rules = ConvertRulesArrayToDictionary(_currentV2Run.Tool.Driver.Rules, _v2RuleIndexToV1KeyMap);
                    run.Tool = CreateToolVersionOne(v2Run.Tool, v2Run.Language);

                    foreach (Result v2Result in v2Run.Results)
                    {
                        run.Results.Add(CreateResultVersionOne(v2Result));
                    }

                    // Stash the entire v2 run in this v1 run's property bag
                    if (EmbedVersionTwoContentInPropertyBag)
                    {
                        run.SetProperty($"{FromPropertyBagPrefix}/run", v2Run);
                    }
                }
            }

            return run;
        }

        // In SARIF v1, run.files was a dictionary, whereas in v2 it is an array.
        // During the v2-to-v1 conversion, it is sometimes necessary to look up
        // information from a v2 FileData object. Rather than having to search the
        // v2 run.files array to find the required FileData object, we construct a
        // dictionary from the array so we can access the required FileData object
        // directly. It turns out that we need mappings in both directions: from
        // array index to dictionary key, and vice versa.
        private static void CreateFileKeyIndexMappings(
            IList<Artifact> v2Files,
            out IDictionary<string, int> fileKeyToIndexDictionary,
            out IDictionary<int, string> fileIndexToKeyDictionary)
        {
            fileKeyToIndexDictionary = null;
            fileIndexToKeyDictionary = null;

            if (v2Files != null)
            {
                fileKeyToIndexDictionary = new Dictionary<string, int>();
                fileIndexToKeyDictionary = new Dictionary<int, string>();

                for (int index = 0; index < v2Files.Count; ++index)
                {
                    Artifact v2File = v2Files[index];
                    string key = CreateFileDictionaryKey(v2File, v2Files);

                    fileKeyToIndexDictionary[key] = index;
                    fileIndexToKeyDictionary[index] = key;
                }
            }
        }

        // Given a v2 FileData object, synthesize a key that will be used to locate that object
        // in the v2 file data dictionary. We will use the same key to locate the corresponding
        // v1 FileData object when we create the v1 run.files dictionary.
        //
        // This method is necessary because, although ideally we would use the FileData
        // object's URI as the dictionary key, it is possible for two FileData objects to
        // have the same URI. For example:
        //
        //    1. Nested files with the same name might exist in two different containers.
        //    2. There might be two files with the same URI but difference uriBaseIds.
        //
        // To deal with Case #1, we synthesize a key as follows:
        //
        //    <root file URI>#<nested file path>/<level 2 nested file path>/...
        //
        // To deal with Case #2, we would follow the chain or uriBaseIds, prepending
        // the value of each one to the URI.
        //
        // WE DO NOT YET HANDLE CASE #2.
        private static string CreateFileDictionaryKey(Artifact v2File, IList<Artifact> v2Files)
        {
            var sb = new StringBuilder(v2File.Location.Uri.OriginalString);
            while (v2File.ParentIndex != -1)
            {
                Artifact parentFile = v2Files[v2File.ParentIndex];

                // The convention for building the key is as follows:
                // The root file URI is separated from the chain of nested files by '#';
                // The nested file URIs are separated from each other by '/'.
                if (parentFile.ParentIndex == -1)
                {
                    sb.Insert(0, '#');
                }
                else
                {
                    string path = parentFile.Location.Uri.OriginalString;
                    if (path.Length == 0 || path[0] != '/')
                    {
                        sb.Insert(0, '/');
                    }
                }

                sb.Insert(0, parentFile.Location.Uri.OriginalString);

                v2File = parentFile;
            }

            return sb.ToString();
        }

        private IDictionary<string, FileDataVersionOne> CreateFileDataVersionOneDictionary()
        {
            Dictionary<string, FileDataVersionOne> filesVersionOne = null;

            if (_v1FileKeyToV2IndexMap != null)
            {
                filesVersionOne = new Dictionary<string, FileDataVersionOne>();
                foreach (KeyValuePair<string, int> entry in _v1FileKeyToV2IndexMap)
                {
                    string key = entry.Key;
                    int index = entry.Value;
                    Artifact v2File = _currentV2Run.Artifacts[index];
                    FileDataVersionOne fileDataVersionOne = CreateFileDataVersionOne(v2File);

                    // There's no need to repeat the URI in the v1 FileData object
                    // if it matches the dictionary key.
                    if (fileDataVersionOne.Uri.OriginalString.Equals(key))
                    {
                        fileDataVersionOne.Uri = null;
                    }

                    filesVersionOne[key] = fileDataVersionOne;
                }
            }

            return filesVersionOne;
        }

        private IDictionary<string, LogicalLocationVersionOne> CreateLogicalLocationVersionOneDictionary(IList<LogicalLocation> logicalLocations)
        {
            Dictionary<string, LogicalLocationVersionOne> logicalLocationsVersionOne = null;

            if (logicalLocations != null)
            {
                logicalLocationsVersionOne = new Dictionary<string, LogicalLocationVersionOne>();
                foreach (LogicalLocation logicalLocation in logicalLocations)
                {
                    logicalLocationsVersionOne[logicalLocation.FullyQualifiedName] = CreateLogicalLocationVersionOne(logicalLocation);
                }
            }

            return logicalLocationsVersionOne;
        }

        // In SARIF v1, run.resources.rules was a dictionary, whereas in v2 it is an array.
        // Normally, the lookup key into the v1 rules dictionary is the rule id. But some
        // tools allow multiple rules to have the same id. In that case we must synthesize
        // a unique key for each rule with that id. We choose "<ruleId>-<n>", where <n> is
        // 1 for the second occurrence, 2 for the third, and so on.
        private static IDictionary<int, string> CreateV2RuleIndexToV1KeyMapping(IList<ReportingDescriptor> rules)
        {
            var v2RuleIndexToV1KeyMap = new Dictionary<int, string>();

            if (rules != null)
            {
                // Keep track of how many distinct rules have each id.
                var ruleIdToCountMap = new Dictionary<string, int>();

                for (int i = 0; i < rules.Count; ++i)
                {
                    string ruleId = rules[i].Id;
                    if (ruleId != null)
                    {
                        ruleIdToCountMap[ruleId] = ruleIdToCountMap.ContainsKey(ruleId)
                            ? ruleIdToCountMap[ruleId] + 1
                            : 1;

                        v2RuleIndexToV1KeyMap[i] = ruleIdToCountMap[ruleId] == 1
                            ? ruleId
                            : ruleId + '-' + (ruleIdToCountMap[ruleId] - 1).ToString();
                    }
                }
            }

            return v2RuleIndexToV1KeyMap;
        }

        private static string GetV1RuleKeyFromV2Index(
            int ruleIndex,
            IDictionary<int, string> v2RuleIndexToV1KeyMap)
        {
            v2RuleIndexToV1KeyMap.TryGetValue(ruleIndex, out string ruleKey);

            // If TryGetValue returned false, ruleKey was set to default(string),
            // otherwise known as null, which is what we want.
            return ruleKey;
        }

        private static IDictionary<string, RuleVersionOne> ConvertRulesArrayToDictionary(
            IList<ReportingDescriptor> v2Rules,
            IDictionary<int, string> v2RuleIndexToV1KeyMap)
        {
            IDictionary<string, RuleVersionOne> v1Rules = null;

            if (v2Rules != null)
            {
                v1Rules = new Dictionary<string, RuleVersionOne>();
                for (int i = 0; i < v2Rules.Count; ++i)
                {
                    ReportingDescriptor v2Rule = v2Rules[i];

                    RuleVersionOne v1Rule = CreateRuleVersionOne(v2Rule);
                    string key = GetV1RuleKeyFromV2Index(i, v2RuleIndexToV1KeyMap);

                    v1Rules[key] = v1Rule;
                }
            }

            return v1Rules;
        }

        internal StackVersionOne CreateStackVersionOne(Stack v2Stack)
        {
            StackVersionOne stack = null;

            if (v2Stack != null)
            {
                stack = new StackVersionOne
                {
                    Message = v2Stack.Message?.Text,
                    Properties = v2Stack.Properties,
                    Frames = v2Stack.Frames?.Select(CreateStackFrameVersionOne).ToList()
                };
            }

            return stack;
        }

        internal StackFrameVersionOne CreateStackFrameVersionOne(StackFrame v2StackFrame)
        {
            StackFrameVersionOne stackFrame = null;

            if (v2StackFrame != null)
            {
                stackFrame = new StackFrameVersionOne
                {
                    // https://github.com/Microsoft/sarif-sdk/issues/1469
                    // TODO: Ensure stackFrame.address and offset properties are being transformed correctly during v1 -> v2 conversion and vice-versa

                    // Address = v2StackFrame.Location?.PhysicalLocation?.Address?.AbsoluteAddress ?? 0,
                    Module = v2StackFrame.Module,
                    // Offset = v2StackFrame.Location?.PhysicalLocation?.Address?.OffsetFromParent ?? 0,
                    Parameters = v2StackFrame.Parameters,
                    Properties = v2StackFrame.Properties,
                    ThreadId = v2StackFrame.ThreadId
                };

                Location location = v2StackFrame.Location;
                if (location != null)
                {
                    string fqln = location.LogicalLocation?.FullyQualifiedName;

                    if (_currentV2Run.LogicalLocations != null &&
                        location.LogicalLocation != null &&
                        !string.IsNullOrWhiteSpace(_currentV2Run.LogicalLocations[location.LogicalLocation.Index].FullyQualifiedName))
                    {
                        stackFrame.FullyQualifiedLogicalName = _currentV2Run.LogicalLocations[location.LogicalLocation.Index].FullyQualifiedName;
                        stackFrame.LogicalLocationKey = fqln != _currentV2Run.LogicalLocations[location.LogicalLocation.Index].FullyQualifiedName ? fqln : null;
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
                        stackFrame.Uri = physicalLocation.ArtifactLocation?.Uri;
                        stackFrame.UriBaseId = physicalLocation.ArtifactLocation?.UriBaseId;
                    }
                }
            }

            return stackFrame;
        }

        internal ToolVersionOne CreateToolVersionOne(Tool v2Tool, string language)
        {
            ToolVersionOne tool = null;

            if (v2Tool != null)
            {
                tool = new ToolVersionOne()
                {
                    FileVersion = v2Tool.Driver.DottedQuadFileVersion,
                    FullName = v2Tool.Driver.FullName,
                    Language = language,
                    Name = v2Tool.Driver.Name,
                    Properties = v2Tool.Properties,
                    SemanticVersion = v2Tool.Driver.SemanticVersion,
                    Version = v2Tool.Driver.Version
                };
            }

            return tool;
        }
    }
}

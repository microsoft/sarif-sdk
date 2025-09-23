// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class SarifLogger : BaseLogger, IDisposable, IAnalysisLogger
    {
        private TextWriter _textWriter;
        private JsonTextWriter _jsonTextWriter;
        private ResultLogJsonWriter _issueLogJsonWriter;

        private readonly Run _run;
        private readonly bool _closeWriterOnDispose;
        private readonly OptionallyEmittedData _dataToInsert;
        private readonly OptionallyEmittedData _dataToRemove;
        private readonly FilePersistenceOptions _filePersistenceOptions;
        private readonly InsertOptionalDataVisitor _insertOptionalDataVisitor;

        protected const FilePersistenceOptions DefaultLogFilePersistenceOptions = FilePersistenceOptions.PrettyPrint;

        public SarifLogger(string outputFilePath,
                           FilePersistenceOptions logFilePersistenceOptions = DefaultLogFilePersistenceOptions,
                           OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
                           OptionallyEmittedData dataToRemove = OptionallyEmittedData.None,
                           Run run = null,
                           IEnumerable<string> analysisTargets = null,
                           IEnumerable<string> invocationTokensToRedact = null,
                           IEnumerable<string> invocationPropertiesToLog = null,
                           string defaultFileEncoding = null,
                           bool closeWriterOnDispose = true,
                           FailureLevelSet levels = null,
                           ResultKindSet kinds = null,
                           IEnumerable<string> insertProperties = null,
                           FileRegionsCache fileRegionsCache = null)
            : this(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read)),
                                    logFilePersistenceOptions,
                                    dataToInsert,
                                    dataToRemove,
                                    run,
                                    analysisTargets,
                                    invocationTokensToRedact,
                                    invocationPropertiesToLog,
                                    defaultFileEncoding,
                                    closeWriterOnDispose,
                                    levels,
                                    kinds,
                                    insertProperties,
                                    fileRegionsCache)
        {
        }

        public SarifLogger(TextWriter textWriter,
                           FilePersistenceOptions logFilePersistenceOptions = DefaultLogFilePersistenceOptions,
                           OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
                           OptionallyEmittedData dataToRemove = OptionallyEmittedData.None,
                           Run run = null,
                           IEnumerable<string> analysisTargets = null,
                           IEnumerable<string> invocationTokensToRedact = null,
                           IEnumerable<string> invocationPropertiesToLog = null,
                           string defaultFileEncoding = null,
                           bool closeWriterOnDispose = true,
                           FailureLevelSet levels = null,
                           ResultKindSet kinds = null,
                           IEnumerable<string> insertProperties = null,
                           FileRegionsCache fileRegionsCache = null) : base(failureLevels: levels, resultKinds: kinds)
        {
            _textWriter = textWriter;
            _closeWriterOnDispose = closeWriterOnDispose;
            _jsonTextWriter = new JsonTextWriter(_textWriter)
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            };

            _filePersistenceOptions = logFilePersistenceOptions;

            if (PrettyPrint)
            {
                // Indented output is preferable for debugging
                _jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
            }

            _jsonTextWriter.DateFormatString = DateTimeConverter.DateTimeFormat;
            _jsonTextWriter.CloseOutput = _closeWriterOnDispose;

            _issueLogJsonWriter = new ResultLogJsonWriter(_jsonTextWriter);
            RuleToReportingDescriptorReferenceMap = new Dictionary<ReportingDescriptor, ReportingDescriptorReference>(ReportingDescriptor.ValueComparer);
            RuleToIndexMap = new Dictionary<ReportingDescriptor, int>(ReportingDescriptor.ValueComparer);
            ExtensionGuidToIndexMap = new Dictionary<Guid, int>();

            _run = run ?? new Run();

            if (dataToInsert.HasFlag(OptionallyEmittedData.Hashes) ||
                dataToInsert.HasFlag(OptionallyEmittedData.RegionSnippets) ||
                dataToInsert.HasFlag(OptionallyEmittedData.ContextRegionSnippets))
            {
                _insertOptionalDataVisitor = new InsertOptionalDataVisitor(dataToInsert,
                                                                           fileRegionsCache,
                                                                           _run,
                                                                           insertProperties);
            }

            FileRegionsCache = fileRegionsCache ?? new FileRegionsCache();

            EnhanceRun(analysisTargets,
                       dataToInsert,
                       dataToRemove,
                       invocationTokensToRedact,
                       invocationPropertiesToLog,
                       defaultFileEncoding);

            _run.Tool ??= Tool.CreateFromAssemblyData();
            _dataToInsert = dataToInsert;
            _dataToRemove = dataToRemove;
            _issueLogJsonWriter.Initialize(_run);

            // Map existing Rules to ensure duplicates aren't created
            if (_run.Tool.Extensions != null)
            {
                RecordRules(extensionIndex: null, _run.Tool.Driver);

                for (int extensionIndex = 0; extensionIndex < _run.Tool.Extensions.Count; ++extensionIndex)
                {
                    ToolComponent extension = _run.Tool.Extensions[extensionIndex];
                    ExtensionGuidToIndexMap[extension.Guid.Value] = extensionIndex;
                    RecordRules(extensionIndex, extension);
                }

            }

            if (_run.Tool.Driver?.Rules != null)
            {
                for (int i = 0; i < _run.Tool.Driver.Rules.Count; ++i)
                {
                    RuleToIndexMap[_run.Tool.Driver.Rules[i]] = i;
                }
            }
        }

        private FileRegionsCache _fileRegionsCache;
        public FileRegionsCache FileRegionsCache
        {
            get => _fileRegionsCache;
            set
            {
                _fileRegionsCache = value;
                if (_insertOptionalDataVisitor != null)
                {

                    _insertOptionalDataVisitor.FileRegionsCache = _fileRegionsCache = value;
                }
            }
        }

        private void RecordRules(int? extensionIndex, ToolComponent toolComponent)
        {
            if (toolComponent.Rules == null) { return; }

            for (int ruleIndex = 0; ruleIndex < toolComponent.Rules.Count; ruleIndex++)
            {
                ReportingDescriptor rule = toolComponent.Rules[ruleIndex];
                RuleToReportingDescriptorReferenceMap[rule] =
                new ReportingDescriptorReference
                {
                    Id = rule.Id,
                    Index = ruleIndex,
                    ToolComponent = extensionIndex != null
                        ? new ToolComponentReference
                        {
                            Index = extensionIndex.Value,
                        }
                        : null,
                };
            }
        }

        private void EnhanceRun(IEnumerable<string> analysisTargets,
                                OptionallyEmittedData dataToInsert,
                                OptionallyEmittedData dataToRemove,
                                IEnumerable<string> invocationTokensToRedact,
                                IEnumerable<string> invocationPropertiesToLog,
                                string defaultFileEncoding = null)
        {
            _run.Invocations ??= new List<Invocation>();
            if (defaultFileEncoding != null)
            {
                _run.DefaultEncoding = defaultFileEncoding;
            }

            Encoding encoding = SarifUtilities.GetEncodingFromName(_run.DefaultEncoding);

            if (analysisTargets != null)
            {
                _run.Artifacts ??= new List<Artifact>();

                foreach (string target in analysisTargets)
                {
                    string uriText = UriHelper.MakeValidUri(target);
                    var uri = new Uri(uriText, UriKind.RelativeOrAbsolute);

                    HashData hashData = null;
                    if (dataToInsert.HasFlag(OptionallyEmittedData.Hashes) && FileRegionsCache != null)
                    {
                        hashData = FileRegionsCache.GetHashData(uri);
                    }

                    var artifact = Artifact.Create(
                        new Uri(target, UriKind.RelativeOrAbsolute),
                        dataToInsert,
                        encoding,
                        hashData: hashData);

                    var fileLocation = new ArtifactLocation
                    {
                        Uri = uri
                    };

                    artifact.Location = fileLocation;

                    // This call will insert the file object into run.Files if not already present
                    artifact.Location.Index = _run.GetFileIndex(
                        artifact.Location,
                        addToFilesTableIfNotPresent: true,
                        dataToInsert: dataToInsert,
                        encoding: encoding,
                        hashData: hashData);
                }
            }

            var invocation = Invocation.Create(
                emitMachineEnvironment: dataToInsert.HasFlag(OptionallyEmittedData.EnvironmentVariables),
                emitTimestamps: !dataToRemove.HasFlag(OptionallyEmittedData.NondeterministicProperties),
                invocationPropertiesToLog);

            // TODO we should actually redact across the complete log file context
            // by a dedicated rewriting visitor or some other approach.

            if (invocationTokensToRedact != null)
            {
                invocation.CommandLine = Redact(invocation.CommandLine, invocationTokensToRedact);
                invocation.Machine = Redact(invocation.Machine, invocationTokensToRedact);
                invocation.Account = Redact(invocation.Account, invocationTokensToRedact);

                if (invocation.WorkingDirectory != null)
                {
                    invocation.WorkingDirectory.Uri = Redact(invocation.WorkingDirectory.Uri, invocationTokensToRedact);
                }

                if (invocation.EnvironmentVariables != null)
                {
                    string[] keys = invocation.EnvironmentVariables.Keys.ToArray();

                    foreach (string key in keys)
                    {
                        string value = invocation.EnvironmentVariables[key];
                        invocation.EnvironmentVariables[key] = Redact(value, invocationTokensToRedact);
                    }
                }
            }

            _run.Invocations.Add(invocation);
        }

        public IDictionary<ReportingDescriptor, ReportingDescriptorReference> RuleToReportingDescriptorReferenceMap { get; }

        public IDictionary<ReportingDescriptor, int> RuleToIndexMap { get; }

        public Dictionary<Guid, int> ExtensionGuidToIndexMap { get; }

        public bool ComputeFileHashes => _dataToInsert.HasFlag(OptionallyEmittedData.Hashes);

        public bool PersistBinaryContents => _dataToInsert.HasFlag(OptionallyEmittedData.BinaryFiles);

        public bool PersistTextFileContents => _dataToInsert.HasFlag(OptionallyEmittedData.TextFiles);

        public bool PersistEnvironment => _dataToInsert.HasFlag(OptionallyEmittedData.EnvironmentVariables);

        public bool OverwriteExistingOutputFile => _filePersistenceOptions.HasFlag(FilePersistenceOptions.ForceOverwrite);

        public bool PrettyPrint => _filePersistenceOptions.HasFlag(FilePersistenceOptions.PrettyPrint);

        public bool Optimize => _filePersistenceOptions.HasFlag(FilePersistenceOptions.Optimize);

        public virtual void Dispose()
        {
            // Disposing the json writer closes the stream but the textwriter
            // still needs to be disposed or closed to write the results
            if (_issueLogJsonWriter != null)
            {
                _issueLogJsonWriter.CloseResults();

                if (_run?.Invocations?.Count > 0 && _run.Invocations[0].StartTimeUtc != new DateTime() &&
                    !_dataToRemove.HasFlag(OptionallyEmittedData.NondeterministicProperties))
                {
                    _run.Invocations[0].EndTimeUtc = DateTime.UtcNow;
                }

                _issueLogJsonWriter.CompleteRun();
                _issueLogJsonWriter.Dispose();
                _issueLogJsonWriter = null;
            }

            if (_closeWriterOnDispose)
            {
                _textWriter?.Dispose();
                _jsonTextWriter?.Close();

                _textWriter = null;
                _jsonTextWriter = null;
            }

            GC.SuppressFinalize(this);
        }

        public void AnalysisStarted()
        {
            _issueLogJsonWriter.OpenResults();
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            _insertOptionalDataVisitor?.VisitRun(_run);

            if (_run.Invocations != null && _run.Invocations.Count > 0 &&
                !_dataToRemove.HasFlag(OptionallyEmittedData.NondeterministicProperties))
            {
                _run.Invocations[0].EndTimeUtc = DateTime.UtcNow;
            }
        }

        public void Log(ReportingDescriptor rule, Result result, int? extensionIndex)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.RuleId != null && !result.RuleId.IsEqualToOrHierarchicalDescendantOf(rule.Id))
            {
                // The rule id '{0}' specified by the result does not match the actual id of the rule '{1}'
                string message = string.Format(CultureInfo.InvariantCulture, SdkResources.ResultRuleIdDoesNotMatchRule,
                    result.RuleId,
                    rule.Id);

                throw new ArgumentException(message);
            }

            if (!ShouldLog(result))
            {
                return;
            }

            if (extensionIndex == null)
            {
                result.RuleIndex = LogRule(rule);
            }
            else
            {
                result.Rule = LogRule(rule, extensionIndex.Value);
            }

            CaptureFilesInResult(result);

            _insertOptionalDataVisitor?.Visit(result);

            _issueLogJsonWriter.WriteResult(result);
        }

        private int LogRule(ReportingDescriptor rule)
        {
            if (!RuleToIndexMap.TryGetValue(rule, out int ruleIndex))
            {
                ruleIndex = RuleToIndexMap.Count;
                RuleToIndexMap[rule] = ruleIndex;
                _run.Tool.Driver.Rules ??= new OrderSensitiveValueComparisonList<ReportingDescriptor>(ReportingDescriptor.ValueComparer);
                _run.Tool.Driver.Rules.Add(rule);
            }

            return ruleIndex;
        }

        private ReportingDescriptorReference LogRule(ReportingDescriptor rule, int extensionIndex)
        {
            ToolComponent toolComponent = _run.Tool.Extensions[extensionIndex];

            if (!RuleToReportingDescriptorReferenceMap.TryGetValue(rule, out ReportingDescriptorReference reference))
            {
                toolComponent.Rules ??= new OrderSensitiveValueComparisonList<ReportingDescriptor>(ReportingDescriptor.ValueComparer);
                int ruleIndex = toolComponent.Rules.Count;
                toolComponent.Rules.Add(rule);

                reference = RuleToReportingDescriptorReferenceMap[rule] = new ReportingDescriptorReference
                {
                    Id = rule.Id,
                    Index = ruleIndex,
                    ToolComponent = new ToolComponentReference
                    {
                        Index = extensionIndex
                    },
                };
            }

            return reference;
        }

        private void CaptureFilesInResult(Result result)
        {
            if (result.AnalysisTarget != null)
            {
                CaptureArtifact(result.AnalysisTarget);
            }

            if (result.Locations != null)
            {
                foreach (Location location in result.Locations)
                {
                    if (location.PhysicalLocation != null)
                    {
                        CaptureArtifact(location.PhysicalLocation.ArtifactLocation);
                    }
                }
            }

            if (result.RelatedLocations != null)
            {
                foreach (Location relatedLocation in result.RelatedLocations)
                {
                    if (relatedLocation.PhysicalLocation != null)
                    {
                        CaptureArtifact(relatedLocation.PhysicalLocation.ArtifactLocation);
                    }
                }
            }

            if (result.Stacks != null)
            {
                foreach (Stack stack in result.Stacks)
                {
                    foreach (StackFrame frame in stack.Frames)
                    {
                        CaptureArtifact(frame.Location?.PhysicalLocation?.ArtifactLocation);
                    }
                }
            }

            if (result.CodeFlows != null)
            {
                foreach (CodeFlow codeFlow in result.CodeFlows)
                {
                    foreach (ThreadFlow threadFlow in codeFlow.ThreadFlows)
                    {
                        CaptureThreadFlowLocations(threadFlow.Locations);
                    }
                }
            }

            if (result.Fixes != null)
            {
                foreach (Fix fix in result.Fixes)
                {
                    if (fix.ArtifactChanges != null)
                    {
                        foreach (ArtifactChange fileChange in fix.ArtifactChanges)
                        {
                            CaptureArtifact(fileChange.ArtifactLocation);
                        }
                    }
                }
            }
        }

        private void CaptureThreadFlowLocations(IList<ThreadFlowLocation> locations)
        {
            if (locations == null) { return; }

            foreach (ThreadFlowLocation tfl in locations)
            {
                if (tfl.Location?.PhysicalLocation != null)
                {
                    CaptureArtifact(tfl.Location.PhysicalLocation.ArtifactLocation);
                }
            }
        }

        private void CaptureArtifact(ArtifactLocation fileLocation)
        {
            if (fileLocation == null || fileLocation.Uri == null)
            {
                return;
            }

            // We only populate the artifacts table is there is some data
            // in addition to the location URI. Otherwise, each results
            // stores the URI and an artifact index that points to an
            // entry that merely recapitulates the URI with no other data.
            bool createArtifactEntries =
                _dataToInsert.HasFlag(OptionallyEmittedData.Hashes) ||
                _dataToInsert.HasFlag(OptionallyEmittedData.TextFiles) ||
                _dataToInsert.HasFlag(OptionallyEmittedData.BinaryFiles) ||
                !string.IsNullOrEmpty(fileLocation.UriBaseId) ||
                this.Optimize;

            Encoding encoding = null;

            if (_run.DefaultEncoding != null)
            {
                try
                {
                    encoding = Encoding.GetEncoding(_run.DefaultEncoding);
                }
                catch (ArgumentException) { } // Unrecognized encoding name
            }

            HashData hashData = null;
            if (_dataToInsert.HasFlag(OptionallyEmittedData.Hashes) && FileRegionsCache != null)
            {
                hashData = FileRegionsCache.GetHashData(fileLocation.Uri);
            }

            // Ensure Artifact is in Run.Artifacts and ArtifactLocation.Index is set to point to it
            int index = _run.GetFileIndex(fileLocation,
                                          addToFilesTableIfNotPresent: createArtifactEntries,
                                          _dataToInsert,
                                          encoding,
                                          hashData);

            // Remove redundant Uri and UriBaseId once index has been set
            if (index > -1 && this.Optimize)
            {
                fileLocation.Uri = null;
                fileLocation.UriBaseId = null;
            }

            _insertOptionalDataVisitor?.VisitArtifactLocation(fileLocation);
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
        }

        public void TargetAnalyzed(IAnalysisContext context)
        {

        }

        public void LogToolNotification(Notification notification, ReportingDescriptor associatedRule)
        {
            if (!ShouldLog(notification))
            {
                return;
            }

            if (_run.Invocations.Count == 0)
            {
                _run.Invocations.Add(new Invocation());
            }

            _run.Invocations[0].ToolExecutionNotifications = _run.Invocations[0].ToolExecutionNotifications ?? new List<Notification>();
            _run.Invocations[0].ToolExecutionNotifications.Add(notification);
            _run.Invocations[0].ExecutionSuccessful &= notification.Level != FailureLevel.Error;

            CaptureFilesInNotification(notification);

            if (associatedRule != null)
            {
                int ruleIndex = LogRule(associatedRule);
                notification.AssociatedRule = new ReportingDescriptorReference
                {
                    Index = ruleIndex,
                    Id = associatedRule.Id,
                };
            }
        }

        public void LogConfigurationNotification(Notification notification)
        {
            if (!ShouldLog(notification))
            {
                return;
            }

            if (_run.Invocations.Count == 0)
            {
                _run.Invocations.Add(new Invocation());
            }

            _run.Invocations[0].ToolConfigurationNotifications = _run.Invocations[0].ToolConfigurationNotifications ?? new List<Notification>();
            _run.Invocations[0].ToolConfigurationNotifications.Add(notification);
            _run.Invocations[0].ExecutionSuccessful &= notification.Level != FailureLevel.Error;

            CaptureFilesInNotification(notification);
        }

        private void CaptureFilesInNotification(Notification notification)
        {
            if (notification.Locations != null)
            {
                foreach (Location location in notification.Locations)
                {
                    if (location.PhysicalLocation != null)
                    {
                        CaptureArtifact(location.PhysicalLocation.ArtifactLocation);
                    }
                }
            }
        }

        private static string Redact(string text, IEnumerable<string> tokensToRedact)
        {
            if (text == null) { return text; }

            foreach (string tokenToRedact in tokensToRedact)
            {
                text = text.Replace(tokenToRedact, SarifConstants.RedactedMarker);
            }
            return text;
        }

        private static Uri Redact(Uri uri, IEnumerable<string> tokensToRedact)
        {
            if (uri == null) { return uri; }

            string uriText = uri.OriginalString;

            foreach (string tokenToRedact in tokensToRedact)
            {
                uriText = uriText.Replace(tokenToRedact, SarifConstants.RedactedMarker);
            }
            return new Uri(uriText, UriKind.RelativeOrAbsolute);
        }
    }
}

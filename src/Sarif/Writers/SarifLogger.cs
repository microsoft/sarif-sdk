// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
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
        private readonly Run _run;
        private readonly TextWriter _textWriter;
        private readonly bool _persistArtifacts;
        private readonly bool _closeWriterOnDispose;
        private readonly LogFilePersistenceOptions _logFilePersistenceOptions;
        private readonly JsonTextWriter _jsonTextWriter;
        private readonly OptionallyEmittedData _dataToInsert;
        private readonly OptionallyEmittedData _dataToRemove;
        private readonly ResultLogJsonWriter _issueLogJsonWriter;
        private readonly InsertOptionalDataVisitor _insertOptionalDataVisitor;

        protected const LogFilePersistenceOptions DefaultLogFilePersistenceOptions = LogFilePersistenceOptions.PrettyPrint;

        public SarifLogger(string outputFilePath,
                           LogFilePersistenceOptions logFilePersistenceOptions = DefaultLogFilePersistenceOptions,
                           OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
                           OptionallyEmittedData dataToRemove = OptionallyEmittedData.None,
                           Tool tool = null,
                           Run run = null,
                           IEnumerable<string> analysisTargets = null,
                           IEnumerable<string> invocationTokensToRedact = null,
                           IEnumerable<string> invocationPropertiesToLog = null,
                           string defaultFileEncoding = null,
                           bool quiet = false,
                           IEnumerable<FailureLevel> levels = null,
                           IEnumerable<ResultKind> kinds = null,
                           IEnumerable<string> insertProperties = null)
            : this(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                  logFilePersistenceOptions: logFilePersistenceOptions,
                  dataToInsert: dataToInsert,
                  dataToRemove: dataToRemove,
                  tool: tool,
                  run: run,
                  analysisTargets: analysisTargets,
                  invocationTokensToRedact: invocationTokensToRedact,
                  invocationPropertiesToLog: invocationPropertiesToLog,
                  defaultFileEncoding: defaultFileEncoding,
                  quiet: quiet,
                  levels: levels,
                  kinds: kinds,
                  insertProperties: insertProperties)
        {
        }

        public SarifLogger(TextWriter textWriter,
                           LogFilePersistenceOptions logFilePersistenceOptions = DefaultLogFilePersistenceOptions,
                           OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
                           OptionallyEmittedData dataToRemove = OptionallyEmittedData.None,
                           Tool tool = null,
                           Run run = null,
                           IEnumerable<string> analysisTargets = null,
                           IEnumerable<string> invocationTokensToRedact = null,
                           IEnumerable<string> invocationPropertiesToLog = null,
                           string defaultFileEncoding = null,
                           bool closeWriterOnDispose = true,
                           bool quiet = false,
                           IEnumerable<FailureLevel> levels = null,
                           IEnumerable<ResultKind> kinds = null,
                           IEnumerable<string> insertProperties = null) : this(textWriter, logFilePersistenceOptions, closeWriterOnDispose, levels, kinds)
        {
            if (dataToInsert.HasFlag(OptionallyEmittedData.Hashes))
            {
                AnalysisTargetToHashDataMap = HashUtilities.MultithreadedComputeTargetFileHashes(analysisTargets, quiet) ?? new ConcurrentDictionary<string, HashData>();
            }

            _run = run ?? new Run();

            if (dataToInsert.HasFlag(OptionallyEmittedData.RegionSnippets) ||
                dataToInsert.HasFlag(OptionallyEmittedData.ContextRegionSnippets))
            {
                _insertOptionalDataVisitor = new InsertOptionalDataVisitor(dataToInsert, _run, insertProperties);
            }

            EnhanceRun(analysisTargets,
                       dataToInsert,
                       dataToRemove,
                       invocationTokensToRedact,
                       invocationPropertiesToLog,
                       defaultFileEncoding,
                       AnalysisTargetToHashDataMap);

            tool = tool ?? Tool.CreateFromAssemblyData();

            _run.Tool = tool;
            _dataToInsert = dataToInsert;
            _dataToRemove = dataToRemove;
            _issueLogJsonWriter.Initialize(_run);

            // Map existing Rules to ensure duplicates aren't created
            if (_run.Tool.Driver?.Rules != null)
            {
                for (int i = 0; i < _run.Tool.Driver.Rules.Count; ++i)
                {
                    RuleToIndexMap[_run.Tool.Driver.Rules[i]] = i;
                }
            }

            _persistArtifacts =
                (_dataToInsert & OptionallyEmittedData.Hashes) != 0 ||
                (_dataToInsert & OptionallyEmittedData.TextFiles) != 0 ||
                (_dataToInsert & OptionallyEmittedData.BinaryFiles) != 0;
        }

        private SarifLogger(TextWriter textWriter,
                            LogFilePersistenceOptions logFilePersistenceOptions,
                            bool closeWriterOnDipose,
                            IEnumerable<FailureLevel> levels,
                            IEnumerable<ResultKind> kinds) : base(failureLevels: levels, resultKinds: kinds)
        {
            _textWriter = textWriter;
            _closeWriterOnDispose = closeWriterOnDipose;
            _jsonTextWriter = new JsonTextWriter(_textWriter);

            _logFilePersistenceOptions = logFilePersistenceOptions;

            if (PrettyPrint)
            {
                // Indented output is preferable for debugging
                _jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
            }

            _jsonTextWriter.DateFormatString = DateTimeConverter.DateTimeFormat;
            _jsonTextWriter.CloseOutput = _closeWriterOnDispose;

            _issueLogJsonWriter = new ResultLogJsonWriter(_jsonTextWriter);
            RuleToIndexMap = new Dictionary<ReportingDescriptor, int>(ReportingDescriptor.ValueComparer);
        }

        private void EnhanceRun(IEnumerable<string> analysisTargets,
                                OptionallyEmittedData dataToInsert,
                                OptionallyEmittedData dataToRemove,
                                IEnumerable<string> invocationTokensToRedact,
                                IEnumerable<string> invocationPropertiesToLog,
                                string defaultFileEncoding = null,
                                IDictionary<string, HashData> filePathToHashDataMap = null)
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
                    Uri uri = new Uri(UriHelper.MakeValidUri(target), UriKind.RelativeOrAbsolute);

                    HashData hashData = null;
                    if (dataToInsert.HasFlag(OptionallyEmittedData.Hashes))
                    {
                        filePathToHashDataMap?.TryGetValue(target, out hashData);
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

        public IDictionary<string, HashData> AnalysisTargetToHashDataMap { get; }

        public IDictionary<ReportingDescriptor, int> RuleToIndexMap { get; }

        public bool ComputeFileHashes => _dataToInsert.HasFlag(OptionallyEmittedData.Hashes);

        public bool PersistBinaryContents => _dataToInsert.HasFlag(OptionallyEmittedData.BinaryFiles);

        public bool PersistTextFileContents => _dataToInsert.HasFlag(OptionallyEmittedData.TextFiles);

        public bool PersistEnvironment => _dataToInsert.HasFlag(OptionallyEmittedData.EnvironmentVariables);

        public bool OverwriteExistingOutputFile => _logFilePersistenceOptions.HasFlag(LogFilePersistenceOptions.OverwriteExistingOutputFile);

        public bool PrettyPrint => _logFilePersistenceOptions.HasFlag(LogFilePersistenceOptions.PrettyPrint);

        public bool Optimize => _logFilePersistenceOptions.HasFlag(LogFilePersistenceOptions.Optimize);

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
            }

            if (_closeWriterOnDispose)
            {
                if (_textWriter != null) { _textWriter.Dispose(); }
                if (_jsonTextWriter != null) { _jsonTextWriter.Close(); }
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

        public void Log(ReportingDescriptor rule, Result result)
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
                //The rule id '{0}' specified by the result does not match the actual id of the rule '{1}'
                string message = string.Format(CultureInfo.InvariantCulture, SdkResources.ResultRuleIdDoesNotMatchRule,
                    result.RuleId,
                    rule.Id);

                throw new ArgumentException(message);
            }

            if (!ShouldLog(result))
            {
                return;
            }

            result.RuleIndex = LogRule(rule);

            CaptureFilesInResult(result);

            if (_insertOptionalDataVisitor != null)
            {
                _insertOptionalDataVisitor.VisitResult(result);
            }

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
            AnalysisTargetToHashDataMap?.TryGetValue(fileLocation.Uri.OriginalString, out hashData);

            // Ensure Artifact is in Run.Artifacts and ArtifactLocation.Index is set to point to it
            int index = _run.GetFileIndex(fileLocation,
                                          addToFilesTableIfNotPresent: _persistArtifacts || hashData != null,
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

        public void LogToolNotification(Notification notification)
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

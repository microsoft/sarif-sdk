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
    public class SarifLogger : IDisposable, IAnalysisLogger
    {
        private readonly Run _run;
        private readonly TextWriter _textWriter;
        private readonly bool _closeWriterOnDispose;
        private readonly LoggingOptions _loggingOptions;
        private readonly JsonTextWriter _jsonTextWriter;
        private readonly OptionallyEmittedData _dataToInsert;
        private readonly OptionallyEmittedData _dataToRemove;
        private readonly ResultLogJsonWriter _issueLogJsonWriter;
        private readonly InsertOptionalDataVisitor _insertOptionalDataVisitor;

        protected const LoggingOptions DefaultLoggingOptions = LoggingOptions.PrettyPrint;

        public SarifLogger(
            string outputFilePath,
            LoggingOptions loggingOptions = DefaultLoggingOptions,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            OptionallyEmittedData dataToRemove = OptionallyEmittedData.None,
            Tool tool = null,
            Run run = null,
            IEnumerable<string> analysisTargets = null,
            IEnumerable<string> invocationTokensToRedact = null,
            IEnumerable<string> invocationPropertiesToLog = null,
            string defaultFileEncoding = null)
            : this(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                  loggingOptions: loggingOptions,
                  dataToInsert: dataToInsert,
                  dataToRemove: dataToRemove,
                  tool: tool,
                  run: run,
                  analysisTargets: analysisTargets,
                  invocationTokensToRedact: invocationTokensToRedact,
                  invocationPropertiesToLog: invocationPropertiesToLog,
                  defaultFileEncoding: defaultFileEncoding)
        {
        }

        public SarifLogger(
            TextWriter textWriter,
            LoggingOptions loggingOptions = DefaultLoggingOptions,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            OptionallyEmittedData dataToRemove = OptionallyEmittedData.None,
            Tool tool = null,
            Run run = null,
            IEnumerable<string> analysisTargets = null,
            IEnumerable<string> invocationTokensToRedact = null,
            IEnumerable<string> invocationPropertiesToLog = null,
            string defaultFileEncoding = null,
            bool closeWriterOnDispose = true) : this(textWriter, loggingOptions, closeWriterOnDispose)
        {
            if (dataToInsert.HasFlag(OptionallyEmittedData.Hashes))
            {
                AnalysisTargetToHashDataMap = HashUtilities.MultithreadedComputeTargetFileHashes(analysisTargets);
            }

            _run = run ?? new Run();

            if (dataToInsert.HasFlag(OptionallyEmittedData.RegionSnippets))
            {
                _insertOptionalDataVisitor = new InsertOptionalDataVisitor(dataToInsert, _run);
            }

            EnhanceRun(
                analysisTargets,
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
        }

        private SarifLogger(TextWriter textWriter, LoggingOptions loggingOptions, bool closeWriterOnDipose)
        {
            _textWriter = textWriter;
            _closeWriterOnDispose = closeWriterOnDipose;
            _jsonTextWriter = new JsonTextWriter(_textWriter);

            _loggingOptions = loggingOptions;

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

        private void EnhanceRun(
            IEnumerable<string> analysisTargets,
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

        public bool OverwriteExistingOutputFile => _loggingOptions.HasFlag(LoggingOptions.OverwriteExistingOutputFile);

        public bool PrettyPrint => _loggingOptions.HasFlag(LoggingOptions.PrettyPrint);

        public bool Verbose => _loggingOptions.HasFlag(LoggingOptions.Verbose);

        public bool Optimize => _loggingOptions.HasFlag(LoggingOptions.Optimize);

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
                if (_jsonTextWriter == null) { _jsonTextWriter.Close(); }
            }

            GC.SuppressFinalize(this);
        }

        public void LogMessage(bool verbose, string message)
        {
            // We do not persist these to log file
        }

        public void AnalysisStarted()
        {
            _issueLogJsonWriter.OpenResults();
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
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

            if (!ShouldLog(result.Level))
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
                _run.Tool.Driver.Rules = _run.Tool.Driver.Rules ?? new OrderSensitiveValueComparisonList<ReportingDescriptor>(ReportingDescriptor.ValueComparer);
                _run.Tool.Driver.Rules.Add(rule);
            }

            return ruleIndex;
        }

        private void CaptureFilesInResult(Result result)
        {
            if (result.AnalysisTarget != null)
            {
                CaptureFile(result.AnalysisTarget);
            }

            if (result.Locations != null)
            {
                foreach (Location location in result.Locations)
                {
                    if (location.PhysicalLocation != null)
                    {
                        CaptureFile(location.PhysicalLocation.ArtifactLocation);
                    }
                }
            }

            if (result.RelatedLocations != null)
            {
                foreach (Location relatedLocation in result.RelatedLocations)
                {
                    if (relatedLocation.PhysicalLocation != null)
                    {
                        CaptureFile(relatedLocation.PhysicalLocation.ArtifactLocation);
                    }
                }
            }

            if (result.Stacks != null)
            {
                foreach (Stack stack in result.Stacks)
                {
                    foreach (StackFrame frame in stack.Frames)
                    {
                        CaptureFile(frame.Location?.PhysicalLocation?.ArtifactLocation);
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
                            CaptureFile(fileChange.ArtifactLocation);
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
                    CaptureFile(tfl.Location.PhysicalLocation.ArtifactLocation);
                }
            }
        }

        private void CaptureFile(ArtifactLocation fileLocation)
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

            // Ensure Artifact is in Run.Artifacts and ArtifactLocation.Index is set to point to it
            _run.GetFileIndex(
                fileLocation,
                addToFilesTableIfNotPresent: true,
                _dataToInsert,
                encoding);

            // Remove redundant Uri and UriBaseId once index has been set
            if (this.Optimize)
            {
                fileLocation.Uri = null;
                fileLocation.UriBaseId = null;
            }
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
        }

        public void Log(FailureLevel level, IAnalysisContext context, Region region, string ruleMessageId, params string[] arguments)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            int ruleIndex = -1;
            if (context.Rule != null)
            {
                ruleIndex = LogRule(context.Rule);
            }

            ruleMessageId = RuleUtilities.NormalizeRuleMessageId(ruleMessageId, context.Rule.Id);
            LogJsonIssue(level, context.TargetUri.LocalPath, region, context.Rule.Id, ruleIndex, ruleMessageId, arguments);
        }

        private void LogJsonIssue(FailureLevel level, string targetPath, Region region, string ruleId, int ruleIndex, string ruleMessageId, params string[] arguments)
        {
            if (!ShouldLog(level))
            {
                return;
            }

            Result result = new Result
            {
                RuleId = ruleId,
                RuleIndex = ruleIndex,
                Message = new Message
                {
                    Id = ruleMessageId,
                    Arguments = arguments
                },
                Level = level
            };

            if (targetPath != null)
            {
                result.Locations = new List<Location> {
                    new Sarif.Location {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(targetPath)
                            },
                            Region = region
                        }
                    }
                };
            }

            _issueLogJsonWriter.WriteResult(result);
        }

        public bool ShouldLog(FailureLevel level)
        {
            switch (level)
            {
                case FailureLevel.None:
                case FailureLevel.Note:
                {
                    if (!Verbose)
                    {
                        return false;
                    }
                    break;
                }

                case FailureLevel.Error:
                case FailureLevel.Warning:
                {
                    break;
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }
            return true;
        }

        public void LogToolNotification(Notification notification)
        {
            if (_run.Invocations.Count == 0)
            {
                _run.Invocations.Add(new Invocation());
            }

            _run.Invocations[0].ToolExecutionNotifications = _run.Invocations[0].ToolExecutionNotifications ?? new List<Notification>();
            _run.Invocations[0].ToolExecutionNotifications.Add(notification);
            _run.Invocations[0].ExecutionSuccessful &= notification.Level != FailureLevel.Error;
        }

        public void LogConfigurationNotification(Notification notification)
        {
            if (_run.Invocations.Count == 0)
            {
                _run.Invocations.Add(new Invocation());
            }

            _run.Invocations[0].ToolConfigurationNotifications = _run.Invocations[0].ToolConfigurationNotifications ?? new List<Notification>();
            _run.Invocations[0].ToolConfigurationNotifications.Add(notification);
            _run.Invocations[0].ExecutionSuccessful &= notification.Level != FailureLevel.Error;
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
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class SarifLogger : IDisposable, IAnalysisLogger
    {
        private Run _run;
        private TextWriter _textWriter;
        private LoggingOptions _loggingOptions;
        private JsonTextWriter _jsonTextWriter;
        private OptionallyEmittedData _dataToInsert;
        private ResultLogJsonWriter _issueLogJsonWriter;
        private IDictionary<Rule, int> _ruleToIndexMap;

        protected const LoggingOptions DefaultLoggingOptions = LoggingOptions.PrettyPrint;

        private static Run CreateRun(
            IEnumerable<string> analysisTargets,
            OptionallyEmittedData dataToInsert,
            IEnumerable<string> invocationTokensToRedact,
            IEnumerable<string> invocationPropertiesToLog,
            string defaultFileEncoding = null)
        {
            var run = new Run
            {
                Invocations = new List<Invocation>(),
                DefaultFileEncoding = defaultFileEncoding
            };

            if (analysisTargets != null)
            {
                run.Files = new List<FileData>();

                foreach (string target in analysisTargets)
                {
                    Uri uri = new Uri(UriHelper.MakeValidUri(target), UriKind.RelativeOrAbsolute);

                    var fileData = FileData.Create(
                        new Uri(target, UriKind.RelativeOrAbsolute),
                        dataToInsert);

                    var fileLocation = new FileLocation
                    {
                        Uri = uri
                    };

                    fileData.FileLocation = fileLocation;

                    // This call will insert the file object into run.Files if not already present
                    fileData.FileLocation.FileIndex = run.GetFileIndex(fileData.FileLocation, addToFilesTableIfNotPresent: true, dataToInsert);
                }
            }

            var invocation = Invocation.Create(dataToInsert.HasFlag(OptionallyEmittedData.EnvironmentVariables), invocationPropertiesToLog);

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

            run.Invocations.Add(invocation);
            return run;
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

        public SarifLogger(
            string outputFilePath,
            LoggingOptions loggingOptions = DefaultLoggingOptions,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            Tool tool = null,
            Run run = null,
            IEnumerable<string> analysisTargets = null,
            IEnumerable<string> invocationTokensToRedact = null,
            IEnumerable<string> invocationPropertiesToLog = null,
            string defaultFileEncoding = null)
            : this(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                  loggingOptions: loggingOptions,
                  dataToInsert: dataToInsert,
                  tool: tool,
                  run: run,
                  analysisTargets: analysisTargets,
                  invocationTokensToRedact: invocationTokensToRedact,
                  invocationPropertiesToLog: invocationPropertiesToLog)
        {
        }

        public SarifLogger(
            TextWriter textWriter,
            LoggingOptions loggingOptions = DefaultLoggingOptions,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            Tool tool = null,
            Run run = null,
            IEnumerable<string> analysisTargets = null,
            IEnumerable<string> invocationTokensToRedact = null,
            IEnumerable<string> invocationPropertiesToLog = null,
            string defaultFileEncoding = null) : this(textWriter, loggingOptions)
        {
            _run = run ?? CreateRun(
                            analysisTargets,
                            dataToInsert,
                            invocationTokensToRedact,
                            invocationPropertiesToLog,
                            defaultFileEncoding);



            tool = tool ?? Tool.CreateFromAssemblyData();
            SetSarifLoggerVersion(tool);

            _run.Tool = tool;
            _dataToInsert = dataToInsert;
            _issueLogJsonWriter.Initialize(_run);

        }

        private static void SetSarifLoggerVersion(Tool tool)
        {
            string sarifLoggerLocation = typeof(SarifLogger).Assembly.Location;
            tool.SarifLoggerVersion = FileVersionInfo.GetVersionInfo(sarifLoggerLocation).FileVersion;
        }

        private SarifLogger(TextWriter textWriter, LoggingOptions loggingOptions)
        {
            _textWriter = textWriter;
            _jsonTextWriter = new JsonTextWriter(_textWriter);

            _loggingOptions = loggingOptions;

            if (PrettyPrint)
            {
                // Indented output is preferable for debugging
                _jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
            }        

            _jsonTextWriter.DateFormatString = DateTimeConverter.DateTimeFormat;

            _issueLogJsonWriter = new ResultLogJsonWriter(_jsonTextWriter);
        }

        public IDictionary<Rule, int> RuleToIndexMap
        {
            get
            {
                _ruleToIndexMap = _ruleToIndexMap ?? new Dictionary<Rule, int>(Rule.ValueComparer);
                return _ruleToIndexMap;
            }
        }

        public bool ComputeFileHashes { get { return _dataToInsert.HasFlag(OptionallyEmittedData.Hashes); } }

        public bool PersistBinaryContents { get { return _dataToInsert.HasFlag(OptionallyEmittedData.BinaryFiles); } }

        public bool PersistTextFileContents { get { return _dataToInsert.HasFlag(OptionallyEmittedData.TextFiles); } }

        public bool PersistEnvironment { get { return _dataToInsert.HasFlag(OptionallyEmittedData.EnvironmentVariables); } }

        public bool OverwriteExistingOutputFile { get { return _loggingOptions.HasFlag(LoggingOptions.OverwriteExistingOutputFile); } }

        public bool PrettyPrint { get { return _loggingOptions.HasFlag(LoggingOptions.PrettyPrint); } }

        public bool Verbose { get { return _loggingOptions.HasFlag(LoggingOptions.Verbose); } }

        public virtual void Dispose()
        {
            // Disposing the json writer closes the stream but the textwriter 
            // still needs to be disposed or closed to write the results
            if (_issueLogJsonWriter != null)
            {
                _issueLogJsonWriter.CloseResults();

                if (_run?.Invocations?.Count > 0 && _run.Invocations[0].StartTimeUtc != new DateTime())
                {
                    _run.Invocations[0].EndTimeUtc = DateTime.UtcNow;
                }

                // Note: we write out the backing rules
                // to prevent the property accessor from populating
                // this data with an empty collection.
                if (_ruleToIndexMap != null)
                {
                    _issueLogJsonWriter.WriteRules(new List<Rule>(_ruleToIndexMap.Keys));
                }

                if (_run?.Files != null)
                {
                    _issueLogJsonWriter.WriteFiles(_run.Files);
                }

                if (_run?.Invocations != null)
                {
                    _issueLogJsonWriter.WriteInvocations(invocations: _run.Invocations);
                }

                if (_run?.Properties != null)
                {
                    _issueLogJsonWriter.WriteRunProperties(_run.Properties);
                }

                _issueLogJsonWriter.Dispose();
            }

            if (_textWriter != null) { _textWriter.Dispose(); }

            if (_jsonTextWriter == null) { _jsonTextWriter.Close(); }

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
            if (_run.Invocations != null && _run.Invocations.Count > 0)
            {
                _run.Invocations[0].EndTimeUtc = DateTime.UtcNow;
            }
        }

        public void Log(IRule iRule, Result result)
        {
            if (iRule == null)
            {
                throw new ArgumentNullException(nameof(iRule));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (iRule.Id != result.RuleId)
            {
                //The rule id '{0}' specified by the result does not match the actual id of the rule '{1}'
                string message = string.Format(CultureInfo.InvariantCulture, SdkResources.ResultRuleIdDoesNotMatchRule,
                    result.RuleId.ToString(),
                    iRule.Id.ToString());

                throw new ArgumentException(message);
            }

            if (!ShouldLog(result.Level))
            {
                return;
            }

            result.RuleIndex = LogRule(iRule);

            CaptureFilesInResult(result);
            _issueLogJsonWriter.WriteResult(result);
        }

        private int LogRule(IRule iRule)
        {
            // TODO: we need to finish eliminating the IRule interface from the OM
            // https://github.com/Microsoft/sarif-sdk/issues/1189
            Rule rule = (Rule)iRule;

            if (!RuleToIndexMap.TryGetValue(rule, out int ruleIndex))
            {
                ruleIndex = _ruleToIndexMap.Count;
                _ruleToIndexMap[rule] = ruleIndex;
                _run.Resources = _run.Resources ?? new Resources();
                _run.Resources.Rules = _run.Resources.Rules ?? new OrderSensitiveValueComparisonList<Rule>(Rule.ValueComparer);
                _run.Resources.Rules.Add(rule);
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
                        CaptureFile(location.PhysicalLocation.FileLocation);
                    }
                }
            }

            if (result.RelatedLocations != null)
            {
                foreach (Location relatedLocation in result.RelatedLocations)
                {
                    if (relatedLocation.PhysicalLocation != null)
                    {
                        CaptureFile(relatedLocation.PhysicalLocation.FileLocation);
                    }
                }
            }

            if (result.Stacks != null)
            {
                foreach (Stack stack in result.Stacks)
                {
                    foreach (StackFrame frame in stack.Frames)
                    {
                        CaptureFile(frame.Location?.PhysicalLocation?.FileLocation);
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
                    if (fix.FileChanges != null)
                    {
                        foreach (FileChange fileChange in fix.FileChanges)
                        {
                            CaptureFile(fileChange.FileLocation);
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
                    CaptureFile(tfl.Location.PhysicalLocation.FileLocation);
                }
            }
        }

        private void CaptureFile(FileLocation fileLocation)
        {             
            if (fileLocation == null || fileLocation.Uri == null)
            {
                return;
            }

            Encoding encoding = null;
            try
            {
                encoding = Encoding.GetEncoding(_run.DefaultFileEncoding);
            }
            catch (ArgumentException) { } // Unrecognized or null encoding name

            _run.GetFileIndex(
                fileLocation, 
                addToFilesTableIfNotPresent: true,
                _dataToInsert,
                encoding);
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            // This code doesn't call through a common helper, such as
            // provided by the SDK Notes class, because we are in a specific
            // logger. If we called through a helper, we'd re-enter
            // through all aggregated loggers.

            // Analyzing target '{0}'...

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string message = string.Format(CultureInfo.InvariantCulture, 
                SdkResources.MSG001_AnalyzingTarget,
                context.TargetUri.GetFileName());

            LogToolNotification(
                new Notification
                {
                    PhysicalLocation = new PhysicalLocation
                    {
                        FileLocation = new FileLocation
                        {
                            Uri = context.TargetUri
                        }
                    },
                    Id = Notes.Msg001AnalyzingTarget,
                    Message = new Message { Text = message },
                    Level = NotificationLevel.Note,
                    TimeUtc = DateTime.UtcNow,
                });
        }

        public void Log(ResultLevel messageKind, IAnalysisContext context, Region region, string ruleMessageId, params string[] arguments)
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
            LogJsonIssue(messageKind, context.TargetUri.LocalPath, region, context.Rule.Id, ruleIndex, ruleMessageId, arguments);
        }

        private void LogJsonIssue(ResultLevel level, string targetPath, Region region, string ruleId, int ruleIndex, string ruleMessageId, params string[] arguments)
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
                    MessageId = ruleMessageId,
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
                            FileLocation = new FileLocation
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

        public bool ShouldLog(ResultLevel level)
        {
            switch (level)
            {
                case ResultLevel.Note:
                case ResultLevel.Pass:
                case ResultLevel.Open:
                case ResultLevel.NotApplicable:
                {
                    if (!Verbose)
                    {
                        return false;
                    }
                    break;
                }

                case ResultLevel.Error:
                case ResultLevel.Default:
                case ResultLevel.Warning:
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

            _run.Invocations[0].ToolNotifications = _run.Invocations[0].ToolNotifications ?? new List<Notification>();
            _run.Invocations[0].ToolNotifications.Add(notification);
        }

        public void LogConfigurationNotification(Notification notification)
        {
            if (_run.Invocations.Count == 0)
            {
                _run.Invocations.Add(new Invocation());
            }

            _run.Invocations[0].ConfigurationNotifications = _run.Invocations[0].ConfigurationNotifications ?? new List<Notification>();
            _run.Invocations[0].ConfigurationNotifications.Add(notification);
        }
    }
}
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
    sealed public class SarifLogger : IDisposable, IAnalysisLogger
    {
        private Run _run;
        private TextWriter _textWriter;
        private LoggingOptions _loggingOptions;
        private JsonTextWriter _jsonTextWriter;
        private ResultLogJsonWriter _issueLogJsonWriter;
        private Dictionary<string, IRule> _rules;

        private const LoggingOptions DefaultLoggingOptions = LoggingOptions.PrettyPrint;

        private static Run CreateRun(
            IEnumerable<string> analysisTargets,
            LoggingOptions loggingOptions,
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
                run.Files = new Dictionary<string, FileData>();

                foreach (string target in analysisTargets)
                {
                    string fileDataKey = UriHelper.MakeValidUri(target);

                    var fileData = FileData.Create(
                        new Uri(target, UriKind.RelativeOrAbsolute),
                        loggingOptions);

                    run.Files.Add(fileDataKey, fileData);
                }
            }

            var invocation = Invocation.Create(loggingOptions.Includes(LoggingOptions.PersistEnvironment), invocationPropertiesToLog);

            // TODO we should actually redact across the complete log file context
            // by a dedicated rewriting visitor or some other approach.
            if (invocationTokensToRedact != null)
            {
                invocation.CommandLine = Redact(invocation.CommandLine, invocationTokensToRedact);
                invocation.Machine = Redact(invocation.Machine, invocationTokensToRedact);
                invocation.Account = Redact(invocation.Account, invocationTokensToRedact);
                invocation.WorkingDirectory = Redact(invocation.WorkingDirectory, invocationTokensToRedact);

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
            if (text == null ) { return text; }

            foreach (string tokenToRedact in tokensToRedact)
            {
                text = text.Replace(tokenToRedact, SarifConstants.RemovedMarker);
            }
            return text;
        }

        public SarifLogger(
            string outputFilePath, 
            LoggingOptions loggingOptions = DefaultLoggingOptions,
            Tool tool = null,
            Run run = null,
            IEnumerable<string> analysisTargets = null,
            string prereleaseInfo = null,
            IEnumerable<string> invocationTokensToRedact = null,
            IEnumerable<string> invocationPropertiesToLog = null,
            string defaultFileEncoding = null)
            : this(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                  loggingOptions,
                  tool,
                  run)
        {
        }

        public SarifLogger(
            TextWriter textWriter,
            LoggingOptions loggingOptions = LoggingOptions.PrettyPrint,
            Tool tool = null,
            Run run = null,
            IEnumerable<string> analysisTargets = null,
            bool targetsAreTextFiles = true,
            string prereleaseInfo = null,
            IEnumerable<string> invocationTokensToRedact = null,
            IEnumerable<string> invocationPropertiesToLog = null,
            string defaultFileEncoding = null) : this(textWriter, loggingOptions)
        {
            _run = run ?? CreateRun(
                            analysisTargets,
                            loggingOptions,
                            invocationTokensToRedact,
                            invocationPropertiesToLog,
                            defaultFileEncoding);

            if (!string.IsNullOrWhiteSpace(defaultFileEncoding))
            {
                _run.DefaultFileEncoding = defaultFileEncoding;
            }


            tool = tool ?? Tool.CreateFromAssemblyData();
            SetSarifLoggerVersion(tool);
            _issueLogJsonWriter.WriteTool(tool);
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

        public Dictionary<string, IRule> Rules
        {
            get
            {
                _rules = _rules ?? new Dictionary<string, IRule>();
                return _rules;
            }
        }

        public bool ComputeFileHashes { get { return _loggingOptions.Includes(LoggingOptions.ComputeFileHashes); } }

        public bool OverwriteExistingOutputFile { get { return _loggingOptions.Includes(LoggingOptions.OverwriteExistingOutputFile); } }

        public bool PersistEnvironment { get { return _loggingOptions.Includes(LoggingOptions.PersistEnvironment); } }

        public bool PersistFileContents { get { return _loggingOptions.Includes(LoggingOptions.PersistFileContents); } }

        public bool PrettyPrint { get { return _loggingOptions.Includes(LoggingOptions.PrettyPrint); } }

        public bool Verbose { get { return _loggingOptions.Includes(LoggingOptions.Verbose); } }

        public void Dispose()
        {
            // Disposing the json writer closes the stream but the textwriter 
            // still needs to be disposed or closed to write the results
            if (_issueLogJsonWriter != null)
            {
                _issueLogJsonWriter.CloseResults();

                if (_run?.Invocations?.Count > 0 && _run.Invocations[0].StartTime != new DateTime())
                {
                    _run.Invocations[0].EndTime = DateTime.UtcNow;
                }

                // Note: we write out the backing rules
                // to prevent the property accessor from populating
                // this data with an empty collection.
                if (_rules != null)
                {
                    _issueLogJsonWriter.WriteRules(_rules);
                }

                if (_run?.Files != null)
                {
                    _issueLogJsonWriter.WriteFiles(_run.Files);
                }

                if (_run?.Invocations != null)
                {
                    _issueLogJsonWriter.WriteInvocations(invocations: _run.Invocations);
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
                _run.Invocations[0].EndTime = DateTime.UtcNow;
            }
        }

        public void Log(IRule rule, Result result)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (rule.Id != result.RuleId)
            {
                //The rule id '{0}' specified by the result does not match the actual id of the rule '{1}'
                string message = string.Format(CultureInfo.InvariantCulture, SdkResources.ResultRuleIdDoesNotMatchRule,
                    result.RuleId.ToString(),
                    rule.Id.ToString());

                throw new ArgumentException(message);
            }

            if (!ShouldLog(result.Level))
            {
                return;
            }

            Rules[result.RuleKey ?? result.RuleId] = rule;

            CaptureFilesInResult(result);
            _issueLogJsonWriter.WriteResult(result);
        }

        private void CaptureFilesInResult(Result result)
        {
            if (result.AnalysisTarget != null)
            {
                CaptureFile(result.AnalysisTarget.Uri);
            }

            if (result.Locations != null)
            {
                foreach (Location location in result.Locations)
                {
                    if (location.PhysicalLocation != null)
                    {
                        CaptureFile(location.PhysicalLocation.FileLocation?.Uri);
                    }
                }
            }

            if (result.RelatedLocations != null)
            {
                foreach (Location relatedLocation in result.RelatedLocations)
                {
                    if (relatedLocation.PhysicalLocation != null)
                    {
                        CaptureFile(relatedLocation.PhysicalLocation.FileLocation?.Uri);
                    }
                }
            }

            if (result.Stacks != null)
            {
                foreach (Stack stack in result.Stacks)
                {
                    foreach (StackFrame frame in stack.Frames)
                    {
                        CaptureFile(frame.Location?.PhysicalLocation?.FileLocation?.Uri);
                    }
                }
            }

            if (result.CodeFlows != null)
            {
                foreach (CodeFlow codeFlow in result.CodeFlows)
                {
                    foreach (ThreadFlow threadFlow in codeFlow.ThreadFlows)
                    {
                        CaptureCodeFlowLocations(threadFlow.Locations);
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
                            CaptureFile(fileChange.FileLocation.Uri);
                        }
                    }
                }
            }
        }

        private void CaptureCodeFlowLocations(IList<CodeFlowLocation> locations)
        {
            if (locations == null) { return; }

            foreach (CodeFlowLocation cfl in locations)
            {
                if (cfl.Location?.PhysicalLocation != null)
                {
                    CaptureFile(cfl.Location.PhysicalLocation.FileLocation?.Uri);
                }
            }
        }

        private void CaptureFile(Uri uri)
        { 
            if (uri == null) { return; }

            _run.Files = _run.Files ?? new Dictionary<string, FileData>();

            string fileDataKey = UriHelper.MakeValidUri(uri.OriginalString);
            if (_run.Files.ContainsKey(fileDataKey))
            {
                // Already populated
                return;
            }


            Encoding encoding = null;

            try
            {
                encoding = Encoding.GetEncoding(_run.DefaultFileEncoding);
            }
            catch (ArgumentException) { }

            _run.Files[fileDataKey] = FileData.Create(uri, _loggingOptions, null, encoding);
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
                    Time = DateTime.UtcNow,
                });
        }

        public void Log(ResultLevel messageKind, IAnalysisContext context, Region region, string ruleMessageId, params string[] arguments)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Rule != null)
            {
                Rules[context.Rule.Id] = context.Rule;
            }

            ruleMessageId = RuleUtilities.NormalizeRuleMessageId(ruleMessageId, context.Rule.Id);
            LogJsonIssue(messageKind, context.TargetUri.LocalPath, region, context.Rule.Id, ruleMessageId, arguments);
        }

        private void LogJsonIssue(ResultLevel level, string targetPath, Region region, string ruleId, string ruleMessageId, params string[] arguments)
        {
            if (!ShouldLog(level))
            {
                return;
            }

            Result result = new Result();

            result.RuleId = ruleId;

            result.Message = new Message()
            {
                MessageId = ruleMessageId,
                Arguments = arguments
            };

            result.Level = level;

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
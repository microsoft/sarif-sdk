// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Readers;

using Newtonsoft.Json;
using System.Globalization;

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

        private static Run CreateRun(
            IEnumerable<string> analysisTargets,
            bool computeTargetsHash,
            bool logEnvironment,
            IEnumerable<string> invocationTokensToRedact,
            IEnumerable<string> invocationPropertiesToLog)
        {
            var run = new Run();

            if (analysisTargets != null)
            {
                run.Files = new Dictionary<string, FileData>();

                foreach (string target in analysisTargets)
                {
                    string fileDataKey = UriHelper.MakeValidUri(target);

                    var fileData = FileData.Create(
                        new Uri(target, UriKind.RelativeOrAbsolute), 
                        computeTargetsHash);

                    run.Files.Add(fileDataKey, fileData);
                }
            }

            run.Invocation = Invocation.Create(logEnvironment, invocationPropertiesToLog);

            // TODO we should actually redact across the complete log file context
            // by a dedicated rewriting visitor or some other approach.
            if (invocationTokensToRedact != null)
            {
                run.Invocation.CommandLine = Redact(run.Invocation.CommandLine, invocationTokensToRedact);
                run.Invocation.Machine = Redact(run.Invocation.Machine, invocationTokensToRedact);
                run.Invocation.Account = Redact(run.Invocation.Account, invocationTokensToRedact);
                run.Invocation.WorkingDirectory = Redact(run.Invocation.WorkingDirectory, invocationTokensToRedact);

                if (run.Invocation.EnvironmentVariables != null)
                {
                    string[] keys = run.Invocation.EnvironmentVariables.Keys.ToArray();

                    foreach (string key in keys)
                    {
                        string value = run.Invocation.EnvironmentVariables[key];
                        run.Invocation.EnvironmentVariables[key] = Redact(value, invocationTokensToRedact);
                    }
                }
            }
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
            LoggingOptions loggingOptions = LoggingOptions.PrettyPrint,
            Tool tool = null, 
            Run run = null)
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
            Run run = null) : this(textWriter, loggingOptions)
        {
            _run = run ?? CreateRun(null, ComputeFileHashes, false, null, null);

            tool = tool ?? Tool.CreateFromAssemblyData();
            SetSarifLoggerVersion(tool);
            _issueLogJsonWriter.WriteTool(tool);
        }

        public SarifLogger(
            string outputFilePath,
            LoggingOptions loggingOptions,
            IEnumerable<string> analysisTargets,
            string prereleaseInfo,
            IEnumerable<string> invocationTokensToRedact,
            IEnumerable<string> invocationPropertiesToLog = null)
            : this(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                    loggingOptions,
                    analysisTargets,
                    prereleaseInfo,
                    invocationTokensToRedact,
                    invocationPropertiesToLog)
        {
        }

        public SarifLogger(
            TextWriter textWriter,
            LoggingOptions loggingOptions,
            IEnumerable<string> analysisTargets,
            string prereleaseInfo,
            IEnumerable<string> invocationTokensToRedact,
            IEnumerable<string> invocationPropertiesToLog = null) : this(textWriter, loggingOptions)
        {
            Tool tool = Tool.CreateFromAssemblyData(prereleaseInfo);

            SetSarifLoggerVersion(tool);

            _issueLogJsonWriter.WriteTool(tool);

            _run = CreateRun(
                analysisTargets,
                ComputeFileHashes,
                PersistEnvironment,
                invocationTokensToRedact,
                invocationPropertiesToLog);

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
                // for debugging it is nice to have the following line added.
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

        public bool Verbose { get { return (_loggingOptions & LoggingOptions.Verbose) == LoggingOptions.Verbose; } }

        public bool PrettyPrint { get { return (_loggingOptions & LoggingOptions.PrettyPrint) == LoggingOptions.PrettyPrint; } }

        public bool ComputeFileHashes { get { return (_loggingOptions & LoggingOptions.ComputeFileHashes) == LoggingOptions.ComputeFileHashes; } }

        public bool PersistEnvironment { get { return (_loggingOptions & LoggingOptions.PersistEnvironment) == LoggingOptions.PersistEnvironment; } }

        public bool PersistFileContents { get { return (_loggingOptions & LoggingOptions.PersistFileContents) == LoggingOptions.PersistFileContents; } }
        

        public void Dispose()
        {
            // Disposing the json writer closes the stream but the textwriter 
            // still needs to be disposed or closed to write the results
            if (_issueLogJsonWriter != null)
            {
                _issueLogJsonWriter.CloseResults();

                if (_run != null && _run.ConfigurationNotifications != null)
                {
                    _issueLogJsonWriter.WriteConfigurationNotifications(_run.ConfigurationNotifications);
                }

                if (_run != null && _run.ToolNotifications != null)
                {
                    _issueLogJsonWriter.WriteToolNotifications(_run.ToolNotifications);
                }

                if (_run != null &&
                    _run.Invocation != null &&
                    _run.Invocation.StartTime != new DateTime())
                {
                    _run.Invocation.EndTime = DateTime.UtcNow;
                }

                // Note: we write out the backing rules
                // to prevent the property accessor from populating
                // this data with an empty collection.
                if (_rules != null)
                {
                    _issueLogJsonWriter.WriteRules(_rules);
                }

                if (_run != null && _run.Files != null)
                {
                    _issueLogJsonWriter.WriteFiles(_run.Files);
                }

                if (_run != null && _run.Invocation != null)
                {
                    _issueLogJsonWriter.WriteInvocation(invocation: _run.Invocation);
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
            _run.Invocation = Invocation.Create();
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            _run.Invocation.EndTime = DateTime.UtcNow;
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

            if (rule != null)
            {
                Rules[result.RuleKey ?? result.RuleId] = rule;
            }

            CaptureFilesInResult(result);
            _issueLogJsonWriter.WriteResult(result);
        }

        private void CaptureFilesInResult(Result result)
        {
            if (result.Locations != null)
            {
                foreach (Location location in result.Locations)
                {
                    if (location.AnalysisTarget != null)
                    {
                        CaptureFile(location.AnalysisTarget.Uri);
                    }

                    if (location.ResultFile != null)
                    {
                        CaptureFile(location.ResultFile.Uri);
                    }
                }
            }

            CaptureAnnotatedCodeLocations(result.RelatedLocations);

            if (result.Stacks != null)
            {
                foreach (Stack stack in result.Stacks)
                {
                    foreach (StackFrame frame in stack.Frames)
                    {
                        CaptureFile(frame.Uri);
                    }
                }
            }

            if (result.CodeFlows != null)
            {
                foreach (CodeFlow codeFlow in result.CodeFlows)
                {
                    CaptureAnnotatedCodeLocations(codeFlow.Locations);
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
                            CaptureFile(fileChange.Uri);
                        }
                    }
                }
            }
        }

        private void CaptureAnnotatedCodeLocations(IList<AnnotatedCodeLocation> locations)
        {
            if (locations == null) { return; }

            foreach (AnnotatedCodeLocation acl in locations)
            {
                if (acl.PhysicalLocation != null)
                {
                    CaptureFile(acl.PhysicalLocation.Uri);
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

            _run.Files[fileDataKey] = FileData.Create(uri, false);
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
                    PhysicalLocation = new PhysicalLocation { Uri = context.TargetUri },
                    Id = Notes.Msg001AnalyzingTarget,
                    Message = message,
                    Level = NotificationLevel.Note,
                    Time = DateTime.UtcNow,
                });
        }

        public void Log(ResultLevel messageKind, IAnalysisContext context, Region region, string formatId, params string[] arguments)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Rule != null)
            {
                Rules[context.Rule.Id] = context.Rule;
            }

            formatId = RuleUtilities.NormalizeFormatId(context.Rule.Id, formatId);
            LogJsonIssue(messageKind, context.TargetUri.LocalPath, region, context.Rule.Id, formatId, arguments);
        }

        private void LogJsonIssue(ResultLevel level, string targetPath, Region region, string ruleId, string formatId, params string[] arguments)
        {
            if (!ShouldLog(level))
            {
                return;
            }

            Result result = new Result();

            result.RuleId = ruleId;

            result.FormattedRuleMessage = new FormattedRuleMessage()
            {
                FormatId = formatId,
                Arguments = arguments
            };

            result.Level = level;

            if (targetPath != null)
            {
                result.Locations = new List<Location> {
                    new Sarif.Location {
                        AnalysisTarget = new PhysicalLocation
                        {
                            Uri = new Uri(targetPath),
                            Region = region
                        }
               }};
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
            _run.ToolNotifications = _run.ToolNotifications ?? new List<Notification>();
            _run.ToolNotifications.Add(notification);
        }

        public void LogConfigurationNotification(Notification notification)
        {
            _run.ConfigurationNotifications = _run.ConfigurationNotifications ?? new List<Notification>();
            _run.ConfigurationNotifications.Add(notification);
        }
    }
}
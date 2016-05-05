// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Readers;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class SarifLogger : IDisposable, IAnalysisLogger
    {
        private Run _run;
        private TextWriter _textWriter;
        private JsonTextWriter _jsonTextWriter;
        private ResultLogJsonWriter _issueLogJsonWriter;
        private Dictionary<string, IRule> _rules;

        private static Run CreateRun(
            IEnumerable<string> analysisTargets,
            bool computeTargetsHash,
            bool logEnvironment,
            IEnumerable<string> invocationTokensToRedact)
        {
            var run = new Run();

            if (analysisTargets != null)
            {
                run.Files = new Dictionary<string, IList<FileData>>();

                foreach (string target in analysisTargets)
                {
                    string fileDataKey;

                    var fileData = FileData.Create(
                        new Uri[] { new Uri(target, UriKind.RelativeOrAbsolute) }, 
                        computeTargetsHash, 
                        out fileDataKey);

                    run.Files.Add(fileDataKey, fileData);
                }
            }

            run.Invocation = Invocation.Create(logEnvironment);

            // TODO we should actually redact across the complete log file context
            // by a dedicated rewriting visitor or some other approach.
            if (invocationTokensToRedact != null)
            {
                run.Invocation.CommandLine = Redact(run.Invocation.CommandLine, invocationTokensToRedact);
                run.Invocation.Machine = Redact(run.Invocation.Machine, invocationTokensToRedact);
                run.Invocation.Account = Redact(run.Invocation.Account, invocationTokensToRedact);
                run.Invocation.CommandLine = Redact(run.Invocation.CommandLine, invocationTokensToRedact);
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
            bool verbose,
            Tool tool, 
            Run run)
            : this(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                  verbose,
                  tool,
                  run)
        {

        }

        public SarifLogger(
            TextWriter textWriter, 
            bool verbose, 
            Tool tool, 
            Run run) : this(textWriter, verbose)
        {
            _run = run;
            SetSarifLoggerVersion(tool);
            _issueLogJsonWriter.WriteTool(tool);
        }

        public SarifLogger(
            string outputFilePath,
            IEnumerable<string> analysisTargets,
            bool verbose,
            bool logEnvironment,
            bool computeTargetsHash,
            string prereleaseInfo,
            IEnumerable<string> invocationTokensToRedact)
            : this(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                    analysisTargets,
                    verbose,
                    logEnvironment,
                    computeTargetsHash,
                    prereleaseInfo,
                    invocationTokensToRedact)
        {

        }

        public SarifLogger(
            TextWriter textWriter,
            IEnumerable<string> analysisTargets,
            bool verbose,
            bool logEnvironment,
            bool computeTargetsHash,
            string prereleaseInfo,
            IEnumerable<string> invocationTokensToRedact) : this(textWriter, verbose)
        {
            Tool tool = Tool.CreateFromAssemblyData(prereleaseInfo);

            SetSarifLoggerVersion(tool);

            _issueLogJsonWriter.WriteTool(tool);

            _run = CreateRun(
                analysisTargets,             
                computeTargetsHash,
                logEnvironment,
                invocationTokensToRedact);
        }

        private void SetSarifLoggerVersion(Tool tool)
        {
            tool.Properties = tool.Properties ?? new Dictionary<string, string>();

            string sarifLoggerLocation = typeof(SarifLogger).Assembly.Location;

            tool.Properties["SarifLoggerVersion"] = FileVersionInfo.GetVersionInfo(sarifLoggerLocation).FileVersion;
        }

        public SarifLogger(TextWriter textWriter, bool verbose)
        {
            Verbose = verbose;

            _textWriter = textWriter;

            _jsonTextWriter = new JsonTextWriter(_textWriter);

            // for debugging it is nice to have the following line added.
            _jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
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

        public bool Verbose { get; set; }

        public void Dispose()
        {
            // Disposing the json writer closes the stream but the textwriter 
            // still needs to be disposed or closed to write the results
            if (_issueLogJsonWriter != null)
            {
                _issueLogJsonWriter.CloseResults();

                if (_run.ConfigurationNotifications != null)
                {
                    _issueLogJsonWriter.WriteConfigurationNotifications(_run.ConfigurationNotifications);
                }

                if (_run.ToolNotifications != null)
                {
                    _issueLogJsonWriter.WriteToolNotifications(_run.ToolNotifications);
                }

                if (_run?.Invocation?.StartTime != new DateTime())
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

                if (_run.Files != null) { _issueLogJsonWriter.WriteFiles(_run.Files); }

                if (_run.Invocation != null)
                {
                    _issueLogJsonWriter.WriteInvocation(invocation: _run.Invocation);
                }

                _issueLogJsonWriter.Dispose();
            }
            if (_textWriter != null) { _textWriter.Dispose(); }
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
            if (!ShouldLog(result.Level))
            {
                return;
            }

            if (rule != null)
            {
                Rules[rule.Id] = rule;
            }

            _issueLogJsonWriter.WriteResult(result);
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            // This code doesn't call through a common helper, such as
            // provided by the SDK Notes class, because we are in a specific
            // logger. If we called through a helper, we'd re-enter
            // through all aggregated loggers.
            context.Rule = Notes.AnalyzingTarget;
            Log(context.Rule,
                RuleUtilities.BuildResult(ResultLevel.Note, context, null,
                    nameof(SdkResources.MSG1001_AnalyzingTarget)));
        }

        public void Log(ResultLevel messageKind, IAnalysisContext context, Region region, string formatId, params string[] arguments)
        {
            if (context.Rule != null)
            {
                Rules[context.Rule.Id] = context.Rule;
            }

            formatId = RuleUtilities.NormalizeFormatId(context.Rule.Id, formatId);
            LogJsonIssue(messageKind, context.TargetUri?.LocalPath, region, context.Rule.Id, formatId, arguments);
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
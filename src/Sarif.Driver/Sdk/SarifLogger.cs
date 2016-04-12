// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class SarifLogger : IDisposable, IAnalysisLogger
    {
        private Run _run;
        private TextWriter _textWriter;
        private JsonTextWriter _jsonTextWriter;
        private ResultLogJsonWriter _issueLogJsonWriter;
        private HashSet<IRule> _rules;

        public static Tool CreateDefaultTool(string prereleaseInfo = null)
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string name = Path.GetFileNameWithoutExtension(assembly.Location);
            Version version = assembly.GetName().Version;

            Tool tool = new Tool();
            tool.Name = name;
            tool.Version = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString();
            tool.FullName = name + " " + tool.Version + (prereleaseInfo ?? "");

            return tool;
        }

        private static Run CreateRun(
            IEnumerable<string> analysisTargets,
            bool computeTargetsHash,
            IEnumerable<string> invocationTokensToRedact)
        {
            var run = new Run();

            if (analysisTargets != null)
            {
                run.Files = new Dictionary<Uri, IList<FileData>>();

                foreach (string target in analysisTargets)
                {
                    var fileReference = new FileData();

                    if (computeTargetsHash)
                    {
                        string sha256Hash = HashUtilities.ComputeSha256Hash(target) ?? "[could not compute file hash]";
                        fileReference.Hashes = new List<Hash>(new Hash[]
                        {
                            new Hash()
                            {
                                Value = sha256Hash,
                                Algorithm = AlgorithmKind.Sha256,
                            }
                        });
                    }
                        run.Files.Add(new Uri(target), new List<FileData> { fileReference });
                }
            }

            string invocation = Environment.CommandLine;

            if (invocationTokensToRedact != null)
            {
                foreach (string tokenToRedact in invocationTokensToRedact)
                {
                    invocation = invocation.Replace(tokenToRedact, SarifConstants.RemovedMarker);
                }
            }
            run.Invocation = invocation;
            run.StartTime = DateTime.UtcNow;
            return run;
        }

        public SarifLogger(string outputFilePath, bool verbose, Tool tool, Run run)
            : this (new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                  verbose,
                  tool, 
                  run)
        {

        }

        public SarifLogger(TextWriter textWriter, bool verbose, Tool tool, Run run) : this(textWriter, verbose)
        {
            _run = run;
            _issueLogJsonWriter.WriteTool(tool);
        }

        public SarifLogger(
            string outputFilePath,
            bool verbose,
            IEnumerable<string> analysisTargets,
            bool computeTargetsHash,
            string prereleaseInfo,
            IEnumerable<string> invocationTokensToRedact)
            : this(new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None)),
                    verbose,
                    analysisTargets,
                    computeTargetsHash, 
                    prereleaseInfo,
                    invocationTokensToRedact)
        {

        }

        public SarifLogger(
            TextWriter textWriter,
            bool verbose,
            IEnumerable<string> analysisTargets,
            bool computeTargetsHash,
            string prereleaseInfo,
            IEnumerable<string> invocationTokensToRedact) : this(textWriter, verbose)
        {
            Tool tool = CreateDefaultTool(prereleaseInfo);
            _issueLogJsonWriter.WriteTool(tool);

            _run = CreateRun(analysisTargets, computeTargetsHash, invocationTokensToRedact);
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

        public HashSet<IRule> Rules
        {
            get
            {
                _rules = _rules ?? new HashSet<IRule>();
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

                if (_run != null && _run.StartTime != new DateTime())
                {
                    _run.EndTime = DateTime.UtcNow;
                }

                _issueLogJsonWriter.WriteRun(_run);

                // Note: we write out the backing rules
                // to prevent the property accessor from populating
                // this data with an empty collection.
                if (_rules != null)
                {
                    _issueLogJsonWriter.WriteRules(_rules);
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
            _run.StartTime = DateTime.UtcNow;
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            _run.EndTime = DateTime.UtcNow;
        }

        public void Log(IRule rule, Result result)
        {
            if (!ShouldLog(result.Kind))
            {
                return;
            }

            if (rule != null)
            {
                Rules.Add(rule);
            }

            _issueLogJsonWriter.WriteResult(result);
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            // This code doesn't call through a common helper, such as
            // provided by the SDK Notes class, becuase we are in a specifier
            // logger. If we called through a helper, we'd re-enter
            // through all aggregated loggers.
            context.Rule = Notes.AnalyzingTarget;
            Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.Note, context, null,
                    nameof(SdkResources.MSG1001_AnalyzingTarget)));
        }

        public void Log(ResultKind messageKind, IAnalysisContext context, Region region, string formatId, params string[] arguments)
        {
            if (context.Rule != null)
            {
                Rules.Add(context.Rule);
            }

            formatId = RuleUtilities.NormalizeFormatId(context.Rule.Id, formatId);
            LogJsonIssue(messageKind, context.TargetUri?.LocalPath, region, context.Rule.Id, formatId, arguments);
        }

        private void LogJsonIssue(ResultKind messageKind, string targetPath, Region region, string ruleId, string formatId, params string[] arguments)
        {
            if (!ShouldLog(messageKind))
            {
                return;
            }

            Result result = new Result();

            result.RuleId = ruleId;

            result.FormattedMessage = new FormattedMessage()
            {
                FormatId = formatId,
                Arguments = arguments
            };
             
            result.Kind = messageKind;

            if (targetPath != null)
            {
                result.Locations = new[] {
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

        public bool ShouldLog(ResultKind messageKind)
        {
            switch (messageKind)
            {
                case ResultKind.Note:
                case ResultKind.Pass:
                case ResultKind.NotApplicable:
                {
                    if (!Verbose)
                    {
                        return false;
                    }
                    break;
                }

                case ResultKind.Error:
                case ResultKind.Warning:
                case ResultKind.InternalError:
                case ResultKind.ConfigurationError:
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
    }
}




// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private Dictionary<string, IRule> _rules;

        private static Run CreateRun(
            IEnumerable<string> analysisTargets,
            bool computeTargetsHash,
            IEnumerable<string> invocationTokensToRedact)
        {
            var run = new Run();

            if (analysisTargets != null)
            {
                run.Files = new Dictionary<string, IList<FileData>>();

                foreach (string target in analysisTargets)
                {
                    var fileReference = new FileData();

                    if (computeTargetsHash)
                    {
                        string md5, sha1, sha256;

                        HashUtilities.ComputeHashes(target, out md5, out sha1, out sha256);
                        fileReference.Hashes = new HashSet<Hash>
                        {
                            new Hash()
                            {
                                Value = md5,
                                Algorithm = AlgorithmKind.MD5,
                            },
                            new Hash()
                            {
                                Value = sha1,
                                Algorithm = AlgorithmKind.Sha1,
                            },
                            new Hash()
                            {
                                Value = sha256,
                                Algorithm = AlgorithmKind.Sha256,
                            },
                        };
                    }
                        run.Files.Add(new Uri(target).ToString(), new List<FileData> { fileReference });
                }
            }


            run.Invocation = Invocation.Create();

            if (invocationTokensToRedact != null)
            {
                run.Invocation.Machine = Redact(run.Invocation.Machine, invocationTokensToRedact);
                run.Invocation.Parameters = Redact(run.Invocation.Parameters, invocationTokensToRedact);
                run.Invocation.WorkingDirectory = Redact(run.Invocation.WorkingDirectory, invocationTokensToRedact);

                string[] keys = run.Invocation.EnvironmentVariables.Keys.ToArray();

                foreach (string key in keys)
                {
                    string value = run.Invocation.EnvironmentVariables[key];
                    run.Invocation.EnvironmentVariables[key] = Redact(value, invocationTokensToRedact);
                }
            }
            return run;
        }

        private static string Redact(string text, IEnumerable<string> tokensToRedact)
        {
            foreach (string tokenToRedact in tokensToRedact)
            {
                text = text.Replace(tokenToRedact, SarifConstants.RemovedMarker);
            }
            return text;
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
            Tool tool = Tool.CreateFromAssemblyData(prereleaseInfo);
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
            if (!ShouldLog(result.Kind))
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
                Rules[context.Rule.Id] = context.Rule;
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
                result.Locations = new HashSet<Location> {
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




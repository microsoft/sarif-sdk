// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class SarifLogger : IDisposable, IAnalysisLogger
    {
        private FileStream _fileStream;
        private TextWriter _textWriter;
        private JsonTextWriter _jsonTextWriter;
        private ResultLogJsonWriter _issueLogJsonWriter;
        private HashSet<IRuleDescriptor> ruleDescriptors;

        public static ToolInfo CreateDefaultToolInfo(string prereleaseInfo = null)
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string name = Path.GetFileNameWithoutExtension(assembly.Location);
            Version version = assembly.GetName().Version;

            ToolInfo toolInfo = new ToolInfo();
            toolInfo.Name = name;
            toolInfo.Version = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString();
            toolInfo.FullName = name + " " + toolInfo.Version + (prereleaseInfo ?? "");

            return toolInfo;
        }

        public SarifLogger(
            string outputFilePath,
            bool verbose,
            IEnumerable<string> analysisTargets,
            bool computeTargetsHash,
            string prereleaseInfo)
        {
            Verbose = verbose;

            _fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            _textWriter = new StreamWriter(_fileStream);
            _jsonTextWriter = new JsonTextWriter(_textWriter);

            // for debugging it is nice to have the following line added.
            _jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;

            _issueLogJsonWriter = new ResultLogJsonWriter(_jsonTextWriter);

            var toolInfo = CreateDefaultToolInfo(prereleaseInfo);

            RunInfo runInfo = new RunInfo();
            runInfo.AnalysisTargets = new List<FileReference>();

            foreach (string target in analysisTargets)
            {
                var fileReference = new FileReference()
                {
                    Uri = target.CreateUriForJsonSerialization(),
                };

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
                runInfo.AnalysisTargets.Add(fileReference);
            }
            runInfo.InvocationInfo = Environment.CommandLine;

            _issueLogJsonWriter.WriteToolAndRunInfo(toolInfo, runInfo);

            this.ruleDescriptors = new HashSet<IRuleDescriptor>();
        }

        public bool Verbose { get; set; }

        public void Dispose()
        {    
            // Disposing the json writer closes the stream but the textwriter 
            // still needs to be disposed or closed to write the results
            if (_issueLogJsonWriter != null)
            {
                _issueLogJsonWriter.CloseResults();
                _issueLogJsonWriter.WriteRuleInfo(this.ruleDescriptors);
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
            // TODO emit a message with timestamp
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            // TODO emit a message with timestamp and success/failure results
        }

        public void Log(IRuleDescriptor rule, Result result)
        {
            switch (result.Kind)
            {
                case ResultKind.Note:
                case ResultKind.Pass:
                case ResultKind.NotApplicable:
                {
                    if (!Verbose)
                    {
                        return;
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
            this.ruleDescriptors.Add(rule);
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

        public void Log(ResultKind messageKind, IAnalysisContext context, Region region, string formatSpecifierId, params string[] arguments)
        {
            this.ruleDescriptors.Add(context.Rule);

            formatSpecifierId = RuleUtilities.NormalizeFormatSpecifierId(context.Rule.Id, formatSpecifierId);
            LogJsonIssue(messageKind, context.TargetUri?.LocalPath, region, context.Rule.Id, formatSpecifierId, arguments);
        }

        private void LogJsonIssue(ResultKind messageKind, string targetPath, Region region, string ruleId, string formatSpecifierId, params string[] arguments)
        {
            switch (messageKind)
            {
                case ResultKind.Note:
                case ResultKind.Pass:
                case ResultKind.NotApplicable:
                    {
                        if (!Verbose)
                        {
                            return;
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

            Result result = new Result();

            result.RuleId = ruleId;

            result.FormattedMessage = new FormattedMessage()
            {
                SpecifierId = formatSpecifierId,
                Arguments = arguments
            };
             
            result.Kind = messageKind;

            if (targetPath != null)
            {
                result.Locations = new[] {
                new Sarif.Sdk.Location {
                    AnalysisTarget = new[]
                    {
                        new PhysicalLocationComponent
                        {
                            // Why? When NewtonSoft serializes this Uri, it will use the
                            // original string used to construct the Uri. For a file path, 
                            // this will be the local file path. We want to persist this 
                            // information using the file:// protocol rendering, however.
                            Uri = targetPath.CreateUriForJsonSerialization(),
                            MimeType = MimeType.Binary,
                            Region = region
                        }, 
                    }
               }};
            }

            _issueLogJsonWriter.WriteResult(result);
        }
    }
}




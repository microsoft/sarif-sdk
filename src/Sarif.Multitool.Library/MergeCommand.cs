// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Processors;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class MergeCommand : CommandBase
    {
        private MergeOptions _options;
        private long _filesToProcessCount;
        private Channel<string> _logLoadChannel;
        private Channel<SarifLog> _mergeLogsChannel;
        private readonly Dictionary<string, Run> _ruleIdToRunsMap;
        private readonly Dictionary<string, SarifLog> _idToSarifLogMap;
        private readonly Dictionary<string, HashSet<Result>> _ruleIdToResultsMap;
        private readonly Dictionary<string, RunMergingVisitor> _ruleIdToMergeVisitorsMap;

        public MergeCommand(IFileSystem fileSystem = null) : base(fileSystem)
        {
            _ruleIdToRunsMap = new Dictionary<string, Run>();
            _idToSarifLogMap = new Dictionary<string, SarifLog>();
            _ruleIdToResultsMap = new Dictionary<string, HashSet<Result>>();
            _ruleIdToMergeVisitorsMap = new Dictionary<string, RunMergingVisitor>();
        }

        public int Run(MergeOptions mergeOptions)
        {
            Stopwatch w = Stopwatch.StartNew();
            try
            {
                _options = mergeOptions;
                string outputDirectory = mergeOptions.OutputDirectoryPath ?? Environment.CurrentDirectory;
                string outputFilePath = Path.Combine(outputDirectory, GetOutputFileName(_options));

                if (mergeOptions.Inline)
                {
                    Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.WRN997_InvalidOption,
                        nameof(mergeOptions.Inline)));
                    return FAILURE;
                }

                if (_options.SplittingStrategy == 0)
                {
                    if (!DriverUtilities.ReportWhetherOutputFileCanBeCreated(outputFilePath, _options.Force, FileSystem))
                    {
                        return FAILURE;
                    }
                }

                var logLoadOptions = new BoundedChannelOptions(1000)
                {
                    SingleWriter = true,
                    SingleReader = false,
                };
                _logLoadChannel = Channel.CreateBounded<string>(logLoadOptions);

                var mergeLogsOptions = new UnboundedChannelOptions()
                {
                    SingleWriter = false,
                    SingleReader = true
                };
                _mergeLogsChannel = Channel.CreateUnbounded<SarifLog>(mergeLogsOptions);

                _options.Threads = _options.Threads > 0 ? _options.Threads : Environment.ProcessorCount;

                // creating readers
                var readers = new Task<bool>[_options.Threads];
                for (int i = 0; i < _options.Threads; i++)
                {
                    readers[i] = Task.Run(LoadSarifLogs);
                }

                // reading and dispatching
                FindFilesAsync().Wait();

                // creating writer
                var writer = Task.Run(MergeSarifLogsAsync);

                // waiting all readers and closing merge channel
                Task.WhenAll(readers)
                    .ContinueWith(_ => _mergeLogsChannel.Writer.Complete())
                    .Wait();

                // waiting writer
                writer.Wait();

                foreach (string key in _idToSarifLogMap.Keys)
                {
                    SarifLog mergedLog = _idToSarifLogMap[key]
                                            .InsertOptionalData(this._options.DataToInsert.ToFlags())
                                            .RemoveOptionalData(this._options.DataToInsert.ToFlags());

                    // If there were no input files, the Merge operation set combinedLog.Runs to null. Although
                    // null is valid in certain error cases, it is not valid here. Here, the correct value is
                    // an empty list. See the SARIF spec, §3.13.4, "runs property".
                    mergedLog.Runs ??= new List<Run>();
                    mergedLog.Version = SarifVersion.Current;
                    mergedLog.SchemaUri = mergedLog.Version.ConvertToSchemaUri();

                    FileSystem.DirectoryCreateDirectory(outputDirectory);
                    outputFilePath = Path.Combine(outputDirectory, GetOutputFileName(_options, key));
                    WriteSarifFile(FileSystem, mergedLog, outputFilePath, _options.Minify);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }
            finally
            {
                Console.WriteLine($"Merge completed in {w.Elapsed}.");
            }
            return SUCCESS;
        }

        private async Task<bool> MergeSarifLogsAsync()
        {
            if (_options.SplittingStrategy != SplittingStrategy.PerRule)
            {
                this._idToSarifLogMap[""] = new SarifLog()
                {
                    Runs = new List<Run>()
                };
            }

            ChannelReader<SarifLog> reader = _mergeLogsChannel.Reader;

            // Wait until there is work or the channel is closed.
            while (await reader.WaitToReadAsync())
            {
                // Loop while there is work to do.
                while (reader.TryRead(out SarifLog sarifLog))
                {
                    foreach (Run run in sarifLog.Runs)
                    {
                        if (_options.SplittingStrategy == SplittingStrategy.PerRule || !_options.MergeEmptyLogs)
                        {
                            if (run.Results == null || run.Results.Count == 0)
                            {
                                continue;
                            }
                        }

                        run.SetRunOnResults();

                        if (run.Results != null)
                        {
                            foreach (Result result in run.Results)
                            {
                                string key = _options.SplittingStrategy == SplittingStrategy.PerRule
                                    ? result.RuleId
                                    : string.Empty;

                                if (!_idToSarifLogMap.TryGetValue(key, out SarifLog splitLog))
                                {
                                    splitLog = _idToSarifLogMap[key] = new SarifLog()
                                    {
                                        Runs = new List<Run>()
                                    };
                                }

                                key = CreateRuleKey(result.RuleId, run);

                                if (!_ruleIdToRunsMap.TryGetValue(key, out Run splitRun))
                                {
                                    Run emptyRun = run.DeepClone();
                                    emptyRun.Results.Clear();
                                    splitRun = _ruleIdToRunsMap[key] = emptyRun;
                                    splitLog.Runs.Add(splitRun);
                                    _ruleIdToResultsMap[key] = new HashSet<Result>(Result.ValueComparer);
                                    _ruleIdToMergeVisitorsMap[key] = new RunMergingVisitor();
                                }

                                if (!_ruleIdToResultsMap[key].Contains(result))
                                {
                                    _ruleIdToResultsMap[key].Add(result);

                                    RunMergingVisitor currentVisitor = _ruleIdToMergeVisitorsMap[key];
                                    currentVisitor.CurrentRun = result.Run;
                                    currentVisitor.VisitResult(result.DeepClone());
                                    currentVisitor.PopulateWithMerged(splitRun);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private string CreateRuleKey(string ruleId, Run run)
        {
            return
                (ruleId ?? "") +
                (run.Tool.Driver.Name ?? "") +
                (run.Tool.Driver.Version ?? "") +
                (run.Tool.Driver.SemanticVersion ?? "") +
                (run.Tool.Driver.DottedQuadFileVersion ?? "");
        }

        private async Task<bool> LoadSarifLogs()
        {
            ChannelReader<string> reader = _logLoadChannel.Reader;

            // Wait until there is work or the channel is closed.
            while (await reader.WaitToReadAsync())
            {
                if (!reader.TryRead(out string filePath))
                {
                    continue;
                }
                try
                {
                    ProcessInputSarifLog(filePath);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }

            return true;
        }

        private void ProcessInputSarifLog(string filePath)
        {
            SarifLog sarifLog = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(
                FileSystem.FileReadAllText(filePath),
                formatting: Formatting.None,
                out string sarifText);

            if (_options.MergeRuns)
            {
                new FixupVisitor().VisitSarifLog(sarifLog);
            }

            _mergeLogsChannel.Writer.TryWrite(sarifLog);
            Interlocked.Decrement(ref _filesToProcessCount);
        }

        private async Task<bool> FindFilesAsync()
        {
            SearchOption searchOption = _options.Recurse
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            foreach (string specifier in _options.TargetFileSpecifiers)
            {
                string expandedSpecifier = Environment.ExpandEnvironmentVariables(specifier);

                string filter = Path.GetFileName(expandedSpecifier);
                string directory = Path.GetDirectoryName(expandedSpecifier);

                if (directory.Length == 0)
                {
                    directory = @".\";
                }

                if (!FileSystem.DirectoryExists(directory))
                {
                    continue;
                }

                foreach (string file in FileSystem.DirectoryEnumerateFiles(directory, filter, searchOption))
                {
                    Interlocked.Increment(ref _filesToProcessCount);
                    await _logLoadChannel.Writer.WriteAsync(file);
                }
            }
            _logLoadChannel.Writer.Complete();
            return true;
        }

        internal static string GetOutputFileName(MergeOptions mergeOptions, string prefix = "")
        {
            return string.IsNullOrEmpty(mergeOptions.OutputFileName) == false
                ? GetPrefix(prefix) + mergeOptions.OutputFileName
                : GetPrefix("merged.sarif");
        }

        private static string GetPrefix(string prefix)
        {
            if (!string.IsNullOrWhiteSpace(prefix) && !prefix.EndsWith("_"))
            {
                prefix += "_";
            }

            return prefix ?? string.Empty;
        }
    }

    internal class FixupVisitor : SarifRewritingVisitor
    {
        private Run _run;

        public override Run VisitRun(Run node)
        {
            _run = node;
            _run = base.VisitRun(node);

            _run.Invocations = null;
            _run.Artifacts = null;
            _run.Tool.Driver = new ToolComponent
            {
                Name = _run.Tool.Driver.Name,
                Version = _run.Tool.Driver.Version,
                SemanticVersion = _run.Tool.Driver.SemanticVersion,
                DottedQuadFileVersion = _run.Tool.Driver.DottedQuadFileVersion
            };

            return _run;
        }

        public override Result VisitResult(Result node)
        {
            node.RuleIndex = -1;
            return base.VisitResult(node);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (_run.Artifacts != null && node.Index > -1)
            {
                node = _run.Artifacts[node.Index].Location;
            }
            node.Index = -1;
            return base.VisitArtifactLocation(node);
        }
    }
}

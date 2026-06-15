// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        // Coalescing is keyed by tool name + version. Each distinct tool maps to a single
        // RunMergingVisitor that absorbs every contributing run, deduping results and
        // remapping descriptor, artifact, logical-location, and invocation indices into one
        // merged run. _toolKeyOrder preserves first-seen order so the output is deterministic.
        private readonly List<string> _toolKeyOrder;
        private readonly Dictionary<string, Run> _toolKeyToMergedRun;
        private readonly Dictionary<string, RunMergingVisitor> _toolKeyToVisitor;
        private readonly Dictionary<string, HashSet<Result>> _toolKeyToResults;

        public MergeCommand(IFileSystem fileSystem = null) : base(fileSystem)
        {
            _toolKeyOrder = new List<string>();
            _toolKeyToMergedRun = new Dictionary<string, Run>();
            _toolKeyToVisitor = new Dictionary<string, RunMergingVisitor>();
            _toolKeyToResults = new Dictionary<string, HashSet<Result>>();
        }

        public int Run(MergeOptions mergeOptions)
        {
            var w = Stopwatch.StartNew();
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

                if (!DriverUtilities.ReportWhetherOutputFileCanBeCreated(outputFilePath, _options.ForceOverwrite, FileSystem))
                {
                    return FAILURE;
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

                var mergedLog = new SarifLog { Runs = new List<Run>() };
                foreach (string toolKey in _toolKeyOrder)
                {
                    Run mergedRun = _toolKeyToMergedRun[toolKey];
                    _toolKeyToVisitor[toolKey].PopulateWithMerged(mergedRun);
                    mergedLog.Runs.Add(mergedRun);
                }

                mergedLog = mergedLog
                                .InsertOptionalData(this._options.DataToInsert.ToFlags())
                                .RemoveOptionalData(this._options.DataToInsert.ToFlags());

                mergedLog.Version = SarifVersion.Current;
                mergedLog.SchemaUri = mergedLog.Version.ConvertToSchemaUri();

                FileSystem.DirectoryCreateDirectory(outputDirectory);
                WriteSarifFile(FileSystem, mergedLog, outputFilePath, _options.Minify);
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
            ChannelReader<SarifLog> reader = _mergeLogsChannel.Reader;

            // Wait until there is work or the channel is closed.
            while (await reader.WaitToReadAsync())
            {
                // Loop while there is work to do.
                while (reader.TryRead(out SarifLog sarifLog))
                {
                    if (sarifLog.Runs == null)
                    {
                        continue;
                    }

                    foreach (Run run in sarifLog.Runs)
                    {
                        bool hasResults = run.Results != null && run.Results.Count > 0;
                        if (!hasResults && !_options.MergeEmptyLogs)
                        {
                            continue;
                        }

                        run.SetRunOnResults();

                        string toolKey = CreateToolKey(run);
                        if (!_toolKeyToVisitor.TryGetValue(toolKey, out RunMergingVisitor visitor))
                        {
                            visitor = _toolKeyToVisitor[toolKey] = new RunMergingVisitor();
                            _toolKeyToResults[toolKey] = new HashSet<Result>(Result.ValueComparer);
                            _toolKeyOrder.Add(toolKey);

                            // The first run of a given tool + version supplies the merged run's
                            // header (tool metadata, automationDetails, columnKind, etc.). Its
                            // result, artifact, and rule collections are replaced by the merged
                            // sets in PopulateWithMerged once every contributing run is absorbed.
                            _toolKeyToMergedRun[toolKey] = run.DeepClone();
                        }

                        if (run.Results == null)
                        {
                            continue;
                        }

                        HashSet<Result> seenResults = _toolKeyToResults[toolKey];
                        foreach (Result result in run.Results)
                        {
                            // Drop results that are value-identical to one already merged for this
                            // tool. A sharded scan can re-report the same finding in more than one
                            // input log; the merged run should carry each finding exactly once.
                            if (!seenResults.Add(result))
                            {
                                continue;
                            }

                            visitor.CurrentRun = run;
                            visitor.VisitResult(result.DeepClone());
                        }
                    }
                }
            }
            return true;
        }

        private static string CreateToolKey(Run run)
        {
            return
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
                    directory = Directory.GetCurrentDirectory();
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

        internal static string GetOutputFileName(MergeOptions mergeOptions)
        {
            return string.IsNullOrEmpty(mergeOptions.OutputFileName)
                ? "merged.sarif"
                : mergeOptions.OutputFileName;
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Map;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class PageCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public PageCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(PageOptions options)
        {
            int returnCode;

            try
            {
                returnCode = RunWithoutCatch(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                returnCode = FAILURE;
            }

            return returnCode;
        }

        public int RunWithoutCatch(PageOptions options)
        {
            if (!ValidateOptions(options, _fileSystem)) { return 1; }

            // Load the JsonMap, if previously built and up-to-date, or rebuild it
            JsonMapNode root = LoadOrRebuildMap(options);

            // Write the desired page from the Sarif file
            ExtractPage(options, root);

            return SUCCESS;
        }

        internal bool ValidateOptions(PageOptions options, IFileSystem fileSystem)
        {
            bool valid = true;

            valid &= ValidateNonNegativeCommandLineOption<PageOptions>(options.RunIndex, nameof(options.RunIndex));
            valid &= ValidateNonNegativeCommandLineOption<PageOptions>(options.Index, nameof(options.Index));
            valid &= ValidateNonNegativeCommandLineOption<PageOptions>(options.Count, nameof(options.Count));


            if (!fileSystem.FileExists(options.InputFilePath))
            {
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.InputFileNotFound,
                        options.InputFilePath));
                valid = false;
            }

            valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(options.OutputFilePath, options.Force, fileSystem);

            return valid;
        }

        internal SarifLog PageViaOm(PageOptions options)
        {
            SarifLog actualLog = ReadSarifFile<SarifLog>(_fileSystem, options.InputFilePath);

            // Validate Run in range
            if (options.RunIndex >= actualLog?.Runs.Count)
            {
                throw new ArgumentOutOfRangeException($"Page requested for RunIndex {options.RunIndex}, but Log had only {actualLog.Runs?.Count} runs.");
            }

            // Filter to desired run only
            Run run = actualLog.Runs[options.RunIndex];
            actualLog.Runs = new List<Run>() { run };

            // Validate results in range
            if (options.Index + options.Count > run?.Results.Count)
            {
                throw new ArgumentOutOfRangeException($"Page requested from Result {options.Index} to {options.Index + options.Count}, but Run has only {run.Results?.Count} results.");
            }

            // Filter to desired results only
            run.Results = run.Results.Skip(options.Index).Take(options.Count).ToList();

            WriteSarifFile(_fileSystem, actualLog, options.OutputFilePath, Formatting.None);
            return actualLog;
        }

        private JsonMapNode LoadOrRebuildMap(PageOptions options)
        {
            JsonMapNode root;
            Stopwatch w = Stopwatch.StartNew();

            string mapPath = Path.ChangeExtension(options.InputFilePath, ".map.json");

            if (_fileSystem.FileExists(mapPath) && _fileSystem.GetLastWriteTime(mapPath) > _fileSystem.GetLastWriteTime(options.InputFilePath))
            {
                // If map exists and is up-to-date, just reload it
                Console.WriteLine($"Loading Json Map \"{mapPath}\"...");
                root = JsonConvert.DeserializeObject<JsonMapNode>(_fileSystem.ReadAllText(mapPath));
            }
            else
            {
                // Otherwise, build the map and save it (1% -> 10MB limit)
                double mapSizeLimit = 10 * JsonMapSettings.Megabyte * (options.TargetMapSizeRatio / 0.01);

                Console.WriteLine($"Building {options.TargetMapSizeRatio:p0} Json Map of \"{options.InputFilePath}\" into \"{mapPath}\"...");
                root = JsonMapBuilder.Build(() => _fileSystem.OpenRead(options.InputFilePath), new JsonMapSettings(options.TargetMapSizeRatio, mapSizeLimit));

                if (root != null)
                {
                    _fileSystem.WriteAllText(mapPath, JsonConvert.SerializeObject(root, Formatting.None));
                }
            }

            w.Stop();
            Console.WriteLine($"Done in {w.Elapsed.TotalSeconds:n1}s.");

            return root;
        }

        private void ExtractPage(PageOptions options, JsonMapNode root)
        {
            Stopwatch w = Stopwatch.StartNew();
            Console.WriteLine($"Extracting {options.Count:n0} results from index {options.Index:n0}\r\n  from \"{options.InputFilePath}\"\r\n  into \"{options.OutputFilePath}\"...");

            JsonMapNode runs, run, results;

            // Get 'runs' node from map. If log was too small, page using the object model
            if (root == null
                || root.Nodes == null
                || root.Nodes.TryGetValue("runs", out runs) == false)
            {
                PageViaOm(options);
                return;
            }

            // Verify RunIndex in range
            if (options.RunIndex >= runs.Count)
            {
                throw new ArgumentOutOfRangeException($"Page requested for RunIndex {options.RunIndex}, but Log had only {runs.Count} runs.");
            }

            // Get 'results' from map. If log was too small, page using the object model
            if (!runs.Nodes.TryGetValue(options.RunIndex.ToString(), out run)
                || run.Nodes == null
                || run.Nodes.TryGetValue("results", out results) == false
                || results.ArrayStarts == null)
            {
                // Log too small; convert via OM
                PageViaOm(options);
                return;
            }

            if (options.Index >= results.Count)
            {
                throw new ArgumentOutOfRangeException($"Index requested was {options.Index} but Run has only {results.Count} results.");
            }

            if (options.Index + options.Count > results.Count)
            {
                Console.WriteLine($"Page requested from Result {options.Index} to {options.Index + options.Count} but Run has only {results.Count} results.");
                options.Count = results.Count - options.Index;
            }


            Console.WriteLine($"Run {options.RunIndex} in \"{options.InputFilePath}\" has {results.Count:n0} results.");

            Func<Stream> inputStreamProvider = () => _fileSystem.OpenRead(options.InputFilePath);
            long firstResultStart = results.FindArrayStart(options.Index, inputStreamProvider);
            long lastResultEnd = results.FindArrayStart(options.Index + options.Count, inputStreamProvider) - 1;

            // Ensure output directory exists
            string outputFolder = Path.GetDirectoryName(Path.GetFullPath(options.OutputFilePath));
            Directory.CreateDirectory(outputFolder);

            // Build the Sarif Log subset
            long lengthWritten = 0;
            byte[] buffer = new byte[64 * 1024];

            using (Stream output = _fileSystem.Create(options.OutputFilePath))
            using (Stream source = _fileSystem.OpenRead(options.InputFilePath))
            {
                // Copy everything up to 'runs' (includes the '[')
                JsonMapNode.CopyStreamBytes(source, output, 0, runs.Start, buffer);

                // In the run, copy everything to 'results' (includes the '[')
                JsonMapNode.CopyStreamBytes(source, output, run.Start, results.Start, buffer);

                // Find and copy the desired range of results, excluding the last ','
                JsonMapNode.CopyStreamBytes(source, output, firstResultStart, lastResultEnd, buffer, omitFromLast: (byte)',');

                // Copy everything after the results array to the end of the run (includes the '}')
                JsonMapNode.CopyStreamBytes(source, output, results.End, run.End, buffer);

                // Omit all subsequent runs

                // Copy everything after all runs (includes runs ']' and log '}')
                JsonMapNode.CopyStreamBytes(source, output, runs.End, root.End, buffer);

                lengthWritten = output.Length;
            }

            w.Stop();
            Console.WriteLine($"Done; wrote {(lengthWritten / (double)(1024 * 1024)):n2} MB in {w.Elapsed.TotalSeconds:n1}s.");
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Map;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class PageCommand : CommandBase
    {
        private const double SarifMapTargetSizeRatio = 0.01;
        private readonly IFileSystem _fileSystem;

        public PageCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(PageOptions options)
        {
            try
            {
                string mapPath = Path.Combine(Path.GetDirectoryName(options.InputFilePath), Path.GetFileNameWithoutExtension(options.InputFilePath) + ".map.json");

                // Load the JsonMap, if previously built and up-to-date, or rebuild it
                JsonMapNode root = LoadOrRebuildMap(options.InputFilePath, mapPath);

                // Write the desired page from the Sarif file
                ExtractPage(options, root);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }

        private void PageViaOm(PageOptions options)
        {
            SarifLog actualLog = ReadSarifFile<SarifLog>(_fileSystem, options.InputFilePath);

            // Filter to desired run only
            Run run = actualLog.Runs[options.RunIndex];
            actualLog.Runs = new List<Run>() { run };

            // Filter to desired results only
            run.Results = run.Results.Skip(options.Index).Take(options.Count).ToList();

            WriteSarifFile(_fileSystem, actualLog, options.OutputFilePath, Formatting.None);
        }

        private JsonMapNode LoadOrRebuildMap(string inputFilePath, string mapPath)
        {
            // If map exists and is up-to-date, just reload it
            if (_fileSystem.FileExists(mapPath) && _fileSystem.GetLastWriteTime(mapPath) > _fileSystem.GetLastWriteTime(inputFilePath))
            {
                return JsonConvert.DeserializeObject<JsonMapNode>(_fileSystem.ReadAllText(mapPath));
            }

            // Otherwise, build the map and save it
            Stopwatch w = Stopwatch.StartNew();
            Console.WriteLine($"Building Json Map of \"{inputFilePath}\" into \"{mapPath}\"...");

            JsonMapBuilder builder = new JsonMapBuilder(SarifMapTargetSizeRatio);
            JsonMapNode root = builder.Build(() => _fileSystem.OpenRead(inputFilePath));
            _fileSystem.WriteAllText(mapPath, JsonConvert.SerializeObject(root, Formatting.None));

            w.Stop();
            Console.WriteLine($"Done in {w.Elapsed.TotalSeconds:n1}s.");

            return root;
        }

        private void ExtractPage(PageOptions options, JsonMapNode root)
        {
            if (options.RunIndex < 0) { throw new ArgumentOutOfRangeException("runIndex"); }
            if (options.Index < 0) { throw new ArgumentOutOfRangeException("index"); }
            if (options.Count < 0) { throw new ArgumentOutOfRangeException("count"); }

            Stopwatch w = Stopwatch.StartNew();
            Console.WriteLine($"Extracting Page [{options.Index}, {options.Index + options.Count}) from \"{options.InputFilePath}\" into \"{options.OutputFilePath}\"...");

            JsonMapNode runs, run, results;

            // Get 'runs' node from map. If log was too small, page using the object model
            if (root == null || root.Nodes == null || !root.Nodes.TryGetValue("runs", out runs))
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
            if (!runs.Nodes.TryGetValue(options.RunIndex.ToString(), out run) || !run.Nodes.TryGetValue("results", out results) || results.ArrayStarts == null)
            {
                // Log too small; convert via OM
                PageViaOm(options);
                return;
            }

            if (options.Index + options.Count >= results.Count)
            {
                throw new ArgumentOutOfRangeException($"Page requested from Result {options.Index} to {options.Index + options.Count}, but Run has only {results.Count} results.");
            }

            Console.WriteLine($"Run {options.RunIndex} in \"{options.InputFilePath}\" has {results.Count:n0} results.");

            Func<Stream> inputStreamProvider = () => _fileSystem.OpenRead(options.InputFilePath);
            long firstResultStart = results.FindArrayStart(options.Index, inputStreamProvider);
            long lastResultEnd = results.FindArrayStart(options.Index + options.Count + 1, inputStreamProvider) - 1;

            // Build the Sarif Log subset
            long lengthWritten = 0;
            byte[] buffer = new byte[64 * 1024];

            using (FileStream output = File.Create(options.OutputFilePath))
            using (FileStream source = File.OpenRead(options.InputFilePath))
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

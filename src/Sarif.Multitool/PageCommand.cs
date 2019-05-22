// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Newtonsoft.Json;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Map;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class PageCommand : CommandBase
    {
        private readonly IFileSystem _fileSystem;

        public PageCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public int Run(PageOptions options)
        {
            // TODO: Factor FileSystem usage to IFileSystem

            try
            {
                string mapPath = Path.Combine(Environment.ExpandEnvironmentVariables("%LocalAppData%"), "Sarif.MultiTool", Path.GetFileNameWithoutExtension(options.InputFilePath) + ".map.json");

                // Load the JsonMap, if previously built and up-to-date, or rebuild it
                JsonMapNode root = LoadOrRebuildMap(options.InputFilePath, mapPath);

                // Write the desired page from the Sarif file
                ExtractPage(options, root);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }

        static JsonMapNode LoadOrRebuildMap(string inputFilePath, string mapPath)
        {
            // If map exists and is up-to-date, just reload it
            if (File.Exists(mapPath) && File.GetLastWriteTimeUtc(mapPath) > File.GetLastWriteTimeUtc(inputFilePath))
            {
                return JsonConvert.DeserializeObject<JsonMapNode>(File.ReadAllText(mapPath));
            }

            // Otherwise, build the map and save it
            Stopwatch w = Stopwatch.StartNew();
            Console.WriteLine($"Building Json Map of \"{inputFilePath}\" into \"{mapPath}\"...");

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(mapPath)));
            JsonMapBuilder builder = new JsonMapBuilder(0.01);
            JsonMapNode root = builder.Build(inputFilePath);
            File.WriteAllText(mapPath, JsonConvert.SerializeObject(root, Formatting.None));

            w.Stop();
            Console.WriteLine($"Done in {w.Elapsed.TotalSeconds:n1}s.");

            return root;
        }

        static void ExtractPage(PageOptions options, JsonMapNode root)
        {
            Stopwatch w = Stopwatch.StartNew();
            Console.WriteLine($"Extracting Page [{options.Index}, {options.Index + options.Count}) from \"{options.InputFilePath}\" into \"{options.OutputFilePath}\"...");

            byte[] buffer = new byte[64 * 1024];

            using (FileStream output = File.Create(options.OutputFilePath))
            using (FileStream source = File.OpenRead(options.InputFilePath))
            {
                JsonMapNode runs = root.Nodes["runs"];

                // Copy everything up to 'runs'
                // Include '['
                CopyStreamBytes(source, output, 0, runs.Start + 2, buffer);

                foreach (JsonMapNode run in runs.Nodes.Values)
                {
                    JsonMapNode results = run.Nodes["results"];

                    // In the run, copy everything to 'results'
                    // Include '['
                    CopyStreamBytes(source, output, run.Start, results.Start + 2 - run.Start, buffer);

                    // Find and copy the desired range of results
                    // Exclude last ','
                    long firstResultStart = results.ArrayStarts[options.Index];
                    long lastResultEnd = results.ArrayStarts[options.Index + options.Count + 1] - 1;
                    CopyStreamBytes(source, output, firstResultStart, lastResultEnd - firstResultStart, buffer);

                    // Copy everything after the results array to the end of the run
                    // Include ']'
                    CopyStreamBytes(source, output, results.End, run.End - results.End, buffer);
                }

                // Copy everything after all runs
                // Missing stuff?
                CopyStreamBytes(source, output, runs.End, root.End + 1 - runs.End, buffer);
            }

            w.Stop();
            Console.WriteLine($"Done in {w.Elapsed.TotalSeconds:n1}s.");
        }

        static void CopyStreamBytes(Stream source, Stream destination, long offset, long length, byte[] buffer)
        {
            source.Seek(offset, SeekOrigin.Begin);

            long lengthLeft = length;
            while (lengthLeft > 0)
            {
                // Decide how much to read
                int lengthToRead = buffer.Length;
                if (lengthLeft < lengthToRead) { lengthToRead = (int)lengthLeft; }

                // Copy from input to output
                int lengthRead = source.Read(buffer, 0, lengthToRead);
                destination.Write(buffer, 0, lengthRead);

                lengthLeft -= lengthRead;
            }
        }
    }
}

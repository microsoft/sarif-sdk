// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using BSOA.Benchmarks;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace BSOA.Demo
{
    public class Modes
    {
        public static void Convert(string inputPath)
        {
            Friendly.HighlightLine($"-> Converting '{inputPath}' to BSOA...");

            ConsoleTable table = new ConsoleTable(
                new ConsoleColumn("FileName"),
                new ConsoleColumn("Size", Align.Right),
                new ConsoleColumn("JSON Read", Align.Right),
                new ConsoleColumn("BSOA Size", Align.Right),
                new ConsoleColumn("RAM", Align.Right, Highlight.On),
                new ConsoleColumn("Ratio", Align.Right),
                new ConsoleColumn("BSOA Read", Align.Right, Highlight.On));

            foreach (string filePath in QuickBenchmarker.FilesForPath(inputPath))
            {
                string fileName = Path.GetFileName(filePath);
                string outputPath = OutputPath(filePath);

                // Load OM from JSON
                SarifLog log = null;
                MeasureResult<SarifLog> load = Measure.Operation<SarifLog>(() => log = SarifLog.Load(filePath), MeasureSettings.LoadOnce);

                // Save as BSOA
                load.Output.Save(outputPath);

                // Load OM from BSOA
                MeasureResult<SarifLog> bsoaLoad = Measure.Operation(() => SarifLog.Load(outputPath));

                long jsonBytes = new FileInfo(filePath).Length;
                long bsoaBytes = new FileInfo(outputPath).Length;

                table.AppendRow(
                    fileName,
                    Friendly.Size(jsonBytes),
                    Friendly.Rate(jsonBytes, load.Elapsed, load.Iterations),
                    Friendly.Size(bsoaBytes),
                    Friendly.Size(load.AddedMemoryBytes),
                    Friendly.Percentage(bsoaBytes, jsonBytes),
                    Friendly.Rate(bsoaBytes, bsoaLoad.Elapsed, bsoaLoad.Iterations));
            }
        }

        public static void Benchmarks(string inputPath)
        {
            Friendly.HighlightLine($"-> Benchmarking ", AssemblyDescription<SarifLog>(), $" on '{inputPath}'...");
            QuickBenchmarker.RunFiles<SarifLogBenchmarks, SarifLog>(inputPath, SarifLog.Load);
        }

        public static string OutputPath(string inputPath)
        {
            string outputDirectory = Path.Combine(Path.GetDirectoryName(inputPath), "Out");
            string outputPath = Path.Combine(outputDirectory, Path.GetFileName(inputPath));

#if BSOA
            outputPath = outputPath + ".bsoa";
#endif
            Directory.CreateDirectory(outputDirectory);
            return outputPath;
        }

        public static void Diagnostics(string filePath, int logToDepth)
        {
#if BSOA
            BSOA.IO.TreeDiagnostics diagnostics = SarifLog.Diagnostics(filePath);
            diagnostics.Write(Console.Out, logToDepth);
#endif
        }

        public static void LeakTest(string filePath, int count = 100000)
        {
#if BSOA
            SarifLog sourceLog = SarifLog.Load(filePath);
            IList<Result> sourceResults = sourceLog.Runs[0].Results;
            ReportingDescriptor rule = sourceResults[0].GetRule();

            SarifLog destination = new SarifLog();
            Run destinationRun = new Run(destination);
            destination.Runs = new List<Run>() { destinationRun };

            long beforeMemory = GC.GetTotalMemory(true);
            long peakMemory = beforeMemory;
            Stopwatch w = Stopwatch.StartNew();

            using (SarifLogger logger = new SarifLogger(Path.ChangeExtension(filePath, ".out.sarif")))
            {
                for (int i = 0; i < count; ++i)
                {
                    Result result = new Result(destination, sourceResults[i % sourceResults.Count]);
                    result.RuleId = rule.Id;
                    result.RuleIndex = -1;
                    result.Rule = null;

                    logger.Log(rule, result);

                    result.Clear();

                    if (i % 1000 == 0)
                    {
                        long memory = GC.GetTotalMemory(false);
                        if (memory > peakMemory) { peakMemory = memory; }
                    }
                }
            }

            Console.WriteLine($"{count:n0} results in {Friendly.Time(w.Elapsed)}. Peak Memory: {Friendly.Size(peakMemory)} (+ {Friendly.Size(peakMemory - beforeMemory)})");
#endif
        }

        public static string AssemblyDescription<T>()
        {
            AssemblyName name = typeof(T).Assembly.GetName();
            string suffix = typeof(T).Assembly.GetType("Microsoft.CodeAnalysis.Sarif.SarifLogDatabase") != null ? " BSOA" : "";
            return $"{name.Name}{suffix} v{name.Version}";
        }
    }
}

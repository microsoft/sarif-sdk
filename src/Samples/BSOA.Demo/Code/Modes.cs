// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using RoughBench;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;
using System.Linq;

namespace BSOA.Demo
{
    public class Modes
    {
        public static void Convert(string inputPath)
        {
            Format.HighlightLine($"-> Converting '{inputPath}' to BSOA...");

            ConsoleTable table = new ConsoleTable(
                new TableCell("FileName"),
                new TableCell("Size", Align.Right),
                new TableCell("JSON Read", Align.Right),
                new TableCell("BSOA Size", Align.Right),
                new TableCell("RAM", Align.Right, TableColor.Green),
                new TableCell("Ratio", Align.Right),
                new TableCell("BSOA Read", Align.Right, TableColor.Green));

            foreach (string filePath in FilesBenchmarker.FilesForPath(inputPath))
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
                    TableCell.String(fileName),
                    TableCell.Size(jsonBytes),
                    TableCell.Rate(jsonBytes, load.SecondsPerIteration),
                    TableCell.Size(bsoaBytes),
                    TableCell.Size(load.AddedMemoryBytes),
                    TableCell.Percentage(bsoaBytes, jsonBytes),
                    TableCell.Rate(bsoaBytes, bsoaLoad.SecondsPerIteration));
            }
        }

        public static void Benchmarks(string inputPath)
        {
            Format.HighlightLine($"-> Benchmarking ", AssemblyDescription<SarifLog>(), $" on '{inputPath}'...");
            FilesBenchmarker.RunFiles<SarifLog>(typeof(SarifLogBenchmarks), inputPath, SarifLog.Load);
        }

        public static void New()
        {
            Format.HighlightLine($"-> Benchmarking Empty Log in ", AssemblyDescription<SarifLog>());

            // Warm up any statics
            Measure.Operation(() => new SarifLog());

            SarifLog log = null;
            MeasureResult result = Measure.Operation(() =>
            {
                log = new SarifLog();
                return log;
            }, new MeasureSettings(TimeSpan.FromSeconds(5), 10, 1000, true));

            Format.HighlightLine($"new SarifLog() in ", Format.Time(result.SecondsPerIteration), " using ", Format.Size(result.AddedMemoryBytes), ".");
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
            Console.WriteLine($"Leak Test: Loading \"{filePath}\"...");
            long start = System.GC.GetTotalMemory(true);
            SarifLog sourceLog = SarifLog.Load(filePath);

            IList<Result> sourceResults = sourceLog.Runs[0].Results;
            ReportingDescriptor rule = sourceResults[0].GetRule();

            long beforeLoggerMemory = System.GC.GetTotalMemory(true);

            Console.WriteLine($"{sourceLog.Runs.Sum((run) => run.Results.Count):n0} results, +{Format.Size(beforeLoggerMemory - start)}");
            Console.WriteLine($"Logging {count:n0} Results...");

            long peakMemory = beforeLoggerMemory;
            long firstPassMemory = beforeLoggerMemory;
            Stopwatch w = Stopwatch.StartNew();

            using (SarifLogger logger = new SarifLogger(Path.ChangeExtension(filePath, ".out.sarif")))
            {
                for (int i = 0; i < count; ++i)
                {
                    Result result = new Result(sourceResults[i % sourceResults.Count]);
                    result.RuleId = rule.Id;
                    result.RuleIndex = -1;
                    result.Rule = null;
                    
                    logger.Log(rule, result);

                    // Capture memory after 1,024 results (cutoff for new temp log) or one pass through results (end of new locations)
                    if (i == sourceResults.Count || i == 1024)
                    {
                        firstPassMemory = System.GC.GetTotalMemory(false);
                    }

                    if (i % 1000 == 0)
                    {
                        long memory = System.GC.GetTotalMemory(false);
                        if (memory > peakMemory) { peakMemory = memory; }
                    }
                }
            }

            Console.WriteLine($"{count:n0} results in {Format.Time(w.Elapsed)}. Peak Memory: {Format.Size(peakMemory)}. Use after first pass: (+ {Format.Size(peakMemory - firstPassMemory)})");
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

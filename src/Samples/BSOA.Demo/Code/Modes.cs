// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace BSOA.Demo
{
    public static class Modes
    {
        public static TimeSpan MeasureTime = TimeSpan.FromSeconds(2);

        public static MeasureSettings LoadSettings = new MeasureSettings(MeasureTime, 1, 4, true);
        public static MeasureSettings OnceSettings = new MeasureSettings(TimeSpan.Zero, 1, 1, true);
        public static MeasureSettings FastOperationSettings = new MeasureSettings(MeasureTime, 50, 100, false);

        public static void Convert(string inputPath)
        {
            Friendly.HighlightLine($"-> Converting '{inputPath}' to BSOA...");

            ConsoleTable table = new ConsoleTable(
                new ConsoleColumn("FileName"),
                new ConsoleColumn("Size", Align.Right),
                new ConsoleColumn("JSON Read", Align.Right),
                new ConsoleColumn("BSOA Size", Align.Right, Highlight.On),
                new ConsoleColumn("Ratio", Align.Right),
                new ConsoleColumn("BSOA Read", Align.Right));

            foreach (string filePath in FilesForPath(inputPath))
            {
                string fileName = Path.GetFileName(filePath);
                string outputPath = OutputPath(filePath);

                // Load OM from JSON
                MeasureResult<SarifLog> load 
                    = Measure.Operation<SarifLog>(filePath, SarifLog.Load, LoadSettings);

                // Save as BSOA
                load.Output.Save(outputPath);

                // Load OM from BSOA
                MeasureResult<SarifLog> bsoaLoad
                    = Measure.Operation(outputPath, SarifLog.Load, LoadSettings);

                table.AppendRow(
                    fileName,
                    Friendly.Size(load.InputSizeBytes),
                    Friendly.Rate(load.InputSizeBytes, load.AverageTime),
                    Friendly.Size(bsoaLoad.InputSizeBytes),
                    Friendly.Percentage(bsoaLoad.InputSizeBytes, load.InputSizeBytes),
                    Friendly.Rate(bsoaLoad.InputSizeBytes, bsoaLoad.AverageTime));
            }
        }

        public static void Load(string inputPath)
        {
            Friendly.HighlightLine($"-> Loading '{inputPath}' with ", AssemblyDescription<SarifLog>(), "...");

            ConsoleTable table = new ConsoleTable(
                new ConsoleColumn("FileName"),
                new ConsoleColumn("Size", Align.Right),
                new ConsoleColumn("Read", Align.Right, Highlight.On),
                new ConsoleColumn("RAM", Align.Right, Highlight.On));

            MeasureSettings settings = new MeasureSettings(MeasureTime, minIterations: 1, maxIterations: 8, measureMemory: true);

            foreach (string filePath in FilesForPath(inputPath))
            {
                MeasureResult<SarifLog> load = Measure.Operation<SarifLog>(filePath, (path) => SarifLog.Load(path), settings);

                table.AppendRow(
                    Path.GetFileName(filePath),
                    Friendly.Size(load.InputSizeBytes),
                    Friendly.Rate(load.InputSizeBytes, load.AverageTime),
                    Friendly.Size(load.AddedMemoryBytes));
            }
        }

        public static void ObjectModelOverhead(string inputPath)
        {
            Friendly.HighlightLine($"-> Computing SUM(Result.StartLine) in '{inputPath}' with ", AssemblyDescription<SarifLog>(), "...");

            ConsoleTable table = new ConsoleTable(
                new ConsoleColumn("FileName"),
                new ConsoleColumn("Size", Align.Right),
                new ConsoleColumn("SUM(StartLine)", Align.Right, Highlight.On));

            foreach (string filePath in FilesForPath(inputPath))
            {
                SarifLog log = SarifLog.Load(filePath);
                MeasureResult<long> count = Measure.Operation<long>(filePath, (path) => LineTotal(log), FastOperationSettings);

                table.AppendRow(
                    Path.GetFileName(filePath),
                    Friendly.Size(count.InputSizeBytes),
                    Friendly.Time(count.AverageTime));
            }
        }

        private static long LineTotal(SarifLog log)
        {
            long lineTotal = 0;

            foreach (Run run in log.Runs)
            {
                foreach (Result result in run.Results)
                {
                    foreach (Location location in result.Locations)
                    {
                        lineTotal += location?.PhysicalLocation?.Region?.StartLine ?? 0;
                    }
                }
            }

            return lineTotal;
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

        public static IEnumerable<string> FilesForPath(string inputPath)
        {
            if (Directory.Exists(inputPath))
            {
                return Directory.EnumerateFiles(inputPath).ToList();
            }
            else
            {
                return new string[] { inputPath };
            }
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

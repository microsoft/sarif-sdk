// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Newtonsoft.Json;

namespace BSOA.Demo
{
    public static class Modes
    {
        public static void Load(string filePath, int iterations)
        {
            SarifLog log = Measure.LoadPerformance(filePath, iterations: iterations, (path) =>
            {
                return SarifLog.Load(path);
            });
        }

        public static void LoadAndSave(string filePath, string outputPath, int iterations)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));

            SarifLog log = Measure.LoadPerformance(filePath, iterations: iterations, (path) =>
            {
                return SarifLog.Load(path);
            });

            Measure.SavePerformance(outputPath, iterations: 1, (path) =>
            {
                log.Save(path);
            });
        }

        public static void LoadAndSaveFolder(string folderPath)
        {
            foreach (string filePath in Directory.GetFiles(folderPath, "*.sarif"))
            {
                Console.WriteLine($" - {filePath}");

                SarifLog log = SarifLog.LoadDeferred(filePath);

                string outputPath = Path.Combine(Path.GetDirectoryName(filePath), @"..\RoundTripped", Path.GetFileName(filePath));
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                log.Save(outputPath);
            }
        }

        public static long ObjectModelOverhead(string filePath, int iterations = 50)
        {
            SarifLog log = Measure.LoadPerformance(filePath, iterations: 1, (path) =>
            {
                return SarifLog.Load(path);
            });

            long lineTotal = 0;

            Stopwatch w = Stopwatch.StartNew();
            for (int iteration = 0; iteration < iterations; ++iteration)
            {
                lineTotal = 0;

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
            }

            w.Stop();
            Friendly.HighlightLine($"  -> Sum(StartLine) = ", $"{lineTotal:n0}", $" in ", Friendly.Time(w.Elapsed / iterations), $" ({iterations:n0}x)");
            return lineTotal;
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

        public static void IndentFolder(string folderPath)
        {
            JsonSerializer serializer = JsonSerializer.Create();

            foreach (string filePath in Directory.GetFiles(folderPath, "*.sarif"))
            {
                Console.WriteLine($" - {filePath}");

                SarifLog log = SarifLog.LoadDeferred(filePath);

                string outputPath = Path.Combine(Path.GetDirectoryName(filePath), @"..\Indented", Path.GetFileName(filePath));
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                using (JsonTextWriter writer = new JsonTextWriter(File.CreateText(outputPath)))
                {
                    writer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, log);
                }
            }
        }

        public static void ConvertFolder(string folderPath)
        {
            string outputFolderPath = Path.Combine(folderPath, "Out");
            Directory.CreateDirectory(outputFolderPath);

            ConsoleTable table = new ConsoleTable(
                new ConsoleColumn("FileName"),
                new ConsoleColumn("Size", rightAligned: true),
                new ConsoleColumn("BSOA Size", rightAligned: true, highlighted: true),
                new ConsoleColumn("Ratio", rightAligned: true));

            foreach (string filePath in Directory.GetFiles(folderPath, "*.sarif"))
            {
                string fileName = Path.GetFileName(filePath);

                SarifLog log = null;

                Stopwatch readWatch = Stopwatch.StartNew();
                log = SarifLog.Load(filePath);
                readWatch.Stop();

                string outputPath = Path.Combine(outputFolderPath, fileName + ".bsoa");
                Stopwatch writeWatch = Stopwatch.StartNew();
                log.Save(outputPath);
                writeWatch.Stop();

                long originalSize = new FileInfo(filePath).Length;
                long bsoaSize = new FileInfo(outputPath).Length;
                
                table.AppendRow(
                    fileName, 
                    Friendly.Size(originalSize), 
                    Friendly.Size(bsoaSize), 
                    Friendly.Percentage(bsoaSize, originalSize));
            }
        }
    }
}

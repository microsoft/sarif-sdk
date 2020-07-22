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

        public static void Diagnostics(string filePath, int logToDepth)
        {
            BSOA.IO.TreeDiagnostics diagnostics = SarifLog.Diagnostics(filePath);
            diagnostics.Write(Console.Out, logToDepth);
        }

        public static long LineTotal(SarifLog log)
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

        public static void LeakTest(string filePath, int count = 100000)
        {
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

            Console.WriteLine($"{count:n0} results in {w.Elapsed.TotalSeconds:n1} sec. Peak Memory: {(peakMemory) / (1024 * 1024.0):n3} MB (+ {(peakMemory - beforeMemory) / (1024 * 1024.0):n3} MB)");
        }
    }
}

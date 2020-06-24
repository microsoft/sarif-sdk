// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

using Microsoft.CodeAnalysis.Sarif;

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
    }
}

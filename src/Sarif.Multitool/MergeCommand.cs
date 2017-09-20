using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class MergeCommand
    {
        public static int Run(MergeOptions mergeOptions)
        {
            // Get all SARIF files listed in options.
            IEnumerable<string> sarifFiles = from path in mergeOptions.Files
                let lastBackslashPos = path.LastIndexOf('\\') + 1
                let directory = path.Substring(0, lastBackslashPos)
                let filename = path.Substring(lastBackslashPos, path.Length - lastBackslashPos)
                from fileName in Directory.GetFiles(directory, filename)
                where fileName.EndsWith(".sarif", StringComparison.InvariantCultureIgnoreCase)
                select fileName;

            // Combine all Runs from SARIF files.
            IEnumerable<Run> allRuns = from file in sarifFiles
                select ReadFile(file)
                into log
                where log.Runs != null
                from run in log.Runs
                where run != null
                select new Run(run);

            // Build one SarifLog with all the Runs.
            SarifLog combinedLog = new SarifLog(SarifVersion.OneZeroZero.ConvertToSchemaUri(), SarifVersion.OneZeroZero, allRuns);

            // Write output to file.
            string outputName = GetOutputFileName(mergeOptions);
            var formatting = mergeOptions.PrettyPrint
                ? Newtonsoft.Json.Formatting.Indented
                : Newtonsoft.Json.Formatting.None;
            var settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = formatting
            };
            LoggingOptions loggingOptions = mergeOptions.ConvertToLoggingOptions();
            SarifLog reformattedLog = new ReformattingVisitor(loggingOptions).VisitSarifLog(combinedLog);
            File.WriteAllText(outputName, JsonConvert.SerializeObject(reformattedLog, settings));

            return 0;
        }

        private static SarifLog ReadFile(string filePath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance
            };

            string logText = File.ReadAllText(filePath);
            
            SarifLog log = JsonConvert.DeserializeObject<SarifLog>(logText, settings);

            return log;
        }

        internal static string GetOutputFileName(MergeOptions mergeOptions)
        {
            return !string.IsNullOrEmpty(mergeOptions.OutputFilePath)
                ? mergeOptions.OutputFilePath 
                : "combined.sarif";
        }
    }
}

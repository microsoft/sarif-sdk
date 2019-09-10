using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace SarifTrim
{
    /// <summary>
    ///  SarifTrim is a one-off sample removing some redundant and some excessively large parts of a Sarif log
    ///  for handling of a huge log.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(@"Usage: SarifTrim [inputFilePath] [outputFilePath?] [removeParts?]
    removeParts: Comma-separated sections to remove. Options: CodeFlows,RelatedLocations");
                return;
            }

            string inputFilePath = args[0];
            string outputFilePath = (args.Length > 1 ? args[1] : Path.ChangeExtension(inputFilePath, $"trim{Path.GetExtension(inputFilePath)}"));
            HashSet<string> removeParts = new HashSet<string>((args.Length > 2 ? args[2] : "").Split(',').Select(entry => entry.Trim()), StringComparer.OrdinalIgnoreCase);

            SarifLog log = null;
            using (new ConsoleWatch($"Loading \"{inputFilePath}\"..."))
            {
                log = SarifLog.LoadDeferred(inputFilePath);
            }

            using (new ConsoleWatch($"Trimming \"{inputFilePath}\" into \"{outputFilePath}\"..."))
            {
                Run run = log.Runs[0];

                SarifConsolidator consolidator = new SarifConsolidator(run);
                consolidator.MessageLengthLimitBytes = 1024;
                consolidator.RemoveCodeFlows = (removeParts.Contains("CodeFlows"));
                consolidator.RemoveRelatedLocations = (removeParts.Contains("RelatedLocations"));

                // Remove Artifact UriBaseIds
                foreach (Artifact a in run.Artifacts)
                {
                    a.Location.UriBaseId = null;
                }

                // Trim Result CodeFlowLocations, long Messages, and redundant Region properties
                using (SarifLogger logger = new SarifLogger(outputFilePath, LoggingOptions.OverwriteExistingOutputFile, tool: run.Tool, run: run))
                {
                    foreach (Result result in run.Results)
                    {
                        consolidator.Trim(result);
                        logger.Log(result.GetRule(run), result);
                    }
                }

                Console.WriteLine($"Done. Consolidated {consolidator.TotalThreadFlowLocations:n0} TFLs into {consolidator.UniqueThreadFlowLocations:n0} unique entries.");
            }
        }
    }
}

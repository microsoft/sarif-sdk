using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

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
    removeParts: Comma-separated sections to remove. Options: UriBaseIds,CodeFlows,RelatedLocations,Graphs,GraphTraversals,Stacks,WebRequests,WebResponses");
                return;
            }

            string inputFilePath = args[0];
            string outputFilePath = (args.Length > 1 ? args[1] : Path.ChangeExtension(inputFilePath, $"trim{Path.GetExtension(inputFilePath)}"));

            // Split on comma, remove whitespace, and put in case-insensitive HashSet
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
                consolidator.RemoveGraphs = (removeParts.Contains("Graphs"));
                consolidator.RemoveGraphTraversals = (removeParts.Contains("GraphTraversals"));
                consolidator.RemoveStacks = (removeParts.Contains("Stacks"));
                consolidator.RemoveWebRequests = (removeParts.Contains("WebRequests"));
                consolidator.RemoveWebResponses = (removeParts.Contains("WebResponses"));

                // Trim Result CodeFlowLocations, long Messages, and redundant Region properties
                using (SarifLogger logger = new SarifLogger(outputFilePath, LoggingOptions.OverwriteExistingOutputFile, tool: run.Tool, run: run))
                {
                    foreach (Result result in run.Results)
                    {
                        consolidator.Trim(result);
                        logger.Log(result.GetRule(run), result);
                    }

                    if (removeParts.Contains("UriBaseIds"))
                    {
                        foreach (Artifact a in run.Artifacts)
                        {
                            a.Location.UriBaseId = null;
                        }
                    }
                }

                if (consolidator.UniqueThreadFlowLocations < consolidator.TotalThreadFlowLocations)
                {
                    Console.WriteLine($"Consolidated {consolidator.TotalThreadFlowLocations:n0} ThreadFlowLocations into {consolidator.UniqueThreadFlowLocations:n0} unique ones.");
                }

                if (consolidator.UniqueLocations < consolidator.TotalLocations)
                {
                    Console.WriteLine($"Consolidated {consolidator.TotalLocations:n0} Locations into {consolidator.UniqueLocations:n0} unique per-Result Locations.");
                }
            }
        }
    }
}

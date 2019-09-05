using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

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
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SarifTrim [inputFilePath] [outputFilePath] [itemsToRemove?]");
                return;
            }

            string inputFilePath = args[0];
            string outputFilePath = args[1];


            Stopwatch w = Stopwatch.StartNew();

            JsonSerializer serializer = new JsonSerializer();
            serializer.ContractResolver = new SarifDeferredContractResolver();

            SarifLog log;

            // Read the SarifLog with the deferred reader
            using (new ConsoleWatch($"Loading \"{inputFilePath}\"..."))
            using (JsonPositionedTextReader jtr = new JsonPositionedTextReader(inputFilePath))
            {
                log = serializer.Deserialize<SarifLog>(jtr);
            }

            using (new ConsoleWatch($"Trimming \"{inputFilePath}\" into \"{outputFilePath}\"..."))
            {
                Run run = log.Runs[0];

                // Workaround if string properties not rewriting properly (doubled quotes)
                run.RemoveProperty("semmle.sourceLanguage");

                // Trim duplicate Rule properties
                foreach (ReportingDescriptor rule in run.Tool.Driver.Rules)
                {
                    rule.RemoveProperty("name");
                    rule.RemoveProperty("description");
                    rule.RemoveProperty("opaque-id");
                    rule.RemoveProperty("id");
                    rule.DefaultConfiguration = null;
                }

                // Trim Artifact indices
                foreach (Artifact a in run.Artifacts)
                {
                    a.Location.Index = -1;
                }

                // Trim Result CodeFlowLocations, long Messages, and redundant Region properties
                using (SarifLogger logger = new SarifLogger(outputFilePath, LoggingOptions.OverwriteExistingOutputFile, tool: run.Tool, run: run))
                {
                    int resultCount = 0;

                    foreach (Result result in run.Results)
                    {
                        Trim(result);
                        logger.Log(result.GetRule(run), result);

                        resultCount++;
                        //if (resultCount == 10) { break; }
                    }
                }
            }
        }

        static void Trim(Result result)
        {
            // Remove CodeFlows
            result.CodeFlows = null;
            //foreach (CodeFlow codeFlow in result.CodeFlows)
            //{
            //    foreach (ThreadFlow threadFlow in codeFlow.ThreadFlows)
            //    {
            //        Trim(threadFlow);
            //    }
            //}

            // Trim Messages over 1KB
            if (result?.Message?.Text?.Length > 1024)
            {
                result.Message.Text = result.Message.Text.Substring(0, 1024);
            }

            // Trim Location redundancy
            if (result.Locations != null)
            {
                foreach (Location loc in result.Locations)
                {
                    Trim(loc);
                }
            }

            if (result.RelatedLocations != null)
            {
                foreach (Location loc in result.RelatedLocations)
                {
                    Trim(loc);
                }
            }
        }

        static void Trim(ThreadFlow tFlow)
        {
            if (tFlow?.Locations != null)
            {
                for (int i = 0; i < tFlow.Locations.Count; ++i)
                {
                    ThreadFlowLocation tfl = tFlow.Locations[i];
                    if (tfl.Index != -1)
                    {
                        tFlow.Locations[i] = new ThreadFlowLocation() { Index = tfl.Index };
                    }
                    else
                    {
                        Trim(tfl.Location);
                    }
                }
            }
        }

        static void Trim(Location loc)
        {
            if (loc != null)
            {
                Trim(loc.PhysicalLocation);
            }
        }

        static void Trim(PhysicalLocation loc)
        {
            if (loc != null)
            {
                Trim(loc.ArtifactLocation);
                Trim(loc.Region);
                Trim(loc.ContextRegion);
            }
        }

        static void Trim(ArtifactLocation a)
        {
            if (a != null)
            {
                // Leave only index if index is present
                if (a.Index != -1)
                {
                    a.Uri = null;
                    a.UriBaseId = null;
                }
            }
        }

        static void Trim(Region r)
        {
            if (r != null)
            {
                if (r.EndLine == r.StartLine)
                {
                    r.EndLine = 0;
                }

                r.CharOffset = -1;
                r.CharLength = 0;
            }
        }
    }
}

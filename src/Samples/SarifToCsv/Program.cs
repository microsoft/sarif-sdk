using AzureDevOpsCrawlers.Common.IO;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;

namespace SarifToCsv
{
    /// <summary>
    ///  SarifToCsv is a simple example of converting a few key SARIF result properties to CSV format
    ///  to allow easily viewing results compared with the 'match-results-forward' Sarif Multitool mode.
    /// </summary>
    /// <remarks>
    ///  Should be modified to:
    ///    - Make the output columns configurable.
    ///    - Enable writing more result properties.
    ///    - Integrate as a Sarif Multitool mode.
    /// </remarks>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SarifToCsv [sarifFilePath] [csvFilePath]");
                return;
            }

            try
            {
                string sarifFilePath = args[0];
                string csvFilePath = args[1];

                Console.WriteLine($"Converting \"{sarifFilePath}\" to \"{csvFilePath}\"...");
                Stopwatch w = Stopwatch.StartNew();

                SarifLog log = null;

                // Read the SarifLog with the deferred reader
                JsonSerializer serializer = new JsonSerializer();
                serializer.ContractResolver = new SarifDeferredContractResolver();
                using (JsonPositionedTextReader jtr = new JsonPositionedTextReader(sarifFilePath))
                {
                    log = serializer.Deserialize<SarifLog>(jtr);
                }

                // Convert the each result location into a row in a CSV
                using (CsvWriter writer = new CsvWriter(csvFilePath))
                {
                    writer.SetColumns(new string[] { "RuleId", "BaselineState", "Location.Region.Snippet.Text", "Location.Region.StartLine", "Location.Region.StartColumn", "Location.Uri", "Message.Text", "Fingerprint" });

                    foreach (var run in log.Runs)
                    {
                        foreach (var result in run.Results)
                        {
                            foreach (PhysicalLocation location in result.PhysicalLocations())
                            {
                                writer.Write(result.RuleId);
                                writer.Write(result.BaselineState.ToString());

                                writer.Write(location?.Region?.Snippet?.Text ?? "");
                                writer.Write(location?.Region.StartLine ?? -1);
                                writer.Write(location?.Region.StartColumn ?? -1);

                                writer.Write(location.FileLocation.FileUri(run) ?? "");

                                writer.Write(result.Message?.Text ?? "");
                                writer.Write(result.Fingerprints.FirstOrDefault().Value ?? "");

                                writer.NextRow();
                            }
                        }
                    }

                    Console.WriteLine($"Done. Wrote {writer.RowCountWritten:n0} rows to \"{csvFilePath}\" in {w.Elapsed.TotalSeconds:n0}s.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }
    }
}
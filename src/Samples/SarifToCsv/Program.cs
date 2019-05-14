// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

using AzureDevOpsCrawlers.Common.IO;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;

using Newtonsoft.Json;

namespace SarifToCsv
{
    /// <summary>
    ///  SarifToCsv is a simple example of converting a few key SARIF result properties to CSV format
    ///  to allow easily viewing results compared with the 'match-results-forward' Sarif Multitool mode.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Dictionary<string, Action<CsvWriter, Result, PhysicalLocation>> writers = BuildWriters();

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SarifToCsv [sarifFileOrFolderPath] [csvFilePath]");
                Console.WriteLine("  Column Names are configured in SarifToCsv.exe.config, in the 'ColumnNames' property.");
                Console.WriteLine($"  Available Column Names: {String.Join(", ", writers.Keys)}");

                return;
            }

            try
            {
                string sarifFilePath = args[0];
                string csvFilePath = args[1];
                IEnumerable<string> columnNames = ConfigurationManager.AppSettings["ColumnNames"].Split(',').Select((value) => value.Trim());
                IEnumerable<Action<CsvWriter, Result, PhysicalLocation>> selectedWriters = columnNames.Select((name) => writers[name]).ToArray();

                Console.WriteLine($"Converting \"{sarifFilePath}\" to \"{csvFilePath}\"...");
                Stopwatch w = Stopwatch.StartNew();

                JsonSerializer serializer = new JsonSerializer();
                serializer.ContractResolver = new SarifDeferredContractResolver();

                using (CsvWriter writer = new CsvWriter(csvFilePath))
                {
                    writer.SetColumns(columnNames);

                    // Read the Sarif file (or all Sarif files in the folder) and write to the CSV
                    if (Directory.Exists(sarifFilePath))
                    {
                        foreach (string filePath in Directory.GetFiles(sarifFilePath, "*.sarif", SearchOption.AllDirectories))
                        {
                            ConvertSarifLog(serializer, filePath, writer, selectedWriters);
                        }
                    }
                    else
                    {
                        ConvertSarifLog(serializer, sarifFilePath, writer, selectedWriters);
                    }

                    Console.WriteLine($"Done. Wrote {writer.RowCountWritten:n0} rows to \"{csvFilePath}\" in {w.Elapsed.TotalSeconds:n0}s.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        public static void ConvertSarifLog(JsonSerializer serializer, string sarifFilePath, CsvWriter writer, IEnumerable<Action<CsvWriter, Result, PhysicalLocation>> selectedWriters)
        {
            SarifLog log = null;

            // Read the SarifLog with the deferred reader
            using (JsonPositionedTextReader jtr = new JsonPositionedTextReader(sarifFilePath))
            {
                log = serializer.Deserialize<SarifLog>(jtr);
            }

            // Write a row in the output file for each result location
            if (log.Runs != null)
            {
                foreach (var run in log.Runs)
                {
                    if (run.Results != null)
                    {
                        foreach (var result in run.Results)
                        {
                            foreach (PhysicalLocation location in result.PhysicalLocations())
                            {
                                foreach (Action<CsvWriter, Result, PhysicalLocation> columnWriter in selectedWriters)
                                {
                                    columnWriter(writer, result, location);
                                }

                                writer.NextRow();
                            }
                        }
                    }
                }
            }
        }

        public static Dictionary<string, Action<CsvWriter, Result, PhysicalLocation>> BuildWriters()
        {
            Dictionary<string, Action<CsvWriter, Result, PhysicalLocation>> writers = new Dictionary<string, Action<CsvWriter, Result, PhysicalLocation>>(StringComparer.OrdinalIgnoreCase);

            // Result Basic Properties
            writers["BaselineState"] = (writer, result, pLoc) => { writer.Write(result.BaselineState.ToString()); };
            writers["CorrelationGuid"] = (writer, result, pLoc) => { writer.Write(result.CorrelationGuid ?? ""); };
            writers["Fingerprints"] = (writer, result, pLoc) => { writer.Write(String.Join("; ", result.Fingerprints?.Values ?? Array.Empty<string>())); };
            writers["HostedViewerUri"] = (writer, result, pLoc) => { writer.Write(result.HostedViewerUri?.ToString() ?? ""); };
            writers["Guid"] = (writer, result, pLoc) => { writer.Write(result.Guid ?? ""); };
            writers["Level"] = (writer, result, pLoc) => { writer.Write(result.Level.ToString()); };
            writers["Message.Text"] = (writer, result, pLoc) => { writer.Write(result.Message?.Text ?? ""); };
            writers["OccurrenceCount"] = (writer, result, pLoc) => { writer.Write(result.OccurrenceCount); };
            writers["PartialFingerprints"] = (writer, result, pLoc) => { writer.Write(String.Join("; ", result.PartialFingerprints?.Values ?? Array.Empty<string>())); };
            writers["Provenance"] = (writer, result, pLoc) => { writer.Write(result.Provenance?.ToString() ?? ""); };
            writers["Rank"] = (writer, result, pLoc) => { writer.Write(result.Rank.ToString()); };
            writers["RuleId"] = (writer, result, pLoc) => { writer.Write(result.RuleId ?? ""); };
            writers["RuleIndex"] = (writer, result, pLoc) => { writer.Write(result.RuleIndex); };
            writers["Tags"] = (writer, result, pLoc) => { writer.Write(String.Join("; ", ((IEnumerable<string>)result.Tags) ?? Array.Empty<string>())); };
            writers["WorkItemUris"] = (writer, result, pLoc) => { writer.Write(String.Join("; ", result.WorkItemUris?.Select((uri) => uri.ToString()) ?? Array.Empty<string>())); };
            
            // PhysicalLocation Properties
            writers["Location.Tags"] = (writer, result, pLoc) => { writer.Write(String.Join("; ", ((IEnumerable<string>)pLoc.Tags) ?? Array.Empty<string>())); };
            writers["Location.Uri"] = (writer, result, pLoc) => { writer.Write(pLoc.ArtifactLocation?.Uri?.ToString() ?? ""); };

            // Region Properties
            writers["Location.Region.ByteLength"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.ByteLength ?? -1); };
            writers["Location.Region.ByteOffset"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.ByteOffset ?? -1); };
            writers["Location.Region.CharLength"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.CharLength ?? -1); };
            writers["Location.Region.CharOffset"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.CharOffset ?? -1); };
            writers["Location.Region.EndColumn"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.EndColumn ?? -1); };
            writers["Location.Region.EndLine"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.EndLine ?? -1); };
            writers["Location.Region.IsBinaryRegion"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.IsBinaryRegion.ToString() ?? ""); };
            writers["Location.Region.Message.Text"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.Message?.Text ?? ""); };
            writers["Location.Region.Snippet.Text"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.Snippet?.Text ?? ""); };
            writers["Location.Region.SourceLanguage"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.SourceLanguage ?? ""); };
            writers["Location.Region.StartColumn"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.StartColumn ?? -1); };
            writers["Location.Region.StartLine"] = (writer, result, pLoc) => { writer.Write(pLoc.Region?.StartLine ?? -1); };
            writers["Location.Region.Tags"] = (writer, result, pLoc) => { writer.Write(String.Join("; ", ((IEnumerable<string>)pLoc.Region?.Tags) ?? Array.Empty<string>())); };

            return writers;
        }
    }
}
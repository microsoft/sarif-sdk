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
        public class WriteContext
        {
            public CsvWriter Writer;
            public Run Run;
            public Result Result;
            public PhysicalLocation PLoc;

            public int RunIndex;
            public int ResultIndex;
        }

        public static void Main(string[] args)
        {
            Dictionary<string, Action<WriteContext>> writers = BuildWriters();

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SarifToCsv [sarifFileOrFolderPath] [csvFilePath] [columnNamesCommaDelimited?]");
                Console.WriteLine("  Column Names are configured in SarifToCsv.exe.config, in the 'ColumnNames' property.");
                Console.WriteLine($"  Available Column Names: {String.Join(", ", writers.Keys)}");

                return;
            }

            try
            {
                string sarifFilePath = args[0];
                string csvFilePath = args[1];
                IEnumerable<string> columnNames = (args.Length > 2 ? args[2] : ConfigurationManager.AppSettings["ColumnNames"]).Split(',').Select((value) => value.Trim());
                IEnumerable<Action<WriteContext>> selectedWriters = columnNames.Select((name) => writers[name]).ToArray();

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

                    Console.WriteLine();
                    Console.WriteLine($"Done. Wrote {writer.RowCountWritten:n0} rows to \"{csvFilePath}\" in {w.Elapsed.TotalSeconds:n0}s.");
                }
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine($"Error: {ex}");
            }
        }

        public static void ConvertSarifLog(JsonSerializer serializer, string sarifFilePath, CsvWriter writer, IEnumerable<Action<WriteContext>> selectedWriters)
        {
            SarifLog log = null;

            // Read the SarifLog with the deferred reader
            using (JsonPositionedTextReader jtr = new JsonPositionedTextReader(sarifFilePath))
            {
                log = serializer.Deserialize<SarifLog>(jtr);
            }

            WriteContext context = new WriteContext();
            context.Writer = writer;

            // Write a row in the output file for each result location
            if (log.Runs != null)
            {
                context.RunIndex = 0;

                foreach (var run in log.Runs)
                {
                    context.Run = run;
                    context.ResultIndex = 0;

                    if (run.Results != null)
                    {
                        foreach (var result in run.Results)
                        {
                            context.Result = result;

                            if (result.Locations == null || result.Locations.Count == 0)
                            {
                                context.PLoc = null;
                                Write(context, selectedWriters);
                            }
                            else
                            {
                                foreach (PhysicalLocation location in result.PhysicalLocations())
                                {
                                    context.PLoc = location;
                                    Write(context, selectedWriters);
                                }
                            }

                            context.ResultIndex++;
                        }
                    }

                    context.RunIndex++;
                }
            }
        }

        public static void Write(WriteContext context, IEnumerable<Action<WriteContext>> columnWriters)
        {
            foreach (Action<WriteContext> columnWriter in columnWriters)
            {
                columnWriter(context);
            }

            context.Writer.NextRow();
            if(context.Writer.RowCountWritten % 1000 == 0) { Console.Write("."); }
        }

        public static Dictionary<string, Action<WriteContext>> BuildWriters()
        {
            Dictionary<string, Action<WriteContext>> writers = new Dictionary<string, Action<WriteContext>>(StringComparer.OrdinalIgnoreCase);

            // Result Basic Properties
            writers["BaselineState"] = (c) => { c.Writer.Write(c.Result.BaselineState.ToString()); };
            writers["CorrelationGuid"] = (c) => { c.Writer.Write(c.Result.CorrelationGuid ?? ""); };
            writers["Fingerprints"] = (c) => { c.Writer.Write(String.Join("; ", c.Result.Fingerprints?.Values ?? Array.Empty<string>())); };
            writers["HostedViewerUri"] = (c) => { c.Writer.Write(c.Result.HostedViewerUri?.ToString() ?? ""); };
            writers["Guid"] = (c) => { c.Writer.Write(c.Result.Guid ?? ""); };
            writers["Level"] = (c) => { c.Writer.Write(c.Result.Level.ToString()); };
            writers["Message.Text"] = (c) => { c.Writer.Write(c.Result.Message?.Text ?? ""); };
            writers["OccurrenceCount"] = (c) => { c.Writer.Write(c.Result.OccurrenceCount); };
            writers["PartialFingerprints"] = (c) => { c.Writer.Write(String.Join("; ", c.Result.PartialFingerprints?.Values ?? Array.Empty<string>())); };
            writers["Provenance"] = (c) => { c.Writer.Write(c.Result.Provenance?.ToString() ?? ""); };
            writers["Rank"] = (c) => { c.Writer.Write(c.Result.Rank.ToString()); };
            writers["RuleId"] = (c) => { c.Writer.Write(c.Result.RuleId ?? ""); };
            writers["RuleIndex"] = (c) => { c.Writer.Write(c.Result.RuleIndex); };
            writers["Tags"] = (c) => { c.Writer.Write(String.Join("; ", ((IEnumerable<string>)c.Result.Tags) ?? Array.Empty<string>())); };
            writers["WorkItemUris"] = (c) => { c.Writer.Write(String.Join("; ", c.Result.WorkItemUris?.Select((uri) => uri.ToString()) ?? Array.Empty<string>())); };
            
            // PhysicalLocation Properties
            writers["Location.Tags"] = (c) => { c.Writer.Write(String.Join("; ", ((IEnumerable<string>)c.PLoc?.Tags) ?? Array.Empty<string>())); };
            writers["Location.Uri"] = (c) => { c.Writer.Write(c.PLoc?.ArtifactLocation?.FileUri(c.Run)?.ToString() ?? ""); };

            // Region Properties
            writers["Location.Region.ByteLength"] = (c) => { c.Writer.Write(c.PLoc?.Region?.ByteLength ?? -1); };
            writers["Location.Region.ByteOffset"] = (c) => { c.Writer.Write(c.PLoc?.Region?.ByteOffset ?? -1); };
            writers["Location.Region.CharLength"] = (c) => { c.Writer.Write(c.PLoc?.Region?.CharLength ?? -1); };
            writers["Location.Region.CharOffset"] = (c) => { c.Writer.Write(c.PLoc?.Region?.CharOffset ?? -1); };
            writers["Location.Region.EndColumn"] = (c) => { c.Writer.Write(c.PLoc?.Region?.EndColumn ?? -1); };
            writers["Location.Region.EndLine"] = (c) => { c.Writer.Write(c.PLoc?.Region?.EndLine ?? -1); };
            writers["Location.Region.IsBinaryRegion"] = (c) => { c.Writer.Write(c.PLoc?.Region?.IsBinaryRegion.ToString() ?? ""); };
            writers["Location.Region.Message.Text"] = (c) => { c.Writer.Write(c.PLoc?.Region?.Message?.Text ?? ""); };
            writers["Location.Region.Snippet.Text"] = (c) => { c.Writer.Write(c.PLoc?.Region?.Snippet?.Text ?? ""); };
            writers["Location.Region.SourceLanguage"] = (c) => { c.Writer.Write(c.PLoc?.Region?.SourceLanguage ?? ""); };
            writers["Location.Region.StartColumn"] = (c) => { c.Writer.Write(c.PLoc?.Region?.StartColumn ?? -1); };
            writers["Location.Region.StartLine"] = (c) => { c.Writer.Write(c.PLoc?.Region?.StartLine ?? -1); };
            writers["Location.Region.Tags"] = (c) => { c.Writer.Write(String.Join("; ", ((IEnumerable<string>)c.PLoc?.Region?.Tags) ?? Array.Empty<string>())); };

            // Run Identity Properties
            writers["Run.BaselineGuid"] = (c) => { c.Writer.Write(c.Run?.BaselineGuid ?? ""); };
            writers["Run.AutomationDetails.CorrelationGuid"] = (c) => { c.Writer.Write(c.Run?.AutomationDetails?.CorrelationGuid ?? ""); };
            writers["Run.AutomationDetails.Id"] = (c) => { c.Writer.Write(c.Run?.AutomationDetails?.Id ?? ""); };
            writers["Run.AutomationDetails.Guid"] = (c) => { c.Writer.Write(c.Run?.AutomationDetails?.Guid ?? ""); };

            // Run and Result Index (alternate identity if Guids not provided)
            writers["RunIndex"] = (c) => { c.Writer.Write(c.RunIndex); };
            writers["ResultIndex"] = (c) => { c.Writer.Write(c.ResultIndex); };


            return writers;
        }
    }
}
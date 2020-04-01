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
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: SarifToCsv [sarifFileOrFolderPath] [csvFilePath?] [columnNamesCommaDelimited?]");
                Console.WriteLine("  Column Names are configured in SarifToCsv.exe.config, in the 'ColumnNames' property.");
                Console.WriteLine($"  Available Column Names:\r\n\t{String.Join("\r\n\t", SarifCsvColumnWriters.SupportedColumns)}");

                return;
            }

            try
            {
                string sarifFilePath = args[0];
                string csvFilePath = (args.Length > 1 ? args[1] : Path.ChangeExtension(args[0], ".csv"));
                IEnumerable<string> columnNames = (args.Length > 2 ? args[2] : ConfigurationManager.AppSettings["ColumnNames"]).Split(',').Select((value) => value.Trim());
                bool removeNewlines = bool.Parse(ValueOrDefault(ConfigurationManager.AppSettings["RemoveNewlines"], "false"));
                bool loadDeferred = bool.Parse(ValueOrDefault(ConfigurationManager.AppSettings["LoadDeferred"], "true"));

                IEnumerable<Action<WriteContext>> selectedWriters = columnNames.Select((name) => SarifCsvColumnWriters.GetWriter(name)).ToArray();

                Console.WriteLine($"Converting \"{sarifFilePath}\" to \"{csvFilePath}\"...");
                Stopwatch w = Stopwatch.StartNew();

                JsonSerializer serializer = new JsonSerializer();
                if (loadDeferred)
                {
                    serializer.ContractResolver = new SarifDeferredContractResolver();
                }

                using (CsvWriter writer = new CsvWriter(csvFilePath))
                {
                    writer.RemoveNewlines = removeNewlines;
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
            if (context.Writer.RowCountWritten % 1000 == 0) { Console.Write("."); }
        }

        private static string ValueOrDefault(string value, string defaultValue)
        {
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
}
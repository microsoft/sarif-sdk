// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Projects the results of a SARIF log into flat rows over a caller-specified, ordered column list.
    /// One row is emitted per result location by default (opt out with <c>--first-location-only</c>);
    /// results with no location emit a single row with empty location columns. Rows stream to the output
    /// as the log is walked.
    /// </summary>
    public class ProjectCommand : CommandBase
    {
        private static readonly Encoding s_utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private readonly IFileSystem _fileSystem;

        public ProjectCommand(IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        public int Run(ProjectOptions options)
        {
            try
            {
                return RunWithoutCatch(options);
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.Error.WriteLine(ex);
                return FAILURE;
            }
        }

        public int RunWithoutCatch(ProjectOptions options)
        {
            string[] columnNames = options.Columns.Select(c => c.Trim()).Where(c => c.Length > 0).ToArray();
            if (columnNames.Length == 0)
            {
                Console.Error.WriteLine("No columns were specified.");
                return FAILURE;
            }

            Func<ProjectionContext, string>[] accessors;
            try
            {
                accessors = columnNames.Select(RowProjection.GetAccessor).ToArray();
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return FAILURE;
            }

            bool toFile = !string.IsNullOrEmpty(options.OutputFilePath);
            if (toFile && !DriverUtilities.ReportWhetherOutputFileCanBeCreated(options.OutputFilePath, options.Force, _fileSystem))
            {
                return FAILURE;
            }

            SarifLog log = ReadSarifFile<SarifLog>(_fileSystem, options.InputFilePath);

            TextWriter writer = toFile
                ? new StreamWriter(_fileSystem.FileCreate(options.OutputFilePath), s_utf8NoBom)
                : Console.Out;

            try
            {
                writer.NewLine = "\n";
                ProjectLog(log, columnNames, accessors, options, writer);
            }
            finally
            {
                if (toFile) { writer.Dispose(); }
                else { writer.Flush(); }
            }

            return SUCCESS;
        }

        private static void ProjectLog(
            SarifLog log,
            string[] columnNames,
            Func<ProjectionContext, string>[] accessors,
            ProjectOptions options,
            TextWriter writer)
        {
            bool tabular = options.Format != ProjectionFormat.Ndjson;
            if (tabular && !options.NoHeader)
            {
                writer.WriteLine(FormatRow(columnNames, options.Format));
            }

            if (log.Runs == null) { return; }

            var cells = new string[accessors.Length];

            for (int runIndex = 0; runIndex < log.Runs.Count; runIndex++)
            {
                Run run = log.Runs[runIndex];
                if (run?.Results == null) { continue; }

                for (int resultIndex = 0; resultIndex < run.Results.Count; resultIndex++)
                {
                    Result result = run.Results[resultIndex];
                    var context = new ProjectionContext
                    {
                        Run = run,
                        Result = result,
                        Rule = result.GetRule(run),
                        RunIndex = runIndex,
                        ResultIndex = resultIndex,
                    };

                    foreach (PhysicalLocation location in EnumerateRowLocations(result, options.FirstLocationOnly))
                    {
                        context.Location = location;
                        EmitRow(writer, columnNames, accessors, cells, context, options.Format);
                    }
                }
            }
        }

        private static IEnumerable<PhysicalLocation> EnumerateRowLocations(Result result, bool firstLocationOnly)
        {
            List<PhysicalLocation> locations = result.Locations
                ?.Select(l => l.PhysicalLocation)
                .Where(p => p != null)
                .ToList();

            if (locations == null || locations.Count == 0)
            {
                // A result with no physical location still projects one row (empty location columns).
                yield return null;
                yield break;
            }

            if (firstLocationOnly)
            {
                yield return locations[0];
                yield break;
            }

            foreach (PhysicalLocation location in locations)
            {
                yield return location;
            }
        }

        private static void EmitRow(
            TextWriter writer,
            string[] columnNames,
            Func<ProjectionContext, string>[] accessors,
            string[] cells,
            ProjectionContext context,
            ProjectionFormat format)
        {
            for (int i = 0; i < accessors.Length; i++)
            {
                cells[i] = accessors[i](context) ?? "";
            }

            if (format == ProjectionFormat.Ndjson)
            {
                var row = new JObject();
                for (int i = 0; i < columnNames.Length; i++)
                {
                    row[columnNames[i]] = cells[i];
                }

                writer.WriteLine(row.ToString(Formatting.None));
            }
            else
            {
                writer.WriteLine(FormatRow(cells, format));
            }
        }

        private static string FormatRow(IReadOnlyList<string> cells, ProjectionFormat format)
        {
            if (format == ProjectionFormat.Tsv)
            {
                return string.Join("\t", cells.Select(EscapeTsv));
            }

            return string.Join(",", cells.Select(EscapeCsv));
        }

        private static string EscapeCsv(string value)
        {
            value ??= "";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string EscapeTsv(string value)
        {
            if (string.IsNullOrEmpty(value)) { return ""; }
            return value.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
        }
    }
}

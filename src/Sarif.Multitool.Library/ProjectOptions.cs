// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>The row format the 'project' verb emits.</summary>
    public enum ProjectionFormat
    {
        Csv,
        Tsv,
        Ndjson,
    }

    /// <summary>
    ///  Options for the 'project' command, which projects SARIF results into flat rows (CSV / TSV /
    ///  NDJSON) over a caller-specified, ordered column list.
    /// </summary>
    [Verb("project", HelpText = "Project SARIF results into flat rows (CSV/TSV/NDJSON) over a chosen, ordered column list.")]
    public class ProjectOptions
    {
        [Value(0,
            MetaName = "<inputFile>",
            HelpText = "A path to a SARIF file whose results are projected.",
            Required = true)]
        public string InputFilePath { get; set; }

        [Option(
            'c',
            "columns",
            Separator = ',',
            Required = true,
            HelpText = "Ordered, comma-separated column names (e.g. RuleId,Level,Location.Uri,Properties.security-severity). "
                + "Dynamic columns: Properties.<key>, Fingerprints.<key>, PartialFingerprints.<key>.")]
        public IEnumerable<string> Columns { get; set; }

        [Option(
            "format",
            Default = ProjectionFormat.Csv,
            HelpText = "Output row format: csv, tsv, or ndjson.")]
        public ProjectionFormat Format { get; set; }

        [Option(
            'o',
            "output",
            HelpText = "A path to the output file. Writes to stdout when omitted.",
            Required = false)]
        public string OutputFilePath { get; set; }

        [Option(
            'f',
            "force",
            Default = false,
            HelpText = "Force overwrite of output file if it exists.")]
        public bool Force { get; set; }

        [Option(
            "first-location-only",
            Default = false,
            HelpText = "Emit one row per result (its first physical location) instead of one row per location.")]
        public bool FirstLocationOnly { get; set; }

        [Option(
            "no-header",
            Default = false,
            HelpText = "Suppress the header row (csv/tsv only; ndjson never emits a header).")]
        public bool NoHeader { get; set; }
    }
}

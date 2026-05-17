// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

using Microsoft.Extensions.Options;

using ModelContextProtocol.Server;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools
{
    [McpServerToolType]
    public sealed class MergeRunTool
    {
        private readonly SarifMcpOptions _options;

        public MergeRunTool(IOptions<SarifMcpOptions> options) => this._options = options.Value;

        [McpServerTool(Name = "sarif_merge")]
        [Description(
            "Merge multiple SARIF files into a single output file. " +
            "Each input file's runs are concatenated into one SarifLog (runs are preserved as-is, " +
            "no rule/artifact re-indexing is performed).")]
        public string Merge(
            [Description("JSON array of SARIF file paths to merge")] string sarifPaths,
            [Description("Output path for the merged .sarif file")] string outputPath)
        {
            List<string>? paths = JsonSerializer.Deserialize<List<string>>(sarifPaths)
                ?? throw new ArgumentException("sarifPaths must be a JSON array of file paths.");

            if (paths.Count == 0)
            {
                throw new ArgumentException("At least one SARIF file path is required.");
            }

            var allRuns = new List<Run>();

            foreach (string path in paths)
            {
                string canonical = Path.GetFullPath(path);
                ValidatePathUnderOutputRoot(canonical);
                if (!File.Exists(canonical))
                {
                    throw new FileNotFoundException($"SARIF file not found: {canonical}");
                }

                SarifLog log = SarifLog.Load(canonical);
                if (log.Runs != null)
                {
                    allRuns.AddRange(log.Runs);
                }
            }

            var mergedLog = new SarifLog
            {
                SchemaUri = new Uri(SarifUtilities.SarifSchemaUri),
                Version = SarifVersion.Current,
                Runs = allRuns
            };

            string canonicalOutput = Path.GetFullPath(outputPath);
            ValidatePathUnderOutputRoot(canonicalOutput);

            SarifLogWriter.Save(mergedLog, canonicalOutput);

            return JsonSerializer.Serialize(new
            {
                output_path = canonicalOutput,
                files_merged = paths.Count,
                total_runs = allRuns.Count,
                status = "merged"
            });
        }

        private void ValidatePathUnderOutputRoot(string canonicalPath)
        {
            if (string.IsNullOrEmpty(this._options.OutputRoot))
            {
                return;
            }

            string canonicalRoot = Path.GetFullPath(this._options.OutputRoot)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!canonicalPath.StartsWith(canonicalRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Path '{canonicalPath}' is outside the configured OutputRoot '{this._options.OutputRoot}'.");
            }
        }
    }
}

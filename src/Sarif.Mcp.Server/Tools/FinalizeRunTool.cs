// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

using ModelContextProtocol.Server;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools
{
    [McpServerToolType]
    public sealed class FinalizeRunTool
    {
        private readonly SarifRunStore _store;

        public FinalizeRunTool(SarifRunStore store) => this._store = store;

        [McpServerTool(Name = "sarif_finalize")]
        [Description("Close a SARIF run: writes the final .sarif file to disk.")]
        public string Finalize(
            [Description("Run GUID from sarif_create_run")] string runGuid,
            [Description("Repo-wide remediation handoff notes")] string? handoffNotes = null)
        {
            SarifRunContext ctx = this._store.Get(runGuid);

            // Atomically: apply handoff, close invocations, mark finalized.
            // After this, ctx.Run is effectively immutable — safe to serialize
            // without cloning because all mutation paths now throw.
            (int resultCount, int ruleCount) = ctx.FinalizeWithMetadata(handoffNotes);

            var log = new SarifLog
            {
                SchemaUri = new Uri(SarifUtilities.SarifSchemaUri),
                Version = SarifVersion.Current,
                Runs = new List<Run> { ctx.Run }
            };
            string outputPath = ctx.OutputPath;

            SarifLogWriter.Save(log, outputPath);

            // Clean up the .wip placeholder if one was created at run start.
            string wipPath = outputPath + ".wip";
            if (File.Exists(wipPath))
            {
                File.Delete(wipPath);
            }

            this._store.TryRemove(runGuid, out _);

            return JsonSerializer.Serialize(new
            {
                full_path = outputPath,
                result_count = resultCount,
                rule_count = ruleCount,
                status = "finalized"
            });
        }
    }
}

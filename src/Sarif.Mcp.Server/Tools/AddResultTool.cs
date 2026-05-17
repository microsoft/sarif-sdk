// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text.Json;

using ModelContextProtocol.Server;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools
{
    [McpServerToolType]
    public sealed class AddResultTool
    {
        private readonly SarifRunStore _store;
        private readonly SarifEnricher _enricher = new();

        public AddResultTool(SarifRunStore store) => this._store = store;

        [McpServerTool(Name = "sarif_add_result")]
        [Description(
            "Add a security finding to a SARIF run. The server auto-enriches with " +
            "code snippets, context regions, char offsets, artifact hashes, and rule indices. " +
            "Validates AI profile vocabulary and all-or-nothing consistency.")]
        public string AddResult(
            [Description("Run GUID from sarif_create_run")] string runGuid,
            [Description("Rule ID in CWE-NNN/sub-id format (e.g., CWE-502/binaryformatter-deserialization)")] string ruleId,
            [Description("One-sentence finding summary")] string messageText,
            [Description("Structured markdown finding report")] string messageMarkdown,
            [Description("Human-readable rule name")] string? ruleName = null,
            [Description("Rule description")] string? ruleDescription = null,
            [Description("Finding severity: error, warning, or note")] string level = "warning",
            [Description("Confidence rank 0.0\u2013100.0")] double? rank = null,
            [Description("Source file path (relative to sourceRoot)")] string? uri = null,
            [Description("Start line (1-based, required when uri is provided)")] int? startLine = null,
            [Description("Start column (1-based)")] int? startColumn = null,
            [Description("End line (1-based)")] int? endLine = null,
            [Description("End column (1-based)")] int? endColumn = null,
            [Description("Exploitability: demonstrated, poc, or theoretical")] string? exploitability = null,
            [Description("Attacker position (e.g., unauthenticated-network, local-host)")] string? attackerPosition = null,
            [Description("Remediation handoff notes for downstream agents")] string? handoff = null)
        {
            SarifRunContext ctx = this._store.Get(runGuid);

            var request = new AddResultRequest
            {
                RuleId = ruleId,
                RuleName = ruleName,
                RuleDescription = ruleDescription,
                Level = level,
                Rank = rank,
                MessageText = messageText,
                MessageMarkdown = messageMarkdown,
                Exploitability = exploitability,
                AttackerPosition = attackerPosition,
                Handoff = handoff
            };

            if (uri != null)
            {
                request.Location = new LocationRequest
                {
                    Uri = uri,
                    StartLine = startLine,
                    StartColumn = startColumn,
                    EndLine = endLine,
                    EndColumn = endColumn
                };
            }

            EnrichmentResult result = this._enricher.Enrich(ctx, request);

            return JsonSerializer.Serialize(new
            {
                result_guid = result.ResultGuid,
                result_index = result.ResultIndex,
                rule_registered = result.RuleRegistered,
                warnings = result.Warnings,
                status = "added"
            });
        }
    }
}

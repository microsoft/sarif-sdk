// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Text.Json;

using ModelContextProtocol.Server;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools
{
    [McpServerToolType]
    public sealed class StartInvocationTool
    {
        private readonly SarifRunStore _store;

        public StartInvocationTool(SarifRunStore store) => this._store = store;

        [McpServerTool(Name = "sarif_start_invocation")]
        [Description(
            "Begin a new invocation within a SARIF run. Each pipeline agent (detect, merge, " +
            "triage) should start its own invocation to capture its execution narrative " +
            "independently. Returns an invocation_index for use with sarif_add_notification " +
            "and sarif_end_invocation.")]
        public string StartInvocation(
            [Description("Run GUID from sarif_create_run")] string runGuid,
            [Description("Agent or tool name (e.g., 'detect-agent', 'merge-agent')")] string agentName,
            [Description("Human-readable description of what this invocation does")] string description,
            [Description("Machine name or execution environment")] string? machine = null,
            [Description("Account under which the invocation ran")] string? account = null)
        {
            SarifRunContext ctx = this._store.Get(runGuid);

            var invocation = new Invocation
            {
                StartTimeUtc = DateTime.UtcNow,
                ExecutionSuccessful = true
            };

            invocation.SetProperty("ai/agentName", agentName);

            if (description != null)
            {
                invocation.SetProperty("ai/description", description);
            }

            if (machine != null)
            {
                invocation.Machine = machine;
            }

            if (account != null)
            {
                invocation.Account = account;
            }

            int index = ctx.AddInvocation(invocation);

            return JsonSerializer.Serialize(new
            {
                invocationIndex = index,
                agentName,
                startedAt = invocation.StartTimeUtc,
                status = "started"
            });
        }
    }
}

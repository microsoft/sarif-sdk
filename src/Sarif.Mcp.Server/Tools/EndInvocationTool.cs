// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text.Json;

using ModelContextProtocol.Server;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools
{
    [McpServerToolType]
    public sealed class EndInvocationTool
    {
        private readonly SarifRunStore _store;

        public EndInvocationTool(SarifRunStore store) => this._store = store;

        [McpServerTool(Name = "sarif_end_invocation")]
        [Description(
            "Close a previously started invocation. Sets end time, execution success/failure, " +
            "and an optional exit code. Call this when the agent finishes its work.")]
        public string EndInvocation(
            [Description("Run GUID from sarif_create_run")] string runGuid,
            [Description("Invocation index from sarif_start_invocation")] int invocationIndex,
            [Description("Whether the invocation completed successfully")] bool success = true,
            [Description("Process exit code (0 = success)")] int? exitCode = null,
            [Description("Summary of what happened during this invocation")] string summary = "")
        {
            SarifRunContext ctx = this._store.Get(runGuid);

            ctx.CloseInvocation(invocationIndex, success, exitCode, summary);

            return JsonSerializer.Serialize(new
            {
                invocationIndex,
                success,
                status = "closed"
            });
        }
    }
}

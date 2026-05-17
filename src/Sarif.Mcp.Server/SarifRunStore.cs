// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// Thread-safe store for active SARIF run contexts, keyed by run GUID
    /// (matches <c>run.automationDetails.guid</c>). Registered as a singleton in DI.
    /// </summary>
    public sealed class SarifRunStore
    {
        private readonly ConcurrentDictionary<Guid, SarifRunContext> _runs = new();

        public SarifRunContext Create(Guid runId, SarifRunContext context)
        {
            if (!this._runs.TryAdd(runId, context))
            {
                throw new InvalidOperationException($"Run '{runId}' already exists.");
            }

            return context;
        }

        public SarifRunContext Get(string runIdString)
        {
            if (!Guid.TryParse(runIdString, out Guid runId))
            {
                throw new ArgumentException($"Invalid run_id '{runIdString}'. Expected a GUID.");
            }

            if (!this._runs.TryGetValue(runId, out SarifRunContext? ctx))
            {
                throw new InvalidOperationException($"Run '{runId}' not found. Call sarif_create_run first.");
            }

            return ctx;
        }

        public bool TryRemove(string runIdString, out SarifRunContext? context)
        {
            if (Guid.TryParse(runIdString, out Guid runId))
            {
                return this._runs.TryRemove(runId, out context);
            }

            context = null;
            return false;
        }
    }
}

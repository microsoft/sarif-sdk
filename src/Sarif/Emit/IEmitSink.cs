// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Append-only destination for the SARIF event stream that backs incremental authoring. A sink
    /// hides where events live — a <c>*.wip.jsonl</c> file on disk, an in-memory list, or a per-run
    /// shard — so a <see cref="RunEmitContext"/> can validate-and-append without owning that policy.
    /// </summary>
    /// <remarks>
    /// <see cref="ReadAll"/> returns every event appended so far in append order; it backs the
    /// descriptor duplicate scan and run resolution. Implementations that stream from disk MUST
    /// tolerate being read while the sink is still open for append.
    /// </remarks>
    public interface IEmitSink : IDisposable
    {
        /// <summary>Appends one event of the given kind with the given payload.</summary>
        void Append(string kind, JToken payload);

        /// <summary>Enumerates every event appended to the sink so far, in append order.</summary>
        IEnumerable<SarifEvent> ReadAll();
    }
}

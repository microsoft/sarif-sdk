// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// An <see cref="IEmitSink"/> that accumulates events in memory. Lets a .NET producer drive a
    /// <see cref="RunEmitContext"/> with no file handle and no process spawn; the accumulated events
    /// are resolved into a <see cref="SarifLog"/> at finalize. Trades the file sink's crash-safety
    /// and resumability for speed on small/short runs.
    /// </summary>
    public sealed class InMemoryEmitSink : IEmitSink
    {
        private readonly List<SarifEvent> _events = new List<SarifEvent>();

        /// <summary>The events appended so far, in append order.</summary>
        public IReadOnlyList<SarifEvent> Events => _events;

        public void Append(string kind, JToken payload)
        {
            _events.Add(new SarifEvent
            {
                Version = SarifEventKinds.CurrentSchemaVersion,
                Kind = kind,
                Payload = payload ?? new JObject(),
            });
        }

        public IEnumerable<SarifEvent> ReadAll() => _events;

        public void Dispose()
        {
        }
    }
}

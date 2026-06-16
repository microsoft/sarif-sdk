// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// An <see cref="IEmitSink"/> backed by a SARIF event-log file (<c>*.wip.jsonl</c>). Appends go
    /// through a single lazily-opened <see cref="SarifEventLogWriter"/>; <see cref="ReadAll"/> opens
    /// a fresh forward-only reader each call. This is the durable, crash-safe, resumable sink and the
    /// shape the CLI emit verbs use.
    /// </summary>
    public sealed class FileEmitSink : IEmitSink
    {
        private readonly string _path;
        private SarifEventLogWriter _writer;

        public FileEmitSink(string path)
        {
            _path = path;
        }

        public void Append(string kind, JToken payload)
        {
            // Open on first append so a read-only context (e.g. a duplicate scan that never appends)
            // never takes the exclusive append handle.
            _writer ??= new SarifEventLogWriter(_path);
            _writer.Append(kind, payload);
        }

        public IEnumerable<SarifEvent> ReadAll() => new SarifEventLogReader().Read(_path);

        public void Dispose() => _writer?.Dispose();
    }
}

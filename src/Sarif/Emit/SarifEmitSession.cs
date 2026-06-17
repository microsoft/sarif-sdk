// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// In-memory authoring surface for a multi-run SARIF log. A session owns the log envelope
    /// (<c>$schema</c>, <c>version</c>) and an ordered set of <see cref="RunEmitContext"/> children;
    /// each run is a self-contained shard with its own <see cref="IEmitSink"/> and its own index
    /// space. Finalization resolves each run independently and assembles them into a single
    /// <see cref="SarifLog"/>.
    /// </summary>
    /// <remarks>
    /// <para>The run is the unit of isolation, which is exactly what SARIF's object model already
    /// guarantees: every index a result carries (<c>ruleIndex</c>, an artifact index, a taxon index)
    /// is scoped to its own run, never across runs. Because the emit pipeline keeps results
    /// index-free until resolution, two runs share no mutable state — so a producer can author them
    /// concurrently, one context per scan target, and the session reconciles indices per run at
    /// finalize.</para>
    /// <para>Two finalize paths trade memory against speed. <see cref="Finalize"/> resolves the runs
    /// in parallel and returns the assembled <see cref="SarifLog"/> in memory (peak memory holds every
    /// run). <see cref="FinalizeToFile"/> streams the runs to disk one at a time inside a single open
    /// <c>runs</c> array — a byte-splice of compact, already-resolved run objects — so peak memory
    /// holds only one run plus its run-level state. The splice is safe because each run is serialized
    /// from a typed <see cref="Run"/> produced by resolution, never hand-assembled.</para>
    /// </remarks>
    public sealed class SarifEmitSession : IDisposable
    {
        private static readonly Encoding s_utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private readonly Func<int, IEmitSink> _sinkFactory;
        private readonly List<IEmitSink> _sinks = new List<IEmitSink>();
        private readonly List<RunEmitContext> _contexts = new List<RunEmitContext>();

        /// <summary>
        /// Creates a session. <paramref name="sinkFactory"/> supplies the backing sink for each run,
        /// keyed by zero-based run index; when omitted, every run is backed by an
        /// <see cref="InMemoryEmitSink"/>. Pass a factory that returns per-run <see cref="FileEmitSink"/>
        /// shards to bound memory or to checkpoint long multi-run scans.
        /// </summary>
        public SarifEmitSession(Func<int, IEmitSink> sinkFactory = null)
        {
            _sinkFactory = sinkFactory ?? (_ => new InMemoryEmitSink());
        }

        /// <summary>The run contexts, in declared order. The final <c>runs[]</c> follows this order.</summary>
        public IReadOnlyList<RunEmitContext> Runs => _contexts;

        /// <summary>
        /// Adds a run and returns its context. When <paramref name="runHeader"/> is supplied it is
        /// appended as the run skeleton before the context is returned.
        /// </summary>
        public RunEmitContext AddRun(JObject runHeader = null)
        {
            IEmitSink sink = _sinkFactory(_contexts.Count);
            var context = new RunEmitContext(sink);
            if (runHeader != null) { context.SetRunHeader(runHeader); }

            _sinks.Add(sink);
            _contexts.Add(context);
            return context;
        }

        /// <summary>
        /// Resolves every run (in parallel — the runs share no state) and assembles them into a
        /// single in-memory <see cref="SarifLog"/> in declared order.
        /// </summary>
        public SarifLog Finalize()
        {
            var runs = new Run[_contexts.Count];

            if (_contexts.Count <= 1)
            {
                for (int i = 0; i < _contexts.Count; i++)
                {
                    runs[i] = ResolveRun(_contexts[i]);
                }
            }
            else
            {
                Parallel.For(0, _contexts.Count, i => runs[i] = ResolveRun(_contexts[i]));
            }

            return new SarifLog { Runs = new List<Run>(runs) };
        }

        /// <summary>
        /// Streams the runs to <paramref name="destinationPath"/> one at a time inside a single open
        /// <c>runs</c> array, holding only one resolved run in memory at a time. The write is atomic.
        /// </summary>
        /// <param name="destinationPath">The final SARIF file path.</param>
        /// <param name="prettyPrint">When <c>true</c>, indents the output; otherwise emits compact JSON.</param>
        public void FinalizeToFile(string destinationPath, bool prettyPrint = false)
        {
            SarifVersion sarifVersion = SarifVersion.Current;

            AtomicSarifWriter.Write(destinationPath, stream =>
            {
                using var writer = new StreamWriter(stream, s_utf8NoBom);
                using var jsonWriter = new JsonTextWriter(writer)
                {
                    Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                };

                JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });

                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("$schema");
                jsonWriter.WriteValue(sarifVersion.ConvertToSchemaUri().OriginalString);
                jsonWriter.WritePropertyName("version");
                jsonWriter.WriteValue(sarifVersion.ConvertToText());
                jsonWriter.WritePropertyName("runs");
                jsonWriter.WriteStartArray();

                // Resolve one run at a time so peak memory holds a single run, not the whole log.
                foreach (RunEmitContext context in _contexts)
                {
                    serializer.Serialize(jsonWriter, ResolveRun(context));
                }

                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
            });
        }

        public void Dispose()
        {
            foreach (IEmitSink sink in _sinks)
            {
                sink.Dispose();
            }
        }

        private static Run ResolveRun(RunEmitContext context) => context.ResolveToLog().Runs[0];
    }
}

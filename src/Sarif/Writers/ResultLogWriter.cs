// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    /// Writes SARIF JSON without exposing the underlying JSON serializer in its public API.
    /// </summary>
    public sealed class ResultLogWriter : IResultLogWriter, IDisposable
    {
        private readonly JsonTextWriter _jsonWriter;
        private readonly ResultLogJsonWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultLogWriter"/> class.
        /// </summary>
        /// <param name="textWriter">
        /// The text writer. This class does not take ownership of the writer; the caller is responsible
        /// for disposing it.
        /// </param>
        /// <param name="indented">Whether to indent the generated JSON.</param>
        public ResultLogWriter(TextWriter textWriter, bool indented = false)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }

            _jsonWriter = new JsonTextWriter(textWriter)
            {
                CloseOutput = false,
                Formatting = indented ? Formatting.Indented : Formatting.None
            };
            _writer = new ResultLogJsonWriter(_jsonWriter);
        }

        public void Initialize(Run run) => _writer.Initialize(run);

        public void WriteArtifacts(IList<Artifact> artifacts) => _writer.WriteArtifacts(artifacts);

        public void WriteLogicalLocations(IList<LogicalLocation> logicalLocations) =>
            _writer.WriteLogicalLocations(logicalLocations);

        public void WriteTool(Tool tool) => _writer.WriteTool(tool);

        public void OpenResults() => _writer.OpenResults();

        public void WriteResult(Result result) => _writer.WriteResult(result);

        public void CloseResults() => _writer.CloseResults();

        public void WriteResults(IEnumerable<Result> results) => _writer.WriteResults(results);

        public void WriteInvocations(IEnumerable<Invocation> invocations) => _writer.WriteInvocations(invocations);

        public void WriteLog(SarifLog log) => _writer.WriteLog(log);

        public void Dispose()
        {
            try
            {
                _writer.Dispose();
            }
            finally
            {
                ((IDisposable)_jsonWriter).Dispose();
            }
        }
    }
}

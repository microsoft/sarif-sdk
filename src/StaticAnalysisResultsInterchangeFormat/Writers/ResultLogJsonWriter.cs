// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.CodeAnalysis.Sarif.DataContracts;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>An implementation of <see cref="IResultLogWriter"/> that writes the results as JSON to a
    /// <see cref="TextWriter"/>.</summary>
    /// <seealso cref="T:Microsoft.CodeAnalysis.Sarif.IResultLogWriter"/>
    public sealed class ResultLogJsonWriter : IResultLogWriter, IDisposable
    {
        private enum State
        {
            Initial,
            WritingResults,
            Disposed
        }

        private readonly JsonWriter _jsonWriter;
        private readonly JsonSerializer _serializer;
        private State _writeState;

        /// <summary>Initializes a new instance of the <see cref="ResultLogJsonWriter"/> class.</summary>
        /// <param name="jsonWriter">The JSON writer. This class does not take ownership of the JSON
        /// writer; the caller is responsible for destroying it.</param>
        public ResultLogJsonWriter(JsonWriter jsonWriter)
        {
            _jsonWriter = jsonWriter;
            _serializer = new JsonSerializer();
            _serializer.Converters.Add(new StringEnumConverter());
        }

        /// <summary>Writes a tool information entry to the log. This must be the first entry written into
        /// a log, and it may be written at most once.</summary>
        /// <exception cref="IOException">A file IO error occured. Clients implementing
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the tool info block has already been
        /// written.</exception>
        /// <param name="info">The tool information to write.</param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.Sarif.IsarifWriter.WriteToolInfo(ToolInfo)"/>
        public void WriteToolAndRunInfo(ToolInfo toolInfo, RunInfo runInfo)
        {
            if (toolInfo == null)
            {
                throw new ArgumentNullException("toolInfo");
            }


            this.EnsureNotDisposed();
            if (_writeState == State.WritingResults)
            {
                throw new InvalidOperationException(SarifResources.ToolInfoAlreadyWritten);
            }

            Debug.Assert(_writeState == State.Initial);

            _jsonWriter.WriteStartObject(); // Begin: resultLog
            _jsonWriter.WritePropertyName("version");
            _jsonWriter.WriteValue("0.4");

            _jsonWriter.WritePropertyName("runLogs");
            _jsonWriter.WriteStartArray(); // Begin: runLogs

            _jsonWriter.WriteStartObject(); // Begin: runLog

            _jsonWriter.WritePropertyName("toolInfo");
            _serializer.Serialize(_jsonWriter, toolInfo, typeof(ToolInfo));

            if (runInfo != null)
            {
                _jsonWriter.WritePropertyName("runInfo");
                _serializer.Serialize(_jsonWriter, runInfo, typeof(RunInfo));
            }

            _jsonWriter.WritePropertyName("results");
            _jsonWriter.WriteStartArray(); // Begin: results
            _writeState = State.WritingResults;
        }

        /// <summary>Writes a result to the log. The log must have tool info written first by calling
        /// <see cref="M:WriteToolInfo" />.</summary>
        /// <remarks>This function makes a copy of the data stored in <paramref name="result"/>; if a
        /// client wishes to reuse the result instance to avoid allocations they can do so. (This function
        /// may invoke an internal copy of the result or serialize it in place to disk, etc.)</remarks>
        /// <exception cref="IOException">A file IO error occured. Clients implementing
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the tool info is not yet written.</exception>
        /// <param name="result">The result to write.</param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.Sarif.IsarifWriter.WriteResult(Result)"/>
        public void WriteResult(Result result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            this.EnsureNotDisposed();
            if (_writeState == State.Initial)
            {
                throw new InvalidOperationException(SarifResources.CannotWriteResultToolInfoMissing);
            }

            Debug.Assert(_writeState == State.WritingResults);
            _serializer.Serialize(_jsonWriter, result, typeof(Result));
        }

        /// <summary>Writes the log footer and closes the underlying <see cref="JsonWriter"/>.</summary>
        /// <seealso cref="M:System.IDisposable.Dispose()"/>
        public void Dispose()
        {
            if (_writeState == State.Disposed)
            {
                return;
            }

            if (_writeState == State.Initial)
            {
                // Log incomplete. No data should have been written at this point.
            }
            else
            {
                Debug.Assert(_writeState == State.WritingResults);

                // Log complete. Close the result array and write the end object.
                _jsonWriter.WriteEndArray();  // End: results
                _jsonWriter.WriteEndObject(); // End: runLog
                _jsonWriter.WriteEndArray();  // End: runLogs
                _jsonWriter.WriteEndObject(); // End: resultsLog
            }

            _writeState = State.Disposed;
        }

        private void EnsureNotDisposed()
        {
            if (_writeState == State.Disposed)
            {
                throw new ObjectDisposedException("ResultLogJsonWriter");
            }
        }
    }
}

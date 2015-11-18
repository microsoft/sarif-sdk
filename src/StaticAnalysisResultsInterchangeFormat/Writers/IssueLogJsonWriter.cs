// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Writers
{
    /// <summary>An implementation of <see cref="IIssueLogWriter"/> that writes the results as JSON to a
    /// <see cref="TextWriter"/>.</summary>
    /// <seealso cref="T:Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.IIssueLogWriter"/>
    public sealed class IssueLogJsonWriter : IIssueLogWriter, IDisposable
    {
        private enum State
        {
            Initial,
            WritingIssues,
            Disposed
        }

        private readonly JsonWriter _jsonWriter;
        private readonly JsonSerializer _serializer;
        private State _writeState;

        /// <summary>Initializes a new instance of the <see cref="IssueLogJsonWriter"/> class.</summary>
        /// <param name="jsonWriter">The JSON writer. This class does not take ownership of the JSON
        /// writer; the caller is responsible for destroying it.</param>
        public IssueLogJsonWriter(JsonWriter jsonWriter)
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
        /// <seealso cref="M:Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.IsarifWriter.WriteToolInfo(ToolInfo)"/>
        public void WriteToolAndRunInfo(ToolInfo toolInfo, RunInfo runInfo)
        {
            if (toolInfo == null)
            {
                throw new ArgumentNullException("toolInfo");
            }

            if (runInfo == null)
            {
                throw new ArgumentNullException("runInfo");
            }

            this.EnsureNotDisposed();
            if (_writeState == State.WritingIssues)
            {
                throw new InvalidOperationException(SarifResources.ToolInfoAlreadyWritten);
            }

            Debug.Assert(_writeState == State.Initial);

            _jsonWriter.WriteStartObject(); // Begin: issueLog
            _jsonWriter.WritePropertyName("version");
            _jsonWriter.WriteValue("0.3.0.0");

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

            _jsonWriter.WritePropertyName("issues");
            _jsonWriter.WriteStartArray(); // Begin: issues
            _writeState = State.WritingIssues;
        }

        /// <summary>Writes an issue to the log. The log must have tool info written first by calling
        /// <see cref="M:WriteToolInfo" />.</summary>
        /// <remarks>This function makes a copy of the data stored in <paramref name="issue"/>; if a
        /// client wishes to reuse the issue instance to avoid allocations they can do so. (This function
        /// may invoke an internal copy of the issue or serialize it in place to disk, etc.)</remarks>
        /// <exception cref="IOException">A file IO error occured. Clients implementing
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the tool info is not yet written.</exception>
        /// <param name="issue">The issue to write.</param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.IsarifWriter.WriteIssue(Issue)"/>
        public void WriteIssue(Issue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            this.EnsureNotDisposed();
            if (_writeState == State.Initial)
            {
                throw new InvalidOperationException(SarifResources.CannotWriteIssueToolInfoMissing);
            }

            Debug.Assert(_writeState == State.WritingIssues);
            _serializer.Serialize(_jsonWriter, issue, typeof(Issue));
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
                Debug.Assert(_writeState == State.WritingIssues);

                // Log complete. Close the issue array and write the end object.
                _jsonWriter.WriteEndArray();  // End: issues
                _jsonWriter.WriteEndObject(); // End: runLog
                _jsonWriter.WriteEndArray();  // End: runLogs
                _jsonWriter.WriteEndObject(); // End: issueLog
            }

            _writeState = State.Disposed;
        }

        private void EnsureNotDisposed()
        {
            if (_writeState == State.Disposed)
            {
                throw new ObjectDisposedException("IssueLogJsonWriter");
            }
        }
    }
}

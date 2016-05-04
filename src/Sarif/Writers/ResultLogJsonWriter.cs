// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Readers;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>An implementation of <see cref="IResultLogWriter"/> that writes the results as JSON to a
    /// <see cref="TextWriter"/>.</summary>
    /// <seealso cref="T:Microsoft.CodeAnalysis.Sarif.IResultLogWriter"/>
    public sealed class ResultLogJsonWriter : IResultLogWriter, IDisposable
    {
        [Flags]
        private enum Conditions
        {
            None = 0x0,
            Initialized = 0x1,
            ToolWritten = 0x2,
            RulesWritten = 0x4,
            FilesWritten = 0x8,
            ResultsInitialized = 0x10,
            ResultsClosed = 0x20,
            InvocationWritten = 0x40,
            LogicalLocationsWritten = 0x80,
            ToolNotificationsWritten = 0x100,
            ConfigurationNotificationsWritten = 0x200,
            Disposed = 0x40000000
        }

        private Conditions _writeConditions;
        private readonly JsonWriter _jsonWriter;
        private readonly JsonSerializer _serializer;

        /// <summary>Initializes a new instance of the <see cref="ResultLogJsonWriter"/> class.</summary>
        /// <param name="jsonWriter">The JSON writer. This class does not take ownership of the JSON
        /// writer; the caller is responsible for destroying it.</param>
        public ResultLogJsonWriter(JsonWriter jsonWriter)
        {
            _jsonWriter = jsonWriter;
            _serializer = new JsonSerializer();
            _serializer.ContractResolver = SarifContractResolver.Instance;
        }

        public void Initialize()
        {
            this.EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.Initialized);

            SarifVersion sarifVersion = SarifVersion.OneZeroZeroBetaFour;

            _jsonWriter.WriteStartObject(); // Begin: sarifLog
            _jsonWriter.WritePropertyName("$schema");
            _jsonWriter.WriteValue(sarifVersion.ConvertToSchemaUri().OriginalString);
            _jsonWriter.WritePropertyName("version");
            _jsonWriter.WriteValue(sarifVersion.ConvertToText());

            _jsonWriter.WritePropertyName("runs");
            _jsonWriter.WriteStartArray(); // Begin: runs

            _jsonWriter.WriteStartObject(); // Begin: run

            _writeConditions |= Conditions.Initialized;
        }

        /// <summary>Writes a tool information entry to the log. This must be the first entry written into
        /// a log, and it may be written at most once.</summary>
        /// <exception cref="IOException">A file IO error occured. Clients implementing
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the tool info block has already been
        /// written.</exception>
        /// <param name="info">The tool information to write.</param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.Sarif.IsarifWriter.WriteTool(Tool)"/>
        public void WriteTool(Tool tool)
        {
            if (tool == null)
            {
                throw new ArgumentNullException("tool");
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ToolWritten);

            _jsonWriter.WritePropertyName("tool");
            _serializer.Serialize(_jsonWriter, tool, typeof(Tool));

            _writeConditions |= Conditions.ToolWritten;
        }

        /// <summary>
        /// Write information about scanned files to the log. This information may appear
        /// after the results, as the full list of scanned files might not be known until
        /// all results have been generated.
        /// </summary>
        /// <param name="fileDictionary">
        /// A dictionary whose keys are the URIs of scanned files and whose values provide
        /// information about those files.
        /// </param>
        public void WriteFiles(IDictionary<string, IList<FileData>> fileDictionary)
        {
            if (fileDictionary == null)
            {
                throw new ArgumentNullException(nameof(fileDictionary));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.FilesWritten);

            _jsonWriter.WritePropertyName("files");
            _serializer.Serialize(_jsonWriter, fileDictionary, typeof(Dictionary<Uri, IList<FileData>>));

            _writeConditions |= Conditions.FilesWritten;
        }

        /// <summary>
        /// Write information about the logical locations where results were produced to
        /// the log. This information may appear after the results, as the full list of
        /// logical locations will not be known until all results have been generated.
        /// </summary>
        /// <param name="logicalLocationDictionary">
        /// A dictionary whose keys are strings specifying a logical location and
        /// whose values provide information about each component of the logical location.
        /// </param>
        public void WriteLogicalLocations(IDictionary<string, IList<LogicalLocationComponent>> logicalLocationDictionary)
        {
            if (logicalLocationDictionary == null)
            {
                throw new ArgumentNullException(nameof(logicalLocationDictionary));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.LogicalLocationsWritten);

            _jsonWriter.WritePropertyName("logicalLocations");
            _serializer.Serialize(_jsonWriter, logicalLocationDictionary, typeof(Dictionary<string, IList<LogicalLocationComponent>>));

            _writeConditions |= Conditions.LogicalLocationsWritten;
        }

        public void WriteRules(IDictionary<string, IRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.RulesWritten);

            _jsonWriter.WritePropertyName("rules");
            _serializer.Serialize(_jsonWriter, rules, typeof(Dictionary<string, IRule>));

            _writeConditions |= Conditions.RulesWritten;
        }

        public void OpenResults()
        {
            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ResultsClosed);

            _jsonWriter.WritePropertyName("results");
            _jsonWriter.WriteStartArray(); // Begin: results
            _writeConditions = Conditions.ResultsInitialized;
        }

        /// <summary>
        /// Writes a result to the log. 
        /// </summary>
        /// <remarks>
        /// This function makes a copy of the data stored in <paramref name="result"/>; if a
        /// client wishes to reuse the result instance to avoid allocations they can do so. (This function
        /// may invoke an internal copy of the result or serialize it in place to disk, etc.)
        /// </remarks>
        /// <exception cref="IOException">
        /// A file IO error occured. Clients implementing
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the tool info is not yet written.
        /// </exception>
        /// <param name="result">
        /// The result to write.
        /// </param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.Sarif.IsarifWriter.WriteResult(Result)"/>
        public void WriteResult(Result result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ResultsClosed);

            if ((_writeConditions & Conditions.ResultsInitialized) != Conditions.ResultsInitialized)
            {
                OpenResults();
            }

            _serializer.Serialize(_jsonWriter, result, typeof(Result));
        }

        /// <summary>
        /// Writes a set of results to the log.
        /// </summary>
        /// <remarks>
        /// This function makes a copy of the data stored in <paramref name="results"/>; if a
        /// client wishes to reuse the result instance to avoid allocations they can do so. (This function
        /// may invoke an internal copy of the result or serialize it in place to disk, etc.)
        /// </remarks>
        /// <exception cref="IOException">
        /// A file IO error occured. Clients implementing
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the tool info is not yet written.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="result"/> is null.
        /// </exception>
        ///  <param name="results">
        ///  The results to write.
        ///  </param>
        public void WriteResults(IEnumerable<Result> results)
        {
            foreach (Result result in results)
            {
                WriteResult(result);
            }
        }

        public void CloseResults()
        {
            EnsureStateNotAlreadySet(Conditions.Disposed);

            // We allow some resilience for writers that stream individual results to
            // the log without explicit opening/closing the results object
            if ((_writeConditions & Conditions.ResultsInitialized) != Conditions.ResultsInitialized ||
                (_writeConditions & Conditions.ResultsClosed) == Conditions.ResultsClosed)
            {
                return;
            }

            _jsonWriter.WriteEndArray();
            _writeConditions |= Conditions.ResultsClosed;
        }

        public void WriteInvocation(Invocation invocation)
        {
            if (invocation == null)
            {
                throw new ArgumentNullException(nameof(invocation));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.InvocationWritten);

            _jsonWriter.WritePropertyName("invocation");
            _serializer.Serialize(_jsonWriter, invocation, typeof(Invocation));

            _writeConditions |= Conditions.InvocationWritten;
        }

        public void WriteToolNotifications(IEnumerable<Notification> notifications)
        {
            if (notifications == null)
            {
                throw new ArgumentNullException(nameof(notifications));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ToolNotificationsWritten);

            _jsonWriter.WritePropertyName("toolNotifications");
            _serializer.Serialize(_jsonWriter, notifications, notifications.GetType());

            _writeConditions |= Conditions.ToolNotificationsWritten;
        }

        public void WriteConfigurationNotifications(IEnumerable<Notification> notifications)
        {
            if (notifications == null)
            {
                throw new ArgumentNullException(nameof(notifications));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ConfigurationNotificationsWritten);

            _jsonWriter.WritePropertyName("configurationNotifications");
            _serializer.Serialize(_jsonWriter, notifications, notifications.GetType());

            _writeConditions |= Conditions.ConfigurationNotificationsWritten;
        }

        /// <summary>Writes the log footer and closes the underlying <see cref="JsonWriter"/>.</summary>
        /// <seealso cref="M:System.IDisposable.Dispose()"/>
        public void Dispose()
        {
            EnsureInitialized();

            if ((_writeConditions & Conditions.Disposed) == Conditions.Disposed)
            {
                return;
            }

            if (_writeConditions == Conditions.Initialized)
            {
                // Log incomplete. No data should have been written at this point.
            }
            else
            {
                if ((_writeConditions & Conditions.ResultsInitialized) == Conditions.ResultsInitialized &&
                    (_writeConditions & Conditions.ResultsClosed) != Conditions.ResultsClosed)
                {
                    CloseResults();
                }

                // Log complete. Write the end object.

                _jsonWriter.WriteEndObject(); // End: run
                _jsonWriter.WriteEndArray();  // End: runs
                _jsonWriter.WriteEndObject(); // End: sarifLog
            }

            _writeConditions |= Conditions.Disposed;
        }

        private void EnsureInitialized()
        {
            if (_writeConditions == Conditions.None)
            {
                Initialize();
            }
        }

        private void EnsureStateNotAlreadySet(Conditions invalidConditions)
        {
            Conditions observedInvalidConditions = _writeConditions & invalidConditions;
            if (observedInvalidConditions != Conditions.None)
            {
                // 	InvalidState	One or more invalid states were detected during serialization: {0}	
                throw new InvalidOperationException(string.Format(SdkResources.InvalidState, observedInvalidConditions));
            }
        }

        private void EnsureResultsArrayIsNotOpen()
        {
            // This method ensures that no in-progress serialization
            // underway. Currently, only the results serialization
            // is a multi-step process
            if ((_writeConditions & Conditions.ResultsInitialized) == Conditions.ResultsInitialized &&
                (_writeConditions & Conditions.ResultsClosed) != Conditions.ResultsClosed)
            {
                throw new InvalidOperationException(SdkResources.ResultsSerializationNotComplete);
            }
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Readers;

using Newtonsoft.Json;
using System.Globalization;

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
            None = 0x00,
            RunInitialized = 0x001,
            RulesWritten = 0x002,
            FilesWritten = 0x004,
            InvocationsWritten = 0x008,
            ResultsInitialized = 0x010,
            ResultsClosed = 0x020,
            LogicalLocationsWritten = 0x040,
            ToolNotificationsWritten = 0x080,
            ConfigurationNotificationsWritten = 0x100,
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
        }

        /// <summary>
        /// Initializes the SARIF log by emitting properties and other constructs
        /// sufficient to being populating a run with results.
        /// </summary>
        /// <param name="id">A string that uniquely identifies a run.</param>
        /// <param name="automationId">A global identifier for a run that permits correlation with a larger automation process.</param>
        public void Initialize(Run run)
        {
            if (run == null)
            {
                throw new ArgumentNullException(nameof(run));
            }

            if (run.Tool == null)
            {
                throw new ArgumentNullException(nameof(run.Tool));
            }

            this.EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.RunInitialized);

            SarifVersion sarifVersion = SarifVersion.Current;

            _jsonWriter.WriteStartObject(); // Begin: sarifLog
            _jsonWriter.WritePropertyName("$schema");
            _jsonWriter.WriteValue(sarifVersion.ConvertToSchemaUri().OriginalString);
            _jsonWriter.WritePropertyName("version");
            _jsonWriter.WriteValue(sarifVersion.ConvertToText());

            _jsonWriter.WritePropertyName("runs");
            _jsonWriter.WriteStartArray(); // Begin: runs

            _jsonWriter.WriteStartObject(); // Begin: run

            if (run.Id != null)
            {
                _jsonWriter.WritePropertyName("id");
                _serializer.Serialize(_jsonWriter, run.Id);
            }

            if (!string.IsNullOrEmpty(run.BaselineInstanceGuid))
            {
                _jsonWriter.WritePropertyName("baselineInstanceGuid");
                _serializer.Serialize(_jsonWriter, run.BaselineInstanceGuid);
            }

            if (run.AggregateIds != null)
            {
                _jsonWriter.WritePropertyName("aggregateIds");
                _serializer.Serialize(_jsonWriter, run.AggregateIds);
            }

            if (!string.IsNullOrEmpty(run.Architecture))
            {
                _jsonWriter.WritePropertyName("architecture");
                _serializer.Serialize(_jsonWriter, run.Architecture);
            }

            if (run.Tool != null)
            {
                _jsonWriter.WritePropertyName("tool");
                _serializer.Serialize(_jsonWriter, run.Tool);
            }

            if (run.Conversion != null)
            {
                _jsonWriter.WritePropertyName("conversion");
                _serializer.Serialize(_jsonWriter, run.Conversion);
            }

            if (run.VersionControlProvenance != null)
            {
                _jsonWriter.WritePropertyName("versionControlProvenance");
                _serializer.Serialize(_jsonWriter, run.VersionControlProvenance);
            }

            if (run.OriginalUriBaseIds != null)
            {
                _jsonWriter.WritePropertyName("originalUriBaseIds");
                _serializer.Serialize(_jsonWriter, run.OriginalUriBaseIds);
            }

            if (run.DefaultFileEncoding != null)
            {
                _jsonWriter.WritePropertyName("defaultFileEncoding");
                _serializer.Serialize(_jsonWriter, run.DefaultFileEncoding);
            }

            if (run.RichMessageMimeType != null)
            {
                _jsonWriter.WritePropertyName("richMessageMimeType");
                _serializer.Serialize(_jsonWriter, run.RichMessageMimeType);
            }

            if (run.RedactionToken != null)
            {
                _jsonWriter.WritePropertyName("redactionToken");
                _serializer.Serialize(_jsonWriter, run.RedactionToken);
            }

            _writeConditions |= Conditions.RunInitialized;
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
        public void WriteFiles(IDictionary<string, FileData> fileDictionary)
        {
            if (fileDictionary == null)
            {
                throw new ArgumentNullException(nameof(fileDictionary));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.FilesWritten);

            _jsonWriter.WritePropertyName("files");
            _serializer.Serialize(_jsonWriter, fileDictionary, typeof(Dictionary<Uri, FileData>));

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
        public void WriteLogicalLocations(IDictionary<string, LogicalLocation> logicalLocationsDictionary)
        {
            if (logicalLocationsDictionary == null)
            {
                throw new ArgumentNullException(nameof(logicalLocationsDictionary));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.LogicalLocationsWritten);

            _jsonWriter.WritePropertyName("logicalLocations");
            _serializer.Serialize(_jsonWriter, logicalLocationsDictionary, typeof(Dictionary<string, LogicalLocation>));

            _writeConditions |= Conditions.LogicalLocationsWritten;
        }

        public void WriteInvocations(IEnumerable<Invocation> invocations)
        {
            if (invocations == null)
            {
                throw new ArgumentNullException(nameof(invocations));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.InvocationsWritten);

            _jsonWriter.WritePropertyName("invocations");
            _serializer.Serialize(_jsonWriter, invocations, typeof(List<Invocation>));

            _writeConditions |= Conditions.InvocationsWritten;
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

            _jsonWriter.WritePropertyName("resources");
            _jsonWriter.WriteStartObject(); // Begin: resources
            _jsonWriter.WritePropertyName("rules");
            _serializer.Serialize(_jsonWriter, rules);

            _jsonWriter.WriteEndObject();  // End: resources
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
        /// <see cref="ToolFileConverterBase"/> should allow these exceptions to propagate.
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
        /// <see cref="ToolFileConverterBase"/> should allow these exceptions to propagate.
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
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

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

        internal void WriteRunProperties(IDictionary<string, SerializedPropertyInfo> properties)
        {
            _jsonWriter.WritePropertyName("properties");
            _serializer.Serialize(_jsonWriter, properties);
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

            if ((_writeConditions & Conditions.ResultsInitialized) == Conditions.ResultsInitialized &&
                (_writeConditions & Conditions.ResultsClosed) != Conditions.ResultsClosed)
            {
                CloseResults();
            }

            // Log complete. Write the end object.

            _jsonWriter.WriteEndObject(); // End: run
            _jsonWriter.WriteEndArray();  // End: runs
            _jsonWriter.WriteEndObject(); // End: sarifLog

            _writeConditions |= Conditions.Disposed;
        }

        private void EnsureInitialized()
        {
            if (_writeConditions == Conditions.None)
            {
                Initialize(new Run() { Tool = new Tool() });
            }
        }

        private void EnsureStateNotAlreadySet(Conditions invalidConditions)
        {
            Conditions observedInvalidConditions = _writeConditions & invalidConditions;
            if (observedInvalidConditions != Conditions.None)
            {
                // 	InvalidState	One or more invalid states were detected during serialization: {0}	
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, SdkResources.InvalidState, observedInvalidConditions));
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

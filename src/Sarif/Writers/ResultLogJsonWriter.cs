// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            None = 0x00,
            RunInitialized = 0x001,
            ToolWritten = 0x002,
            FilesWritten = 0x004,
            InvocationsWritten = 0x008,
            ResultsInitialized = 0x010,
            ResultsClosed = 0x020,
            LogicalLocationsWritten = 0x040,
            RunCompleted = 0x080,
            Disposed = 0x40000000
        }

        private Run _run;
        private Conditions _writeConditions;
        private readonly JsonWriter _jsonWriter;
        private readonly JsonSerializer _serializer;

        /// <summary>Initializes a new instance of the <see cref="ResultLogJsonWriter"/> class.</summary>
        /// <param name="jsonWriter">The JSON writer. This class does not take ownership of the JSON
        /// writer; the caller is responsible for destroying it.</param>
        public ResultLogJsonWriter(JsonWriter jsonWriter)
        {
            _jsonWriter = jsonWriter;
            _serializer = new JsonSerializer()
            {
            };
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

            _run = run;
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

            _writeConditions |= Conditions.RunInitialized;
        }

        /// <summary>
        /// A list containing information about the relevant files. This information may appear
        /// after the results, as the full list of scanned files might not be known until
        /// all results have been generated.
        /// </summary>
        /// <param name="artifacts">
        /// A dictionary whose keys are the URIs of scanned files and whose values provide
        /// information about those files.
        /// </param>
        public void WriteArtifacts(IList<Artifact> artifacts)
        {
            if (artifacts == null)
            {
                throw new ArgumentNullException(nameof(artifacts));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.FilesWritten);

            if (artifacts.HasAtLeastOneNonNullValue())
            {
                _jsonWriter.WritePropertyName("artifacts");
                _serializer.Serialize(_jsonWriter, artifacts);
            }

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
        public void WriteLogicalLocations(IList<LogicalLocation> logicalLocations)
        {
            if (logicalLocations == null)
            {
                throw new ArgumentNullException(nameof(logicalLocations));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.LogicalLocationsWritten);

            if (logicalLocations.HasAtLeastOneNonNullValue())
            {
                _jsonWriter.WritePropertyName("logicalLocations");
                _serializer.Serialize(_jsonWriter, logicalLocations);
            }

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

            if (invocations.HasAtLeastOneNonNullValue())
            {
                _jsonWriter.WritePropertyName("invocations");
                _serializer.Serialize(_jsonWriter, invocations);
            }

            _writeConditions |= Conditions.InvocationsWritten;
        }

        public void WriteTool(Tool tool)
        {
            if (tool == null)
            {
                throw new ArgumentNullException(nameof(tool));
            }

            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ToolWritten);

            SerializeIfNotNull(tool, "tool");

            _writeConditions |= Conditions.ToolWritten;
        }

        public void OpenResults()
        {
            EnsureInitialized();
            EnsureResultsArrayIsNotOpen();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ResultsClosed);

            _jsonWriter.WritePropertyName("results");
            _jsonWriter.WriteStartArray(); // Begin: results
            _writeConditions |= Conditions.ResultsInitialized;
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

            _serializer.Serialize(_jsonWriter, result);
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

            if (results.Any())
            {
                foreach (Result result in results)
                {
                    WriteResult(result);
                }
            }
            else
            {
                if ((_writeConditions & Conditions.ResultsInitialized) != Conditions.ResultsInitialized)
                {
                    OpenResults();
                }
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

        public void CompleteRun()
        {
            if (_writeConditions.HasFlag(Conditions.RunCompleted)) { return; }

            if ((_writeConditions & Conditions.ResultsInitialized) == Conditions.ResultsInitialized &&
                (_writeConditions & Conditions.ResultsClosed) != Conditions.ResultsClosed)
            {
                CloseResults();
            }

            // NOTE: These components are written in schema declaration order, except for Results.
            // This should be as similar to serializing the whole object as possible.

            if ((_writeConditions & Conditions.ToolWritten) != Conditions.ToolWritten)
            {
                WriteTool(_run.Tool);
            }

            if ((_writeConditions & Conditions.InvocationsWritten) != Conditions.InvocationsWritten &&
                _run.Invocations?.Count > 0)
            {
                WriteInvocations(_run.Invocations);
            }

            SerializeIfNotNull(_run.Tags, "tags");
            SerializeIfNotNull(_run.Conversion, "conversion");
            SerializeIfNotDefault(_run.Language, "language", "en-US");
            SerializeIfNotNull(_run.VersionControlProvenance, "versionControlProvenance");
            SerializeIfNotNull(_run.OriginalUriBaseIds, "originalUriBaseIds");

            if ((_writeConditions & Conditions.FilesWritten) != Conditions.FilesWritten &&
                _run.Artifacts != null)
            {
                WriteArtifacts(_run.Artifacts);
            }

            if ((_writeConditions & Conditions.LogicalLocationsWritten) != Conditions.LogicalLocationsWritten &&
                _run.LogicalLocations != null)
            {
                WriteLogicalLocations(_run.LogicalLocations);
            }

            SerializeIfNotNull(_run.Graphs, "graphs");

            // Results go here in schema order

            SerializeIfNotNull(_run.AutomationDetails, "automationDetails");
            SerializeIfNotNull(_run.RunAggregates, "runAggregates");
            SerializeIfNotNull(_run.BaselineGuid, "baselineGuid");
            SerializeIfNotNull(_run.RedactionTokens, "redactionTokens");
            SerializeIfNotNull(_run.DefaultEncoding, "defaultEncoding");
            SerializeIfNotNull(_run.DefaultSourceLanguage, "defaultSourceLanguage");
            SerializeIfNotNull(_run.NewlineSequences, "newlineSequences");
            SerializeIfNotNull(_run.ColumnKind == ColumnKind.UnicodeCodePoints ? "unicodeCodePoints" : "utf16CodeUnits", "columnKind");
            SerializeIfNotNull(_run.ExternalPropertyFileReferences, "externalPropertyFileReferences");
            SerializeIfNotNull(_run.ThreadFlowLocations, "threadFlowLocations");
            SerializeIfNotNull(_run.Taxonomies, "taxonomies");
            SerializeIfNotNull(_run.Addresses, "addresses");
            SerializeIfNotNull(_run.Translations, "translations");
            SerializeIfNotNull(_run.Policies, "policies");
            SerializeIfNotNull(_run.WebRequests, "webRequests");
            SerializeIfNotNull(_run.WebResponses, "webResponses");
            SerializeIfNotNull(_run.SpecialLocations, "specialLocations");
            SerializeIfNotNull(_run.Properties, "properties");

            // Log complete. Write the end object.
            _jsonWriter.WriteEndObject(); // End: run
            _jsonWriter.WriteEndArray();  // End: runs
            _jsonWriter.WriteEndObject(); // End: sarifLog

            _writeConditions |= Conditions.RunCompleted;
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

            CompleteRun();
            _writeConditions |= Conditions.Disposed;
        }

        private void SerializeIfNotNull(object value, string propertyName)
        {
            // Don't serialize null objects or empty enumerables
            if (value == null || ExtensionMethods.IsEmptyEnumerable(value)) { return; }

            Serialize(value, propertyName);
        }

        private void SerializeIfNotDefault(string value, string propertyName, string defaultValue)
        {
            if (value == defaultValue) { return; }

            Serialize(value, propertyName);
        }

        private void Serialize(object value, string propertyName)
        {
            _jsonWriter.WritePropertyName(propertyName);
            _serializer.Serialize(_jsonWriter, value);
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

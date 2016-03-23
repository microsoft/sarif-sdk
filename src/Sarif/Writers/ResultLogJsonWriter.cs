// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Sdk;

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
            ToolInfoWritten = 0x2,
            RuleInfoWritten = 0x4,
            RunInfoWritten = 0x8,
            ResultsInitialized = 0x10,
            ResultsClosed = 0x20,
            Disposed = 0x40
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

            _jsonWriter.WriteStartObject(); // Begin: resultLog
            _jsonWriter.WritePropertyName("version");
            _jsonWriter.WriteValue(SarifVersion.OneZeroZeroBetaOne.ConvertToText());

            _jsonWriter.WritePropertyName("runLogs");
            _jsonWriter.WriteStartArray(); // Begin: runLogs

            _jsonWriter.WriteStartObject(); // Begin: runLog

            _writeConditions |= Conditions.Initialized;
        }

        /// <summary>Writes a tool information entry to the log. This must be the first entry written into
        /// a log, and it may be written at most once.</summary>
        /// <exception cref="IOException">A file IO error occured. Clients implementing
        /// <see cref="IToolFileConverter"/> should allow these exceptions to propagate.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the tool info block has already been
        /// written.</exception>
        /// <param name="info">The tool information to write.</param>
        /// <seealso cref="M:Microsoft.CodeAnalysis.Sarif.IsarifWriter.WriteToolInfo(ToolInfo)"/>
        public void WriteToolInfo(ToolInfo toolInfo)
        {
            if (toolInfo == null)
            {
                throw new ArgumentNullException("toolInfo");
            }

            EnsureInitialized();
            EnsureNoInProgressSerialization();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ToolInfoWritten);

            _jsonWriter.WritePropertyName("toolInfo");
            _serializer.Serialize(_jsonWriter, toolInfo, typeof(ToolInfo));

            _writeConditions |= Conditions.ToolInfoWritten;
        }

        public void WriteRunInfo(RunInfo runInfo)
        {
            if (runInfo == null)
            {
                throw new ArgumentNullException("runInfo");
            }

            EnsureInitialized();
            EnsureNoInProgressSerialization();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.RunInfoWritten);

            _jsonWriter.WritePropertyName("runInfo");
            _serializer.Serialize(_jsonWriter, runInfo, typeof(RunInfo));

            _writeConditions |= Conditions.RunInfoWritten;
        }

        public void WriteRuleInfo(IEnumerable<IRuleDescriptor> ruleDescriptors)
        {
            if (ruleDescriptors == null)
            {
                throw new ArgumentNullException("ruleDescriptors");
            }

            EnsureInitialized();
            EnsureNoInProgressSerialization();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.RuleInfoWritten);

            _jsonWriter.WritePropertyName("ruleInfo");
            _jsonWriter.WriteStartArray();

            foreach(IRuleDescriptor ruleDescriptor in ruleDescriptors)
            {
                RuleDescriptor descriptor = new RuleDescriptor();
                descriptor.Id = ruleDescriptor.Id;
                descriptor.Name = ruleDescriptor.Name;
                descriptor.FullDescription = ruleDescriptor.FullDescription;
                descriptor.ShortDescription = ruleDescriptor.ShortDescription;
                descriptor.Options = ruleDescriptor.Options;
                descriptor.Properties = ruleDescriptor.Properties;
                descriptor.FormatSpecifiers = ruleDescriptor.FormatSpecifiers;

                _serializer.Serialize(_jsonWriter, descriptor, typeof(RuleDescriptor));
            }

            _jsonWriter.WriteEndArray();
        }

        public void OpenResults()
        {
            EnsureInitialized();
            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ResultsInitialized | Conditions.ResultsClosed);

            _jsonWriter.WritePropertyName("results");
            _jsonWriter.WriteStartArray(); // Begin: results

            _writeConditions |= Conditions.ResultsInitialized;
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

            EnsureStateNotAlreadySet(Conditions.Disposed | Conditions.ResultsClosed);

            if ((_writeConditions & Conditions.ResultsInitialized) != Conditions.ResultsInitialized)
            {
                OpenResults();
            }

            _serializer.Serialize(_jsonWriter, result, typeof(Result));
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
                if ((_writeConditions & Conditions.ResultsInitialized) == Conditions.ResultsInitialized)
                {
                    CloseResults();
                }

                // Log complete. Write the end object.

                _jsonWriter.WriteEndObject(); // End: runLog
                _jsonWriter.WriteEndArray();  // End: runLogs
                _jsonWriter.WriteEndObject(); // End: resultsLog
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
                throw new InvalidOperationException(string.Format(SarifResources.InvalidState, observedInvalidConditions));
            }
        }

        private void EnsureNoInProgressSerialization()
        {
            // This method ensures that no in-progress serialization
            // underway. Currently, only the results serialization
            // is a multi-step process
            if ((_writeConditions & Conditions.ResultsInitialized) == Conditions.ResultsInitialized &&
                (_writeConditions & Conditions.ResultsClosed) != Conditions.ResultsClosed)
            {
                throw new InvalidOperationException(SarifResources.ResultsSerializationNotComplete);
            }
        }
    }
}

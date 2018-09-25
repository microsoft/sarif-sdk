using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    ///  SarifWriter provides memory-efficient writing of Sarif files, by requiring users to
    ///  write large collections item-by-item.
    ///  
    ///  SarifWriter writes the large collections to separate files and then copies the content into
    ///  the primary JSON file in Dispose.
    ///  
    ///  Usage:
    ///  using(SarifWriter writer = new SarifWriter(jsonSerializer, outputFilePath, sarifLogWithoutRuns))
    ///  {
    ///     writer.Write(sarifRunWithoutFilesOrResults);
    ///     
    ///     foreach(FileData file in source)
    ///     {
    ///         writer.Write(file.Uri, file);
    ///     }
    ///     
    ///     foreach(Result result in source)
    ///     {
    ///         writer.Write(result);
    ///     }
    ///  }
    /// </summary>
    public class SarifWriter : IDisposable
    {
        private JsonSerializer _serializer;
        private IStreamProvider _streamProvider;
        private string _baseFilePath;

        private SarifLog _log;

        private JsonTextWriter _filesWriter;
        private JsonTextWriter _resultsWriter;

        public SarifWriter(JsonSerializer serializer, string baseFilePath, SarifLog baseLog)
            : this(serializer, new FileSystemStreamProvider(), baseFilePath, baseLog)
        { }

        public SarifWriter(JsonSerializer serializer, IStreamProvider streamProvider, string baseFilePath, SarifLog baseLog)
        {
            _serializer = serializer;
            _streamProvider = streamProvider;
            _baseFilePath = baseFilePath;

            if (baseLog.Runs != null) throw new ArgumentException("baseLog must not have any runs when SarifWriter is constructed.");
            _log = new SarifLog(baseLog);
        }

        public void Write(Run run)
        {
            if (run.Files != null) throw new ArgumentException("run must not have Files. Write Files by calling Write() for each FileData.");
            if (run.Results != null) throw new ArgumentException("run must not have Results. Write Results by calling Write() for each Result.");

            // Save the run to serialize later
            if (_log.Runs == null) _log.Runs = new List<Run>();
            _log.Runs.Add(run);

            // Close the previous run's Files and Results logs, if any
            EndFilesLog();
            EndResultsLog();
        }

        public void Write(string fileKey, FileData file)
        {
            if (_filesWriter == null) StartFilesLog();
            _filesWriter.WritePropertyName(fileKey);
            _serializer.Serialize(_filesWriter, file);
        }

        private void StartFilesLog()
        {
            if (_log.Runs == null) throw new InvalidOperationException("Write(Run) must be called before Write() for items within a Run.");

            string filePath = Path.ChangeExtension(_baseFilePath, $".Files.{_log.Runs.Count}.sarif");
            _filesWriter = new JsonTextWriter(new StreamWriter(_streamProvider.OpenWrite(filePath)));
            _filesWriter.Formatting = _serializer.Formatting;
            _filesWriter.WriteStartObject();

            // Create a marker to inject from the external file when the outer object is serialized
            _log.Runs[_log.Runs.Count - 1].Files = new DictionaryInjectionStub<string, FileData>(filePath);
        }

        private void EndFilesLog()
        {
            if (_filesWriter != null)
            {
                _filesWriter.WriteEndObject();
                ((IDisposable)_filesWriter).Dispose();
                _filesWriter = null;
            }
        }

        public void Write(Result result)
        {
            if (_resultsWriter == null) StartResultsLog();
            _serializer.Serialize(_resultsWriter, result);
        }

        private void StartResultsLog()
        {
            if (_log.Runs == null) throw new InvalidOperationException("Write(Run) must be called before Write() for items within a Run.");

            string filePath = Path.ChangeExtension(_baseFilePath, $".Results.{_log.Runs.Count}.sarif");
            _resultsWriter = new JsonTextWriter(new StreamWriter(_streamProvider.OpenWrite(filePath)));
            _resultsWriter.Formatting = _serializer.Formatting;
            _resultsWriter.WriteStartArray();

            // Create a marker to inject from the external file when the outer object is serialized
            _log.Runs[_log.Runs.Count - 1].Results = new ListInjectionStub<Result>(filePath);
        }

        private void EndResultsLog()
        {
            if (_resultsWriter != null)
            {
                _resultsWriter.WriteEndArray();
                ((IDisposable)_resultsWriter).Dispose();
                _resultsWriter = null;
            }
        }

        public void Dispose()
        {
            if (_log != null)
            {
                EndFilesLog();
                EndResultsLog();

                IContractResolver previousResolver = _serializer.ContractResolver;
                try
                {
                    // Build a Stream and Writer for the primary log
                    using (Stream outerStream = _streamProvider.OpenWrite(_baseFilePath))
                    using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(outerStream)))
                    {
                        // Replace the ContractResolver with one which will inject the separate files for the correct collections
                        _serializer.ContractResolver = new InjectingContractResolver(previousResolver, _streamProvider, outerStream);

                        // Write the log (with injected collections)
                        _serializer.Serialize(writer, _log);
                    }
                }
                finally
                {
                    _serializer.ContractResolver = previousResolver;
                }

                _log = null;
            }
        }

        #region Cross File Json Injection
        /// <summary>
        ///  Implement IInjectionStub from any type which you'd actually like to be serialized
        ///  by copying another JSON stream into the output stream.
        /// </summary>
        private interface IInjectionStub
        {
            /// <summary>
            ///  File Path containing the JSON to inject for this item
            /// </summary>
            string InjectedFromPath { get; }
        }

        /// <summary>
        ///  ListInjectionStub is a List&lt;T&gt; which will actually be injected at serialization
        ///  time from the specified file.
        /// </summary>
        /// <typeparam name="T">Type of Items in collection</typeparam>
        private class ListInjectionStub<T> : List<T>, IInjectionStub
        {
            public string InjectedFromPath { get; set; }

            public ListInjectionStub(string injectedFromPath)
            {
                this.InjectedFromPath = injectedFromPath;
            }
        }

        /// <summary>
        ///  DictionaryInjectionStub is a Dictionary&lt;T, U&gt; which will actually be injected 
        ///  at serialization time from the specified file.
        /// </summary>
        /// <typeparam name="T">Type of Items in collection</typeparam>
        private class DictionaryInjectionStub<T, U> : Dictionary<T, U>, IInjectionStub
        {
            public string InjectedFromPath { get; set; }

            public DictionaryInjectionStub(string injectedFromPath)
            {
                this.InjectedFromPath = injectedFromPath;
            }
        }

        /// <summary>
        ///  InjectingConverter is a JsonConverter which will inject JSON from another file
        ///  into the serialized JSON when an IInjectionStub is written.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class InjectingConverter<T> : JsonConverter where T : IInjectionStub
        {
            private IStreamProvider _streamProvider;
            private Stream _outerStream;

            public InjectingConverter(IStreamProvider streamProvider, Stream outerStream)
            {
                _streamProvider = streamProvider;
                _outerStream = outerStream;
            }

            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(T));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                IInjectionStub stub = value as IInjectionStub;
                if (stub == null) throw new ArgumentException($"InjectingConverter can only replace objects which implement IInjectionStub. Type Passed: {nameof(value)}.");

                // Write null to convince the JsonWriter not to write it for us
                writer.WriteNull();

                // Copy the whole file into the outer stream here
                using (Stream sourceStream = _streamProvider.OpenRead(stub.InjectedFromPath))
                {
                    // Flush output except the null we just wrote
                    writer.Flush();
                    _outerStream.Seek(-4, SeekOrigin.Current);

                    // Inject the content from the other stream
                    sourceStream.CopyTo(_outerStream);
                }

                // Delete the temporary file
                _streamProvider.Delete(stub.InjectedFromPath);
            }
        }

        /// <summary>
        ///  IContractResolver for SarifWriter, which specifies that the Files and Results collections
        ///  will be injected from separate files.
        /// </summary>
        private class InjectingContractResolver : IContractResolver
        {
            private IContractResolver _inner;
            private IStreamProvider _streamProvider;

            private Stream _outerStream;

            public InjectingContractResolver(IContractResolver inner, IStreamProvider streamProvider, Stream outerStream)
            {
                _inner = inner;
                _streamProvider = streamProvider;

                // Keep a copy of the Stream we're writing the SarifLog to (so we can inject parts)
                _outerStream = outerStream;
            }

            public JsonContract ResolveContract(Type type)
            {
                JsonContract contract = _inner.ResolveContract(type);

                if (type == typeof(ListInjectionStub<Result>))
                {
                    contract.Converter = new InjectingConverter<ListInjectionStub<Result>>(_streamProvider, _outerStream);
                }
                else if (type == typeof(DictionaryInjectionStub<string, FileData>))
                {
                    contract.Converter = new InjectingConverter<DictionaryInjectionStub<string, FileData>>(_streamProvider, _outerStream);
                }

                return contract;
            }
        }
        #endregion
    }
}

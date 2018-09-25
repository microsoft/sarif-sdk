using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public interface IStreamProvider
    {
        Stream OpenWrite(string logicalPath);
        Stream OpenRead(string logicalPath);
        void Delete(string logicalPath);
    }

    public class FileSystemStreamProvider : IStreamProvider
    {
        public void Delete(string logicalPath)
        {
            File.Delete(logicalPath);
        }

        public Stream OpenRead(string logicalPath)
        {
            return File.OpenRead(logicalPath);
        }

        public Stream OpenWrite(string logicalPath)
        {
            return new FileStream(logicalPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        }
    }

    public class SarifWriter : IDisposable
    {
        private JsonSerializer _serializer;
        private IStreamProvider _streamProvider;
        private string _baseFilePath;

        private SarifLog _log;

        private JsonTextWriter _filesWriter;
        private JsonTextWriter _resultsWriter;

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

            if (_log.Runs == null) _log.Runs = new List<Run>();
            _log.Runs.Add(run);

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
            string filePath = Path.ChangeExtension(_baseFilePath, $".Results.{_log.Runs.Count}.sarif");
            _filesWriter = new JsonTextWriter(new StreamWriter(_streamProvider.OpenWrite(filePath)));
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
            string filePath = Path.ChangeExtension(_baseFilePath, $".Results.{_log.Runs.Count}.sarif");
            _resultsWriter = new JsonTextWriter(new StreamWriter(_streamProvider.OpenWrite(filePath)));
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
            if(_log != null)
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

        private interface IInjectionStub
        {
            string InjectedFromPath { get; }
        }

        private class ListInjectionStub<T> : List<T>, IInjectionStub
        {
            public string InjectedFromPath { get; set; }

            public ListInjectionStub(string injectedFromPath)
            {
                this.InjectedFromPath = injectedFromPath;
            }
        }

        private class DictionaryInjectionStub<T, U> : Dictionary<T, U>, IInjectionStub
        {
            public string InjectedFromPath { get; set; }

            public DictionaryInjectionStub(string injectedFromPath)
            {
                this.InjectedFromPath = injectedFromPath;
            }
        }

        private class InjectingConverter<T> : JsonConverter
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

                // Copy the whole file into the outer stream here
                using (Stream sourceStream = _streamProvider.OpenRead(stub.InjectedFromPath))
                {
                    writer.Flush();
                    sourceStream.CopyTo(_outerStream);
                }

                // Delete the temporary file
                _streamProvider.Delete(stub.InjectedFromPath);
            }
        }

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
    }
}

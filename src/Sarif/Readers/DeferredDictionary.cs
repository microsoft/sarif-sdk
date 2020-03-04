// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    /// <summary>
    ///  DeferredDictionary is an IDictionary&lt;string, T&gt; which uses JSON.NET
    ///  to read the dictionary items as they are accessed. It pre-builds the mapping
    ///  from string to JSON file position for fast retrieval.
    ///  
    ///  Enumerate KeyValuePairs for best performance:
    ///  foreach(KeyValuePair&lt;T, U&gt;> item in dictionary)
    ///  { ... }
    /// </summary>
    /// <remarks>
    ///  DeferredDictionary doesn't record anything but the file position of the dictionary in the JSON initially.
    ///  If you foreach over the KeyValuePairs, it constructs one reader and loads the items as you read them.
    ///  If you ask for the Keys, Values, or Dictionary[key], it must build a map from each key to the file position for that value,
    ///  and must seek in the file for each read.
    ///  
    ///  Items are expensive to iterate each time; they are not kept in memory. Copy the values to a List or array to keep them.
    /// </remarks>
    /// <typeparam name="T">Type of items in Dictionary</typeparam>
    public class DeferredDictionary<T> : IDictionary<string, T>, IDisposable
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly Func<Stream> _streamProvider;
        private readonly long _start;

        private Stream _stream;
        private Dictionary<int, long> _itemPositions;

        public DeferredDictionary(JsonSerializer jsonSerializer, JsonPositionedTextReader reader, bool buildPositionsNow = false)
        {
            _jsonSerializer = jsonSerializer;
            _streamProvider = reader.StreamProvider;
            _start = reader.TokenPosition;

            // We have the JsonTextReader, which must scan to after the collection to resume building the outer object
            // We may as well make the map of element positions.
            if (buildPositionsNow)
            {
                BuildPositions(reader, 0);
            }
            else
            {
                reader.Skip();
            }
        }

        private void EnsurePositionsBuilt()
        {
            if (_itemPositions == null) { BuildPositions(); }
        }

        private void BuildPositions()
        {
            using (Stream stream = _streamProvider())
            using (JsonPositionedTextReader reader = new JsonPositionedTextReader(() => stream))
            {
                stream.Seek(_start, SeekOrigin.Begin);
                reader.Read();

                BuildPositions(reader, _start);
            }
        }

        private void BuildPositions(JsonPositionedTextReader reader, long currentOffset)
        {
            var result = new Dictionary<int, long>();
            while (true)
            {
                // Find the position just before the PropertyName (we need to read it back later to confirm the string matches)
                long keyPosition = currentOffset + reader.TokenPosition + 1;

                reader.Read();
                if (reader.TokenType == JsonToken.EndObject) { break; }

                if (reader.TokenType != JsonToken.PropertyName) { throw new InvalidDataException($"@({reader.LineNumber}, {reader.LinePosition}): Expected property name, found {reader.TokenType} \"{reader.Value}\"."); }

                // Read JSON object name (Dictionary key)
                string key = (string)reader.Value;
                int keyHash = key.GetHashCode();

                // Skip the value
                reader.Read();
                reader.Skip();

                // Add the hash to the position to our Dictionary; resolve collisions by incrementing
                while (result.ContainsKey(keyHash))
                {
                    keyHash++;
                }
                result[keyHash] = keyPosition;
            }

            _itemPositions = result;
        }

        public T this[string key]
        {
            get
            {
                if (!this.TryGetValue(key, out T value)) { throw new KeyNotFoundException(key); }

                return value;
            }

            set => throw new NotSupportedException();
        }

        // Keys and Values both use the IEnumerator of this class, returning the Key or Value part, respectively.
        // They call the Count getter if requested by the user to avoid full enumeration until a user requests the Count.
        public ICollection<string> Keys => new ReadOnlyCollectionAdapter<string>(() => new KeyEnumeratorAdapter<string, T>(this.GetEnumerator()), () => this.Count);
        public ICollection<T> Values => new ReadOnlyCollectionAdapter<T>(() => new ValueEnumeratorAdapter<string, T>(this.GetEnumerator()), () => this.Count);

        public int Count
        {
            get
            {
                EnsurePositionsBuilt();
                return _itemPositions.Count;
            }
        }

        public bool IsReadOnly => true;

        #region Dictionary Mutators [not supported]
        public void Add(string key, T value)
        {
            throw new NotSupportedException();
        }

        public void Add(KeyValuePair<string, T> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            throw new NotSupportedException();
        }
        #endregion

        public bool Contains(KeyValuePair<string, T> item)
        {
            T value;
            if (!this.TryGetValue(item.Key, out value)) { return false; }
            return EqualityComparer<T>.Default.Equals(value, item.Value);
        }

        public bool ContainsKey(string key)
        {
            T unused;
            return this.TryGetValue(key, false, out unused);
        }

        public bool TryGetValue(string key, out T value)
        {
            return TryGetValue(key, true, out value);
        }

        private bool TryGetValue(string key, bool readValue, out T value)
        {
            value = default(T);
            if (_stream == null) { _stream = _streamProvider(); }

            EnsurePositionsBuilt();

            // Get the hash of the key
            long keyPosition;
            int keyHash = key.GetHashCode();

            // Find the position for that hash
            while (_itemPositions.TryGetValue(keyHash, out keyPosition))
            {
                _stream.Seek(keyPosition, SeekOrigin.Begin);

                using (JsonInnerTextReader reader = new JsonInnerTextReader(new JsonObjectMemberStreamReader(_stream)))
                {
                    reader.CloseInput = false;

                    // Read past the mock object start from JsonObjectMemberStreamReader [so JsonTextReader expects to see a PropertyName]
                    reader.Read();

                    // Get the string key for the item with this hash
                    reader.Read();
                    string foundKey = (string)reader.Value;
                    if (reader.TokenType != JsonToken.PropertyName) { throw new InvalidDataException($"Did not find JSON object key at position {keyPosition:n0}."); }

                    // If it is the correct key, return the item
                    if (foundKey == key)
                    {
                        if (readValue)
                        {
                            reader.Read();
                            value = _jsonSerializer.Deserialize<T>(reader);
                        }

                        return true;
                    }
                }

                // Otherwise, increment the hash (collision resolution) and repeat
                keyHash++;
            }

            // If no Dictionary entry for the hash, the item is not present
            return false;
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex + this.Count > array.Length) { throw new ArgumentOutOfRangeException("arrayIndex"); }

            int index = arrayIndex;
            foreach (KeyValuePair<string, T> item in this)
            {
                array[index++] = item;
            }
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return new JsonDeferredDictionaryEnumerator<T>(_jsonSerializer, _streamProvider, _start);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new JsonDeferredDictionaryEnumerator<T>(_jsonSerializer, _streamProvider, _start);
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

        /// <summary>
        ///  JsonObjectMemberStreamReader is used to get JsonTextReader to read a property name and
        ///  value somewhere in an outer object.
        ///  
        ///  It alters the first read so that there's a fake outer object (a starting '{') and
        ///  whitespace and any comma before the property name are hidden.
        /// </summary>
        private class JsonObjectMemberStreamReader : StreamReader
        {
            private bool _readCalled;

            public JsonObjectMemberStreamReader(Stream stream)
                : base(stream: stream, encoding: Encoding.Default, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true)
            { }

            public override int Read(char[] buffer, int index, int count)
            {
                int countRead = 0;

                // We are seeking to a position just before a new PropertyName in an object.
                if (!_readCalled)
                {
                    _readCalled = true;

                    // We'll add a '{' so the JsonTextReader understands we're in an object
                    buffer[index++] = '{';

                    // Read the remaining desired count
                    countRead = base.Read(buffer, index + 1, count - 1);

                    // Make everything before the next PropertyName whitespace (old space and comma between items)
                    for (int i = index + 1; i < index + 1 + countRead; ++i)
                    {
                        if (buffer[i] == '"') { break; }
                        buffer[i] = ' ';
                    }

                    // Return the length found plus our added '{'
                    return countRead + 1;
                }

                return base.Read(buffer, index, count);
            }
        }

        /// <summary>
        ///  JsonDeferredDictionaryEnumerator provides enumeration over a Dictionary in a
        ///  Json object starting at a given position.
        ///  
        ///  Items are not pre-loaded and are not kept after enumeration, allowing use with
        ///  collections too large for memory.
        /// </summary>
        /// <typeparam name="U">Dictionary Value item type</typeparam>
        private class JsonDeferredDictionaryEnumerator<U> : IEnumerator<KeyValuePair<string, U>>
        {
            private readonly JsonSerializer _jsonSerializer;
            private readonly Func<Stream> _streamProvider;
            private readonly long _start;

            private JsonTextReader _jsonTextReader;
            private Stream _stream;

            public JsonDeferredDictionaryEnumerator(JsonSerializer jsonSerializer, Func<Stream> streamProvider, long start)
            {
                _jsonSerializer = jsonSerializer;
                _streamProvider = streamProvider;
                _start = start;

                Reset();
            }

            public KeyValuePair<string, U> Current { get; private set; }
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                    _jsonTextReader = null;
                }
            }

            public bool MoveNext()
            {
                if (_jsonTextReader.TokenType == JsonToken.EndObject) { return false; }

                // Read the next key
                string key = (string)_jsonTextReader.Value;

                // Read the value
                _jsonTextReader.Read();
                U value = _jsonSerializer.Deserialize<U>(_jsonTextReader);

                // Read EndObject, next is StartObject of next member
                _jsonTextReader.Read();

                Current = new KeyValuePair<string, U>(key, value);
                return true;
            }

            public void Reset()
            {
                Dispose();

                // Open a new Stream
                _stream = _streamProvider();

                // Seek to the object start
                _stream.Seek(_start, SeekOrigin.Begin);

                // Build a JsonTextReader
                _jsonTextReader = new JsonInnerTextReader(new StreamReader(_stream));

                // StartObject
                _jsonTextReader.Read();

                // PropertyName of first item key
                _jsonTextReader.Read();
            }
        }

        /// <summary>
        ///  ReadOnlyCollectionAdapter wraps a IEnumerator and Count getters
        ///  to implement ICollection.
        /// </summary>
        /// <typeparam name="U">Collection Item type</typeparam>
        private class ReadOnlyCollectionAdapter<U> : ICollection<U>
        {
            private readonly Func<IEnumerator<U>> _enumeratorFactory;
            private readonly Func<int> _countGetter;
            public int Count => _countGetter();
            public bool IsReadOnly => true;

            public ReadOnlyCollectionAdapter(Func<IEnumerator<U>> enumeratorFactory, Func<int> countGetter)
            {
                _enumeratorFactory = enumeratorFactory;
                _countGetter = countGetter;
            }

            #region Mutators [Not Supported]
            public void Add(U item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Remove(U item)
            {
                throw new NotSupportedException();
            }
            #endregion

            public bool Contains(U item)
            {
                EqualityComparer<U> comparer = EqualityComparer<U>.Default;

                foreach (U value in this)
                {
                    if (comparer.Equals(item, value)) { return true; }
                }

                return false;
            }

            public void CopyTo(U[] array, int arrayIndex)
            {
                if (arrayIndex < 0 || arrayIndex + this.Count > array.Length) { throw new ArgumentOutOfRangeException("arrayIndex"); }

                int index = arrayIndex;
                foreach (U value in this)
                {
                    array[index++] = value;
                }
            }

            public IEnumerator<U> GetEnumerator()
            {
                return _enumeratorFactory();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _enumeratorFactory();
            }
        }


        /// <summary>
        ///  ValueEnumeratorAdapter converts an IEnumerator&lt;KeyValuePair&lt;U, V&gt;&gt;
        ///  into an IEnumerator&lt;U&gt; by returning Current.Key.
        /// </summary>
        private class KeyEnumeratorAdapter<U, V> : IEnumerator<U>
        {
            private IEnumerator<KeyValuePair<U, V>> _inner;

            public KeyEnumeratorAdapter(IEnumerator<KeyValuePair<U, V>> inner)
            {
                _inner = inner;
            }

            public U Current => _inner.Current.Key;
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (_inner != null)
                {
                    _inner.Dispose();
                    _inner = null;
                }
            }

            public bool MoveNext()
            {
                return _inner.MoveNext();
            }

            public void Reset()
            {
                _inner.Reset();
            }
        }

        /// <summary>
        ///  ValueEnumeratorAdapter converts an IEnumerator&lt;KeyValuePair&lt;U, V&gt;&gt;
        ///  into an IEnumerator&lt;V&gt; by returning Current.Value.
        /// </summary>
        private class ValueEnumeratorAdapter<U, V> : IEnumerator<V>
        {
            private IEnumerator<KeyValuePair<U, V>> _inner;

            public ValueEnumeratorAdapter(IEnumerator<KeyValuePair<U, V>> inner)
            {
                _inner = inner;
            }

            public V Current => _inner.Current.Value;
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if (_inner != null)
                {
                    _inner.Dispose();
                    _inner = null;
                }
            }

            public bool MoveNext()
            {
                return _inner.MoveNext();
            }

            public void Reset()
            {
                _inner.Reset();
            }
        }
    }
}

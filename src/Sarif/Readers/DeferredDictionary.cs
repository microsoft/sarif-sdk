// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    /// <summary>
    ///  DeferredDictionary is an IDictionary&lt;string, T&gt; which uses JSON.NET
    ///  to read the dictionary items as they are accessed. It pre-builds the mapping
    ///  from string to JSON file position for fast retrieval.
    /// </summary>
    /// <typeparam name="T">Type of items in Dictionary</typeparam>
    public class DeferredDictionary<T> : IDictionary<string, T>
    {
        private JsonSerializer _jsonSerializer;
        private Func<Stream> _streamProvider;

        private Stream _stream;
        private Dictionary<string, long> _itemPositions;

        public DeferredDictionary(JsonSerializer jsonSerializer, JsonPositionedTextReader reader)
        {
            _jsonSerializer = jsonSerializer;
            _streamProvider = reader.StreamProvider;
            _itemPositions = Build(jsonSerializer, reader);
        }

        private static Dictionary<string, long> Build(JsonSerializer serializer, JsonPositionedTextReader reader)
        {
            Dictionary<string, long> result = new Dictionary<string, long>();

            while(true)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndObject) break;

                if (reader.TokenType != JsonToken.PropertyName) throw new InvalidDataException($"@({reader.LineNumber}, {reader.LinePosition}): Expected property name, found {reader.TokenType} \"{reader.Value}\".");
                string key = (string)reader.Value;

                reader.Read();
                long position = reader.TokenPosition;

                reader.Skip();
                result[key] = position;
            }

            return result;
        }

        public T this[string key]
        {
            get
            {
                T value;
                if (!this.TryGetValue(key, out value)) throw new KeyNotFoundException(key);

                return value;
            }

            set => throw new NotSupportedException();
        }

        public ICollection<string> Keys => _itemPositions.Keys;

        public ICollection<T> Values => throw new NotSupportedException("DeferredDictionary is designed not to load all values at once.");

        public int Count => _itemPositions.Count;

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

        public bool TryGetValue(string key, out T value)
        {
            value = default(T);

            long position;
            if (!_itemPositions.TryGetValue(key, out position)) return false;

            if (_stream == null) _stream = _streamProvider();
            _stream.Seek(position, SeekOrigin.Begin);

            using (JsonTextReader reader = new JsonTextReader(new StreamReader(_stream)))
            {
                reader.CloseInput = false;
                value = _jsonSerializer.Deserialize<T>(reader);
            }

            return true;
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            T value;
            if (!this.TryGetValue(item.Key, out value)) return false;
            return EqualityComparer<T>.Default.Equals(value, item.Value);
        }

        public bool ContainsKey(string key)
        {
            return _itemPositions.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex + this.Count > array.Length) throw new ArgumentOutOfRangeException("arrayIndex");

            int index = arrayIndex;
            foreach(KeyValuePair<string, T> item in this)
            {
                array[index++] = item;
            }
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return new JsonDeferredDictionaryEnumerator<T>(this);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new JsonDeferredDictionaryEnumerator<T>(this);
        }

        private class JsonDeferredDictionaryEnumerator<U> : IEnumerator<KeyValuePair<string, U>>
        {
            private IEnumerator<string> _keyEnumerator;
            private IDictionary<string, U> _dictionary;

            public JsonDeferredDictionaryEnumerator(IDictionary<string, U> dictionary)
            {
                _keyEnumerator = dictionary.Keys.GetEnumerator();
                _dictionary = dictionary;
            }

            object IEnumerator.Current => Current;

            public KeyValuePair<string, U> Current
            {
                get
                {
                    string key = _keyEnumerator.Current;
                    return new KeyValuePair<string, U>(key, _dictionary[key]);
                }
            }

            public void Dispose()
            {
                if (_keyEnumerator != null)
                {
                    _keyEnumerator.Dispose();
                    _keyEnumerator = null;
                }
            }

            public bool MoveNext()
            {
                return _keyEnumerator.MoveNext();
            }

            public void Reset()
            {
                _keyEnumerator.Reset();
            }
        }
    }
}

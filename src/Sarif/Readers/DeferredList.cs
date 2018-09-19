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
    ///  DeferredList is an IList&lt;T&gt; which wraps a specific position in a JSON stream.
    ///  It uses JSON.NET to read the items from the stream when they are enumerated in the list,
    ///  saving the memory cost of keeping them around.
    ///  
    ///  DeferredList supports enumerable and in-order access only.
    /// </summary>
    /// <typeparam name="T">Type of items in list</typeparam>
    public class DeferredList<T> : IList<T>
    {
        private JsonSerializer _jsonSerializer;
        private Func<Stream> _streamProvider;
        private long _start;

        private IEnumerator<T> _currentEnumerator;
        private int _currentIndex;

        public int Count { get; private set; }
        public bool IsReadOnly => true;

        public DeferredList(JsonSerializer jsonSerializer, JsonPositionedTextReader reader)
        {
            _jsonSerializer = jsonSerializer;
            _streamProvider = reader.StreamProvider;

            _start = reader.TokenPosition;
            int count = 0;

            while (true)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndArray) break;

                reader.Skip();
                count++;
            }

            Count = count;
        }

        public T this[int index]
        {
            get
            {
                if(index == 0)
                {
                    if (_currentEnumerator != null) _currentEnumerator.Dispose();

                    _currentIndex = 0;
                    _currentEnumerator = this.GetEnumerator();
                }

                if(index != _currentIndex)
                {
                    throw new NotSupportedException("DeferredList only allows enumerating items in order.");
                }

                if (!_currentEnumerator.MoveNext()) throw new IndexOutOfRangeException("index");
                _currentIndex++;
                return _currentEnumerator.Current;
            }

            set => throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex + this.Count > array.Length) throw new ArgumentOutOfRangeException("arrayIndex");

            int index = arrayIndex;
            foreach(T item in this)
            {
                array[index++] = item;
            }
        }

        #region List Search [not supported]
        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(T item)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region List Mutators [not supported]
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            return new JsonDeferredListEnumerator<T>(_jsonSerializer, _streamProvider, _start);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new JsonDeferredListEnumerator<T>(_jsonSerializer, _streamProvider, _start);
        }

        private class JsonDeferredListEnumerator<U> : IEnumerator<U>
        {
            private JsonSerializer _jsonSerializer;
            private Func<Stream> _streamProvider;
            private long _start;

            private JsonTextReader _jsonTextReader;
            private Stream _stream;

            public JsonDeferredListEnumerator(JsonSerializer jsonSerializer, Func<Stream> streamProvider, long start)
            {
                _jsonSerializer = jsonSerializer;
                _streamProvider = streamProvider;
                _start = start;

                Reset();
            }

            public U Current { get; private set; }
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
                if (_jsonTextReader.TokenType == JsonToken.EndArray) return false;

                // Read the next item
                Current = _jsonSerializer.Deserialize<U>(_jsonTextReader);

                // Read EndObject, next is StartObject of next member
                _jsonTextReader.Read();

                return true;
            }

            public void Reset()
            {
                Dispose();

                // Open a new Stream
                _stream = _streamProvider();

                // Seek to the array start
                _stream.Seek(_start, SeekOrigin.Begin);

                // Build a JsonTextReader
                _jsonTextReader = new JsonTextReader(new StreamReader(_stream));

                // StartArray
                _jsonTextReader.Read();

                // StartObject of first member
                _jsonTextReader.Read();
            }
        }
    }
}

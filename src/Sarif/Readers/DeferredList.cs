// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    ///  Enumerate for best performance:
    ///  foreach(T item in list)
    ///  { ... }
    /// </summary>
    /// <remarks>
    ///  DeferredList stores only the file position of the array in the JSON initially.
    ///  If you foreach over the array, it creates one reader and loads the items as you read them.
    ///  If you ask for List[index] or the Count, it must build an array of the file position of each item
    ///  and seek each time you request one.
    ///  
    ///  Items are expensive to iterate each time; they are not kept in memory. Copy the values to a List or array to keep them.
    /// </remarks>
    /// <typeparam name="T">Type of items in list</typeparam>
    public class DeferredList<T> : IList<T>, IDisposable
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly Func<Stream> _streamProvider;
        private readonly long _start;
        private int _count;

        private Stream _stream;
        private long[] _itemPositions;

        private Func<T, T> _transformer;

        private T _lastAccessedItem;
        private int _lastAccessedItemIndex;

        public DeferredList(JsonSerializer jsonSerializer, JsonPositionedTextReader reader, bool buildPositionsNow = true)
        {
            _jsonSerializer = jsonSerializer;
            _streamProvider = reader.StreamProvider;
            _start = reader.TokenPosition;
            _count = -1;
            _lastAccessedItemIndex = -1;

            if (buildPositionsNow)
            {
                BuildPositions(reader, 0);
            }
            else
            {
                // We have to skip the array; it is free to get the count now so we don't have to walk again to get it.
                CountOnly(reader);
            }
        }

        private void CountOnly(JsonPositionedTextReader reader)
        {
            int count = 0;

            // StartArray
            reader.Read();

            while (true)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndArray) { break; }

                count++;
                reader.Skip();
            }

            _count = count;
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
            List<long> positions = new List<long>();

            while (true)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndArray) { break; }

                positions.Add(currentOffset + reader.TokenPosition);
                reader.Skip();
            }

            _itemPositions = positions.ToArray();
            _count = positions.Count;
        }

        /// <summary>
        ///  Add a just-in-time method to transform Results after deserialization but before getter returns them.
        /// </summary>
        /// <param name="transformer">Method to change Result instance and return updated Result</param>
        public void AddTransformer(Func<T, T> transformer)
        {
            if (_transformer == null)
            {
                _transformer = transformer;
            }
            else
            {
                // Run the new transformer after all previous transformers.
                //  [Deseralized Result] -> Transformers in order added -> Result returned from enumeration
                _transformer = (item) => transformer(_transformer(item));
            }
        }

        public int Count
        {
            get
            {
                if (_count < 0) { EnsurePositionsBuilt(); }
                return _count;
            }
        }

        public bool IsReadOnly => true;

        public T this[int index]
        {
            get
            {
                EnsurePositionsBuilt();
                if (_stream == null) { _stream = _streamProvider(); }

                if (index < 0 || index > _itemPositions.Length) { throw new IndexOutOfRangeException("index"); }

                // Return last, if re-requested
                if (index == _lastAccessedItemIndex) { return _lastAccessedItem; }

                // Seek to the item
                long position = _itemPositions[index];
                _stream.Seek(position, SeekOrigin.Begin);

                // Build a JsonTextReader
                using (JsonInnerTextReader reader = new JsonInnerTextReader(new StreamReader(_stream)))
                {
                    reader.CloseInput = false;
                    reader.Read();

                    // Deserialize and transform the item, if required
                    T item = _jsonSerializer.Deserialize<T>(reader);
                    if (_transformer != null) { item = _transformer(item); }

                    _lastAccessedItemIndex = index;
                    _lastAccessedItem = item;
                    return item;
                }
            }

            set
            {
                // Don't throw if setting same instance getter returned (SarifRewritingVisitor on DeferredList)
                if (index == _lastAccessedItemIndex && object.ReferenceEquals(value, _lastAccessedItem)) { return; }

                throw new NotSupportedException();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex + this.Count > array.Length) { throw new ArgumentOutOfRangeException("arrayIndex"); }

            int index = arrayIndex;
            foreach (T item in this)
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

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            _stream?.Dispose();
            _stream = null;
        }

        private class JsonDeferredListEnumerator<U> : IEnumerator<U>
        {
            private readonly JsonSerializer _jsonSerializer;
            private readonly Func<Stream> _streamProvider;
            private readonly long _start;

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
                if (_jsonTextReader.TokenType == JsonToken.EndArray) { return false; }

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
                _jsonTextReader = new JsonInnerTextReader(new StreamReader(_stream));

                // StartArray
                _jsonTextReader.Read();

                // StartObject of first member
                _jsonTextReader.Read();
            }
        }
    }
}

﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    ///  Cache is a simple LRU (least recently used) collection.
    ///  The base class libraries don't offer one without an extra dependency.
    /// </summary>
    /// <typeparam name="TKey">Type of Keys in collection</typeparam>
    /// <typeparam name="TValue">Type of Values being cached in collection</typeparam>
    public class Cache<TKey, TValue> where TKey : IComparable<TKey>
    {
        private readonly Func<TKey, TValue> _builder;
        private readonly ConcurrentDictionary<TKey, TValue> _cache;
        private readonly LinkedList<TKey> _keysInUseOrder;

        public const int DefaultCapacity = 100;
        public int Capacity { get; }

        /// <summary>
        ///  Construct a new Cache
        /// </summary>
        /// <param name="builder">Method to build a value from a key when it isn't in the cache</param>
        /// <param name="capacity">Maximum number of items to keep in cache, zero for no limit</param>
        public Cache(Func<TKey, TValue> builder, int capacity = DefaultCapacity)
        {
            _builder = builder;
            _cache = new ConcurrentDictionary<TKey, TValue>();
            _keysInUseOrder = new LinkedList<TKey>();
            Capacity = capacity;
        }

        /// <summary>
        ///  Returns the number of items currently cached
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        ///  Returns the keys of items currently cached
        /// </summary>
        public IEnumerable<TKey> Keys => _cache.Keys;

        /// <summary>
        ///  Return value for a given key, either from the cache or after rebuilding it.
        /// </summary>
        /// <param name="key">Key for which to retrieve value</param>
        /// <returns>Value for key</returns>
        public TValue this[TKey key]
        {
            get
            {
                TValue value = default;

                lock (_keysInUseOrder)
                {
                    if (!_cache.TryGetValue(key, out value))
                    {
                        // Build and add the new item to cache
                        value = _builder(key);

                        SetValue(key, value);
                    }
                    else
                    {
                        // When an in-cache key is retrieved, move it back to the start of the order used set, if not already most recent
                        if (Capacity > 0 && key.CompareTo(_keysInUseOrder.First.Value) != 0)
                        {
                            _keysInUseOrder.Remove(key);
                            _keysInUseOrder.AddFirst(key);
                        }
                    }
                }

                return value;
            }
            set => SetValue(key, value);
        }

        private void SetValue(TKey key, TValue value)
        {
            // If cache full, remove least recently used item
            if (Capacity > 0 && _cache.Count >= Capacity)
            {
                TKey oldest = _keysInUseOrder.Last.Value;

                _keysInUseOrder.RemoveLast();
                _cache.TryRemove(oldest, out _);
            }

            _cache[key] = value;
            _keysInUseOrder.AddFirst(key);
        }

        /// <summary>
        ///  Return whether the cache currently includes the value for the given key.
        /// </summary>
        /// <param name="key">Key to check cache for</param>
        /// <returns>True if value in cache, False otherwise</returns>
        public bool ContainsKey(TKey key)
        {
            lock (_keysInUseOrder)
            {
                return _cache.ContainsKey(key);
            }
        }

        /// <summary>
        /// Clear current cache.
        /// </summary>
        public void Clear()
        {
            lock (_keysInUseOrder)
            {
                this._cache.Clear();
                this._keysInUseOrder.Clear();
            }
        }
    }
}

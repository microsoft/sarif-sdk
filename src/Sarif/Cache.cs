// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
        private readonly Dictionary<TKey, TValue> _cache;
        private readonly LinkedList<TKey> _keysInUseOrder;

        public const int DefaultCapacity = 100;
        public int Capacity { get; private set; }

        /// <summary>
        ///  Construct a new Cache
        /// </summary>
        /// <param name="builder">Method to build a value from a key when it isn't in the cache</param>
        /// <param name="capacity">Maximum number of items to keep in cache, zero for no limit</param>
        public Cache(Func<TKey, TValue> builder, int capacity = DefaultCapacity)
        {
            _builder = builder;
            _cache = new Dictionary<TKey, TValue>();
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
        public IEnumerable<TKey> CachedKeys => _cache.Keys;

        /// <summary>
        ///  Return value for a given key, either from the cache or after rebuilding it.
        /// </summary>
        /// <param name="key">Key for which to retrieve value</param>
        /// <returns>Value for key</returns>
        public TValue this[TKey key]
        {
            get
            {
                TValue value = default(TValue);

                if (!_cache.TryGetValue(key, out value))
                {
                    // If cache full, remove least recently used item
                    if (Capacity > 0 && _cache.Count >= Capacity)
                    {
                        TKey oldest = _keysInUseOrder.First.Value;
                        _keysInUseOrder.RemoveFirst();
                        _cache.Remove(oldest);
                    }

                    // Build and add the new item to cache
                    value = _builder(key);
                    _cache[key] = value;
                    _keysInUseOrder.AddLast(key);
                }
                else
                {
                    // When an in-cache key is retrieved, move it back to the end of the order used set, if not already most recent
                    if (Capacity > 0 && key.CompareTo(_keysInUseOrder.Last.Value) != 0)
                    {
                        _keysInUseOrder.Remove(key);
                        _keysInUseOrder.AddLast(key);
                    }
                }

                return value;
            }
        }

        /// <summary>
        ///  Return whether the cache currently includes the value for the given key.
        /// </summary>
        /// <param name="key">Key to check cache for</param>
        /// <returns>True if value in cache, False otherwise</returns>
        public bool Contains(TKey key)
        {
            return _cache.ContainsKey(key);
        }
    }
}

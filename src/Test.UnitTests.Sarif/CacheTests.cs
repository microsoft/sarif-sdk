// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    public class CacheTests
    {
        [Fact]
        public void CacheBasics()
        {
            int buildCount = 0;
            Cache<int, int> cache = new Cache<int, int>(
                (key) => { buildCount++; return key; }, 
                capacity: 2);

            // Verify empty, no keys, Contains false
            Assert.Equal(2, cache.Capacity);
            Assert.Equal(0, cache.Count);
            Assert.Empty(cache.CachedKeys);
            Assert.False(cache.Contains(10));

            // Add 10, verify added
            Assert.Equal(10, cache[10]);
            Assert.Equal(1, cache.Count);
            Assert.Equal(1, buildCount);
            Assert.Single(cache.CachedKeys);
            Assert.True(cache.Contains(10));
            Assert.False(cache.Contains(15));

            // Add 15, verify added
            Assert.Equal(15, cache[15]);
            Assert.Equal(2, cache.Count);
            Assert.Equal(2, buildCount);
            Assert.True(cache.Contains(10));
            Assert.True(cache.Contains(15));

            // Request 10 - verify returned from cache
            Assert.Equal(10, cache[10]);
            Assert.Equal(2, cache.Count);
            Assert.Equal(2, buildCount);

            // Add 20, verify 15 evicted, 10 kept (least recently used)
            Assert.Equal(20, cache[20]);
            Assert.Equal(2, cache.Count);
            Assert.Equal(3, buildCount);
            Assert.True(cache.Contains(10));
            Assert.False(cache.Contains(15));
            Assert.True(cache.Contains(20));
        }
    }
}

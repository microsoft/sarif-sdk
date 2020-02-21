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
                (key) => { buildCount++; return 10 * key; }, 
                capacity: 2);

            // Verify empty, no keys, Contains false
            Assert.Equal(2, cache.Capacity);
            Assert.Equal(0, cache.Count);
            Assert.Empty(cache.Keys);
            Assert.False(cache.ContainsKey(10));

            // Add 10, verify added
            Assert.Equal(100, cache[10]);
            Assert.Equal(1, cache.Count);
            Assert.Equal(1, buildCount);
            Assert.Single(cache.Keys);
            Assert.Contains(10, cache.Keys);
            Assert.True(cache.ContainsKey(10));
            Assert.False(cache.ContainsKey(15));

            // Add 15, verify added
            Assert.Equal(150, cache[15]);
            Assert.Equal(2, cache.Count);
            Assert.Equal(2, buildCount);
            Assert.True(cache.ContainsKey(10));
            Assert.True(cache.ContainsKey(15));
            Assert.Contains(15, cache.Keys);

            // Request 10 - verify returned from cache
            Assert.Equal(100, cache[10]);
            Assert.Equal(2, cache.Count);
            Assert.Equal(2, buildCount);

            // Add 20, verify 15 evicted, 10 kept (least recently used)
            Assert.Equal(200, cache[20]);
            Assert.Equal(2, cache.Count);
            Assert.Equal(3, buildCount);
            Assert.True(cache.ContainsKey(10));
            Assert.False(cache.ContainsKey(15));
            Assert.True(cache.ContainsKey(20));
            Assert.Contains(20, cache.Keys);
            Assert.DoesNotContain(15, cache.Keys);
        }
    }
}

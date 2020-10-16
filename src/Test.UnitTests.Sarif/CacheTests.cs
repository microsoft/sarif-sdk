// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
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
            cache.Capacity.Should().Be(2);
            cache.Count.Should().Be(0);
            cache.Keys.Should().BeEmpty();
            cache.ContainsKey(10).Should().BeFalse();

            // Add 10, verify added
            cache[10].Should().Be(100);
            cache.Count.Should().Be(1);
            buildCount.Should().Be(1);
            cache.Keys.Should().ContainSingle();
            cache.Keys.Should().Contain(10);
            cache.ContainsKey(10).Should().BeTrue();
            cache.ContainsKey(15).Should().BeFalse();

            // Add 15, verify added
            cache[15].Should().Be(150);
            cache.Count.Should().Be(2);
            buildCount.Should().Be(2);
            cache.ContainsKey(10).Should().BeTrue();
            cache.ContainsKey(15).Should().BeTrue();
            cache.Keys.Should().Contain(15);

            // Request 10 - verify returned from cache
            cache[10].Should().Be(100);
            cache.Count.Should().Be(2);
            buildCount.Should().Be(2);

            // Add 20, verify 15 evicted, 10 kept (least recently used)
            cache[20].Should().Be(200);
            cache.Count.Should().Be(2);
            buildCount.Should().Be(3);
            cache.ContainsKey(10).Should().BeTrue();
            cache.ContainsKey(15).Should().BeFalse();
            cache.ContainsKey(20).Should().BeTrue();
            cache.Keys.Should().Contain(20);
            cache.Keys.Should().NotContain(15);
        }
    }
}

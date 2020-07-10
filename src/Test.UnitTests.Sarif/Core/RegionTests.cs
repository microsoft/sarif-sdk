// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class RegionTests
    {
        [Fact]
        public void Region_PopulateDefaults_EndLineColumnDefaultsPopulatedCorrectly()
        {
            var region = new Region
            {
                StartLine = 5,
                StartColumn = 20
            };

            region.PopulateDefaults();

            region.StartLine.Should().Be(5);
            region.StartColumn.Should().Be(20);
            region.EndLine.Should().Be(5);
            region.EndColumn.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Region_PopulateDefaults_StartColumnDefaultsPopulatedCorrectly()
        {
            var region = new Region
            {
                StartLine = 5,
            };

            region.PopulateDefaults();

            region.StartLine.Should().Be(5);
            region.StartColumn.Should().Be(1);
            region.EndLine.Should().Be(5);
            region.EndColumn.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Region_PopulateDefaults_TextOffsetDefaultsPopulatedCorrectly()
        {
            var region = new Region
            {
                CharOffset = 5
            };

            region.PopulateDefaults();

            region.CharOffset.Should().Be(5);
            region.CharLength.Should().Be(0);
        }

        [Fact]
        public void Region_PopulateDefaults_BinaryOffsetDefaultsPopulatedCorrectly()
        {
            var region = new Region
            {
                ByteOffset = 15
            };

            region.PopulateDefaults();

            region.ByteOffset.Should().Be(15);
            region.ByteLength.Should().Be(0);
        }

        [Fact]
        public void Region_IsBinaryRegion_ComputesCorrectly()
        {
            var region = new Region
            {
                ByteOffset = 15
            };

            region.IsLineColumnBasedTextRegion.Should().BeFalse();
            region.IsOffsetBasedTextRegion.Should().BeFalse();
            region.IsBinaryRegion.Should().BeTrue();
        }

        [Fact]
        public void Region_IsLineColumnBasedTextRegion_ComputesCorrectly()
        {
            var region = new Region
            {
                StartLine = 23
            };

            region.IsLineColumnBasedTextRegion.Should().BeTrue();
            region.IsOffsetBasedTextRegion.Should().BeFalse();
            region.IsBinaryRegion.Should().BeFalse();
        }

        [Fact]
        public void Region_IsOffsetBasedTextRegion_ComputesCorrectly()
        {
            var region = new Region
            {
                CharOffset = 15
            };

            region.IsLineColumnBasedTextRegion.Should().BeFalse();
            region.IsOffsetBasedTextRegion.Should().BeTrue();
            region.IsBinaryRegion.Should().BeFalse();
        }

        [Fact]
        public void Region_HasAllRegionTypes_ComputesPropertiesCorrectly()
        {
            var region = new Region
            {
                StartLine = 23,
                CharOffset = 15,
                ByteOffset = 15

            };

            region.IsLineColumnBasedTextRegion.Should().BeTrue();
            region.IsOffsetBasedTextRegion.Should().BeTrue();
            region.IsBinaryRegion.Should().BeTrue();
        }
    }
}
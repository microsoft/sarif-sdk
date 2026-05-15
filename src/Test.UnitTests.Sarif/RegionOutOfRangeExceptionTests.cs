// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    /// <summary>
    /// Tests covering SDK-E: a typed <see cref="RegionOutOfRangeException"/> for the case
    /// where SARIF regions reference content past the bounds of the underlying artifact
    /// (submodule drift, partial clone, post-edit replay, etc.). Pre-fix the SDK propagated
    /// the underlying <see cref="ArgumentOutOfRangeException"/>, leaking framework internals
    /// to producers and making the failure both un-catchable selectively and hard to
    /// translate into a SARIF diagnostic.
    /// </summary>
    public class RegionOutOfRangeExceptionTests
    {
        [Fact]
        public void Default_Constructor_DoesNotThrow()
        {
            new RegionOutOfRangeException().Should().NotBeNull();
        }

        [Fact]
        public void StringConstructor_PreservesMessage()
        {
            var ex = new RegionOutOfRangeException("something broke");
            ex.Message.Should().Be("something broke");
        }

        [Fact]
        public void InnerExceptionConstructor_PreservesInnerException()
        {
            var inner = new ArgumentOutOfRangeException("offset");
            var ex = new RegionOutOfRangeException("wrapped", inner);

            ex.Message.Should().Be("wrapped");
            ex.InnerException.Should().BeSameAs(inner);
        }

        [Fact]
        public void RegionConstructor_FormatsLineColumnInformativeMessage()
        {
            var region = new Region { StartLine = 9999, EndLine = 9999 };
            var uri = new Uri("file:///c:/foo/bar.cs");

            var ex = new RegionOutOfRangeException(region, uri);

            ex.Message.Should().Contain("startLine=9999");
            ex.Message.Should().Contain("foo/bar.cs");
            ex.Region.Should().BeSameAs(region);
            ex.ArtifactUri.Should().Be(uri);
        }

        [Fact]
        public void RegionConstructor_FormatsCharOffsetInformativeMessage()
        {
            var region = new Region { CharOffset = 8192, CharLength = 256 };
            var uri = new Uri("file:///c:/foo/bar.cs");

            var ex = new RegionOutOfRangeException(region, uri);

            ex.Message.Should().Contain("charOffset=8192");
            ex.Message.Should().Contain("charLength=256");
        }

        [Fact]
        public void RegionConstructor_TolerantOfNullUri()
        {
            var ex = new RegionOutOfRangeException(new Region { StartLine = 1 }, artifactUri: null);
            ex.Message.Should().Contain("<unknown artifact>");
        }

        [Fact]
        public void RegionConstructor_TolerantOfNullRegion()
        {
            var ex = new RegionOutOfRangeException(region: null, artifactUri: new Uri("file:///x"));
            ex.Message.Should().Contain("references content outside the bounds");
        }
    }
}

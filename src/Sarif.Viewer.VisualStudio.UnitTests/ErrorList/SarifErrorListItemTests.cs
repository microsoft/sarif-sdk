// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class SarifErrorListItemTests
    {
        public SarifErrorListItemTests()
        {
            TestUtilities.InitializeTestEnvironment();
        }

        [Fact]
        public void SarifErrorListItem_WhenRegionHasStartLine_HasLineMarker()
        {
            var item = new SarifErrorListItem
            {
                FileName = "file.ext",
                Region = new Region
                {
                    StartLine = 5
                }
            };

            var lineMarker = item.LineMarker;

            lineMarker.Should().NotBe(null);
        }

        [Fact]
        public void SarifErrorListItem_WhenRegionHasNoStartLine_HasNoLineMarker()
        {
            var item = new SarifErrorListItem
            {
                FileName = "file.ext",
                Region = new Region
                {
                    Offset = 20
                }
            };

            var lineMarker = item.LineMarker;

            lineMarker.Should().Be(null);
        }

        [Fact]
        public void SarifErrorListItem_WhenRegionIsAbsent_HasNoLineMarker()
        {
            var item = new SarifErrorListItem
            {
                FileName = "file.ext"
            };

            var lineMarker = item.LineMarker;

            lineMarker.Should().Be(null);
        }
    }
}
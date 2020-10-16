// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Driver.Sdk
{
    public class SkimmerBaseTests
    {
        [Fact]
        public void SkimmerBase_GetsPlainTextMessageStringsFromResources()
        {
            var skimmer = new TestRule();

            skimmer.MessageStrings.Count.Should().Be(new TestRule().MessageStrings.Count());
            skimmer.MessageStrings["Failed"].Text.Should().Be(SkimmerBaseTestResources.TEST1001_Failed);
            skimmer.MessageStrings["Note"].Text.Should().Be(SkimmerBaseTestResources.TEST1001_Note);
            skimmer.MessageStrings["Pass"].Text.Should().Be(SkimmerBaseTestResources.TEST1001_Pass);
            skimmer.MessageStrings["Open"].Text.Should().Be(SkimmerBaseTestResources.TEST1001_Open);
            skimmer.MessageStrings["Review"].Text.Should().Be(SkimmerBaseTestResources.TEST1001_Review);
            skimmer.MessageStrings["Information"].Text.Should().Be(SkimmerBaseTestResources.TEST1001_Information);
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Resources;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver.UnitTests.Sdk
{
    internal class TestSkimmer : TestSkimmerBase
    {
        public override string Id => "TST0001";

        protected override ResourceManager ResourceManager => SkimmerBaseTestResources.ResourceManager;

        protected override IEnumerable<string> MessageResourceNames => new List<string>
        {
            nameof(SkimmerBaseTestResources.TST0001_Pass),
            nameof(SkimmerBaseTestResources.TST0001_Error)
        };
    }

    public class SkimmerBaseTests
    {
        [Fact]
        public void SkimmerBase_GetsPlainAndRichMessageStringsFromResources()
        {
            var skimmer = new TestSkimmer();

            skimmer.MessageStrings.Count.Should().Be(2);
            skimmer.MessageStrings["Pass"].Text.Should().Be(SkimmerBaseTestResources.TST0001_Pass);
            skimmer.MessageStrings["Error"].Text.Should().Be(SkimmerBaseTestResources.TST0001_Error);

            skimmer.MessageStrings["Pass"].Markdown.Should().Be(SkimmerBaseTestResources.TST0001_Markdown_Pass);
            skimmer.MessageStrings["Error"].Markdown.Should().Be(SkimmerBaseTestResources.TST0001_Markdown_Error);
        }
    }
}

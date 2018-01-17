// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Resources;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    internal class TestSkimmer : TestSkimmerBase
    {
        public override string Id => "TST0001";

        protected override ResourceManager ResourceManager => SkimmerBaseTestResources.ResourceManager;

        protected override IEnumerable<string> TemplateResourceNames => new List<string>
        {
            nameof(SkimmerBaseTestResources.TST0001_Pass),
            nameof(SkimmerBaseTestResources.TST0001_Error)
        };

        protected override IEnumerable<string> RichTemplateResourceNames => new List<string>
        {
            nameof(SkimmerBaseTestResources.TST0001_Rich_Pass),
            nameof(SkimmerBaseTestResources.TST0001_Rich_Error)
        };
    }

    public class SkimmerBaseTests
    {
        [Fact]
        public void SkimmerBase_GetsPlainAndRichMessageTemplatesFromResources()
        {
            var skimmer = new TestSkimmer();

            skimmer.MessageTemplates.Count.Should().Be(2);
            skimmer.MessageTemplates["Pass"].Should().Be(SkimmerBaseTestResources.TST0001_Pass);
            skimmer.MessageTemplates["Error"].Should().Be(SkimmerBaseTestResources.TST0001_Error);

            skimmer.RichMessageTemplates.Count.Should().Be(2);
            skimmer.RichMessageTemplates["Pass"].Should().Be(SkimmerBaseTestResources.TST0001_Rich_Pass);
            skimmer.RichMessageTemplates["Error"].Should().Be(SkimmerBaseTestResources.TST0001_Rich_Error);
        }
    }
}

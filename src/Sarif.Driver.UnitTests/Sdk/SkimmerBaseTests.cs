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

        protected override IEnumerable<string> TemplateResourceIds => new List<string>
        {
            "TST0001_Pass",
            "TST0001_Error"
        };

        protected override IEnumerable<string> RichTemplateResourceIds => new List<string>
        {
            "TST0001_Rich_Pass",
            "TST0001_Rich_Error"
        };
    }

    public class SkimmerBaseTests
    {
        [Fact]
        public void SkimmerBase_GetsPlainAndRichMessageTemplatesFromResources()
        {
            var skimmer = new TestSkimmer();

            skimmer.MessageTemplates.Count.Should().Be(2);
            skimmer.MessageTemplates["Pass"].Should().Be("This test plainly passed.");
            skimmer.MessageTemplates["Error"].Should().Be("This test plainly failed.");

            skimmer.RichMessageTemplates.Count.Should().Be(2);
            skimmer.RichMessageTemplates["Pass"].Should().Be("This test **richly** _passed_.");
            skimmer.RichMessageTemplates["Error"].Should().Be("This test **richly** _failed_.");
        }
    }
}

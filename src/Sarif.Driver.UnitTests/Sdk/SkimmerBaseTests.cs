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

        private readonly string[] _emptyMessageStringIds = new string[0];
        public override IEnumerable<string> MessageStringIds => _emptyMessageStringIds;

        protected override ResourceManager ResourceManager => SkimmerBaseTestResources.ResourceManager;

        protected override IEnumerable<string> MessageResourceNames => new List<string>
        {
            nameof(SkimmerBaseTestResources.TST0001_Pass),
            nameof(SkimmerBaseTestResources.TST0001_Error)
        };

        protected override IEnumerable<string> RichMessageResourceNames => new List<string>
        {
            nameof(SkimmerBaseTestResources.TST0001_Rich_Pass),
            nameof(SkimmerBaseTestResources.TST0001_Rich_Error)
        };
    }

    public class SkimmerBaseTests
    {
        [Fact]
        public void SkimmerBase_GetsPlainAndRichMessageStringsFromResources()
        {
            var skimmer = new TestSkimmer();

            skimmer.MessageStrings.Count.Should().Be(2);
            skimmer.MessageStrings["Pass"].Should().Be(SkimmerBaseTestResources.TST0001_Pass);
            skimmer.MessageStrings["Error"].Should().Be(SkimmerBaseTestResources.TST0001_Error);

            skimmer.RichMessageStrings.Count.Should().Be(2);
            skimmer.RichMessageStrings["Pass"].Should().Be(SkimmerBaseTestResources.TST0001_Rich_Pass);
            skimmer.RichMessageStrings["Error"].Should().Be(SkimmerBaseTestResources.TST0001_Rich_Error);
        }
    }
}

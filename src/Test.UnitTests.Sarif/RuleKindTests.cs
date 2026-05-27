// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Test.UnitTests.Sarif
{
    public class RuleKindTests
    {
        [Fact]
        public void RuleKind_AdoAlias_SharesUnderlyingValueWithGHAzDO()
        {
#pragma warning disable CS0618
            ((int)RuleKind.Ado).Should().Be((int)RuleKind.GHAzDO);
            RuleKind.Ado.Should().Be(RuleKind.GHAzDO);
#pragma warning restore CS0618
        }

        [Fact]
        public void RuleKind_AdoAlias_ParsesCaseInsensitively()
        {
            AssertParsesToGHAzDO("Ado");
            AssertParsesToGHAzDO("ado");
            AssertParsesToGHAzDO("ADO");
        }

        private static void AssertParsesToGHAzDO(string input)
        {
            RuleKind parsed = (RuleKind)Enum.Parse(typeof(RuleKind), input, ignoreCase: true);
            parsed.Should().Be(RuleKind.GHAzDO);
        }
    }
}

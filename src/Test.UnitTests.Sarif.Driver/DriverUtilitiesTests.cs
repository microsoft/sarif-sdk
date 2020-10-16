// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CommandLine;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class DriverUtilitiesTests
    {
        private class TestOptions
        {
            [Option('s')]
            public int HasShortNameOnly { get; set; }

            [Option("l")]
            public int HasLongNameOnly { get; set; }

            [Option('b', "both-names")]
            public int HasBothNames { get; set; }

            [Option('s')]
            public static int StaticProperty { get; set; }

            [Option('n')]
            internal int NonPublicProperty { get; set; }
        }

        [Theory]
        [InlineData(nameof(TestOptions.HasShortNameOnly), "s")]
        [InlineData(nameof(TestOptions.HasLongNameOnly), "l")]
        [InlineData(nameof(TestOptions.HasBothNames), "b, both-names")]
        [InlineData(nameof(TestOptions.StaticProperty), null)]
        [InlineData(nameof(TestOptions.NonPublicProperty), null)]
        public void GetOptionDescription_ConstructsCorrectDescription(string propertyName, string expectedDescription)
        {
            if (expectedDescription == null)
            {
                Action action = () => DriverUtilities.GetOptionDescription<TestOptions>(propertyName);

                action.Should().Throw<ArgumentException>().WithMessage($"*{propertyName}*");
            }
            else
            {
                string description = DriverUtilities.GetOptionDescription<TestOptions>(propertyName);

                description.Should().Be(expectedDescription);
            }
        }
    }
}

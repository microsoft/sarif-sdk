// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using FluentAssertions.Execution;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Test.UnitTests.Sarif
{
    public class RuntimeConditionsTests
    {
        [Fact]
        public void RuntimeConditions_EnsureFatalConditionsAreReserved()
        {
            using var assertionScope = new AssertionScope();

            RuntimeConditions condition;

            // We reserve the first 32 bits of this enum as fatal conditions.
            for (int i = 0; i < 32; i++)
            {
                condition = (RuntimeConditions)(1L << i);

                (condition & RuntimeConditions.Fatal).Should().Be(condition);
                condition.Fatal().Should().Be(condition);

                (condition & RuntimeConditions.Nonfatal).Should().Be(0);
                condition.Nonfatal().Should().Be(0);
            }
        }

        [Fact]
        public void RuntimeConditions_EnsureNonfatalConditionsAreReserved()
        {
            using var assertionScope = new AssertionScope();

            RuntimeConditions condition;

            // We use the most significant 32 bits of the enum for non-fatal conditions.
            // We use a long for this enum to make it as .NET friendly as possible.
            // As a result, the sign bit is not valid for use.
            for (int i = 32; i < 63; i++)
            {
                condition = (RuntimeConditions)(1L << i);

                (condition & RuntimeConditions.Nonfatal).Should().Be(condition);
                condition.Nonfatal().Should().Be(condition);

                (condition & RuntimeConditions.Fatal).Should().Be(0);
                condition.Fatal().Should().Be(0);
            }
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Baseline
{
    public class TrustMapTests
    {
        public const string Rule = "Rule001";
        public const string Location1 = @"src\Program.cs";
        public const string Location2 = @"src\Core\Result.cs";
        public const string Set = "PartialFingerprints";
        public const string NameGreat = "GreatFingerprint/v1";
        public const string NameMedium = "OkayFingerprint/v1";
        public const string NameNoMatches = "NoMatchesFingerprint/v1";
        public const string NameConstant = "ConstantFingerprint/v1";

        [Fact]
        public void TrustMap_Basics()
        {
            // Create a TrustMap for both of the Runs being compared
            TrustMap beforeMap = new TrustMap();
            TrustMap afterMap = new TrustMap();

            // All properties are unknown to start
            beforeMap.Trust(Set, NameGreat).Should().Be(TrustMap.DefaultTrust);

            for (int i = 0; i < 20; ++i)
            {
                // Add a fingerprint which is always unique and all of which match
                beforeMap.Add(new WhatComponent(Rule, Location1, Set, NameGreat, i.ToString()));
                afterMap.Add(new WhatComponent(Rule, Location2, Set, NameGreat, i.ToString()));

                // Add a fingerprint which is somewhat unique and always matches
                beforeMap.Add(new WhatComponent(Rule, Location1, Set, NameMedium, (i % 3).ToString()));
                afterMap.Add(new WhatComponent(Rule, Location2, Set, NameMedium, (i % 3).ToString()));

                // Add a fingerprint which is always unique but has no matches
                beforeMap.Add(new WhatComponent(Rule, Location1, Set, NameNoMatches, i.ToString()));
                afterMap.Add(new WhatComponent(Rule, Location2, Set, NameNoMatches, (100 + i).ToString()));

                // Add a fingerprint with constant value
                beforeMap.Add(new WhatComponent(Rule, Location1, Set, NameConstant, "Constant"));
                afterMap.Add(new WhatComponent(Rule, Location2, Set, NameConstant, "Constant"));
            }

            // Before comparing the runs, trust is based on uniqueness only
            beforeMap.Trust(Set, NameGreat).Should().Be(1.0f);
            beforeMap.Trust(Set, NameMedium).Should().Be(3.0f / 20.0f);
            beforeMap.Trust(Set, NameNoMatches).Should().Be(1.0f);
            beforeMap.Trust(Set, NameConstant).Should().Be(0.0f);

            // Match the two runs to allow trust scoring based on matches
            beforeMap.WasMatched.Should().BeFalse();
            afterMap.WasMatched.Should().BeFalse();
            afterMap.CountMatchesWith(beforeMap);
            beforeMap.WasMatched.Should().BeTrue();
            afterMap.WasMatched.Should().BeTrue();

            // After comparing runs, trust is based on uniqueness and number of matches
            // Note: Attributes with only one value (constants) have zero trust.
            // Note: Attributes which are unique but never match have low (default) trust.
            beforeMap.Trust(Set, NameGreat).Should().Be(1.0f);
            beforeMap.Trust(Set, NameMedium).Should().Be(3.0f / 20.0f);
            beforeMap.Trust(Set, NameNoMatches).Should().Be(TrustMap.DefaultTrust);
            beforeMap.Trust(Set, NameConstant).Should().Be(0.0f);

            // Same scores from both maps
            afterMap.Trust(Set, NameGreat).Should().Be(beforeMap.Trust(Set, NameGreat));
            afterMap.Trust(Set, NameMedium).Should().Be(beforeMap.Trust(Set, NameMedium));
            afterMap.Trust(Set, NameNoMatches).Should().Be(beforeMap.Trust(Set, NameNoMatches));
            afterMap.Trust(Set, NameConstant).Should().Be(beforeMap.Trust(Set, NameConstant));

            // Unknown properties are still unknown
            afterMap.Trust("Set2", NameConstant).Should().Be(TrustMap.DefaultTrust);
        }
    }
}

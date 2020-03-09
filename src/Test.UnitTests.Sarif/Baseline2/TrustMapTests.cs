// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
            Assert.Equal(TrustMap.DefaultTrust, beforeMap.Trust(Set, NameGreat));

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
            Assert.Equal(1.0f, beforeMap.Trust(Set, NameGreat));
            Assert.Equal((3.0f / 20.0f), beforeMap.Trust(Set, NameMedium));
            Assert.Equal(1.0f, beforeMap.Trust(Set, NameNoMatches));
            Assert.Equal(0.0f, beforeMap.Trust(Set, NameConstant));

            // Match the two runs to allow trust scoring based on matches
            Assert.False(beforeMap.WasMatched);
            Assert.False(afterMap.WasMatched);
            afterMap.CountMatchesWith(beforeMap);
            Assert.True(beforeMap.WasMatched);
            Assert.True(afterMap.WasMatched);

            // After comparing runs, trust is based on uniqueness and number of matches
            // Note: Constants have no trust, unique and no match have low (default) trust.
            Assert.Equal(1.0f, beforeMap.Trust(Set, NameGreat));
            Assert.Equal((3.0f / 20.0f), beforeMap.Trust(Set, NameMedium));
            Assert.Equal(TrustMap.DefaultTrust, beforeMap.Trust(Set, NameNoMatches));
            Assert.Equal(0.0f, beforeMap.Trust(Set, NameConstant));

            // Same scores from both maps
            Assert.Equal(1.0f, afterMap.Trust(Set, NameGreat));
            Assert.Equal((3.0f / 20.0f), afterMap.Trust(Set, NameMedium));
            Assert.Equal(TrustMap.DefaultTrust, afterMap.Trust(Set, NameNoMatches));
            Assert.Equal(0.0f, afterMap.Trust(Set, NameConstant));

            // Unknown properties are still unknown
            Assert.Equal(TrustMap.DefaultTrust, afterMap.Trust("Set2", NameConstant));
        }
    }
}

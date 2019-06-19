using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public class WhatComparerTests
    {
        [Fact]
        public void WhatComparer_Basics()
        {
            Result left = new Result() { RuleId = "Rule1", Message = new Message() { Text = "One" } };
            Result right = new Result() { RuleId = "Rule1", Message = new Message() { Text = "Two" } };
            ExtractedResult eLeft = new ExtractedResult(left, null);
            ExtractedResult eRight = new ExtractedResult(right, null);

            // GUIDs
            // =====
            Guid g1 = Guid.NewGuid(), g2 = Guid.NewGuid();

            // Match if Guids match
            left.Guid = g1.ToString();
            right.Guid = g1.ToString();
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Don't match without Guids
            right.Guid = g2.ToString();
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            left.Guid = null;
            right.Guid = null;

            // Fingerprints
            // ============
            left.Fingerprints = new Dictionary<string, string>()
            {
                ["First"] = "0001",
                ["Second"] = "1001"
            };

            right.Fingerprints = new Dictionary<string, string>()
            {
                ["First"] = "0002",
                ["Second"] = "1002"
            };

            // Don't match if no fingerprints match
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            // Match if any fingerprint matches
            right.Fingerprints["Second"] = "1001";
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Don't match if fingerprints with different names have the same value
            right.Fingerprints["Second"] = "0001";
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            // Handle different fingerprint names smoothly
            right.Fingerprints["Third"] = "2002";
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            left.Fingerprints = null;
            right.Fingerprints = null;

            // PartialFingerprints
            // ===================

            left.PartialFingerprints = new Dictionary<string, string>();
            right.PartialFingerprints = new Dictionary<string, string>();

            // Don't match with no matches
            left.PartialFingerprints["First"] = "0001";
            right.PartialFingerprints["First"] = "1001";
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            // Match if only one matches
            right.PartialFingerprints["First"] = "0001";
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Match if one of two (50%) match
            left.PartialFingerprints["Second"] = "0002";
            right.PartialFingerprints["Second"] = "1002";
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Don't match if one of three (under 50%) match
            left.PartialFingerprints["Third"] = "0003";
            right.PartialFingerprints["Third"] = "1003";
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            // Match for two of four matches
            left.PartialFingerprints["Fourth"] = "0004";
            right.PartialFingerprints["Fourth"] = "0004";
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Don't match when names are different
            left.PartialFingerprints["Fifth_Left"] = "0005";
            right.PartialFingerprints["Fifth_Right"] = "0005";
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            left.PartialFingerprints = null;
            right.PartialFingerprints = null;

            // Fallback
            // ========

            // Don't match via fallback (Message doesn't match)
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            // Verify match if messages match
            right.Message.Text = "One";
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Verify no match if not enough matching Properties

            // Don't match with if only Property is a non-match
            left.SetProperty("First", "0001");
            right.SetProperty("First", "1001");
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            // Match if only one matches
            right.SetProperty("First", "0001");
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Match if one of two (50%) matches
            left.SetProperty("Second", "0002");
            right.SetProperty("Second", "1002");
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Don't match if one of three (under 50%) match
            left.SetProperty("Third", "0003");
            right.SetProperty("Third", "1003");
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            // Match for two of four matches
            left.SetProperty("Fourth", "0004");
            right.SetProperty("Fourth", "0004");
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Don't match when names are different
            left.SetProperty("Fifth_Left", "0005");
            right.SetProperty("Fifth_Right", "0005");
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));

            left.Properties.Clear();
            right.Properties.Clear();
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            // Verify snippets must also match
            Location location = new Location()
            {
                PhysicalLocation = new PhysicalLocation()
                {
                    Region = new Region()
                    {
                        Snippet = new ArtifactContent() { Text = "Sample" }
                    }
                }
            };

            left.Locations = new List<Location>() { location };
            right.Locations = new List<Location>() { location.DeepClone() };
            Assert.True(WhatComparer.MatchesWhat(eLeft, eRight));

            right.Locations[0].PhysicalLocation.Region.Snippet.Text = "NewSample";
            Assert.False(WhatComparer.MatchesWhat(eLeft, eRight));
        }
    }
}

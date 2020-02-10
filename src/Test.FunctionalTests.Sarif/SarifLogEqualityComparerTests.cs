// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.FunctionalTests
{
    public class SarifLogEqualityComparerTests
    {
        // This test exercises the generated equality comparers by ensuring that a log compares
        // equal to itself. We introduced this test because of Bug #1046, where the comparison
        // failed because an incorrect code generation hint caused Rule objects to be compared
        // by reference rather than by invoking the RuleEqualityComparer.
        [Fact(DisplayName = nameof(ValueEquals_ReturnsTrueForTwoIdenticalLogObjects))]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1046")]
        public void ValueEquals_ReturnsTrueForTwoIdenticalLogObjects()
        {
            const string ComprehensiveTestSamplePath = @"v2\SpecExamples\Comprehensive.sarif";
            string comprehensiveTestSampleContents = File.ReadAllText(ComprehensiveTestSamplePath);

            PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(comprehensiveTestSampleContents, formatting: Formatting.None, out comprehensiveTestSampleContents);

            SarifLog expectedLog = JsonConvert.DeserializeObject<SarifLog>(comprehensiveTestSampleContents);
            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(comprehensiveTestSampleContents);

            expectedLog.ValueEquals(actualLog).Should().BeTrue();
        }
    }
}

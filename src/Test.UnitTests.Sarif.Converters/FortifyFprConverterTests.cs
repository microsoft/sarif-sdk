// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FortifyFprConverterTests : FileDiffingUnitTests, IClassFixture<FortifyFprConverterTests.FortifyFprConverterTestsFixture>
    {
        public class FortifyFprConverterTestsFixture : DeletesOutputsDirectoryOnClassInitializationFixture { }

        protected override string TestLogResourceNameRoot => "Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Converters.TestData." + TypeUnderTest;

        public FortifyFprConverterTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        protected override string ConstructTestOutputFromInputResource(string inputResource)
        {
            byte[] fprData = GetResourceBytes(inputResource);

            var converter = new FortifyFprConverter();
            return Utilities.GetConverterJson(converter, fprData);
        }

        [Fact]
        public void FortifyFprConverter_Convert_OneResultBasic()
        {
            RunTest("OneResultBasic.fpr");
        }

        [Fact]
        public void FortifyFprConverter_Convert_OneResultWithTwoTraces()
        {
            RunTest("OneResultWithTwoTraces.fpr");
        }

        [Fact]
        public void FortifyFprConverter_Convert_TwoResultsWithNodeRefs()
        {
            RunTest("TwoResultsWithNodeRefs.fpr");
        }

        [Fact]
        public void FortifyFprConverter_Convert_ScanWithFailureLevelMatrices()
        {
            RunTest("ScanWithFailureLevelMatrices.fpr");
        }

        [Fact]
        public void FortifyFprConverter_GetFailureLevelFromRuleMetadata_MissingImpactProperty_ReturnsWarning()
        {
            ReportingDescriptor rule = new ReportingDescriptor();

            FailureLevel level = FortifyFprConverter.GetFailureLevelFromRuleMetadata(rule);

            level.Should().Be(FailureLevel.Warning);
        }

        [Fact]
        public void FortifyFprConverter_GetFailureLevelFromRuleMetadata_ReturnsAppropriateFailureLevel()
        {
            var expectedInputOutputs = new Dictionary<string, FailureLevel>
            {
                { "0.0", FailureLevel.Note },
                { "0.5", FailureLevel.Note },
                { "1.0", FailureLevel.Note },

                { "1.1", FailureLevel.Warning },
                { "2.0", FailureLevel.Warning },
                { "2.5", FailureLevel.Warning },
                { "2.9", FailureLevel.Warning },
                { "3.0", FailureLevel.Warning },

                { "3.1", FailureLevel.Error },
                { "3.5", FailureLevel.Error },
                { "3.9", FailureLevel.Error },
                { "4.5", FailureLevel.Error },
                { "5.0", FailureLevel.Error },

                { "-5.5", FailureLevel.Warning }, //Invalid value, we default it to Warning
                { "5.5", FailureLevel.Error }, // Invalid value, we guess that it should be treated as Error

            };

            foreach(KeyValuePair<string,FailureLevel> keyValuePair in expectedInputOutputs)
            {
                ReportingDescriptor rule = new ReportingDescriptor();
                rule.SetProperty<string>("Impact", keyValuePair.Key);

                FailureLevel level = FortifyFprConverter.GetFailureLevelFromRuleMetadata(rule);

                level.Should().Be(keyValuePair.Value);
            }
            
        }
    }
}

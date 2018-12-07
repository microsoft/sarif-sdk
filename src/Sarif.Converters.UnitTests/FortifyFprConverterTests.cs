// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FortifyFprConverterTests : FileDiffingTests
    {
        protected override string TestLogResourceNameRoot => "Microsoft.CodeAnalysis.Sarif.Converters.UnitTests.TestData." + TypeUnderTest;

        public FortifyFprConverterTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        private void VerifyFortifyFprConversionFromResource(string fprResourceName, string expectedSarifResourceName = null)
        {
            expectedSarifResourceName = expectedSarifResourceName ?? fprResourceName;
            fprResourceName += ".fpr";
            expectedSarifResourceName += ".sarif";

            byte[] fprData = GetResourceBytes(fprResourceName);
            string expectedSarifText = GetResourceText(expectedSarifResourceName);
            expectedSarifText = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(expectedSarifText);

            var converter = new FortifyFprConverter();
            string actualSarifText = Utilities.GetConverterJson(converter, fprData);

            var sb = new StringBuilder();

            string expectedFilePath = null;
            string actualFilePath = null;

            if (!Utilities.RunningInAppVeyor)
            {
                expectedFilePath = GetOutputFilePath("expected", expectedSarifResourceName);
                actualFilePath = GetOutputFilePath("actual", expectedSarifResourceName);
            }

            if (!AreEquivalentSarifLogs<SarifLog>(actualSarifText, expectedSarifText))
            {
                string errorMessage = string.Format(@"Conversion from Fortify FPR to SARIF produced unexpected diffs for test: '{0}'.", fprResourceName);
                sb.AppendLine(errorMessage);

                if (!Utilities.RunningInAppVeyor)
                {
                    string expectedRootDirectory = Path.GetDirectoryName(expectedFilePath);
                    string actualRootDirectory = Path.GetDirectoryName(actualFilePath);

                    Directory.CreateDirectory(expectedRootDirectory);
                    Directory.CreateDirectory(actualRootDirectory);

                    File.WriteAllText(expectedFilePath, expectedSarifText);
                    File.WriteAllText(actualFilePath, actualSarifText);

                    sb.AppendLine("Check individual differences with:");
                    sb.AppendLine(GenerateDiffCommand(expectedFilePath, actualFilePath) + Environment.NewLine);

                    sb.AppendLine("To compare all difference for this test suite:");
                    sb.AppendLine(GenerateDiffCommand(Path.GetDirectoryName(expectedFilePath), Path.GetDirectoryName(actualFilePath)) + Environment.NewLine);
                }

                ValidateResults(sb.ToString());
            }
        }

        [Fact]
        public void FortifyFprConverter_Convert_OneResultBasic()
        {
            VerifyFortifyFprConversionFromResource("OneResultBasic");
        }

        [Fact]
        public void FortifyFprConverter_Convert_OneResultWithTwoTraces()
        {
            VerifyFortifyFprConversionFromResource("OneResultWithTwoTraces");
        }

        [Fact]
        public void FortifyFprConverter_Convert_TwoResultsWithNodeRefs()
        {
            VerifyFortifyFprConversionFromResource("TwoResultsWithNodeRefs");
        }
    }
}

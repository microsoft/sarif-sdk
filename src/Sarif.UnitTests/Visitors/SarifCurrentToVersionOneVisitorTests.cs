// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Transformers
{
    public class SarifCurrentToVersionOneVisitorTests : FileDiffingTests
    {
        public SarifCurrentToVersionOneVisitorTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        private static SarifLog GetSarifLog(string logText)
        {
            return JsonConvert.DeserializeObject<SarifLog>(logText);
        }

        private static SarifLogVersionOne TransformCurrentToVersionOne(string v2LogText)
        {
            SarifLog v2Log = GetSarifLog(v2LogText);

            var transformer = new SarifCurrentToVersionOneVisitor
            {
                EmbedVersionTwoContentInPropertyBag = false
            };         
            transformer.VisitSarifLog(v2Log);

            return transformer.SarifLogVersionOne;
        }

        private bool s_Rebaseline = false;

        private void VerifyCurrentToVersionOneTransformationFromResource(string v2InputResourceName, string v1ExpectedResourceName = null)
        {
            v1ExpectedResourceName = v1ExpectedResourceName ?? v2InputResourceName;

            string v2LogText = GetResourceText($"v2.{v2InputResourceName}");
            string v1ExpectedLogText = GetResourceText($"v1.{v1ExpectedResourceName}");

            v2LogText = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(v2LogText, forceUpdate: true, formatting: Formatting.Indented);

            SarifLogVersionOne v1Log = TransformCurrentToVersionOne(v2LogText);

            string v1ActualLogText = JsonConvert.SerializeObject(v1Log, SarifTransformerUtilities.JsonSettingsV1Indented);

            StringBuilder sb = new StringBuilder();

            string expectedFilePath = GetOutputFilePath("expected", v1ExpectedResourceName);
            string actualFilePath = GetOutputFilePath("actual", v1ExpectedResourceName);

            string expectedRootDirectory = Path.GetDirectoryName(expectedFilePath);
            string actualRootDirectory = Path.GetDirectoryName(actualFilePath);

            if (!AreEquivalentSarifLogs<SarifLogVersionOne>(v1ActualLogText, v1ExpectedLogText, SarifContractResolverVersionOne.Instance))
            {
                Directory.CreateDirectory(expectedRootDirectory);
                Directory.CreateDirectory(actualRootDirectory);

                File.WriteAllText(expectedFilePath, v1ExpectedLogText);
                File.WriteAllText(actualFilePath, v1ActualLogText);


                string errorMessage = string.Format(@"V2 conversion from V1 produced unexpected diffs for test: '{0}'.", v2InputResourceName);
                sb.AppendLine("Check individual differences with:");
                sb.AppendLine(GenerateDiffCommand(expectedFilePath, actualFilePath) + Environment.NewLine);

                sb.AppendLine("To compare all difference for this test suite:");
                sb.AppendLine(GenerateDiffCommand(Path.GetDirectoryName(expectedFilePath), Path.GetDirectoryName(actualFilePath)) + Environment.NewLine);
            }

            if (s_Rebaseline)
            {
                // We rewrite to test output directory. This allows subsequent tests to 
                // pass without requiring a rebuild that recopies SARIF test files
                File.WriteAllText(expectedFilePath, v1ActualLogText);

                string subdirectory = ProductTestDataDirectory;
                expectedFilePath = Path.Combine(ProductTestDataDirectory, "v2", Path.GetFileName(expectedFilePath));

                // We also rewrite the checked in test baselines
                File.WriteAllText(expectedFilePath, v1ActualLogText);

            }

            s_Rebaseline.Should().BeFalse();

            ValidateResults(sb.ToString());
        }

        private static void VerifyCurrentToVersionOneTransformation(string v2LogText, string v1LogExpectedText)
        {
            v2LogText = PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(v2LogText, forceUpdate: true, formatting: Formatting.Indented);
            SarifLogVersionOne v1Log = TransformCurrentToVersionOne(v2LogText);
            string v1LogText = JsonConvert.SerializeObject(v1Log, SarifTransformerUtilities.JsonSettingsV1Indented);
            v1LogText.Should().Be(v1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_RestoreFromPropertyBag()
        {
            string testName = "RestoreFromPropertyBag.sarif";
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_Minimum()
        {
            string testName = "Minimum.sarif";
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithLogicalLocations()
        {
            string testName = "OneRunWithLogicalLocations.sarif";            
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithFiles()
        {
            string testName = "OneRunWithFiles.sarif";
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithRules()
        {
            string testName = "OneRunWithRules.sarif";
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithBasicInvocation()
        {
            string testName = "OneRunWithBasicInvocation.sarif";
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_NotificationExceptionWithStack()
        {
            string testName = "NotificationExceptionWithStack.sarif";
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_ResultLocations()
        {
            string testName = "ResultLocations.sarif";
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_TwoResultsWithFixes()
        {
            string testName = "TwoResultsWithFixes.sarif";
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_Regions()
        {
            string testName = "Regions.sarif";
            VerifyCurrentToVersionOneTransformationFromResource(testName);
        }
    }
}
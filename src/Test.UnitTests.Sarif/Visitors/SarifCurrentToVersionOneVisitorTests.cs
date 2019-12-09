// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Visitors
{
    public class SarifCurrentToVersionOneVisitorTests : FileDiffingUnitTests
    {
        public SarifCurrentToVersionOneVisitorTests(ITestOutputHelper outputHelper)
            : base(outputHelper, testProducesSarifCurrentVersion: false) { }

        protected override string ConstructTestOutputFromInputResource(string inputResource, object parameter)
        {
            string v2LogText = GetResourceText(inputResource);
            PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(v2LogText, formatting: Formatting.Indented, out v2LogText);
            SarifLog v2Log = JsonConvert.DeserializeObject<SarifLog>(v2LogText);

            var transformer = new SarifCurrentToVersionOneVisitor
            {
                EmbedVersionTwoContentInPropertyBag = false
            };

            transformer.VisitSarifLog(v2Log);

            SarifLogVersionOne v1Log = transformer.SarifLogVersionOne;
            return JsonConvert.SerializeObject(v1Log, SarifTransformerUtilities.JsonSettingsV1Indented);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_RestoreFromPropertyBag() => RunTest("RestoreFromPropertyBag.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_Minimum() => RunTest("Minimum.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithLogicalLocations() => RunTest("OneRunWithLogicalLocations.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithFiles() => RunTest("OneRunWithFiles.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithRules() => RunTest("OneRunWithRules.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithBasicInvocation() => RunTest("OneRunWithBasicInvocation.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_NotificationExceptionWithStack() => RunTest("NotificationExceptionWithStack.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_ResultLocations() => RunTest("ResultLocations.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_TwoResultsWithFixes() => RunTest("TwoResultsWithFixes.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_Regions() => RunTest("Regions.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_MinimumWithTwoRuns() => RunTest("MinimumWithTwoRuns.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithInvocationAndNotifications() => RunTest("OneRunWithInvocationAndNotifications.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithNotificationTime() => RunTest("OneRunWithNotificationTime.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OnePartialPartialFingerprint() => RunTest("ResultWithOnePartialFingerprint.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_TwoPartialPartialFingerprints() => RunTest("ResultWithTwoPartialFingerprints.sarif");

        [Fact]
        public void SarifTransformerTests_ToVersionOne_PopulatesRunIdAndStableId() => RunTest("OneRunWithAutomationDetails.sarif");
    }
}
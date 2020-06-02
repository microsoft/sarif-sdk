// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Visitors
{
    public class SarifVersionOneToCurrentVisitorTests : FileDiffingUnitTests
    {
        public SarifVersionOneToCurrentVisitorTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            string v1LogText = GetResourceText(inputResourceName);
            SarifLogVersionOne v1Log = JsonConvert.DeserializeObject<SarifLogVersionOne>(v1LogText, SarifTransformerUtilities.JsonSettingsV1Indented);
            var transformer = new SarifVersionOneToCurrentVisitor();
            transformer.VisitSarifLogVersionOne(v1Log);

            SarifLog v2Log = transformer.SarifLog;
            return JsonConvert.SerializeObject(v2Log, SarifTransformerUtilities.JsonSettingsIndented);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_RestoreFromPropertyBag()
            => RunTest("RestoreFromPropertyBag.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_Minimum()
            => RunTest("Minimum.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithLanguage()
            => RunTest("MinimumWithLanguage.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithTwoRuns()
            => RunTest("MinimumWithTwoRuns.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithPropertyAndTags()
            => RunTest("MinimumWithPropertiesAndTags.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_NestedFiles()
            => RunTest("NestedFiles.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithLogicalLocations()
            => RunTest("OneRunWithLogicalLocations.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithFiles()
            => RunTest("OneRunWithFiles.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithRules()
            => RunTest("OneRunWithRules.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithAllReportingDescriptors()
            => RunTest("OneRunWithAllReportingDescriptors.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithBasicInvocation()
            => RunTest("OneRunWithBasicInvocation.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithInvocationAndNotifications()
            => RunTest("OneRunWithInvocationAndNotifications.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithNotificationsButNoInvocations()
            => RunTest("OneRunWithNotificationsButNoInvocations.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_NotificationExceptionWithStack()
            => RunTest("NotificationExceptionWithStack.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_BasicResult()
            => RunTest("BasicResult.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_TwoResultsWithFixes()
            => RunTest("TwoResultsWithFixes.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_UriBaseId()
            => RunTest("UriBaseId.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_CodeFlows()
            => RunTest("CodeFlows.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_NestedInnerExceptionsInNotifications()
            => RunTest("NestedInnerExceptionsInNotifications.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_ResultLocationsAsEmptyArray()
            => RunTest("ResultLocationsAsEmptyArray.sarif");

        [Fact]
        public void SarifTransformerTests_ToCurrent_NonDottedQuadFileVersion()
            => RunTest("NonDottedQuadFileVersion.sarif");
    }
}

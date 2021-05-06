// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Baseline
{
    public class OverallBaseliningTests
    {
        private const string SampleFilePath = "elfie-arriba.sarif";
        private SarifLog SampleLog { get; }

        private static readonly ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
        private static readonly ResourceExtractor extractor = new ResourceExtractor(typeof(OverallBaseliningTests));

        public OverallBaseliningTests()
        {
            SampleLog = GetLogFromResource(SampleFilePath);
        }

        private static SarifLog GetLogFromResource(string filePath)
        {
            string fileContents = extractor.GetResourceText(filePath);
            return JsonConvert.DeserializeObject<SarifLog>(fileContents);
        }

        private static SarifLog Baseline(SarifLog baseline, SarifLog current)
        {
            return matcher.Match(new[] { baseline }, new[] { current }).FirstOrDefault();
        }

        [Fact]
        public void Overall_Identical()
        {
            SarifLog output = Baseline(SampleLog.DeepClone(), SampleLog.DeepClone());
            output.Runs[0].Results.Where(result => result.BaselineState != BaselineState.Unchanged).Should().BeEmpty();
        }

        [Fact]
        public void Overall_AbsentResultsInBaselineExcluded()
        {
            SarifLog baselineLog = SampleLog.DeepClone();

            // Make the first result already Absent in the baseline run
            baselineLog.Runs[0].Results[0].BaselineState = BaselineState.Absent;

            // The Absent result should be excluded before matching, so the copy in the current run is considered 'New' rather than 'Unchanged'
            SarifLog output = Baseline(baselineLog, SampleLog.DeepClone());
            output.Runs[0].Results.Where(result => result.BaselineState == BaselineState.Unchanged).Should().HaveCount(SampleLog.Runs[0].Results.Count - 1);
            output.Runs[0].Results.Where(result => result.BaselineState == BaselineState.New).Should().HaveCount(1);
        }

        [Fact]
        public void Overall_AbsentResultsInNewRunKept()
        {
            // The BaselineState for Results in the current run should be ignored, and will be overwritten by the outcome of the current baselining
            SarifLog currentLog = SampleLog.DeepClone();
            currentLog.Runs[0].Results[0].BaselineState = BaselineState.Absent;

            SarifLog output = Baseline(SampleLog.DeepClone(), currentLog);

            // The absent result should be marked as absent in the baseline.
            output.Runs[0].Results[0].BaselineState.Should().Be(BaselineState.Absent);

            // All other results are unchanged.
            output.Runs[0].Results.Where(result => result.BaselineState == BaselineState.Unchanged).Count().Should().Be(SampleLog.Runs[0].Results.Count - 1);
        }

        [Fact]
        public void Overall_CheckingAbsentUnchangedAndNew()
        {
            SarifLog baselineSarif = TestData.CreateSimpleLogWithRules(ruleIdStartIndex: 0, resultCount: 2);
            SarifLog currentSarif = TestData.CreateSimpleLogWithRules(ruleIdStartIndex: 1, resultCount: 2);

            ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            SarifLog output = matcher.Match(new SarifLog[] { baselineSarif }, new SarifLog[] { currentSarif }).First();

            output.Runs[0].Results[0].BaselineState.Should().Be(BaselineState.Absent);
            output.Runs[0].Results[1].BaselineState.Should().Be(BaselineState.Unchanged);
            output.Runs[0].Results[2].BaselineState.Should().Be(BaselineState.New);
        }

        [Fact]
        public void Overall_CheckingAbsentAndNew()
        {
            SarifLog baselineSarif = TestData.CreateSimpleLogWithRules(ruleIdStartIndex: 0, resultCount: 2);
            SarifLog currentSarif = TestData.CreateSimpleLogWithRules(ruleIdStartIndex: 10, resultCount: 2);

            ISarifLogMatcher matcher = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            SarifLog output = matcher.Match(new SarifLog[] { baselineSarif }, new SarifLog[] { currentSarif }).First();

            output.Runs[0].Results[0].BaselineState.Should().Be(BaselineState.Absent);
            output.Runs[0].Results[1].BaselineState.Should().Be(BaselineState.Absent);
            output.Runs[0].Results[2].BaselineState.Should().Be(BaselineState.New);
            output.Runs[0].Results[3].BaselineState.Should().Be(BaselineState.New);
        }
    }
}

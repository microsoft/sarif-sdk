// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    public class ResultMatchingBaselinerTests
    {
        private readonly ITestOutputHelper output;

        private readonly SarifLogResultMatcher baseliner =
            new SarifLogResultMatcher(
                exactResultMatchers: new[] { ExactMatchers.ExactResultMatcherFactory.GetIdenticalResultMatcher(considerPropertyBagsWhenComparing: true) },
                heuristicMatchers: null,
                propertyBagMergeBehaviors: DictionaryMergeBehavior.None);

        public ResultMatchingBaselinerTests(ITestOutputHelper outputHelper)
        {
            output = outputHelper;
        }

        [Fact]
        public void ResultMatchingBaseliner_BaselinesTwoSimpleSarifLogs()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog baselineLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            SarifLog currentLog = baselineLog.DeepClone();
            baselineLog.Runs[0].AutomationDetails = new RunAutomationDetails { Guid = Guid.NewGuid() };
            currentLog.Runs[0].AutomationDetails = new RunAutomationDetails { Guid = Guid.NewGuid() };

            if (currentLog.Runs[0].Results.Any())
            {
                currentLog.Runs[0].Results[0].Tags.Add("New Unused Tag");
            }

            foreach (Result result in baselineLog.Runs[0].Results)
            {
                result.CorrelationGuid = Guid.NewGuid();
            }

            SarifLog calculatedNextBaseline = baseliner.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First();

            calculatedNextBaseline.Runs.Should().HaveCount(1);

            if (currentLog.Runs[0].Results.Any())
            {
                calculatedNextBaseline.Runs[0].Results.Should().HaveCount(currentLog.Runs[0].Results.Count + 1);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.CorrelationGuid == null).Should().HaveCount(0);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Absent).Should().HaveCount(1);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Absent).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> AbsentResultProperties).Should().BeTrue();
                AbsentResultProperties.Should().ContainKey("Run");
                AbsentResultProperties["Run"].Should().BeEquivalentTo(baselineLog.Runs[0].AutomationDetails.Guid?.ToString(SarifConstants.GuidFormat));

                int existingCount = currentLog.Runs[0].Results.Count - 1;
                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Unchanged).Count().Should().Be(existingCount);

                if (existingCount > 0)
                {
                    // In the event that we generated a SARIF run of only a single result, we will not have an 'existing' match
                    // since we adjusted the sole result value by adding a property to it.
                    calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Unchanged).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> CurrentResultProperties).Should().BeTrue();
                    CurrentResultProperties.Should().ContainKey("Run");
                    CurrentResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].AutomationDetails.Guid?.ToString(SarifConstants.GuidFormat));
                }

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.New).Should().HaveCount(1);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.New).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> NewResultProperties).Should().BeTrue();
                NewResultProperties.Should().ContainKey("Run");
                NewResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].AutomationDetails.Guid?.ToString(SarifConstants.GuidFormat));
            }
        }

        [Fact]
        public void ResultMatchingBaseliner_ShouldNotThrowExceptionWhenNoSimilarToolsAreFound()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog baselineLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            SarifLog currentLog = baselineLog.DeepClone();

            baselineLog.Runs[0].Tool.Driver.Name = "Test1";
            currentLog.Runs[0].Tool.Driver.Name = "Test2";

            Exception exception = Record.Exception(() => baseliner.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First());
            Assert.Null(exception);
        }

        [Fact]
        public void ResultMatchingBaseliner_ShouldIgnoreRunsThatDontMatch()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog baselineLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            SarifLog currentLog = baselineLog.DeepClone();
            baselineLog.Runs[0].Tool.Driver.Name = "test";

            Run customRun = baselineLog.Runs[0].DeepClone();
            customRun.Tool.Driver.Name = "test1";
            baselineLog.Runs.Add(customRun);

            SarifLog calculatedNextBaseline = baseliner.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First();
            calculatedNextBaseline.Runs.Should().HaveCount(1);
        }
    }
}

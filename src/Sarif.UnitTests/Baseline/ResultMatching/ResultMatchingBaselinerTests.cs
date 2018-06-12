// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    public class ResultMatchingBaselinerTests
    {
        private readonly ITestOutputHelper output;

        private readonly ResultMatchingBaseliner baseliner = new ResultMatchingBaseliner(new IResultMatcher[] { ExactMatchers.ExactResultMatcherFactory.GetIdenticalResultMatcher() }, null);

        public ResultMatchingBaselinerTests(ITestOutputHelper outputHelper)
        {
            output = outputHelper;
        }

        [Fact]
        public void ResultMatchingBaseliner_BaselinesTwoSimpleSarifLogs()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog baselineLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);

            // For now, we will remove all duplicate results, as we aren't addressing that case.
            baselineLog.Runs[0].Results = baselineLog.Runs[0].Results.Distinct(ResultEqualityComparer.Instance).ToList();

            SarifLog currentLog = baselineLog.DeepClone();

            if (currentLog.Runs[0].Results.Any())
            {
                currentLog.Runs[0].Results[0].Tags.Add("New Unused Tag");
            }

            foreach (Result result in baselineLog.Runs[0].Results)
            {
                result.Id = Guid.NewGuid().ToString();
            }
            
            SarifLog calculatedNextBaseline = baseliner.BaselineSarifLogs(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog });
            
            calculatedNextBaseline.Runs.Should().HaveCount(1);

            if (currentLog.Runs[0].Results.Any())
            {
                calculatedNextBaseline.Runs[0].Results.Should().HaveCount(currentLog.Runs[0].Results.Count + 1);

                calculatedNextBaseline.Runs[0].Results.Where(r => string.IsNullOrEmpty(r.Id)).Should().HaveCount(0);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Absent).Should().HaveCount(1);
                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Existing).Should().HaveCount(currentLog.Runs[0].Results.Count - 1);
                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.New).Should().HaveCount(1);
            }
        }
    }
}

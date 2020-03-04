// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public class StrictBaselineUnitTests
    {
        private readonly ITestOutputHelper output;

        public StrictBaselineUnitTests(ITestOutputHelper outputHelper)
        {
            output = outputHelper;
        }

        private readonly ISarifLogBaseliner strictBaseliner = SarifLogBaselinerFactory.CreateSarifLogBaseliner(SarifBaselineType.Strict);

        [Fact]
        public void StrictBaseline_SameResults_AllExisting()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            Run baseline = RandomSarifLogGenerator.GenerateRandomRunWithoutDuplicateIssues(random, Result.ValueComparer, random.Next(100) + 5);
            Run next = baseline.DeepClone();

            Run run = strictBaseliner.CreateBaselinedRun(baseline, next);

            run.Results.Should().OnlyContain(r => r.BaselineState == BaselineState.Unchanged);
            run.Results.Should().HaveCount(baseline.Results.Count());
        }

        [Fact]
        public void StrictBaseline_NewResultAdded_New()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            Run baseline = RandomSarifLogGenerator.GenerateRandomRunWithoutDuplicateIssues(random, Result.ValueComparer, random.Next(100) + 5);
            Run next = baseline.DeepClone();
            next.Results.Add(RandomSarifLogGenerator.GenerateFakeResults(random, new List<string>() { "NEWTESTRESULT" }, new List<Uri>() { new Uri(@"c:\test\testfile") }, 1).First());

            Run result = strictBaseliner.CreateBaselinedRun(baseline, next);

            result.Results.Where(r => r.BaselineState == BaselineState.New).Should().ContainSingle();
            result.Results.Should().HaveCount(baseline.Results.Count() + 1);
        }

        [Fact]
        public void StrictBaseline_RemovedResult_Absent()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            Run baseline = RandomSarifLogGenerator.GenerateRandomRunWithoutDuplicateIssues(random, Result.ValueComparer, random.Next(100) + 5);
            Run next = baseline.DeepClone();
            next.Results.RemoveAt(0);

            Run result = strictBaseliner.CreateBaselinedRun(baseline, next);

            result.Results.Where(r => r.BaselineState == BaselineState.Absent).Should().ContainSingle();
            result.Results.Should().HaveCount(baseline.Results.Count());
        }


        [Fact]
        public void StrictBaseline_ChangedResult_AbsentAndNew()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            random = new Random(181968016);
            Run baseline = RandomSarifLogGenerator.GenerateRandomRunWithoutDuplicateIssues(random, Result.ValueComparer, random.Next(100) + 5);
            Run next = baseline.DeepClone();
            next.Results[0].RuleId += "V2";

            Run result = strictBaseliner.CreateBaselinedRun(baseline, next);

            result.Results.Where(r => r.BaselineState == BaselineState.New).Should().ContainSingle();
            result.Results.Where(r => r.BaselineState == BaselineState.Absent).Should().ContainSingle();
            result.Results.Should().HaveCount(baseline.Results.Count() + 1);
        }
    }
}

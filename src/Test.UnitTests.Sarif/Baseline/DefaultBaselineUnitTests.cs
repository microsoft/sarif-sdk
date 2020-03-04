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
    public class DefaultBaselineUnitTests
    {
        private readonly ITestOutputHelper output;

        public DefaultBaselineUnitTests(ITestOutputHelper outputHelper)
        {
            output = outputHelper;
        }

        private readonly ISarifLogBaseliner defaultBaseliner = SarifLogBaselinerFactory.CreateSarifLogBaseliner(SarifBaselineType.Standard);

        [Fact]
        public void DefaultBaseline_SameResults_AllExisting()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            Run baseline = RandomSarifLogGenerator.GenerateRandomRunWithoutDuplicateIssues(random, DefaultBaseline.ResultBaselineEquals.DefaultInstance, random.Next(100) + 5);
            Run next = baseline.DeepClone();

            Run result = defaultBaseliner.CreateBaselinedRun(baseline, next);

            result.Results.Should().OnlyContain(r => r.BaselineState == BaselineState.Unchanged);
            result.Results.Should().HaveCount(baseline.Results.Count());
        }

        [Fact]
        public void DefaultBaseline_NewResultAdded_New()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            Run baseline = RandomSarifLogGenerator.GenerateRandomRunWithoutDuplicateIssues(random, DefaultBaseline.ResultBaselineEquals.DefaultInstance, random.Next(100) + 5);
            Run next = baseline.DeepClone();
            next.Results.Add(RandomSarifLogGenerator.GenerateFakeResults(random, new List<string>() { "NEWTESTRESULT" }, new List<Uri>() { new Uri(@"c:\test\testfile") }, 1).First());

            Run result = defaultBaseliner.CreateBaselinedRun(baseline, next);

            result.Results.Where(r => r.BaselineState == BaselineState.New).Should().ContainSingle();
            result.Results.Should().HaveCount(baseline.Results.Count() + 1);
        }

        [Fact]
        public void DefaultBaseline_RemovedResult_Absent()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            Run baseline = RandomSarifLogGenerator.GenerateRandomRunWithoutDuplicateIssues(random, DefaultBaseline.ResultBaselineEquals.DefaultInstance, random.Next(100) + 5);
            Run next = baseline.DeepClone();
            next.Results.RemoveAt(0);

            Run result = defaultBaseliner.CreateBaselinedRun(baseline, next);

            result.Results.Where(r => r.BaselineState == BaselineState.Absent).Should().ContainSingle();
            result.Results.Should().HaveCount(baseline.Results.Count());
        }

        [Fact]
        public void DefaultBaseline_ChangedResultOnThumbprint_AbsentAndNew()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            Run baseline = RandomSarifLogGenerator.GenerateRandomRunWithoutDuplicateIssues(random, DefaultBaseline.ResultBaselineEquals.DefaultInstance, 5);
            Run next = baseline.DeepClone();
            next.Results[0].PartialFingerprints = new Dictionary<string, string>();
            next.Results[0].PartialFingerprints.Add("Fingerprint1", "New fingerprint");

            Run result = defaultBaseliner.CreateBaselinedRun(baseline, next);

            result.Results.Where(r => r.BaselineState == BaselineState.New).Should().ContainSingle();
            result.Results.Where(r => r.BaselineState == BaselineState.Absent).Should().ContainSingle();
            result.Results.Should().HaveCount(baseline.Results.Count() + 1);
        }

        [Fact]
        public void DefaultBaseline_ChangedResultOnNonTrackedField_Existing()
        {

            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            Run baseline = RandomSarifLogGenerator.GenerateRandomRunWithoutDuplicateIssues(random, DefaultBaseline.ResultBaselineEquals.DefaultInstance, random.Next(100) + 5);
            Run next = baseline.DeepClone();
            next.Results[0].Message = new Message { Text = "new message" };

            Run result = defaultBaseliner.CreateBaselinedRun(baseline, next);

            result.Results.Should().OnlyContain(r => r.BaselineState == BaselineState.Unchanged);
            result.Results.Should().HaveCount(baseline.Results.Count());
        }
    }
}

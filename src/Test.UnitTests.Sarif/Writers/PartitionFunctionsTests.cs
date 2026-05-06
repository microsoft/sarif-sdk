// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Visitors;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class PartitionFunctionsTests
    {
        // ---- ParseIndexSpec ---------------------------------------------------

        [Fact]
        public void ParseIndexSpec_BareIntsAssumeRunZero()
        {
            IDictionary<PartitionFunctions.ResultAddress, string> map =
                PartitionFunctions.ParseIndexSpec("0,2|1");

            map.Should().HaveCount(3);
            map[new PartitionFunctions.ResultAddress(0, 0)].Should().Be("bucket0");
            map[new PartitionFunctions.ResultAddress(0, 2)].Should().Be("bucket0");
            map[new PartitionFunctions.ResultAddress(0, 1)].Should().Be("bucket1");
        }

        [Fact]
        public void ParseIndexSpec_RunPrefix_AppliesToFollowingTokensOnly()
        {
            IDictionary<PartitionFunctions.ResultAddress, string> map =
                PartitionFunctions.ParseIndexSpec("1:0,3|0:2");

            map.Should().HaveCount(3);
            map[new PartitionFunctions.ResultAddress(1, 0)].Should().Be("bucket0");
            map[new PartitionFunctions.ResultAddress(1, 3)].Should().Be("bucket0");
            map[new PartitionFunctions.ResultAddress(0, 2)].Should().Be("bucket1");
        }

        [Fact]
        public void ParseIndexSpec_SegmentSeparatorDoesNotCarryRunPrefix()
        {
            // After the ';' the runId resets to default (0) unless a new prefix is supplied.
            IDictionary<PartitionFunctions.ResultAddress, string> map =
                PartitionFunctions.ParseIndexSpec("1:5;6");

            map.Should().HaveCount(2);
            map[new PartitionFunctions.ResultAddress(1, 5)].Should().Be("bucket0");
            map[new PartitionFunctions.ResultAddress(0, 6)].Should().Be("bucket0");
        }

        [Fact]
        public void ParseIndexSpec_AcceptsSarifUrls()
        {
            IDictionary<PartitionFunctions.ResultAddress, string> map =
                PartitionFunctions.ParseIndexSpec("sarif:/runs/0/results/3|sarif:/runs/2/results/7");

            map.Should().HaveCount(2);
            map[new PartitionFunctions.ResultAddress(0, 3)].Should().Be("bucket0");
            map[new PartitionFunctions.ResultAddress(2, 7)].Should().Be("bucket1");
        }

        [Fact]
        public void ParseIndexSpec_MixesCompactAndSarifUrlsInSameBucket()
        {
            IDictionary<PartitionFunctions.ResultAddress, string> map =
                PartitionFunctions.ParseIndexSpec("1:5,sarif:/runs/2/results/7");

            map.Should().HaveCount(2);
            map[new PartitionFunctions.ResultAddress(1, 5)].Should().Be("bucket0");
            map[new PartitionFunctions.ResultAddress(2, 7)].Should().Be("bucket0");
        }

        [Fact]
        public void ParseIndexSpec_TrimsWhitespace()
        {
            IDictionary<PartitionFunctions.ResultAddress, string> map =
                PartitionFunctions.ParseIndexSpec("  1 : 0 , 2  |  3  ");

            map.Should().HaveCount(3);
            map[new PartitionFunctions.ResultAddress(1, 0)].Should().Be("bucket0");
            map[new PartitionFunctions.ResultAddress(1, 2)].Should().Be("bucket0");
            map[new PartitionFunctions.ResultAddress(0, 3)].Should().Be("bucket1");
        }

        [Fact]
        public void ParseIndexSpec_DuplicateAcrossBuckets_Throws()
        {
            Action act = () => PartitionFunctions.ParseIndexSpec("0,1|1,2");
            act.Should().Throw<FormatException>().WithMessage("*Duplicate*");
        }

        [Fact]
        public void ParseIndexSpec_DuplicateWithinBucket_Throws()
        {
            Action act = () => PartitionFunctions.ParseIndexSpec("0,0");
            act.Should().Throw<FormatException>().WithMessage("*Duplicate*");
        }

        [Fact]
        public void ParseIndexSpec_DuplicateAcrossFormShorthandAndUrl_Throws()
        {
            // 0,1 (shorthand for run 0) and sarif:/runs/0/results/1 are the same address.
            Action act = () => PartitionFunctions.ParseIndexSpec("0,1|sarif:/runs/0/results/1");
            act.Should().Throw<FormatException>().WithMessage("*Duplicate*");
        }

        [Fact]
        public void ParseIndexSpec_EmptyBucket_Throws()
        {
            Action act = () => PartitionFunctions.ParseIndexSpec("0||1");
            act.Should().Throw<FormatException>().WithMessage("*Empty bucket*");
        }

        [Fact]
        public void ParseIndexSpec_EmptySegment_Throws()
        {
            Action act = () => PartitionFunctions.ParseIndexSpec("0;;1");
            act.Should().Throw<FormatException>().WithMessage("*Empty segment*");
        }

        [Fact]
        public void ParseIndexSpec_NullOrWhiteSpace_Throws()
        {
            Action a1 = () => PartitionFunctions.ParseIndexSpec(null);
            Action a2 = () => PartitionFunctions.ParseIndexSpec("   ");

            a1.Should().Throw<ArgumentException>();
            a2.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ParseIndexSpec_NegativeIndex_Throws()
        {
            Action act = () => PartitionFunctions.ParseIndexSpec("-1");
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void ParseIndexSpec_MalformedSarifUrl_Throws()
        {
            Action act = () => PartitionFunctions.ParseIndexSpec("sarif://runs/0/results/0");
            act.Should().Throw<FormatException>().WithMessage("*does not match the expected form*");
        }

        // ---- ForStrategy ------------------------------------------------------

        [Fact]
        public void ForStrategy_PerRule_KeysByRuleId()
        {
            SarifLog log = BuildLog(
                runs: 1,
                resultsPerRun: 3,
                ruleIdSelector: (run, idx) => $"R{idx}");

            PartitionFunction<string> fn = PartitionFunctions.ForStrategy(log, SplittingStrategy.PerRule);

            HashSet<string> keys = new HashSet<string>(log.Runs[0].Results.Select(r => fn(r)));
            keys.Should().BeEquivalentTo(new[] { "R0", "R1", "R2" });
        }

        [Fact]
        public void ForStrategy_PerRunPerRule_KeysByRunAndRule()
        {
            SarifLog log = BuildLog(
                runs: 2,
                resultsPerRun: 2,
                ruleIdSelector: (run, idx) => $"R{idx}");

            PartitionFunction<string> fn = PartitionFunctions.ForStrategy(log, SplittingStrategy.PerRunPerRule);

            fn(log.Runs[0].Results[0]).Should().Be("run0_R0");
            fn(log.Runs[0].Results[1]).Should().Be("run0_R1");
            fn(log.Runs[1].Results[0]).Should().Be("run1_R0");
            fn(log.Runs[1].Results[1]).Should().Be("run1_R1");
        }

        [Fact]
        public void ForStrategy_PerRun_KeysByRunOnly()
        {
            SarifLog log = BuildLog(2, 2, (r, i) => "R0");

            PartitionFunction<string> fn = PartitionFunctions.ForStrategy(log, SplittingStrategy.PerRun);

            fn(log.Runs[0].Results[0]).Should().Be("run0");
            fn(log.Runs[1].Results[1]).Should().Be("run1");
        }

        [Fact]
        public void ForStrategy_PerResult_AssignsUniqueKeyPerResult()
        {
            SarifLog log = BuildLog(1, 4, (r, i) => "R0");

            PartitionFunction<string> fn = PartitionFunctions.ForStrategy(log, SplittingStrategy.PerResult);

            HashSet<string> keys = new HashSet<string>(log.Runs[0].Results.Select(r => fn(r)));
            keys.Should().HaveCount(4);
        }

        [Fact]
        public void ForStrategy_PerRunPerTarget_KeysByPrimaryLocationUri()
        {
            SarifLog log = BuildLog(1, 2, (r, i) => "R0");
            log.Runs[0].Results[0].Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri("file:///a.cs");
            log.Runs[0].Results[1].Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri("file:///b.cs");

            PartitionFunction<string> fn = PartitionFunctions.ForStrategy(log, SplittingStrategy.PerRunPerTarget);

            fn(log.Runs[0].Results[0]).Should().Be("run0_file:///a.cs");
            fn(log.Runs[0].Results[1]).Should().Be("run0_file:///b.cs");
        }

        [Fact]
        public void ForStrategy_PerRunPerTargetPerRule_CombinesAll()
        {
            SarifLog log = BuildLog(1, 2, (r, i) => $"R{i}");
            log.Runs[0].Results[0].Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri("file:///a.cs");
            log.Runs[0].Results[1].Locations[0].PhysicalLocation.ArtifactLocation.Uri = new Uri("file:///b.cs");

            PartitionFunction<string> fn = PartitionFunctions.ForStrategy(log, SplittingStrategy.PerRunPerTargetPerRule);

            fn(log.Runs[0].Results[0]).Should().Be("run0_file:///a.cs_R0");
            fn(log.Runs[0].Results[1]).Should().Be("run0_file:///b.cs_R1");
        }

        // ---- ForIndexList -----------------------------------------------------

        [Fact]
        public void ForIndexList_AssignsResultsToBuckets()
        {
            SarifLog log = BuildLog(1, 4, (r, i) => "R0");

            PartitionFunction<string> fn = PartitionFunctions.ForIndexList(
                log,
                "0,2|1",
                spilloverBucket: null,
                strictCoverage: false);

            fn(log.Runs[0].Results[0]).Should().Be("bucket0");
            fn(log.Runs[0].Results[1]).Should().Be("bucket1");
            fn(log.Runs[0].Results[2]).Should().Be("bucket0");
            fn(log.Runs[0].Results[3]).Should().BeNull();
        }

        [Fact]
        public void ForIndexList_Spillover_ReceivesUnaddressedResults()
        {
            SarifLog log = BuildLog(1, 3, (r, i) => "R0");

            PartitionFunction<string> fn = PartitionFunctions.ForIndexList(
                log,
                "0",
                spilloverBucket: "rest",
                strictCoverage: false);

            fn(log.Runs[0].Results[0]).Should().Be("bucket0");
            fn(log.Runs[0].Results[1]).Should().Be("rest");
            fn(log.Runs[0].Results[2]).Should().Be("rest");
        }

        [Fact]
        public void ForIndexList_StrictCoverage_FailsOnUnaddressedResults()
        {
            SarifLog log = BuildLog(1, 3, (r, i) => "R0");

            Action act = () => PartitionFunctions.ForIndexList(
                log,
                "0",
                spilloverBucket: null,
                strictCoverage: true);

            act.Should().Throw<InvalidOperationException>().WithMessage("*Strict coverage failed*");
        }

        [Fact]
        public void ForIndexList_OutOfRangeIndex_Throws()
        {
            SarifLog log = BuildLog(1, 2, (r, i) => "R0");

            Action act = () => PartitionFunctions.ForIndexList(log, "0,5", null, false);
            act.Should().Throw<InvalidOperationException>().WithMessage("*references result 5*");
        }

        [Fact]
        public void ForIndexList_OutOfRangeRun_Throws()
        {
            SarifLog log = BuildLog(1, 2, (r, i) => "R0");

            Action act = () => PartitionFunctions.ForIndexList(log, "1:0", null, false);
            act.Should().Throw<InvalidOperationException>().WithMessage("*references run 1*");
        }

        // ---- helpers ----------------------------------------------------------

        private static SarifLog BuildLog(int runs, int resultsPerRun, Func<int, int, string> ruleIdSelector)
        {
            var log = new SarifLog
            {
                Runs = new List<Run>(),
            };

            for (int r = 0; r < runs; r++)
            {
                var run = new Run
                {
                    Tool = new Tool
                    {
                        Driver = new ToolComponent
                        {
                            Name = $"tool{r}",
                            Rules = new List<ReportingDescriptor>(),
                        },
                    },
                    Results = new List<Result>(),
                };

                for (int i = 0; i < resultsPerRun; i++)
                {
                    string ruleId = ruleIdSelector(r, i);

                    if (!run.Tool.Driver.Rules.Any(rd => rd.Id == ruleId))
                    {
                        run.Tool.Driver.Rules.Add(new ReportingDescriptor { Id = ruleId });
                    }

                    run.Results.Add(new Result
                    {
                        RuleId = ruleId,
                        Message = new Message { Text = $"r{r}-i{i}" },
                        Locations = new List<Location>
                        {
                            new Location
                            {
                                PhysicalLocation = new PhysicalLocation
                                {
                                    ArtifactLocation = new ArtifactLocation
                                    {
                                        Uri = new Uri($"file:///run{r}/result{i}.cs"),
                                    },
                                },
                            },
                        },
                    });
                }

                log.Runs.Add(run);
            }

            return log;
        }
    }
}

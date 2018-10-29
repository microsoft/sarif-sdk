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
    public class SarifLogResultMatcherTests
    {
        private readonly ITestOutputHelper output;

        private readonly SarifLogResultMatcher baseliner = new SarifLogResultMatcher(new IResultMatcher[] { ExactMatchers.ExactResultMatcherFactory.GetIdenticalResultMatcher() }, null);

        public SarifLogResultMatcherTests(ITestOutputHelper outputHelper)
        {
            output = outputHelper;
        }

        [Fact]
        public void SarifLogResultMatcher_BaselinesTwoSimpleSarifLogs()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog baselineLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            SarifLog currentLog = baselineLog.DeepClone();
            baselineLog.Runs[0].Id = new RunAutomationDetails { InstanceGuid = Guid.NewGuid().ToString() };
            currentLog.Runs[0].Id = new RunAutomationDetails { InstanceGuid = Guid.NewGuid().ToString() };

            // This code exists to force a result to diverge from the previous run. By modifying this tag, 
            // we ensure that at least one result will be regarded as new (which implies one result
            // will be regarded as going absent).
            if (currentLog.Runs[0].Results.Any())
            {
                currentLog.Runs[0].Results[0].Tags.Add("New Unused Tag");
            }

            string propertyName = "WeLikePi";
            float propertyValue = 3.14159F;
            currentLog.Runs[0].SetProperty(propertyName, propertyValue);

            foreach (Result result in baselineLog.Runs[0].Results)
            {
                result.CorrelationGuid = Guid.NewGuid().ToString();
            }

            SarifLog calculatedNextBaseline = baseliner.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First();

            calculatedNextBaseline.Runs.Should().HaveCount(1);

            calculatedNextBaseline.Runs[0].Properties.Should().NotBeNull();
            calculatedNextBaseline.Runs[0].GetProperty<float>(propertyName).Should().Be(propertyValue);

            if (currentLog.Runs[0].Results.Any())
            {
                calculatedNextBaseline.Runs[0].Results.Should().HaveCount(currentLog.Runs[0].Results.Count + 1);

                calculatedNextBaseline.Runs[0].Results.Where(r => string.IsNullOrEmpty(r.CorrelationGuid)).Should().HaveCount(0);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Absent).Should().HaveCount(1);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Absent).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> AbsentResultProperties).Should().BeTrue();
                AbsentResultProperties.Should().ContainKey("Run");
                AbsentResultProperties["Run"].Should().BeEquivalentTo(baselineLog.Runs[0].Id.InstanceGuid);


                if (currentLog.Runs[0].Results.Count > 1)
                {
                    calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Existing).Should().HaveCount(currentLog.Runs[0].Results.Count - 1);

                    calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Existing).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> CurrentResultProperties).Should().BeTrue();
                    CurrentResultProperties.Should().ContainKey("Run");
                    CurrentResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].Id.InstanceGuid);
                }
                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.New).Should().HaveCount(1);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.New).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> NewResultProperties).Should().BeTrue();
                NewResultProperties.Should().ContainKey("Run");
                NewResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].Id.InstanceGuid);
            }
        }

        [Fact]
        public void SarifLogResultMatcher_BaselinesSarifLogsWithProperties()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            SarifLog baselineLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            SarifLog currentLog = baselineLog.DeepClone();

            string sharedPropertyName = nameof(sharedPropertyName);
            string currentSharedPropertyValue = Guid.NewGuid().ToString();

            string uniqueToBaselinePropertyName = nameof(uniqueToBaselinePropertyName);
            string uniqueToBaselinePropertyValue = Guid.NewGuid().ToString();

            string uniqueToCurrentPropertyName = nameof(uniqueToCurrentPropertyName);
            string uniqueToCurrentPropertyValue = Guid.NewGuid().ToString();

            baselineLog.Runs[0].SetProperty(sharedPropertyName, currentSharedPropertyValue);
            currentLog.Runs[0].SetProperty(sharedPropertyName, currentSharedPropertyValue);

            baselineLog.Runs[0].SetProperty(uniqueToBaselinePropertyName, uniqueToBaselinePropertyValue);
            currentLog.Runs[0].SetProperty(uniqueToCurrentPropertyName, uniqueToCurrentPropertyValue);

            SarifLog calculatedNextBaseline = baseliner.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First();

            calculatedNextBaseline.Runs[0].Properties.Should().NotBeNull();
            calculatedNextBaseline.Runs[0].Properties.Count.Should().Be(3);

            // Identical property values should merge without raising exceptions. Unique properties should be aggregated to final property bag.

            // Note: for shared property keys, we prefer the value associated with the current run, not the baseline
            calculatedNextBaseline.Runs[0].GetProperty(sharedPropertyName).Should().Be(currentSharedPropertyValue);
            
            calculatedNextBaseline.Runs[0].GetProperty(uniqueToCurrentPropertyName).Should().Be(uniqueToCurrentPropertyValue);
            calculatedNextBaseline.Runs[0].GetProperty(uniqueToBaselinePropertyName).Should().Be(uniqueToBaselinePropertyValue);
        }

        [Fact]
        public void SarifLogResultMatcher_CurrentLogEmpty_AllAbsent()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog baselineLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            SarifLog currentLog = new SarifLog();
            currentLog.Runs = new Run[] { new Run() };
            baselineLog.Runs[0].Id = new RunAutomationDetails { InstanceGuid = Guid.NewGuid().ToString() };

            currentLog.Runs[0].Id = new RunAutomationDetails { InstanceGuid = Guid.NewGuid().ToString() };
            currentLog.Runs[0].Tool = new Tool() { Name = "Test" };

            foreach (Result result in baselineLog.Runs[0].Results)
            {
                result.CorrelationGuid = Guid.NewGuid().ToString();
            }

            SarifLog calculatedNextBaseline = baseliner.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First();

            calculatedNextBaseline.Runs.Should().HaveCount(1);

            if (baselineLog.Runs[0].Results.Any())
            {
                calculatedNextBaseline.Runs[0].Results.Should().HaveCount(baselineLog.Runs[0].Results.Count);
                calculatedNextBaseline.Runs[0].Results.Where(r => string.IsNullOrEmpty(r.CorrelationGuid)).Should().HaveCount(0);
                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Absent).Should().HaveCount(baselineLog.Runs[0].Results.Count);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Absent).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> AbsentResultProperties).Should().BeTrue();
                AbsentResultProperties.Should().ContainKey("Run");
                AbsentResultProperties["Run"].Should().BeEquivalentTo(baselineLog.Runs[0].Id.InstanceGuid);
            }
        }

        [Fact]
        public void SarifLogResultMatcher_PreviousLogNull_WorksAsExpected()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog currentLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            currentLog.Runs[0].Id = new RunAutomationDetails { InstanceGuid = Guid.NewGuid().ToString() };
            
            SarifLog calculatedNextBaseline = baseliner.Match(null, new SarifLog[] { currentLog }).First();

            calculatedNextBaseline.Runs.Should().HaveCount(1);

            if (currentLog.Runs[0].Results.Any())
            {
                // We should have the same number of results.
                calculatedNextBaseline.Runs[0].Results.Should().HaveCount(currentLog.Runs[0].Results.Count);

                // They should all have correllation ids
                calculatedNextBaseline.Runs[0]
                    .Results.Where(r => string.IsNullOrEmpty(r.CorrelationGuid)).Should().HaveCount(0);
                
                // They should all be new.
                calculatedNextBaseline.Runs[0]
                    .Results.Where(r => r.BaselineState == BaselineState.New).Should().HaveCount(currentLog.Runs[0].Results.Count);

                // And they should have the correct property set.
                calculatedNextBaseline.Runs[0]
                    .Results.Where(r => r.BaselineState == BaselineState.New)
                    .First().TryGetProperty(
                    SarifLogResultMatcher.ResultMatchingResultPropertyName, 
                    out Dictionary<string, object> NewResultProperties)
                    .Should().BeTrue();
                NewResultProperties.Should().ContainKey("Run");
                NewResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].Id.InstanceGuid);
            }
        }

        [Fact]
        public void SarifLogResultMatcher_PreviousLogEmpty_WorksAsExpected()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog currentLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            currentLog.Runs[0].Id = new RunAutomationDetails { InstanceGuid = Guid.NewGuid().ToString() };

            SarifLog calculatedNextBaseline = baseliner.Match(new SarifLog[0], new SarifLog[] { currentLog }).First();

            calculatedNextBaseline.Runs.Should().HaveCount(1);

            if (currentLog.Runs[0].Results.Any())
            {
                // We should have the same number of results.
                calculatedNextBaseline.Runs[0].Results.Should().HaveCount(currentLog.Runs[0].Results.Count);

                // They should all have correllation ids
                calculatedNextBaseline.Runs[0]
                    .Results.Where(r => string.IsNullOrEmpty(r.CorrelationGuid)).Should().HaveCount(0);

                // They should all be new.
                calculatedNextBaseline.Runs[0]
                    .Results.Where(r => r.BaselineState == BaselineState.New).Should().HaveCount(currentLog.Runs[0].Results.Count);

                // And they should have the correct property set.
                calculatedNextBaseline.Runs[0]
                    .Results.Where(r => r.BaselineState == BaselineState.New)
                    .First().TryGetProperty(
                    SarifLogResultMatcher.ResultMatchingResultPropertyName,
                    out Dictionary<string, object> NewResultProperties)
                    .Should().BeTrue();
                NewResultProperties.Should().ContainKey("Run");
                NewResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].Id.InstanceGuid);
            }
        }

        [Fact]
        public void SarifLogResultMatcher_MultipleLogsDuplicateData_WorksAsExpected()
        {
            SarifLog current1 = new SarifLog()
            {
                Runs = new Run[]
                {
                    new Run()
                    {
                        Tool = new Tool { Name = "TestTool" },
                        Files = new Dictionary<string, FileData>()
                        {
                            { "testfile", new FileData() { Contents = new FileContent() { Text = "TestFileContents" } } }
                        },
                        Results = new Result[0]
                    }
                }
            };
            SarifLog current2 = new SarifLog()
            {
                Runs = new Run[]
                {
                    new Run()
                    {
                        Tool = new Tool { Name = "TestTool" },
                        Files = new Dictionary<string, FileData>()
                        {
                            { "testfile", new FileData() { Contents = new FileContent() { Text = "TestFileContents" } } }
                        },
                        Results = new Result[0],
                    }
                }
            };
            SarifLog result = baseliner.Match(new SarifLog[0], new SarifLog[] { current1, current2 }).First();

            result.Runs[0].Files.Should().HaveCount(1);
            
        }

        [Fact]
        public void SarifLogResultMatcher_MultipleLogsInvalidData_ThrowsInvalidOperationException()
        {
            SarifLog current1 = new SarifLog()
            {
                Runs = new Run[]
                {
                    new Run()
                    {
                        Tool = new Tool { Name = "TestTool" },
                        Files = new Dictionary<string, FileData>()
                        {
                            { "testfile", new FileData() { Contents = new FileContent() { Text = "TestFileContents" } } }
                        },
                        Results = new Result[0]
                    }
                }
            };
            SarifLog current2 = new SarifLog()
            {
                Runs = new Run[]
                {
                    new Run()
                    {
                        Tool = new Tool { Name = "TestTool" },
                        Files = new Dictionary<string, FileData>()
                        {
                            { "testfile", new FileData() { Contents = new FileContent() { Text = "DifferentTestFileContents" } } }
                        },
                        Results = new Result[0],
                    }
                }
            };
            Assert.Throws<InvalidOperationException>(() => baseliner.Match(new SarifLog[0], new SarifLog[] { current1, current2 }));

        }
    }
}

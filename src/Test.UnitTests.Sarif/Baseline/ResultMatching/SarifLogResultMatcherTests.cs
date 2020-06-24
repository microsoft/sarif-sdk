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
    public class SarifLogResultMatcherTests
    {
        private readonly ITestOutputHelper output;

        private static readonly SarifLogResultMatcher s_preserveMostRecentPropertyBagMatcher = new SarifLogResultMatcher(
            exactResultMatchers: new[] { ExactMatchers.ExactResultMatcherFactory.GetIdenticalResultMatcher(considerPropertyBagsWhenComparing: false) },
            heuristicMatchers: null,
            propertyBagMergeBehaviors: DictionaryMergeBehavior.InitializeFromMostRecent);

        private static readonly SarifLogResultMatcher s_preserveOldestPropertyBagMatcher = new SarifLogResultMatcher(
            exactResultMatchers: new[] { ExactMatchers.ExactResultMatcherFactory.GetIdenticalResultMatcher(considerPropertyBagsWhenComparing: false) },
            heuristicMatchers: null,
            propertyBagMergeBehaviors: DictionaryMergeBehavior.InitializeFromOldest);

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
            baselineLog.Runs[0].AutomationDetails = new RunAutomationDetails { Guid = Guid.NewGuid().ToString() };
            currentLog.Runs[0].AutomationDetails = new RunAutomationDetails { Guid = Guid.NewGuid().ToString() };

            // This code exists to force a result to diverge from the previous run. By modifying this tag, 
            // we ensure that at least one result will be regarded as new (which implies one result
            // will be regarded as going absent).
            if (currentLog.Runs[0].Results.Any())
            {
                currentLog.Runs[0].Results[0].Tags.Add("New Unused Tag");
            }

            string propertyName = "WeLikePi";
            float propertyValue = 3.14159F;
            baselineLog.Runs[0].SetProperty(propertyName, propertyValue);

            foreach (Result result in baselineLog.Runs[0].Results)
            {
                result.CorrelationGuid = Guid.NewGuid().ToString();
            }

            SarifLog calculatedNextBaseline = s_preserveOldestPropertyBagMatcher.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First();

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
                AbsentResultProperties["Run"].Should().BeEquivalentTo(baselineLog.Runs[0].AutomationDetails.Guid);


                if (currentLog.Runs[0].Results.Count > 1)
                {
                    calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Unchanged).Should().HaveCount(currentLog.Runs[0].Results.Count - 1);

                    calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Unchanged).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> CurrentResultProperties).Should().BeTrue();
                    CurrentResultProperties.Should().ContainKey("Run");
                    CurrentResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].AutomationDetails.Guid);
                }
                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.New).Should().HaveCount(1);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.New).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> NewResultProperties).Should().BeTrue();
                NewResultProperties.Should().ContainKey("Run");
                NewResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].AutomationDetails.Guid);
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

            SarifLog calculatedNextBaseline = s_preserveOldestPropertyBagMatcher.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First();

            string value = null;

            // The default property bag matching behavior is to retain the property bag in its entirety from the baseline
            calculatedNextBaseline.Runs[0].Properties.Should().NotBeNull();
            calculatedNextBaseline.Runs[0].Properties.Count.Should().Be(2);

            calculatedNextBaseline.Runs[0].GetProperty(sharedPropertyName).Should().Be(currentSharedPropertyValue);
            calculatedNextBaseline.Runs[0].GetProperty(uniqueToBaselinePropertyName).Should().Be(uniqueToBaselinePropertyValue);
            calculatedNextBaseline.Runs[0].TryGetProperty(uniqueToCurrentPropertyName, out value).Should().BeFalse();

            calculatedNextBaseline = s_preserveMostRecentPropertyBagMatcher.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First();

            // The default property bag matching behavior is to retain the property bag in its entirety from the baseline
            calculatedNextBaseline.Runs[0].Properties.Should().NotBeNull();
            calculatedNextBaseline.Runs[0].Properties.Count.Should().Be(2);

            calculatedNextBaseline.Runs[0].GetProperty(sharedPropertyName).Should().Be(currentSharedPropertyValue);
            calculatedNextBaseline.Runs[0].GetProperty(uniqueToCurrentPropertyName).Should().Be(uniqueToCurrentPropertyValue);
            calculatedNextBaseline.Runs[0].TryGetProperty(uniqueToBaselinePropertyName, out value).Should().BeFalse();
        }

        [Fact]
        public void SarifLogResultMatcher_MatchMultipleCurrentLogsWithNoBaseline()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            SarifLog mostRecentLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            SarifLog oldestLog = mostRecentLog.DeepClone();

            string sharedPropertyName = nameof(sharedPropertyName);
            string currentSharedPropertyValue = Guid.NewGuid().ToString();

            string uniqueToMostRecentPropertyName = nameof(uniqueToMostRecentPropertyName);
            string uniqueToMostRecentPropertyValue = Guid.NewGuid().ToString();

            string uniqueToOldestPropertyName = nameof(uniqueToOldestPropertyName);
            string uniqueToOldestPropertyValue = Guid.NewGuid().ToString();

            mostRecentLog.Runs[0].SetProperty(sharedPropertyName, currentSharedPropertyValue);
            oldestLog.Runs[0].SetProperty(sharedPropertyName, currentSharedPropertyValue);

            mostRecentLog.Runs[0].SetProperty(uniqueToMostRecentPropertyName, uniqueToMostRecentPropertyValue);
            oldestLog.Runs[0].SetProperty(uniqueToOldestPropertyName, uniqueToOldestPropertyValue);

            SarifLog calculatedNextBaseline = s_preserveOldestPropertyBagMatcher.Match(
                previousLogs: null,
                currentLogs: new SarifLog[] { oldestLog, mostRecentLog }).First();

            calculatedNextBaseline.Runs[0].Properties.Should().NotBeNull();
            calculatedNextBaseline.Runs[0].Properties.Count.Should().Be(2);

            string value = null;

            // The default property bag matching behavior is to retain the oldest property bag available. Since we have no 
            // baseline in this test, we expect the preserved property bag to be associated with the first (i.e., oldest)
            // run in the currentLogs property

            calculatedNextBaseline.Runs[0].GetProperty(sharedPropertyName).Should().Be(currentSharedPropertyValue);
            calculatedNextBaseline.Runs[0].TryGetProperty(uniqueToOldestPropertyName, out value).Should().BeTrue();
            calculatedNextBaseline.Runs[0].GetProperty(uniqueToOldestPropertyName).Should().Be(uniqueToOldestPropertyValue);

            calculatedNextBaseline.Runs[0].TryGetProperty(uniqueToMostRecentPropertyName, out value).Should().BeFalse();

            calculatedNextBaseline = s_preserveMostRecentPropertyBagMatcher.Match(
                previousLogs: null,
                currentLogs: new SarifLog[] { oldestLog, mostRecentLog }).First();

            calculatedNextBaseline.Runs[0].Properties.Should().NotBeNull();
            calculatedNextBaseline.Runs[0].Properties.Count.Should().Be(2);

            calculatedNextBaseline.Runs[0].GetProperty(sharedPropertyName).Should().Be(currentSharedPropertyValue);
            calculatedNextBaseline.Runs[0].TryGetProperty(uniqueToMostRecentPropertyName, out value).Should().BeTrue();
            calculatedNextBaseline.Runs[0].GetProperty(uniqueToMostRecentPropertyName).Should().Be(uniqueToMostRecentPropertyValue);

            calculatedNextBaseline.Runs[0].TryGetProperty(uniqueToOldestPropertyName, out value).Should().BeFalse();
        }

        [Fact]
        public void SarifLogResultMatcher_CurrentLogEmpty_AllAbsent()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog baselineLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            SarifLog currentLog = new SarifLog();
            currentLog.Runs = new Run[] { new Run() };
            baselineLog.Runs[0].AutomationDetails = new RunAutomationDetails { Guid = Guid.NewGuid().ToString() };

            currentLog.Runs[0].AutomationDetails = new RunAutomationDetails { Guid = Guid.NewGuid().ToString() };
            currentLog.Runs[0].Tool = new Tool() { Driver = new ToolComponent { Name = "Test" } };

            foreach (Result result in baselineLog.Runs[0].Results)
            {
                result.CorrelationGuid = Guid.NewGuid().ToString();
            }

            SarifLog calculatedNextBaseline = s_preserveOldestPropertyBagMatcher.Match(new SarifLog[] { baselineLog }, new SarifLog[] { currentLog }).First();

            calculatedNextBaseline.Runs.Should().HaveCount(1);

            if (baselineLog.Runs[0].Results.Any())
            {
                calculatedNextBaseline.Runs[0].Results.Should().HaveCount(baselineLog.Runs[0].Results.Count);
                calculatedNextBaseline.Runs[0].Results.Where(r => string.IsNullOrEmpty(r.CorrelationGuid)).Should().HaveCount(0);
                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Absent).Should().HaveCount(baselineLog.Runs[0].Results.Count);

                calculatedNextBaseline.Runs[0].Results.Where(r => r.BaselineState == BaselineState.Absent).First().TryGetProperty(SarifLogResultMatcher.ResultMatchingResultPropertyName, out Dictionary<string, string> AbsentResultProperties).Should().BeTrue();
                AbsentResultProperties.Should().ContainKey("Run");
                AbsentResultProperties["Run"].Should().BeEquivalentTo(baselineLog.Runs[0].AutomationDetails.Guid);
            }
        }

        [Fact]
        public void SarifLogResultMatcher_PreviousLogNull_WorksAsExpected()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog currentLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            currentLog.Runs[0].AutomationDetails = new RunAutomationDetails { Guid = Guid.NewGuid().ToString() };

            SarifLog calculatedNextBaseline = s_preserveOldestPropertyBagMatcher.Match(null, new SarifLog[] { currentLog }).First();

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
                NewResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].AutomationDetails.Guid);
            }
        }

        [Fact]
        public void SarifLogResultMatcher_PreviousLogEmpty_WorksAsExpected()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog currentLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            currentLog.Runs[0].AutomationDetails = new RunAutomationDetails { Guid = Guid.NewGuid().ToString() };

            SarifLog calculatedNextBaseline = s_preserveOldestPropertyBagMatcher.Match(new SarifLog[0], new SarifLog[] { currentLog }).First();

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
                NewResultProperties["Run"].Should().BeEquivalentTo(currentLog.Runs[0].AutomationDetails.Guid);
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
                        Tool = new Tool { Driver = new ToolComponent{Name = "TestTool" } },
                        Artifacts = new List<Artifact>
                        {
                            new Artifact() { Contents = new ArtifactContent() { Text = "TestFileContents" } }
                        },
                        Results = new List<Result>
                        {
                            new Result
                            {
                                Message = new Message { Text = "Some testing occurred."},
                                Locations = new List<Location>
                                {
                                    new Location
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                            ArtifactLocation = new ArtifactLocation
                                            {
                                                Index = 0
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            SarifLog current2 = current1.DeepClone();
            SarifLog result = s_preserveOldestPropertyBagMatcher.Match(new SarifLog[0], new SarifLog[] { current1, current2 }).First();

            // In first match operation, the file data objects are identical, they should be collapsed into a single array entry
            result.Runs[0].Artifacts.Should().HaveCount(1);

            current2 = current1.DeepClone();

            // Now we differentiate a files object
            current2.Runs[0].Artifacts[0].Contents.Text = Guid.NewGuid().ToString();
            result = s_preserveOldestPropertyBagMatcher.Match(new SarifLog[0], new SarifLog[] { current1, current2 }).First();

            // Both file data objects should be present
            result.Runs[0].Artifacts.Should().HaveCount(2);

            // Merged results should each refer to differentiated file data object
            result.Runs[0].Results[0].Locations[0].PhysicalLocation.ArtifactLocation.Index.Should().Be(0);
            result.Runs[0].Results[1].Locations[0].PhysicalLocation.ArtifactLocation.Index.Should().Be(1);
        }

        [Fact]
        public void SarifLogResultMatcher_PreservesPropertiesProperly()
        {
            // Verify that SarifLog matching keeps old or latest property values on matched Results
            // depending on how it was configured.

            // This test used to verify that old Artifact properties are also preserved, but this behavior
            // doesn't make sense to us. If we need Artifacts to have property values from old runs, we
            // will need to merge them carefully, keeping the latest URI, File Content, and Hashes, and only
            // taking the older version of the Property Bag properties. (The latest merging code just took the
            // old Artifact wholesale)

            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog baselineLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            SarifLog currentLog = baselineLog.DeepClone();

            string baselinePropertyValue = Guid.NewGuid().ToString();
            string currentPropertyValue = Guid.NewGuid().ToString();

            SetPropertyOnAllResultObjects(baselineLog, "Key", baselinePropertyValue);
            SetPropertyOnAllResultObjects(currentLog, "Key", currentPropertyValue);

            SarifLog matchedLog = s_preserveOldestPropertyBagMatcher.Match(baselineLog.DeepClone(), currentLog.DeepClone());
            matchedLog.Runs[0].Results?.Where((r) => { return r.GetProperty("Key") == baselinePropertyValue; }).Count().Should().Be(matchedLog.Runs[0].Results.Count);

            // Retain property bag values from most current run
            matchedLog = s_preserveMostRecentPropertyBagMatcher.Match(baselineLog.DeepClone(), currentLog.DeepClone());
            matchedLog.Runs[0].Results?.Where((r) => { return r.GetProperty("Key") == currentPropertyValue; }).Count().Should().Be(matchedLog.Runs[0].Results.Count);
        }

        private void SetPropertyOnAllResultObjects(SarifLog sarifLog, string propertyKey, string propertyValue)
        {
            foreach (Run run in sarifLog.Runs)
            {
                foreach (Result result in run.Results)
                {
                    result.SetProperty(propertyKey, propertyValue);
                }
            }
        }
    }
}

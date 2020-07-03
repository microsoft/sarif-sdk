// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class RunMergingVisitorTests
    {
        [Fact]
        public void RunMergingVisitor_RemapsNestedFilesProperly()
        {
            // This run has a single result that points to a doubly nested file.
            var baselineRun = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "Test Tool" } },
                Artifacts = new List<Artifact>
                {
                    new Artifact{ Location = new ArtifactLocation{ Index = 0 }, Contents = new ArtifactContent { Text = "1" } },
                    new Artifact{ Location = new ArtifactLocation{ Index = 1 }, Contents = new ArtifactContent { Text = "1.2" }, ParentIndex = 0 },
                    new Artifact{ Location = new ArtifactLocation{ Index = 2 }, Contents = new ArtifactContent { Text = "1.2.3." }, ParentIndex = 1 }
                },
                Results = new List<Result>
                {
                    new Result
                    {
                        Locations = new List<Location>
                        {
                            new Location
                            {
                                PhysicalLocation = new PhysicalLocation
                                {
                                    ArtifactLocation = new ArtifactLocation
                                    {
                                        Index = 2
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // This run has a single result pointing to a single file location.
            var currentRun = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "Test Tool" } },
                Artifacts = new List<Artifact>
                {
                    new Artifact{ Location = new ArtifactLocation{ Index = 0 }, Contents = new ArtifactContent { Text = "New" } },
                    new Artifact{ Location = new ArtifactLocation{ Index = 1 }, Contents = new ArtifactContent { Text = "Child of new" }, ParentIndex = 0 },
                },
                Results = new List<Result>
                {
                    new Result
                    {
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
            };

            // Use the RunMergingVisitor to merge two runs
            var visitor = new RunMergingVisitor();

            Run mergedRun = currentRun.DeepClone();
            Run baselineRunCopy = baselineRun.DeepClone();

            visitor.VisitRun(mergedRun);
            visitor.VisitRun(baselineRunCopy);

            visitor.PopulateWithMerged(mergedRun);

            // Confirm that Artifacts (including indirectly referenced ones) were all copied to the destination Run
            mergedRun.Artifacts.Count.Should().Be(baselineRun.Artifacts.Count + currentRun.Artifacts.Count);
            
            // Confirm that Artifacts have consistent indices in the merged Run 
            for (int i = 0; i < mergedRun.Artifacts.Count; i++)
            {
                mergedRun.Artifacts[i].Location.Index.Should().Be(i);
            }

            // Verify each merged Run Result ArtifactIndex points to an Artifact with the same
            // text as the corresponding original Result
            VerifyArtifactMatches(mergedRun, currentRun, 0);
            VerifyArtifactMatches(mergedRun, baselineRun, currentRun.Results.Count);

            // Verify that the parent index chain for the nested artifacts was updated properly
            // Recall that visitor placed the artifacts from the baseline run at the end of the
            // merged run's artifacts array, and that in this test case, the artifacts in the
            // baseline run were arranged with the most nested artifact at the end.
            Artifact artifact = mergedRun.Artifacts[mergedRun.Artifacts.Count - 1];

            artifact.ParentIndex.Should().Be(3);
            artifact = mergedRun.Artifacts[artifact.ParentIndex];

            artifact.ParentIndex.Should().Be(2);
            artifact = mergedRun.Artifacts[artifact.ParentIndex];

            artifact.ParentIndex.Should().Be(-1);
        }

        private void VerifyArtifactMatches(Run mergedRun, Run sourceRun, int firstResultIndex)
        {
            for (int i = 0; i < sourceRun.Results.Count; ++i)
            {
                int previousArtifactIndex = sourceRun.Results[i].Locations[0].PhysicalLocation.ArtifactLocation.Index;
                Artifact previousArtifact = sourceRun.Artifacts[previousArtifactIndex];

                int newArtifactIndex = mergedRun.Results[firstResultIndex].Locations[0].PhysicalLocation.ArtifactLocation.Index;
                Artifact newArtifact = mergedRun.Artifacts[newArtifactIndex];

                newArtifact.Contents.Text.Should().Be(previousArtifact.Contents.Text);

                firstResultIndex++;
            }
        }

        [Fact]
        public void RunMergingVisitor_RemapsLogicalLocations()
        {
            var baselineRun = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "Test Tool" } },
                LogicalLocations = new List<LogicalLocation>
                {
                    new LogicalLocation
                    {
                        Index = 0,
                        ParentIndex = -1,
                        Kind = LogicalLocationKind.Namespace,
                        Name = "N1",
                        FullyQualifiedName = "N1"
                    },

                    new LogicalLocation
                    {
                        Index = 1,
                        ParentIndex = 0,
                        Kind = LogicalLocationKind.Type,
                        Name = "T1",
                        FullyQualifiedName = "N1.T1"
                    },

                    new LogicalLocation
                    {
                        Index = 2,
                        ParentIndex = 1,
                        Kind = LogicalLocationKind.Member,
                        Name = "M1",
                        FullyQualifiedName = "N1.T1.M1"
                    },
                },

                Results = new List<Result>
                {
                    new Result
                    {
                        Locations = new List<Location>
                        {
                            new Location
                            {
                                LogicalLocation = new LogicalLocation
                                {
                                    Index = 2
                                }
                            }
                        }
                    },

                    // This result implicates a logical location that has already been encountered.
                    // The visitor should count it only once.
                    new Result
                    {
                        Locations = new List<Location>
                        {
                            new Location
                            {
                                LogicalLocation = new LogicalLocation
                                {
                                    Index = 1
                                }
                            }
                        }
                    }
                }
            };

            // This run has a single result that points to a different logical location.
            var currentRun = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "Test Tool" } },
                LogicalLocations = new List<LogicalLocation>
                {
                    new LogicalLocation
                    {
                        Index = 0,
                        ParentIndex = -1,
                        Kind = LogicalLocationKind.Namespace,
                        Name = "N2",
                        FullyQualifiedName = "N2"
                    },

                    new LogicalLocation
                    {
                        Index = 1,
                        ParentIndex = 0,
                        Kind = LogicalLocationKind.Type,
                        Name = "T2",
                        FullyQualifiedName = "N2.T2"
                    }
                },

                Results = new List<Result>
                {
                    new Result
                    {
                        Locations = new List<Location>
                        {
                            new Location
                            {
                                LogicalLocation = new LogicalLocation
                                {
                                    Index = 1
                                }
                            }
                        }
                    }
                }
            };

            Run mergedRun = currentRun.DeepClone();
            Run baselineRunCopy = baselineRun.DeepClone();

            // Merge Results from the Current and Baseline Run
            var visitor = new RunMergingVisitor();

            visitor.VisitRun(mergedRun);
            visitor.VisitRun(baselineRunCopy);

            visitor.PopulateWithMerged(mergedRun);

            // Verify each Result points to a LogicalLocation with the same FullyQualifiedName as before
            VerifyLogicalLocationMatches(mergedRun, currentRun, 0);
            VerifyLogicalLocationMatches(mergedRun, baselineRun, currentRun.Results.Count);

            // The logical locations in the merged run are the union of the logical locations from
            // the baseline and current runs. In this test case, there are no logical locations in
            // common between the two runs, so the number of logical locations in the merged run is
            // just the sum of the baseline and current runs.
            mergedRun.LogicalLocations.Count.Should().Be(baselineRun.LogicalLocations.Count + currentRun.LogicalLocations.Count);
        }

        private void VerifyLogicalLocationMatches(Run mergedRun, Run sourceRun, int firstResultIndex)
        {
            for (int i = 0; i < sourceRun.Results.Count; ++i)
            {
                int previousIndex = sourceRun.Results[i].Locations[0].LogicalLocation.Index;
                LogicalLocation previousLocation = sourceRun.LogicalLocations[previousIndex];

                int newIndex = mergedRun.Results[firstResultIndex].Locations[0].LogicalLocation.Index;
                LogicalLocation newLocation = mergedRun.LogicalLocations[newIndex];

                newLocation.FullyQualifiedName.Should().Be(previousLocation.FullyQualifiedName);

                firstResultIndex++;
            }
        }

        [Fact]
        public void RunMergingVisitor_MapsRulesProperly()
        {
            var baselineRun = new Run
            {
                Tool = new Tool 
                {
                    Driver = new ToolComponent 
                    { 
                        Name = "Test Tool",
                        Rules = new List<ReportingDescriptor>()
                        { 
                            new ReportingDescriptor() { Id = "Rule001" },
                            new ReportingDescriptor() { Id = "Rule002" },
                        }
                    } ,
                },
                Results = new List<Result>
                {
                    new Result { RuleIndex = 1 },
                    new Result { RuleIndex = 0 },
                    new Result { RuleIndex = 1 }
                }
            };

            var currentRun = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = "Test Tool",
                        Rules = new List<ReportingDescriptor>()
                        {
                            new ReportingDescriptor() { Id = "Rule001" },
                            new ReportingDescriptor() { Id = "Rule003" },
                            new ReportingDescriptor() { Id = "Rule004" },
                        }
                    },
                },
                Results = new List<Result>
                {
                    new Result { RuleIndex = 0 },
                    new Result { RuleIndex = 1 },
                    new Result { RuleIndex = 1 }
                }
            };

            // Use the RunMergingVisitor to merge two runs, via Run.MergeResultsFrom
            Run mergedRun = currentRun.DeepClone();
            Run baselineRunCopy = baselineRun.DeepClone();

            mergedRun.MergeResultsFrom(baselineRunCopy);

            // We should get RuleId001, 002, 003. Rule004 wasn't referenced. Rule001 was used in both Runs.
            mergedRun.Tool.Driver.Rules.Count.Should().Be(3);

            // Verify each merged Run Result ArtifactIndex points to an Artifact with the same
            // text as the corresponding original Result
            VerifyRuleMatches(mergedRun, currentRun, 0);
            VerifyRuleMatches(mergedRun, baselineRun, currentRun.Results.Count);
        }

        private void VerifyRuleMatches(Run mergedRun, Run sourceRun, int firstResultIndex)
        {
            for (int i = 0; i < sourceRun.Results.Count; ++i)
            {
                Result previous = sourceRun.Results[i];
                Result newResult = mergedRun.Results[firstResultIndex];

                string expectedRuleId = previous.GetRule(sourceRun).Id;
                newResult.RuleId.Should().Be(expectedRuleId);
                newResult.GetRule(mergedRun).Id.Should().Be(expectedRuleId);

                firstResultIndex++;
            }
        }
    }
}

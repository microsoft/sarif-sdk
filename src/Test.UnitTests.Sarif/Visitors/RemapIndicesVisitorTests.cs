// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class RemapIndicesVisitorTests
    {

        [Fact]
        public void RemapIndicesVisitor_RemapsNestedFilesProperly()
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

            Run mergedRun = currentRun.DeepClone();

            // How does this remapping work? First, we initialize the indices remapper
            // with a set of existing files (analagous to the files list of the most
            // recent log in a baselining situation). Next, visit a set of results
            // from the baseline itself, providing the historical set of file data
            // objects from the baseline. The visit does two things: 1) updates the
            // visitor files table with the superset of file data objects between
            // the baseline and current run, 2) updates individual results from the
            // baseline so that their file index values are correct.

            // 1. At this point the merged run consists of a copy of the current run.
            //    After visitor construction, we should see that the visitor's
            //    CurrentFiles property is equivalent to mergedRun.Files. The visitor
            //    has also been initialized with a dictionary that uses the complete
            //    file hierarchy as a key into the files array
            var visitor = new RemapIndicesVisitor(mergedRun.Artifacts, currentLogicalLocations: null);
            visitor.CurrentArtifacts.Should().BeEquivalentTo(mergedRun.Artifacts);
            visitor.RemappedArtifacts.Count.Should().Be(mergedRun.Artifacts.Count);


            // 2. We set HistoricalFiles to point to the old files array from the
            //    baseline run, then visit each baseline result. After each result
            //    visit, any file data objects (including parent files) for the
            //    result have been added to visitor.CurrentFiles, if missing. Each
            //    result fileIndex data is updated. We move the updated result
            //    objects to the merged run.

            // Various operations mutate state. We will make a copy so that we can
            // compare our ulitmate results to an unaltered original baseline.
            Run baselineRunCopy = baselineRun.DeepClone();

            visitor.HistoricalFiles = baselineRunCopy.Artifacts;
            foreach (Result result in baselineRunCopy.Results)
            {
                visitor.VisitResult(result);
                mergedRun.Results.Add(result);
            }

            // 3. After completing the results array visit, we'll grab the 
            //    files array that represents all merged files from the runs.
            mergedRun.Artifacts = visitor.CurrentArtifacts;

            // The artifacts in the merged run are the union of the artifacts from the baseline and
            // current runs. In this test case, there are no artifacts in common between the two
            // runs, so the number of artifacts in the merged run is just the sum of the baseline
            // and current runs.
            mergedRun.Artifacts.Count.Should().Be(baselineRun.Artifacts.Count + currentRun.Artifacts.Count);

            // Every artifact in the merged run's artifacts array has an index that points to its
            // own location in the array, even though the artifacts that came from the baseline run
            // originally resided at a different index.
            for (int i = 0; i < mergedRun.Artifacts.Count; i++)
            {
                mergedRun.Artifacts[i].Location.Index.Should().Be(i);
            }

            // The artifacts from the current run are in the same place as before. The artifacts
            // from the baseline run have moved to the end of the merged array. Verify this by
            // matching up their text contents.
            for (int i = 0; i < currentRun.Artifacts.Count; i++)
            {
                int oldIndex = i;
                int newIndex = i;

                currentRun.Artifacts[oldIndex].Contents.Text.Should().Be(mergedRun.Artifacts[newIndex].Contents.Text);
            }

            for (int i = 0; i < baselineRun.Artifacts.Count; i++)
            {
                int oldIndex = i;
                int newIndex = i + currentRun.Artifacts.Count;

                baselineRun.Artifacts[oldIndex].Contents.Text.Should().Be(mergedRun.Artifacts[newIndex].Contents.Text);
            }

            // The artifact index in the merged run's results have been adjusted as well.
            for (int i = 0; i < currentRun.Results.Count; ++i)
            {
                int oldIndex = currentRun.Results[i].Locations[0].PhysicalLocation.ArtifactLocation.Index;
                int newIndex = mergedRun.Results[i].Locations[0].PhysicalLocation.ArtifactLocation.Index;

                mergedRun.Artifacts[newIndex].Contents.Text.Should().Be(currentRun.Artifacts[oldIndex].Contents.Text);
            }

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

        [Fact]
        public void RemapIndicesVisitor_RemapsLogicalLocations()
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

            var visitor = new RemapIndicesVisitor(currentArtifacts: null, currentLogicalLocations: mergedRun.LogicalLocations);
            visitor.CurrentLogicalLocations.Should().BeEquivalentTo(mergedRun.LogicalLocations);
            visitor.RemappedLogicalLocations.Count.Should().Be(mergedRun.LogicalLocations.Count);

            Run baselineRunCopy = baselineRun.DeepClone();

            visitor.HistoricalLogicalLocations = baselineRunCopy.LogicalLocations;
            foreach (Result result in baselineRunCopy.Results)
            {
                visitor.VisitResult(result);
                mergedRun.Results.Add(result);
            }

            mergedRun.LogicalLocations = visitor.CurrentLogicalLocations;

            // The logical locations in the merged run are the union of the logical locations from
            // the baseline and current runs. In this test case, there are no logical locations in
            // common between the two runs, so the number of logical locations in the merged run is
            // just the sum of the baseline and current runs.
            visitor.CurrentLogicalLocations.Count.Should().Be(baselineRun.LogicalLocations.Count + currentRun.LogicalLocations.Count);

            // TODO: Add the rest of the tests in analogy to the artifacts test above.
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class RemapIndicesVisitorTests
    {
        private Run CreateRunWithNestedFilesForTesting()
        {
            var run = new Run()
            {
                Tool = new Tool { Name = "Test Tool" },
                Files = new List<FileData>
                {
                    new FileData{ FileLocation = new FileLocation{ FileIndex = 0 }, Contents = new FileContent { Text = "1" } },
                    new FileData{ FileLocation = new FileLocation{ FileIndex = 1 }, Contents = new FileContent { Text = "1.2" }, ParentIndex = 0 },
                    new FileData{ FileLocation = new FileLocation{ FileIndex = 2 }, Contents = new FileContent { Text = "1.2.3." }, ParentIndex = 1 }
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
                                    FileLocation = new FileLocation
                                    {
                                        FileIndex = 2
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return run;
        }

        [Fact]
        public void RemapIndicesVisitor_RemapsNestedFilesProperly()
        {
            // This run has a single result that points to a doubly nested file
            Run firstRun = CreateRunWithNestedFilesForTesting();

            // This run has a single result pointing to a single file location
            var secondRun = new Run()
            {
                Tool = new Tool { Name = "Test Tool" },
                Files = new List<FileData>
                {
                    new FileData{ FileLocation = new FileLocation{ FileIndex = 0 }, Contents = new FileContent { Text = "New" } },
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
                                    FileLocation = new FileLocation
                                    {
                                        FileIndex = 0
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Run mergedRun = secondRun.DeepClone();

            // First we inject the result from the first run into the second. This is a surrogate
            // for result matching. I.E., this is a baseline result that is persisted forward. By
            // adding this result to the second file, however, we will compromise the file index
            // associated with the added result. 
            mergedRun.Results.Add(firstRun.Results[0]);

            var visitor = new RemapIndicesVisitor();
            visitor.HistoricalFiles = firstRun.Files;

            visitor.VisitRun(mergedRun);

            // We expect the merged files to be a superset of file data objects from the two runs
            mergedRun.Files.Count.Should().Be(firstRun.Files.Count + secondRun.Files.Count);

            // We expect that every merged file data has a corrected file index
            for (int i = 0; i < mergedRun.Files.Count; i++)
            {
                mergedRun.Files[i].FileLocation.FileIndex.Should().Be(i);
            }

            // We should see that the file index of the result we added should be pushed out by 1,
            // to account for the second run's file data objects being appended to the single
            // file data object in the first run.
            mergedRun.Results[1].Locations[0].PhysicalLocation.FileLocation.FileIndex =
                firstRun.Results[1].Locations[0].PhysicalLocation.FileLocation.FileIndex + 1;

            // Similarly, we expect that all file data objects from the first run have been offset by one
            for (int i = 0; i < 3; i++)
            {
                firstRun.Files[i].Contents.Text.Should().Be(mergedRun.Files[i + 1].Contents.Text);
            }

            // Finally, we should ensure that the parent index chain was updated properly on merging
            FileData fileData = mergedRun.Files[mergedRun.Results[1].Locations[0].PhysicalLocation.FileLocation.FileIndex];

            // Most nested
            fileData.ParentIndex.Should().Be(2);
            mergedRun.Files[fileData.ParentIndex].ParentIndex.Should().Be(1);
            mergedRun.Files[mergedRun.Files[fileData.ParentIndex].ParentIndex].ParentIndex.Should().Be(-1);
        }
    }
}

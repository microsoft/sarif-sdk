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
                Tool = new Tool { Driver = new ToolComponent { Name = "Test Tool" } },
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
            Run baselineRun = CreateRunWithNestedFilesForTesting();

            // This run has a single result pointing to a single file location
            var currentRun = new Run()
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "Test Tool" } },
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
            var visitor = new RemapIndicesVisitor(mergedRun.Files);
            visitor.CurrentFiles.Should().BeEquivalentTo(mergedRun.Files);
            visitor.RemappedFiles.Count.Should().Be(mergedRun.Files.Count);


            // 2. We set HistoricalFiles to point to the old files array from the
            //    baseline run, then visit each baseline result. After each result
            //    visit, any file data objects (including parent files) for the
            //    result have been added to visitor.CurrentFiles, if missing. Each
            //    result fileIndex data is updated. We move the updated result
            //    objects to the merged run.

            // Various operations mutate state. We will make a copy so that we can
            // compare our ulitmate results to an unaltered original baseline
            var baselineRunCopy = baselineRun.DeepClone();

            visitor.HistoricalFiles = baselineRunCopy.Files;
            foreach (Result result in baselineRunCopy.Results)
            {
                visitor.VisitResult(result);
                mergedRun.Results.Add(result);
            }

            // 3. After completing the results array visit, we'll grab the 
            //    files array that represents all merged files from the runs.
            mergedRun.Files = visitor.CurrentFiles;

            // We expect the merged files to be a superset of file data objects from the two runs
            mergedRun.Files.Count.Should().Be(baselineRun.Files.Count + currentRun.Files.Count);

            // We expect that every merged file data has a corrected file index
            for (int i = 0; i < mergedRun.Files.Count; i++)
            {
                mergedRun.Files[i].FileLocation.FileIndex.Should().Be(i);
            }

            // We should see that the file index of the result we added should be pushed out by 1,
            // to account for the second run's file data objects being appended to the single
            // file data object in the first run.
            mergedRun.Files[mergedRun.Results[0].Locations[0].PhysicalLocation.FileLocation.FileIndex].Contents.Text.Should()
                .Be(currentRun.Files[currentRun.Results[0].Locations[0].PhysicalLocation.FileLocation.FileIndex].Contents.Text);

            // Similarly, we expect that all file data objects from the first run have been offset by one
            for (int i = 0; i < 3; i++)
            {
                baselineRun.Files[i].Contents.Text.Should().Be(mergedRun.Files[i + 1].Contents.Text);
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

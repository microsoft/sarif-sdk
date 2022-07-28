// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using FluentAssertions;

using Moq;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class MergeCommandUnitTests
    {
        private static readonly string testDirectory = Directory.GetCurrentDirectory();

        [Fact]
        public void MergeCommand_WhenMergeRunsOn_RunShouldAggregateByToolVersion_SingleToolVersion()
        {
            // 2 logs, 2 runs, same tool verison. 9 results
            SarifLog sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(3) } };
            SarifLog sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(6) } };
            string sarifLog1Json = JsonConvert.SerializeObject(sarifLog1);
            string sarifLog2Json = JsonConvert.SerializeObject(sarifLog2);
            string sarifLog1FilePath = Path.Combine(testDirectory, "SarifLog1.sarif");
            string sarifLog2FilePath = Path.Combine(testDirectory, "SarifLog2.sarif");
            string outputFilePath = Path.Combine(testDirectory, "merged.sarif");
            StringBuilder outputStringBuilder = new StringBuilder();

            var mockFileSystem = new Mock<IFileSystem>();
            ArrangeMockFileSystemRead(mockFileSystem, sarifLog1Json, sarifLog1FilePath);
            ArrangeMockFileSystemRead(mockFileSystem, sarifLog2Json, sarifLog2FilePath);
            ArrangeMockFileSystemCreate(mockFileSystem, outputFilePath, outputStringBuilder);
            ArrangeMockFileSystemEnumerate(mockFileSystem, testDirectory, new[] { sarifLog1FilePath, sarifLog2FilePath });

            IFileSystem fileSystem = mockFileSystem.Object;

            var options = new MergeOptions
            {
                PrettyPrint = true,
                OutputDirectoryPath = testDirectory,
                OutputFileName = "merged.sarif",
                TargetFileSpecifiers = new[] { "*.sarif" },
                MergeRuns = true,
                Force = true,
            };

            var mergeCommand = new MergeCommand(fileSystem);
            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(0);

            SarifLog mergedLog = JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder.ToString());
            mergedLog.Runs.Count.Should().Be(1);
            mergedLog.Runs[0].Results.Count.Should().Be(9);
        }

        [Fact]
        public void MergeCommand_WhenMergeRunsOn_RunShouldAggregateByToolVersion_ThreeToolVersions()
        {
            // 2 logs 3 runs, 3 unique tool versions
            SarifLog sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(7, "Tool1"), CreateTestRun(4, "Tool2") } };
            SarifLog sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(2, "Tool3") } };
            string sarifLog1Json = JsonConvert.SerializeObject(sarifLog1);
            string sarifLog2Json = JsonConvert.SerializeObject(sarifLog2);
            string sarifLog1FilePath = Path.Combine(testDirectory, "SarifLog1.sarif");
            string sarifLog2FilePath = Path.Combine(testDirectory, "SarifLog2.sarif");
            string outputFilePath = Path.Combine(testDirectory, "merged.sarif");
            StringBuilder outputStringBuilder = new StringBuilder();

            var mockFileSystem = new Mock<IFileSystem>();
            ArrangeMockFileSystemRead(mockFileSystem, sarifLog1Json, sarifLog1FilePath);
            ArrangeMockFileSystemRead(mockFileSystem, sarifLog2Json, sarifLog2FilePath);
            ArrangeMockFileSystemCreate(mockFileSystem, outputFilePath, outputStringBuilder);
            ArrangeMockFileSystemEnumerate(mockFileSystem, testDirectory, new[] { sarifLog1FilePath, sarifLog2FilePath });

            IFileSystem fileSystem = mockFileSystem.Object;

            var options = new MergeOptions
            {
                PrettyPrint = true,
                OutputDirectoryPath = testDirectory,
                OutputFileName = "merged.sarif",
                TargetFileSpecifiers = new[] { "*.sarif" },
                MergeRuns = true,
                Force = true,
            };

            var mergeCommand = new MergeCommand(fileSystem);
            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(0);

            SarifLog mergedLog = JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder.ToString());
            mergedLog.Runs.Count.Should().Be(3);
            foreach (Run run in mergedLog.Runs)
            {
                if (run.Tool.Driver.Name == "Tool1")
                {
                    run.Results.Count.Should().Be(sarifLog1.Runs[0].Results.Count);
                }
                if (run.Tool.Driver.Name == "Tool2")
                {
                    run.Results.Count.Should().Be(sarifLog1.Runs[1].Results.Count);
                }
                if (run.Tool.Driver.Name == "Tool3")
                {
                    run.Results.Count.Should().Be(sarifLog2.Runs[0].Results.Count);
                }
            }
        }

        [Fact]
        public void MergeCommand_WhenMergeRunsOff_RunShouldAggregateByRuleToolVersion_SingleToolVersion()
        {
            // 2 logs 2 runs, same tool version, 6 rules
            SarifLog sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(5) } };
            SarifLog sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(6) } };
            string sarifLog1Json = JsonConvert.SerializeObject(sarifLog1);
            string sarifLog2Json = JsonConvert.SerializeObject(sarifLog2);
            string sarifLog1FilePath = Path.Combine(testDirectory, "SarifLog1.sarif");
            string sarifLog2FilePath = Path.Combine(testDirectory, "SarifLog2.sarif");
            string outputFilePath = Path.Combine(testDirectory, "merged.sarif");
            StringBuilder outputStringBuilder = new StringBuilder();

            var mockFileSystem = new Mock<IFileSystem>();
            ArrangeMockFileSystemRead(mockFileSystem, sarifLog1Json, sarifLog1FilePath);
            ArrangeMockFileSystemRead(mockFileSystem, sarifLog2Json, sarifLog2FilePath);
            ArrangeMockFileSystemCreate(mockFileSystem, outputFilePath, outputStringBuilder);
            ArrangeMockFileSystemEnumerate(mockFileSystem, testDirectory, new[] { sarifLog1FilePath, sarifLog2FilePath });

            IFileSystem fileSystem = mockFileSystem.Object;

            var options = new MergeOptions
            {
                PrettyPrint = true,
                OutputDirectoryPath = testDirectory,
                OutputFileName = "merged.sarif",
                TargetFileSpecifiers = new[] { "*.sarif" },
                MergeRuns = false,
                Force = true,
            };

            var mergeCommand = new MergeCommand(fileSystem);
            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(0);

            // merged log should have 6 runs grouped by rule id
            SarifLog mergedLog = JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder.ToString());
            mergedLog.Runs.Count.Should().Be(6);
            for (int i = 1; i <= 6; i++)
            {
                mergedLog.Runs[i - 1].Tool.Driver.Name.Should().Be("TestTool");
                mergedLog.Runs[i - 1].Results[0].RuleId.Should().Be($"TESTRULE00{i}");
            }
        }

        [Fact]
        public void MergeCommand_WhenSplitPerRule_LogShouldAggregateByRuleToolVersion()
        {
            // 2 logs 2 runs, same tool version, 6 rules
            SarifLog sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(6, null, true) } };
            SarifLog sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(4, null, true) } };
            string sarifLog1Json = JsonConvert.SerializeObject(sarifLog1);
            string sarifLog2Json = JsonConvert.SerializeObject(sarifLog2);
            string sarifLog1FilePath = Path.Combine(testDirectory, "SarifLog1.sarif");
            string sarifLog2FilePath = Path.Combine(testDirectory, "SarifLog2.sarif");

            string outputFilePath1 = Path.Combine(testDirectory, "TESTRULE.001_merged.sarif");
            StringBuilder outputStringBuilder1 = new StringBuilder();
            string outputFilePath2 = Path.Combine(testDirectory, "TESTRULE.002_merged.sarif");
            StringBuilder outputStringBuilder2 = new StringBuilder();
            string outputFilePath3 = Path.Combine(testDirectory, "TESTRULE.003_merged.sarif");
            StringBuilder outputStringBuilder3 = new StringBuilder();
            string outputFilePath4 = Path.Combine(testDirectory, "TESTRULE.004_merged.sarif");
            StringBuilder outputStringBuilder4 = new StringBuilder();
            string outputFilePath5 = Path.Combine(testDirectory, "TESTRULE.005_merged.sarif");
            StringBuilder outputStringBuilder5 = new StringBuilder();
            string outputFilePath6 = Path.Combine(testDirectory, "TESTRULE.006_merged.sarif");
            StringBuilder outputStringBuilder6 = new StringBuilder();

            var mockFileSystem = new Mock<IFileSystem>();
            ArrangeMockFileSystemRead(mockFileSystem, sarifLog1Json, sarifLog1FilePath);
            ArrangeMockFileSystemRead(mockFileSystem, sarifLog2Json, sarifLog2FilePath);
            ArrangeMockFileSystemCreate(mockFileSystem, outputFilePath1, outputStringBuilder1);
            ArrangeMockFileSystemCreate(mockFileSystem, outputFilePath2, outputStringBuilder2);
            ArrangeMockFileSystemCreate(mockFileSystem, outputFilePath3, outputStringBuilder3);
            ArrangeMockFileSystemCreate(mockFileSystem, outputFilePath4, outputStringBuilder4);
            ArrangeMockFileSystemCreate(mockFileSystem, outputFilePath5, outputStringBuilder5);
            ArrangeMockFileSystemCreate(mockFileSystem, outputFilePath6, outputStringBuilder6);
            ArrangeMockFileSystemEnumerate(mockFileSystem, testDirectory, new[] { sarifLog1FilePath, sarifLog2FilePath });

            IFileSystem fileSystem = mockFileSystem.Object;

            var options = new MergeOptions
            {
                PrettyPrint = true,
                OutputDirectoryPath = testDirectory,
                OutputFileName = "merged.sarif",
                TargetFileSpecifiers = new[] { "*.sarif" },
                SplittingStrategy = SplittingStrategy.PerRule,
                Force = true,
            };

            var mergeCommand = new MergeCommand(fileSystem);
            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(0);

            // should have 6 merged logs each log has result of 1 rule
            List<SarifLog> mergedLogs = new List<SarifLog>
            {
                JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder1.ToString()),
                JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder2.ToString()),
                JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder3.ToString()),
                JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder4.ToString()),
                JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder5.ToString()),
                JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder6.ToString()),
            };

            for (int i = 1; i <= 6; i++)
            {
                mergedLogs[i - 1].Runs[0].Tool.Driver.Name.Should().Be("TestTool");
                mergedLogs[i - 1].Runs[0].Results[0].RuleId.Should().Be($"TESTRULE/00{i}");
            }
        }

        private static Run CreateTestRun(int numberOfResult, string toolName = null, bool createSubRule = false)
        {
            var driver = new ToolComponent
            {
                Name = toolName ?? "TestTool",
                Version = "15.0.0.0",
                SemanticVersion = "15.0.0",
            };

            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = driver,
                },
            };

            run.Results ??= new List<Result>();
            for (int i = 1; i <= numberOfResult; i++)
            {
                run.Results.Add(
                    new Result
                    {
                        RuleId = createSubRule ? $"TESTRULE/00{i}" :  $"TESTRULE00{i}",
                        Guid = Guid.NewGuid().ToString(), // this value makes sure every result does not equal to other results
                    });
            }

            return run;
        }

        private static void ArrangeMockFileSystemRead(Mock<IFileSystem> mockFileSystem, string sarifLogJson, string sariflogFilePath)
        {
            mockFileSystem.Setup(x => x.DirectoryExists(Path.GetDirectoryName(sariflogFilePath))).Returns(true);
            mockFileSystem.Setup(x => x.FileExists(sariflogFilePath)).Returns(true);
            mockFileSystem.Setup(x => x.FileReadAllText(sariflogFilePath)).Returns(sarifLogJson);
        }

        private static void ArrangeMockFileSystemCreate(Mock<IFileSystem> mockFileSystem, string sariflogFilePath, StringBuilder outputStream)
        {
            mockFileSystem.Setup(x => x.FileExists(sariflogFilePath)).Returns(false);
            mockFileSystem.Setup(x => x.FileCreate(sariflogFilePath)).Returns(() => new MemoryStreamToStringBuilder(outputStream));
        }

        private static void ArrangeMockFileSystemEnumerate(Mock<IFileSystem> mockFileSystem, string targetDirectory, IEnumerable<string> files)
        {
            mockFileSystem.Setup(x => x.DirectoryExists(Path.GetDirectoryName(targetDirectory))).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(targetDirectory, It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(files);
        }
    }
}

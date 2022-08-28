// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using FluentAssertions;

using Microsoft.VisualStudio.Services.Common;

using Moq;

using Newtonsoft.Json;

using Xunit;

using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class MergeCommandUnitTests
    {
        private static readonly string testDirectory = Directory.GetCurrentDirectory();

        private readonly ITestOutputHelper output;
        private readonly Random random;

        public MergeCommandUnitTests(ITestOutputHelper testOutput)
        {
            this.output = testOutput;
            this.random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
        }

        [Fact]
        public void MergeCommand_WhenSpecifyInlineOption_ShouldReturnErrorCode()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            IFileSystem fileSystem = mockFileSystem.Object;

            var options = new MergeOptions
            {
                PrettyPrint = true,
                OutputDirectoryPath = testDirectory,
                OutputFileName = "merged.sarif",
                TargetFileSpecifiers = new[] { "*.sarif" },
                MergeRuns = true,
                Force = true,
                Inline = true,
            };

            var mergeCommand = new MergeCommand(fileSystem);
            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(1);
        }

        [Fact]
        public void MergeCommand_IfCanNotCreateOutputFile_ShouldReturnErrorCode()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            IFileSystem fileSystem = mockFileSystem.Object;
            string outputFilePath = Path.Combine(testDirectory, "merged.sarif");
            mockFileSystem.Setup(x => x.FileExists(outputFilePath)).Returns(false);

            var options = new MergeOptions
            {
                PrettyPrint = true,
                OutputDirectoryPath = testDirectory,
                OutputFileName = "merged.sarif",
                TargetFileSpecifiers = new[] { "*.sarif" },
                MergeRuns = true,
                Force = false,
            };

            var mergeCommand = new MergeCommand(fileSystem);
            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(1);
        }

        [Fact]
        public void MergeCommand_WhenMergeRunsOn_RunShouldAggregateByToolVersion_SingleToolVersion()
        {
            var sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(3) } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(6) } };
            string sarifLog1Json = JsonConvert.SerializeObject(sarifLog1);
            string sarifLog2Json = JsonConvert.SerializeObject(sarifLog2);
            string sarifLog1FilePath = Path.Combine(testDirectory, "SarifLog1.sarif");
            string sarifLog2FilePath = Path.Combine(testDirectory, "SarifLog2.sarif");
            string outputFilePath = Path.Combine(testDirectory, "merged.sarif");
            var outputStringBuilder = new StringBuilder();

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
            var sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(7, false, "Tool1"), CreateTestRun(4, false, "Tool2") } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(2, false, "Tool3") } };
            string sarifLog1Json = JsonConvert.SerializeObject(sarifLog1);
            string sarifLog2Json = JsonConvert.SerializeObject(sarifLog2);
            string sarifLog1FilePath = Path.Combine(testDirectory, "SarifLog1.sarif");
            string sarifLog2FilePath = Path.Combine(testDirectory, "SarifLog2.sarif");
            string outputFilePath = Path.Combine(testDirectory, "merged.sarif");
            var outputStringBuilder = new StringBuilder();

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
            var sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(5) } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(6) } };
            string sarifLog1Json = JsonConvert.SerializeObject(sarifLog1);
            string sarifLog2Json = JsonConvert.SerializeObject(sarifLog2);
            string sarifLog1FilePath = Path.Combine(testDirectory, "SarifLog1.sarif");
            string sarifLog2FilePath = Path.Combine(testDirectory, "SarifLog2.sarif");
            string outputFilePath = Path.Combine(testDirectory, "merged.sarif");
            var outputStringBuilder = new StringBuilder();

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
            var sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(6, true) } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(4, true) } };
            string sarifLog1Json = JsonConvert.SerializeObject(sarifLog1);
            string sarifLog2Json = JsonConvert.SerializeObject(sarifLog2);
            string sarifLog1FilePath = Path.Combine(testDirectory, "SarifLog1.sarif");
            string sarifLog2FilePath = Path.Combine(testDirectory, "SarifLog2.sarif");

            string outputFilePath1 = Path.Combine(testDirectory, "TESTRULE.001_merged.sarif");
            var outputStringBuilder1 = new StringBuilder();
            string outputFilePath2 = Path.Combine(testDirectory, "TESTRULE.002_merged.sarif");
            var outputStringBuilder2 = new StringBuilder();
            string outputFilePath3 = Path.Combine(testDirectory, "TESTRULE.003_merged.sarif");
            var outputStringBuilder3 = new StringBuilder();
            string outputFilePath4 = Path.Combine(testDirectory, "TESTRULE.004_merged.sarif");
            var outputStringBuilder4 = new StringBuilder();
            string outputFilePath5 = Path.Combine(testDirectory, "TESTRULE.005_merged.sarif");
            var outputStringBuilder5 = new StringBuilder();
            string outputFilePath6 = Path.Combine(testDirectory, "TESTRULE.006_merged.sarif");
            var outputStringBuilder6 = new StringBuilder();

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
            var mergedLogs = new List<SarifLog>
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

        private Run CreateTestRun(int numberOfResult, bool createSubRule = false, string toolName = null, string version = null, string semanticVersion = null)
        {
            Run run = RandomSarifLogGenerator.GenerateRandomRun(this.random, 0);
            run.Tool.Driver.Name = toolName ?? "TestTool";
            run.Tool.Driver.Version = version ?? "15.0.0.0";
            run.Tool.Driver.SemanticVersion = semanticVersion ?? "15.0.0.0";
            run.Results ??= new List<Result>();

            var artifactUri = new Uri("path/to/file", UriKind.Relative);

            for (int i = 1; i <= numberOfResult; i++)
            {
                string ruleId = createSubRule ? $"TESTRULE/00{i}" : $"TESTRULE00{i}";
                run.Results.AddRange(
                    RandomSarifLogGenerator.GenerateFakeResults(this.random, new List<string> { ruleId }, new List<string> { }, new List<Uri> { artifactUri }, 1));
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

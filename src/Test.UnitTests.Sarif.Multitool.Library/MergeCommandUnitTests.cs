// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                OutputDirectoryPath = testDirectory,
                OutputFileName = "merged.sarif",
                TargetFileSpecifiers = new[] { "*.sarif" },
                OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite, FilePersistenceOptions.Inline, FilePersistenceOptions.PrettyPrint },
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
                OutputFileOptions = new[] { FilePersistenceOptions.PrettyPrint },
                OutputDirectoryPath = testDirectory,
                OutputFileName = "merged.sarif",
                TargetFileSpecifiers = new[] { "*.sarif" },
            };

            var mergeCommand = new MergeCommand(fileSystem);
            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(1);
        }

        [Fact]
        public void MergeCommand_CoalescesSameToolIntoSingleRun()
        {
            var sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(5, ruleIdPrefix: "ALPHA") } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(6, ruleIdPrefix: "BETA") } };

            SarifLog mergedLog = RunMerge(sarifLog1, sarifLog2);

            // Every contributing run shares one tool + version, so the merge coalesces them
            // into a single run that carries all (distinct) results.
            mergedLog.Runs.Count.Should().Be(1);
            mergedLog.Runs[0].Tool.Driver.Name.Should().Be("TestTool");
            mergedLog.Runs[0].Results.Count.Should().Be(11);
        }

        [Fact]
        public void MergeCommand_DedupsValueIdenticalResultsAcrossInputs()
        {
            // The same finding can be re-reported in more than one shard of a sharded scan.
            // Coalescing collapses value-identical results so the merged run carries each once.
            var sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(5) } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(6) } };

            SarifLog mergedLog = RunMerge(sarifLog1, sarifLog2);

            mergedLog.Runs.Count.Should().Be(1);
            mergedLog.Runs[0].Results.Count.Should().Be(6);
        }

        [Fact]
        public void MergeCommand_CoalescesByToolVersion_SingleToolVersion()
        {
            var sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(3, ruleIdPrefix: "ALPHA") } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(6, ruleIdPrefix: "BETA") } };

            SarifLog mergedLog = RunMerge(sarifLog1, sarifLog2);

            mergedLog.Runs.Count.Should().Be(1);
            mergedLog.Runs[0].Results.Count.Should().Be(9);
        }

        [Fact]
        public void MergeCommand_CoalescesByToolVersion_ThreeToolVersions()
        {
            var sarifLog1 = new SarifLog { Runs = new[] { CreateTestRun(7, false, "Tool1"), CreateTestRun(4, false, "Tool2") } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateTestRun(2, false, "Tool3") } };

            SarifLog mergedLog = RunMerge(sarifLog1, sarifLog2);

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
        public void MergeCommand_AggregatesInvocationsAndRebasesInvocationIndex()
        {
            var sarifLog1 = new SarifLog { Runs = new[] { CreateRunReferencingInvocations("cmd-1a", "cmd-1b") } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateRunReferencingInvocations("cmd-2a") } };

            SarifLog mergedLog = RunMerge(sarifLog1, sarifLog2);

            mergedLog.Runs.Count.Should().Be(1);

            Run merged = mergedLog.Runs[0];

            // All invocations are aggregated (concatenated, no dedup).
            merged.Invocations.Count.Should().Be(3);
            merged.Results.Count.Should().Be(3);

            // Each result still resolves to the same invocation it referenced before the merge.
            // We seed Message.Text == the referenced invocation's CommandLine, so the cross-run
            // rebasing of provenance.invocationIndex is verified independently of array order.
            foreach (Result result in merged.Results)
            {
                int index = result.Provenance.InvocationIndex;
                index.Should().BeGreaterOrEqualTo(0);
                index.Should().BeLessThan(merged.Invocations.Count);
                merged.Invocations[index].CommandLine.Should().Be(result.Message.Text);
            }
        }

        [Fact]
        public void MergeCommand_PreservesDescriptorEnrichmentAndBaseRuleId()
        {
            // Two runs of the same tool+version, each carrying base CWE descriptors enriched
            // with a security-severity property and help text, and results whose ruleId uses
            // the hierarchical sub-id form ("CWE-79/<sub>"). The merge must collapse them into
            // one run while preserving the descriptor enrichment and the base reportingDescriptor
            // id, and keeping each result's full sub-id ruleId.
            var sarifLog1 = new SarifLog { Runs = new[] { CreateEnrichedSubIdRun(tag: "log1") } };
            var sarifLog2 = new SarifLog { Runs = new[] { CreateEnrichedSubIdRun(tag: "log2") } };

            SarifLog mergedLog = RunMerge(sarifLog1, sarifLog2);

            mergedLog.Runs.Count.Should().Be(1);
            Run merged = mergedLog.Runs[0];

            // Descriptors are deduped to their base id; the hierarchical sub-id never leaks into a
            // reportingDescriptor.id.
            List<string> ruleIds = merged.Tool.Driver.Rules.Select(r => r.Id).ToList();
            ruleIds.Should().Contain("CWE-79");
            ruleIds.Should().Contain("CWE-89");
            ruleIds.Should().NotContain(id => id.Contains("/"));

            // Descriptor enrichment (security-severity property + help) survives the merge.
            ReportingDescriptor xss = merged.Tool.Driver.Rules.Single(r => r.Id == "CWE-79");
            xss.GetProperty<string>("security-severity").Should().Be("7.8");
            xss.Help.Text.Should().Be("XSS help");

            // Every result keeps its full sub-id ruleId and resolves to the base descriptor.
            merged.Results.Count.Should().Be(6);
            foreach (Result result in merged.Results)
            {
                result.RuleId.Should().Contain("/");
                result.GetRule(merged).Id.Should().Be(result.RuleId.Split('/')[0]);
            }
        }

        private SarifLog RunMerge(SarifLog sarifLog1, SarifLog sarifLog2)
        {
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
                OutputDirectoryPath = testDirectory,
                OutputFileName = "merged.sarif",
                TargetFileSpecifiers = new[] { "*.sarif" },
                OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite, FilePersistenceOptions.PrettyPrint },
            };

            var mergeCommand = new MergeCommand(fileSystem);
            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(0);

            return JsonConvert.DeserializeObject<SarifLog>(outputStringBuilder.ToString());
        }

        private static Run CreateRunReferencingInvocations(params string[] commandLines)
        {
            var invocations = new List<Invocation>();
            var results = new List<Result>();

            for (int i = 0; i < commandLines.Length; i++)
            {
                invocations.Add(new Invocation { CommandLine = commandLines[i] });
                results.Add(new Result
                {
                    RuleId = "TESTRULE001",
                    Message = new Message { Text = commandLines[i] },
                    Provenance = new ResultProvenance { InvocationIndex = i },
                });
            }

            return new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "TestTool", Version = "15.0.0.0", SemanticVersion = "15.0.0.0" } },
                Invocations = invocations,
                Results = results,
            };
        }

        private Run CreateTestRun(int numberOfResult, bool createSubRule = false, string toolName = null, string version = null, string semanticVersion = null, string ruleIdPrefix = "TESTRULE")
        {
            Run run = RandomSarifLogGenerator.GenerateRandomRun(this.random, 0);
            run.Tool.Driver.Name = toolName ?? "TestTool";
            run.Tool.Driver.Version = version ?? "15.0.0.0";
            run.Tool.Driver.SemanticVersion = semanticVersion ?? "15.0.0.0";
            run.Results ??= new List<Result>();

            var artifactUri = new Uri("path/to/file", UriKind.Relative);

            for (int i = 1; i <= numberOfResult; i++)
            {
                string ruleId = createSubRule ? $"{ruleIdPrefix}/00{i}" : $"{ruleIdPrefix}00{i}";
                run.Results.AddRange(
                    RandomSarifLogGenerator.GenerateFakeResults(this.random, new List<string> { ruleId }, new List<Uri> { artifactUri }, 1));
            }

            return run;
        }

        private static Run CreateEnrichedSubIdRun(string toolName = "TestTool", string version = "15.0.0.0", string tag = "")
        {
            var rules = new List<ReportingDescriptor>
            {
                new ReportingDescriptor { Id = "CWE-79", Name = "CrossSiteScripting", Help = new MultiformatMessageString { Text = "XSS help" } },
                new ReportingDescriptor { Id = "CWE-89", Name = "SqlInjection", Help = new MultiformatMessageString { Text = "SQLi help" } },
            };
            rules[0].SetProperty("security-severity", "7.8");
            rules[1].SetProperty("security-severity", "8.8");

            var results = new List<Result>
            {
                new Result { RuleId = "CWE-79/unescaped-view-input", RuleIndex = 0, Message = new Message { Text = $"a-{tag}" } },
                new Result { RuleId = "CWE-89/string-concat-query", RuleIndex = 1, Message = new Message { Text = $"b-{tag}" } },
                new Result { RuleId = "CWE-79/dom-xss-via-sanitizer-bypass", RuleIndex = 0, Message = new Message { Text = $"c-{tag}" } },
            };

            return new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = toolName, Version = version, SemanticVersion = version, Rules = rules } },
                Results = results,
            };
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

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

using Moq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class RewriteCommandTests : FileDiffingUnitTests
    {
        private RewriteOptions options;

        public RewriteCommandTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        private static RewriteOptions CreateDefaultOptions()
        {
            return new RewriteOptions
            {
                BasePath = @"C:\vs\src\2\s\",
                BasePathToken = "SRCROOT",
                OutputFileOptions = new[] { FilePersistenceOptions.Inline, FilePersistenceOptions.PrettyPrint },
                SarifOutputVersion = SarifVersion.Current,
                NormalizeForGhas = true,
            };
        }

        protected override string ConstructTestOutputFromInputResource(string testFilePath, object parameter)
        {
            return RunRewriteCommand(testFilePath);
        }

        [Fact]
        public void RunRewriteCommand_RewriteForGhas()
        {
            string testFilePath = "RunWithArtifacts.sarif";

            this.options = CreateDefaultOptions();

            RunTest(testFilePath);
        }

        private string RunRewriteCommand(string testFilePath)
        {
            string inputSarifLog = GetInputSarifTextFromResource(testFilePath);

            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "mylog.sarif");
            StringBuilder transformedContents = new StringBuilder();

            this.options.InputFilePath = logFilePath;
            this.options.OutputFilePath = null;

            Mock<IFileSystem> mockFileSystem = ArrangeMockFileSystem(inputSarifLog, logFilePath, transformedContents);

            var RewriteCommand = new RewriteCommand(mockFileSystem.Object);

            int returnCode = RewriteCommand.Run(this.options);
            string actualOutput = transformedContents.ToString();

            returnCode.Should().Be(0);

            return actualOutput;
        }

        private static Mock<IFileSystem> ArrangeMockFileSystem(string sarifLog, string logFilePath, StringBuilder transformedContents)
        {
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.FileReadAllText(logFilePath)).Returns(sarifLog);
            mockFileSystem.Setup(x => x.FileOpenRead(logFilePath)).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(sarifLog)));
            mockFileSystem.Setup(x => x.FileCreate(logFilePath)).Returns(() => new MemoryStreamToStringBuilder(transformedContents));
            mockFileSystem.Setup(x => x.FileWriteAllText(logFilePath, It.IsAny<string>())).Callback<string, string>((path, contents) => { transformedContents.Append(contents); });
            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { logFilePath });
            return mockFileSystem;
        }

        public const string MinimalPrereleaseV2Text =
    @"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0-csd.2.beta.2018-10-10"",
  ""runs"": [
    {
      ""tool"": {
        ""fullName"": ""TestTool 1.0.0"",
        ""name"": ""TestTool""
      },
      ""automationDetails"": { },
      ""results"": []
    }
  ]
}";

        public static readonly SarifLog MinimalCurrentV2 = new SarifLog
        {
            Runs = new List<Run>
            {
                new Run
                {
                    Tool = new Tool
                    {
                        Driver = new ToolComponent
                        {
                            FullName = "TestTool-1.0.0",
                            Name = "TestTool",
                            Rules = new List<ReportingDescriptor>() { new ReportingDescriptor() { Id = "JS1001" } }
                        }
                    },
                    Results = new List<Result>(),
                    AutomationDetails = new RunAutomationDetails() { Id = "automation-id" }
                }
            }
        };

        public static readonly string MinimalCurrentV2Text = JsonConvert.SerializeObject(MinimalCurrentV2);

        // A minimal valid log file. We add a single result to ensure
        // there's enough v1 contents so that we trigger any bad code
        // paths that assume the file contents is v2.
        public const string MinimalV1Text =
@"{
  ""version"": ""1.0.0"",
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""fullName"": ""TestTool 1.0.0"",
        ""name"": ""TestTool""
      },
      ""automationDetails"": { },
      ""results"": [{ ""ruleId"" : ""JS1001"", ""message"" : ""My rule message."" } ]
    }
  ]
}";

        [Fact]
        public void TransformCommand_TransformsMinimalCurrentV2FileToCurrentV2()
        {
            // A minimal valid pre-release v2 log file.
            string logFileContents = MinimalCurrentV2Text;

            RunTransformationToV2Test(logFileContents);
        }

        [Fact]
        public void TransformCommand_TransformsMinimalPrereleaseV2FileToCurrentV2()
        {
            // A minimal valid pre-release v2 log file.
            string logFileContents = MinimalPrereleaseV2Text;

            // First, ensure that our test sample\ schema uri and SARIF version differs 
            // from current. Otherwise we won't realize any value from this test

            PrereleaseSarifLogVersionDiffersFromCurrent(MinimalPrereleaseV2Text);

            RunTransformationToV2Test(logFileContents);
        }

        [Fact]
        public void TransformCommand_TransformsMinimalV1FileToCurrentV2()
        {
            RunTransformationToV2Test(MinimalV1Text);
        }

        [Fact]
        public void TransformCommand_TransformsMinimalCurrentV2FileToV1()
        {
            // A minimal valid pre-release v2 log file.
            string logFileContents = MinimalCurrentV2Text;

            RunTransformationToV1Test(logFileContents);
        }

        [Fact]
        public void TransformCommand_TransformsMinimalPrereleaseV2FileToV1()
        {
            // A minimal valid pre-release v2 log file.
            string logFileContents = MinimalPrereleaseV2Text;

            PrereleaseSarifLogVersionDiffersFromCurrent(MinimalPrereleaseV2Text);

            RunTransformationToV1Test(logFileContents);
        }

        [Fact]
        public void TransformCommand_TransformsMinimalV1FileToV1()
        {
            RunTransformationToV1Test(MinimalV1Text);
        }

        [Fact]
        public void TransformCommand_WhenOutputFormatOptionsAreInconsistent_PrefersPrettyPrint()
        {
            var options = new RewriteOptions
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Minify, FilePersistenceOptions.PrettyPrint },
            };

            options.PrettyPrint.Should().BeTrue();
            options.Minify.Should().BeFalse();

            (string _, int returnCode) = RunTransformationCore(MinimalV1Text, SarifVersion.Current, options);
            returnCode.Should().Be(0);
        }

        private static void RunTransformationToV2Test(string logFileContents)
        {
            (string transformedContents, int returnCode) = RunTransformationCore(logFileContents, SarifVersion.Current);
            returnCode.Should().Be(0);

            // Finally, ensure that transformation corrected schema uri and SARIF version.
            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(transformedContents);
            sarifLog.SchemaUri.Should().Be(SarifUtilities.SarifSchemaUri);
            sarifLog.Version.Should().Be(SarifVersion.Current);
        }

        private void PrereleaseSarifLogVersionDiffersFromCurrent(string prereleaseV2Text)
        {
            JObject sarifLog = JObject.Parse(prereleaseV2Text);

            ((string)sarifLog["$schema"]).Should().NotBe(SarifUtilities.SarifSchemaUri);
            ((string)sarifLog["version"]).Should().NotBe(SarifUtilities.StableSarifVersion);
        }

        private void RunTransformationToV1Test(string logFileContents)
        {
            (string transformedContents, int returnCode) = RunTransformationCore(logFileContents, SarifVersion.OneZeroZero);
            returnCode.Should().Be(0);

            // Finally, ensure that transformation corrected schema uri and SARIF version.
            var settings = new JsonSerializerSettings { ContractResolver = SarifContractResolverVersionOne.Instance };
            SarifLogVersionOne v1SarifLog = JsonConvert.DeserializeObject<SarifLogVersionOne>(transformedContents, settings);
            v1SarifLog.SchemaUri.Should().Be(SarifVersion.OneZeroZero.ConvertToSchemaUri());
            v1SarifLog.Version.Should().Be(SarifVersionVersionOne.OneZeroZero);
        }

        private static (string transformedContents, int returnCode) RunTransformationCore(
            string logFileContents,
            SarifVersion targetVersion,
            RewriteOptions options = null)
        {
            string LogFilePath = Path.Combine(Directory.GetCurrentDirectory(), "mylog.sarif");

            options ??= new RewriteOptions
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Inline },
                SarifOutputVersion = targetVersion,
                InputFilePath = LogFilePath
            };

            if (options.SarifOutputVersion == SarifVersion.Unknown)
            {
                options.SarifOutputVersion = targetVersion;
            }

            if (options.InputFilePath == null)
            {
                options.OutputFileOptions = new HashSet<FilePersistenceOptions>(options.OutputFileOptions) { FilePersistenceOptions.Inline }.ToArray();
                options.InputFilePath = LogFilePath;
            }

            var transformedContents = new StringBuilder();
            transformedContents.Append(logFileContents);

            var mockFileSystem = new Mock<IFileSystem>();
            //  This only works because we're testing "Inline"
            //  TODO: Verify a separate OutputFilePath works as expected
            mockFileSystem.Setup(x => x.FileReadAllText(options.InputFilePath)).Returns(transformedContents.ToString());
            mockFileSystem.Setup(x => x.FileOpenRead(options.InputFilePath)).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(transformedContents.ToString())));
            mockFileSystem.Setup(x => x.FileCreate(options.InputFilePath)).Returns(() => new MemoryStreamToStringBuilder(transformedContents));
            mockFileSystem.Setup(x => x.FileWriteAllText(options.InputFilePath, It.IsAny<string>())).Callback<string, string>((path, contents) =>
            {
                transformedContents.Clear();
                transformedContents.Append(contents);
            });

            var rewriteCommand = new RewriteCommand(mockFileSystem.Object);

            int returnCode = rewriteCommand.Run(options);

            return (transformedContents.ToString(), returnCode);
        }
    }
}

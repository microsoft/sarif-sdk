// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Converters;

using Moq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ConvertCommandTests
    {
        private static readonly TestAssetResourceExtractor s_extractor = new TestAssetResourceExtractor(typeof(ConvertCommandTests));

        [Fact]
        public void ConvertCommand_SemmleQlExample()
        {
            // Try converting a tiny sample SemmleQl file
            string sampleFilePath = "SemmleQlSample.csv";
            string outputFilePath = Path.ChangeExtension(sampleFilePath, ".sarif");
            File.WriteAllText(sampleFilePath, s_extractor.GetResourceText(sampleFilePath));

            var options = new ConvertOptions
            {
                ToolFormat = ToolFormat.SemmleQL,
                InputFilePath = sampleFilePath,
                OutputFilePath = outputFilePath,
                OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
            };

            // Verify command returned success
            int returnCode = new ConvertCommand().Run(options);
            returnCode.Should().Be(0);

            // Verify SARIF output log exists
            File.Exists(outputFilePath).Should().BeTrue();

            // Verify log loads, has correct Result count, and spot check a Result
            var log = SarifLog.Load(outputFilePath);
            log.Runs[0].Results.Count.Should().Be(8);
            log.Runs[0].Results[7].Locations[0].PhysicalLocation.Region.StartLine.Should().Be(40);
            log.Runs[0].Results[7].Locations[0].PhysicalLocation.Region.StartColumn.Should().Be(43);
        }

        [Fact]
        public void Run_WhenOutputFilePathIsADirectory_Fails()
        {
            const string InputFilePath = @"C:\input\ToolOutput.xml";
            const string OutputFilePath = @"C:\output\ToolOutput.xml.sarif";

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.DirectoryExists(OutputFilePath)).Returns(true);
            IFileSystem fileSystem = mockFileSystem.Object;

            var options = new ConvertOptions
            {
                ToolFormat = ToolFormat.FxCop,
                InputFilePath = InputFilePath,
                OutputFilePath = OutputFilePath,
                OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
            };

            int returnCode = new ConvertCommand().Run(options, fileSystem);

            returnCode.Should().Be(1);
        }

        [Fact]
        public void Run_WhenOutputFileExistsAndForceNotSpecified_Fails()
        {
            const string InputFilePath = @"C:\input\ToolOutput.xml";
            const string OutputFilePath = @"C:\output\ToolOutput.xml.sarif";

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.DirectoryExists(OutputFilePath)).Returns(false);
            mockFileSystem.Setup(x => x.FileExists(OutputFilePath)).Returns(true);
            IFileSystem fileSystem = mockFileSystem.Object;

            var options = new ConvertOptions
            {
                ToolFormat = ToolFormat.FxCop,
                InputFilePath = InputFilePath,
                OutputFilePath = OutputFilePath
            };

            int returnCode = new ConvertCommand().Run(options, fileSystem);

            returnCode.Should().Be(1);
        }

        [Fact]
        public void Run_WhenOutputFormatOptionsAreInconsistent_PrefersPrettyPrint()
        {
            // Run on the same sample file that succeeded in the test ConvertCommand_SemmleQlExample.
            // This time we expect it to fail because of the inconsistent output format options.
            string sampleFilePath = "SemmleQlSample.csv";
            string outputFilePath = Path.ChangeExtension(sampleFilePath, ".sarif");
            File.WriteAllText(sampleFilePath, s_extractor.GetResourceText(sampleFilePath));

            var options = new ConvertOptions
            {
                ToolFormat = ToolFormat.SemmleQL,
                InputFilePath = sampleFilePath,
                OutputFilePath = outputFilePath,
                OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite, FilePersistenceOptions.Minify, FilePersistenceOptions.PrettyPrint },
            };

            options.PrettyPrint.Should().BeTrue();
            options.Minify.Should().BeFalse();

            int returnCode = new ConvertCommand().Run(options);
            returnCode.Should().Be(0);
        }

        [Fact]
        public void Run_WhenNormalizeForGHAzDO_StampsDetectedPipelineContext()
        {
            string sampleFilePath = "SemmleQlSample.csv";
            string outputFilePath = Path.ChangeExtension(sampleFilePath, ".ghazdo.sarif");
            File.WriteAllText(sampleFilePath, s_extractor.GetResourceText(sampleFilePath));

            var environment = new Mock<IEnvironmentVariableGetter>();
            environment.Setup(x => x.GetEnvironmentVariable("TF_BUILD")).Returns("True");
            environment.Setup(x => x.GetEnvironmentVariable("SYSTEM_COLLECTIONURI")).Returns("https://dev.azure.com/example/");
            environment.Setup(x => x.GetEnvironmentVariable("SYSTEM_TEAMPROJECTID")).Returns("11111111-1111-1111-1111-111111111111");
            environment.Setup(x => x.GetEnvironmentVariable("BUILD_DEFINITIONID")).Returns("42");
            environment.Setup(x => x.GetEnvironmentVariable("BUILD_DEFINITIONNAME")).Returns("Security Scan");
            environment.Setup(x => x.GetEnvironmentVariable("BUILD_BUILDID")).Returns("101");
            environment.Setup(x => x.GetEnvironmentVariable("SYSTEM_PHASEID")).Returns("22222222-2222-2222-2222-222222222222");
            environment.Setup(x => x.GetEnvironmentVariable("SYSTEM_PHASENAME")).Returns("Analyze");
            environment.Setup(x => x.GetEnvironmentVariable("BUILD_SOURCEBRANCH")).Returns("refs/heads/main");

            var options = new ConvertOptions
            {
                ToolFormat = ToolFormat.SemmleQL,
                InputFilePath = sampleFilePath,
                OutputFilePath = outputFilePath,
                OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                NormalizeForGHAzDO = true,
            };

            int returnCode = new ConvertCommand(environment.Object).Run(options);

            returnCode.Should().Be(0);
            RunAutomationDetails automation = SarifLog.Load(outputFilePath).Runs[0].AutomationDetails;
            automation.Id.Should().StartWith("azuredevops/pipeline/build/example/");
            automation.GetProperty<string>("azuredevops/pipeline/build/buildDefinitionId").Should().Be("42");
        }
    }
}

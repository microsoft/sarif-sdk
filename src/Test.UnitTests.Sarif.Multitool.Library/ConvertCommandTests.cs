// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Moq;
using System.IO;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ConvertCommandTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(PageCommandTests));

        [Fact]
        public void ConvertCommand_SemmleQlExample()
        {
            // Try converting a tiny sample SemmleQl file
            string sampleFilePath = "SemmleQlSample.csv";
            string outputFilePath = Path.ChangeExtension(sampleFilePath, ".sarif");
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText($"ConvertCommand.{sampleFilePath}"));

            var options = new ConvertOptions
            {
                ToolFormat = ToolFormat.SemmleQL,
                InputFilePath = sampleFilePath,
                OutputFilePath = outputFilePath,
                Force = true
            };

            // Verify command returned success
            int returnCode = new ConvertCommand().Run(options);
            returnCode.Should().Be(0);

            // Verify SARIF output log exists
            File.Exists(outputFilePath).Should().BeTrue();

            // Verify log loads, has correct Result count, and spot check a Result
            SarifLog log = SarifLog.Load(outputFilePath);
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
                Force = true
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
    }
}

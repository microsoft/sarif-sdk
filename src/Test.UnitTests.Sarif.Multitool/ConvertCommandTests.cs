// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ConvertCommandTests
    {
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

            int returnCode = ConvertCommand.Run(options, fileSystem);

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

            int returnCode = ConvertCommand.Run(options, fileSystem);

            returnCode.Should().Be(1);
        }
    }
}

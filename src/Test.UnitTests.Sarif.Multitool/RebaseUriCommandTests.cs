// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Multitool
{
    public class RebaseUriCommandTests : FileDiffingUnitTests
    {
        private static ResourceExtractor Extractor = new ResourceExtractor(typeof(RebaseUriCommandTests));
        private RebaseUriOptions options;

        public RebaseUriCommandTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void RebaseUriCommand_RebaseRunWithArtifacts()
        {
            string testFilePath = "RunWithArtifacts.sarif";

            this.options = new RebaseUriOptions
            {
                BasePath = @"C:\vs\src\2\s\",
                BasePathToken = "SRCROOT",
                Inline = true,
                SarifOutputVersion = SarifVersion.Current,
                PrettyPrint = true
            };

            RunTest(testFilePath);
        }

        protected override string ConstructTestOutputFromInputResource(string testFilePath)
        {
            return RunRebaseUriCommand(testFilePath, this.options);
        }

        protected override string GetResourceText(string resourceName)
        {
            return Extractor.GetResourceText($"RebaseUriCommand.{resourceName}");
        }

        private string RunRebaseUriCommand(string testFilePath, RebaseUriOptions options)
        {
            string inputSarifLog = Extractor.GetResourceText($"RebaseUriCommand.{testFilePath}");

            string logFilePath = @"c:\logs\mylog.sarif";
            StringBuilder transformedContents = new StringBuilder();

            options.TargetFileSpecifiers = new string[] { logFilePath };

            Mock<IFileSystem> mockFileSystem = ArrangeMockFileSystem(inputSarifLog, logFilePath, transformedContents);

            var rebaseUriCommand = new RebaseUriCommand(mockFileSystem.Object);

            int returnCode = rebaseUriCommand.Run(options);
            string actualOutput = transformedContents.ToString();

            returnCode.Should().Be(0);

            return actualOutput;
        }

        private static Mock<IFileSystem> ArrangeMockFileSystem(string sarifLog, string logFilePath, StringBuilder transformedContents)
        {
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.ReadAllText(logFilePath)).Returns(sarifLog);
            mockFileSystem.Setup(x => x.OpenRead(logFilePath)).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(sarifLog)));
            mockFileSystem.Setup(x => x.Create(logFilePath)).Returns(() => new MemoryStreamToStringBuilder(transformedContents));
            mockFileSystem.Setup(x => x.WriteAllText(logFilePath, It.IsAny<string>())).Callback<string, string>((path, contents) => { transformedContents.Append(contents); });
            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.GetFilesInDirectory(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { logFilePath });
            return mockFileSystem;
        }
    }
}

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Multitool
{
    public class RebaseUriCommandTests : FileDiffingUnitTests
    {
        private static ResourceExtractor Extractor = new ResourceExtractor(typeof(RebaseUriCommandTests));

        public RebaseUriCommandTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void RebaseUriCommand_RebaseRunWithArtifacts()
        {
            string testFilePath = "RunWithArtifacts.sarif";

            var options = new RebaseUriOptions
            {
                BasePath = @"C:\vs\src\2\s\",
                BasePathToken = "SRCROOT",
                Inline = true,
                SarifOutputVersion = SarifVersion.Current,
                PrettyPrint = true
            };

            RunAndValidateRebaseUriCommand(testFilePath, options);
        }

        private void RunAndValidateRebaseUriCommand(string testFilePath, RebaseUriOptions options)
        {
            string inputSarifLog = Extractor.GetResourceText($"RebaseUriCommand.Inputs.{testFilePath}");
            string expectedOutput = Extractor.GetResourceText($"RebaseUriCommand.ExpectedOutputs.{testFilePath}");

            string logFilePath = @"c:\logs\mylog.sarif";
            StringBuilder transformedContents = new StringBuilder();

            options.TargetFileSpecifiers = new string[] { logFilePath };

            Mock<IFileSystem> mockFileSystem = ArrangeMockFileSystem(inputSarifLog, logFilePath, transformedContents);

            var rebaseUriCommand = new RebaseUriCommand(mockFileSystem.Object);

            int returnCode = rebaseUriCommand.Run(options);
            string actualOutput = transformedContents.ToString();

            returnCode.Should().Be(0);

            Assert.True(
                AreEquivalent<SarifLog>(actualOutput, expectedOutput),
                GetErrorMessageWithDiffFileDetails(testFilePath, expectedOutput, actualOutput)
                );
        }

        private static string GetErrorMessageWithDiffFileDetails(string testFilePath, string expectedOutput, string actualOutput)
        {
            // TODO: Rebaseline test output functionality is not integrated for these tests yet.
            // Any change in expected behaviour must be manually updated in the output file.

            string expectedOutputFile = $"expected.{testFilePath}";
            string actualOutputFile = $"actual.{testFilePath}";

            File.WriteAllText(expectedOutputFile, expectedOutput);
            File.WriteAllText(actualOutputFile, actualOutput);

            return $"The output did not match with the expected log. Check differences with: {GenerateDiffCommand(null, expectedOutputFile, actualOutputFile)}";
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

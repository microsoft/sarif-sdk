// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class LogResultTests
    {
        // A simple schema against which a SARIF log file successfully validates.
        // This way, we don't have to read the SARIF schema from disk to run these tests.
        private const string SchemaFileContents =
@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object""
}";

        private const string SchemaFilePath = @"c:\schemas\SimpleSchemaForTest.json";
        private const string LogFileDirectory = @"C:\Users\John\logs";
        private const string LogFileName = "example.sarif";
        private const string OutputFilePath = "example-validation.sarif";

        [Fact]
        public void LogResult_ADO1003()
        {
            var rule = new Rules.AdoProvideToolDriver();
            //// Here's the space:
            //string LogFileDirectoryWithSpace =
            //    Path.Combine(Directory.GetCurrentDirectory(), "directory name with space", "sub directory name with space");

            //string logFilePath = Path.Combine(LogFileDirectoryWithSpace, LogFileName);

            //var mockFileSystem = new Mock<IFileSystem>();
            //mockFileSystem.Setup(x => x.FileInfoLength(It.IsAny<string>())).Returns(1024);
            //mockFileSystem.Setup(x => x.DirectoryExists(LogFileDirectoryWithSpace)).Returns(true);
            //mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(LogFileDirectoryWithSpace, It.IsAny<string>(), SearchOption.TopDirectoryOnly)).Returns(new[] { LogFileName });
            //mockFileSystem.Setup(x => x.FileReadAllText(logFilePath)).Returns(RewriteCommandTests.MinimalCurrentV2Text);
            //mockFileSystem.Setup(x => x.FileOpenRead(logFilePath)).Returns(new MemoryStream(Encoding.UTF8.GetBytes(RewriteCommandTests.MinimalCurrentV2Text)));
            //mockFileSystem.Setup(x => x.FileReadAllText(SchemaFilePath)).Returns(SchemaFileContents);

            //var validateCommand = new ValidateCommand(mockFileSystem.Object);

            //var options = new ValidateOptions
            //{
            //    SchemaFilePath = SchemaFilePath,
            //    TargetFileSpecifiers = new string[] { logFilePath },
            //    Kind = new List<ResultKind> { ResultKind.Fail },
            //    Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error }
            //};

            //var context = new SarifValidationContext { FileSystem = mockFileSystem.Object };
            //int returnCode = validateCommand.Run(options, ref context);
            //context.RuntimeErrors.Should().Be(RuntimeConditions.OneOrMoreWarningsFired);
            //returnCode.Should().Be(0);
        }
    }
}

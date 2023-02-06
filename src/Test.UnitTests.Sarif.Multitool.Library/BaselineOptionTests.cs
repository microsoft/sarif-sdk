// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Moq;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class BaselineOptionTests
    {
        private static readonly TestAssetResourceExtractor s_extractor = new TestAssetResourceExtractor(typeof(ValidateCommandTests));

        // test setup

        private static readonly string CurrentDirectory = Directory.GetCurrentDirectory();
        private static readonly string LogFileDirectory = Path.Combine(CurrentDirectory, "John", "Sarif logs");
        private static readonly string LogFileName = "example.sarif";
        private static readonly string BaseFileDirectory = Path.Combine(CurrentDirectory, "Scan Results", "First Run");
        private static readonly string BaselineFileName = "baseline.sarif";
        private static readonly string SchemaFilePath = Path.Combine(CurrentDirectory, "schemas", "SimpleSchemaForTest.json");

        private const string SchemaFileContents =
@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object""
}";

        [Fact]
        public void ValidateCommandWithBaseline_Invalid_WhenOutputFileIsIsAbsent()
        {
            string logFilePath = Path.Combine(LogFileDirectory, LogFileName);
            string baseLineFilePath = Path.Combine(BaseFileDirectory, BaselineFileName);

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.DirectoryExists(LogFileDirectory)).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryExists(BaseFileDirectory)).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryGetDirectories(It.IsAny<string>())).Returns(Array.Empty<string>());
            mockFileSystem.Setup(x => x.DirectoryGetFiles(LogFileDirectory, LogFileName)).Returns(new string[] { logFilePath, });
            mockFileSystem.Setup(x => x.DirectoryGetFiles(BaseFileDirectory, BaselineFileName)).Returns(new string[] { baseLineFilePath, });
            mockFileSystem.Setup(x => x.FileReadAllText(logFilePath)).Returns(RewriteCommandTests.MinimalCurrentV2Text);
            mockFileSystem.Setup(x => x.FileReadAllText(baseLineFilePath)).Returns(RewriteCommandTests.MinimalCurrentV2Text);
            mockFileSystem.Setup(x => x.FileReadAllText(SchemaFilePath)).Returns(SchemaFileContents);

            var validateCommand = new ValidateCommand(mockFileSystem.Object);

            var options = new ValidateOptions
            {
                SchemaFilePath = SchemaFilePath,
                TargetFileSpecifiers = new string[] { logFilePath },
                BaselineSarifFile = baseLineFilePath,
            };

            int returnCode = validateCommand.Run(options);
            returnCode.Should().Be(1);
            validateCommand.ExecutionException.Should().BeOfType<ExitApplicationException<ExitReason>>();
        }

        [Fact]
        public void ValidateCommandWithBaseline_ResultShouldHaveBaselineStatus()
        {
            string path = "ValidateSarif.sarif";
            string configuration = "Configuration.json";
            string outputPath = "ValidateSarifOutput.sarif";
            string baselineFilePath = "Baseline.sarif";
            File.WriteAllText(path, s_extractor.GetResourceText(path));
            File.WriteAllText(configuration, s_extractor.GetResourceText(configuration));
            File.WriteAllText(baselineFilePath, s_extractor.GetResourceText(baselineFilePath));

            var options = new ValidateOptions
            {
                ConfigurationFilePath = configuration,
                TargetFileSpecifiers = new string[] { path },
                OutputFilePath = outputPath,
                Force = true,
                BaselineSarifFile = baselineFilePath,
                Kind = new List<ResultKind> { ResultKind.Fail },
                Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error },
                MaxFileSizeInKilobytes = 1024,
            };

            // Verify command returned success
            int returnCode = new ValidateCommand().Run(options);
            returnCode.Should().Be(0);

            SarifLog sarifLog = SarifLog.Load(outputPath);
            sarifLog.Runs.Count.Should().Be(1);
            sarifLog.Runs[0].Results.Count.Should().Be(1);

            // all results should have baseline status
            sarifLog.Runs[0].Results.Any(r => r.BaselineState == BaselineState.None).Should().BeFalse();
        }

        [Fact]
        public void ValidateCommandWithBaseline_InlineUpdate()
        {
            string path = "ValidateSarif.sarif";
            string configuration = "Configuration.json";
            string outputPath = "ValidateSarifOutput.sarif";
            string baselineFilePath = "Baseline.sarif";
            File.WriteAllText(path, s_extractor.GetResourceText(path));
            File.WriteAllText(configuration, s_extractor.GetResourceText(configuration));
            File.WriteAllText(baselineFilePath, s_extractor.GetResourceText(baselineFilePath));

            var options = new ValidateOptions
            {
                ConfigurationFilePath = configuration,
                TargetFileSpecifiers = new string[] { path },
                OutputFilePath = outputPath,
                Inline = true,
                Force = true,
                BaselineSarifFile = baselineFilePath,
                Kind = new List<ResultKind> { ResultKind.Fail },
                Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error }
            };

            // Verify command returned success
            int returnCode = new ValidateCommand().Run(options);
            returnCode.Should().Be(0);

            SarifLog sarifLog = SarifLog.Load(baselineFilePath);
            sarifLog.Runs.Count.Should().Be(1);
            sarifLog.Runs[0].Results.Count.Should().Be(1);

            // Baseline file should be updated with baseline status because --inline switch
            sarifLog.Runs[0].Results.Any(r => r.BaselineState == BaselineState.None).Should().BeFalse();
        }

        private string SniffVersion(string sarifPath)
        {
            using (var reader = new JsonTextReader(new StreamReader(File.OpenRead(sarifPath))))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName && ((string)reader.Value).Equals("version"))
                    {
                        reader.Read();
                        return (string)reader.Value;
                    }
                }
            }

            return null;
        }
    }
}

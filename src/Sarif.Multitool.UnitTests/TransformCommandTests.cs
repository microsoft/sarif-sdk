// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Sarif.Multitool.UnitTests
{
    public class TransformCommandTests
    {
        // Regression test for Issue #1064, "ValidatorCommand fails when target file name contains a space"
        [Fact]
        public void TransformCommand_TransformsMinimalFile()
        {
            // A minimal valid log file.
            string logFileContents =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0-beta.2018-09-26"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""TestTool""
      },
      ""results"": []
    }
  ]
}";
            // First, ensure that our test sample\ schema uri and SARIF version differs 
            // from current. Otherwise we won't realize any value from this test
            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(logFileContents);
            sarifLog.SchemaUri.Should().NotBe(SarifUtilities.SarifSchemaUri);
            sarifLog.Version.Should().NotBe(SarifUtilities.SemanticVersion);


            string logFilePath = @"c:\logs\mylog.sarif";
            string transformedContents = null;

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.ReadAllText(logFilePath)).Returns(logFileContents);
            mockFileSystem.Setup(x => x.WriteAllText(logFilePath, It.IsAny<string>())).Callback<string, string>((path, contents) => { transformedContents = contents; });

            var transformCommand = new TransformCommand(mockFileSystem.Object, testing: true);

            var options = new TransformOptions
            {
                Inline = true,
                Version = SarifVersion.Current,
                InputFilePath = logFilePath
            };

            int returnCode = transformCommand.Run(options);            
            returnCode.Should().Be(0);

            // Finally, ensure that transformation corrected schema uri and SARIF version.
            sarifLog = JsonConvert.DeserializeObject<SarifLog>(transformedContents);
            sarifLog.SchemaUri.Should().Be(SarifUtilities.SarifSchemaUri);
            sarifLog.Version.Should().Be(SarifVersion.Current);
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

using Moq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class TransformCommandTests
    {
        public const string MinimalPrereleaseV2Text =
    @"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0-csd.2.beta.2018-10-10"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""TestTool""
      },
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
                            Name = "TestTool"
                        }
                    },
                    Results = new List<Result>()
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
        ""name"": ""TestTool""
      },
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
        public void TransformCommand_WhenOutputFormatOptionsAreInconsistent_Fails()
        {
            var options = new TransformOptions
            {
                PrettyPrint = true,
                Minify = true
            };

            (string _, int returnCode) = RunTransformationCore(MinimalV1Text, SarifVersion.Current, options);
            returnCode.Should().Be(1);
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
            TransformOptions options = null)
        {
            const string LogFilePath = @"c:\logs\mylog.sarif";

            options ??= new TransformOptions
            {
                Inline = true,
                SarifOutputVersion = targetVersion,
                InputFilePath = LogFilePath
            };

            if (options.SarifOutputVersion == SarifVersion.Unknown)
            {
                options.SarifOutputVersion = targetVersion;
            }

            if (options.InputFilePath == null)
            {
                options.Inline = true;
                options.InputFilePath = LogFilePath;
            }

            var transformedContents = new StringBuilder();

            // Complex: TransformCommand has code paths that use Create and OpenRead, but also ReadAllText and WriteAllText.
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.FileReadAllText(options.InputFilePath)).Returns(logFileContents);
            mockFileSystem.Setup(x => x.FileOpenRead(options.InputFilePath)).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(logFileContents)));
            mockFileSystem.Setup(x => x.FileCreate(options.InputFilePath)).Returns(() => new MemoryStreamToStringBuilder(transformedContents));
            mockFileSystem.Setup(x => x.FileWriteAllText(options.InputFilePath, It.IsAny<string>())).Callback<string, string>((path, contents) => { transformedContents.Append(contents); });

            var transformCommand = new TransformCommand(mockFileSystem.Object);


            int returnCode = transformCommand.Run(options);

            return (transformedContents.ToString(), returnCode);
        }
    }
}

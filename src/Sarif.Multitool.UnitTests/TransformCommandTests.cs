﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

namespace Sarif.Multitool.UnitTests
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

        private static void RunTransformationToV2Test(string logFileContents)
        {
            string transformedContents = RunTransformationCore(logFileContents, SarifVersion.Current);

            // Finally, ensure that transformation corrected schema uri and SARIF version.
            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(transformedContents);
            sarifLog.SchemaUri.Should().Be(SarifUtilities.SarifSchemaUri);
            sarifLog.Version.Should().Be(SarifVersion.Current);
        }

        private void PrereleaseSarifLogVersionDiffersFromCurrent(string prereleaseV2Text)
        {
            JObject sarifLog = JObject.Parse(prereleaseV2Text);

            ((string)sarifLog["$schema"]).Should().NotBe(SarifUtilities.SarifSchemaUri);
            ((string)sarifLog["version"]).Should().NotBe(SarifUtilities.SemanticVersion);
        }

        private void RunTransformationToV1Test(string logFileContents)
        {
            string transformedContents = RunTransformationCore(logFileContents, SarifVersion.OneZeroZero);

            // Finally, ensure that transformation corrected schema uri and SARIF version.
            var settings = new JsonSerializerSettings { ContractResolver = SarifContractResolverVersionOne.Instance };
            SarifLogVersionOne v1SarifLog = JsonConvert.DeserializeObject<SarifLogVersionOne>(transformedContents, settings);
            v1SarifLog.SchemaUri.Should().Be(@"http://json.schemastore.org/sarif-1.0.0");
            v1SarifLog.Version.Should().Be(SarifVersionVersionOne.OneZeroZero);
        }

        private static string RunTransformationCore(string logFileContents, SarifVersion targetVersion)
        {
            string logFilePath = @"c:\logs\mylog.sarif";
            string transformedContents = null;

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.ReadAllText(logFilePath)).Returns(logFileContents);
            mockFileSystem.Setup(x => x.WriteAllText(logFilePath, It.IsAny<string>())).Callback<string, string>((path, contents) => { transformedContents = contents; });

            var transformCommand = new TransformCommand(mockFileSystem.Object, testing: true);

            var options = new TransformOptions
            {
                Inline = true,
                SarifOutputVersion = targetVersion,
                InputFilePath = logFilePath
            };

            int returnCode = transformCommand.Run(options);
            returnCode.Should().Be(0);

            return transformedContents;
        }
    }
}

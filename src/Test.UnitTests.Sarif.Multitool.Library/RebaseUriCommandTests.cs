// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class RebaseUriCommandTests : FileDiffingUnitTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(RebaseUriCommandTests));

        private RebaseUriOptions options;

        public RebaseUriCommandTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void RebaseUriCommand_InjectsRegions()
        {
            string productDirectory = FileDiffingFunctionalTests.GetProductDirectory();
            string analysisFile = Path.Combine(productDirectory, @"ReleaseHistory.md");
            File.Exists(analysisFile).Should().BeTrue();

            var sarifLog = new SarifLog
            {
                Runs = new[]
                {
                    new Run { Results = new[] { 
                        new Result { Locations = new [] {
                               new Location {
                                    PhysicalLocation = new PhysicalLocation {
                                         Region = new Region { StartLine = 7 },
                                         ArtifactLocation = new ArtifactLocation
                                         {
                                             Uri = new Uri(analysisFile)
                                         }
                                   }
                               } }
                    } } }
                }
            };

            string inputSarifLog = JsonConvert.SerializeObject(sarifLog);

            string logFilePath = @"c:\logs\mylog.sarif";
            StringBuilder transformedContents = new StringBuilder();

            RebaseUriOptions options = CreateDefaultOptions();

            options.TargetFileSpecifiers = new string[] { logFilePath };
            
            options.DataToInsert = new[] 
            { 
                OptionallyEmittedData.RegionSnippets |
                OptionallyEmittedData.ContextRegionSnippets 
            };

            Mock<IFileSystem> mockFileSystem = ArrangeMockFileSystem(inputSarifLog, logFilePath, transformedContents);

            // Test snippet injection.
            var rebaseUriCommand = new RebaseUriCommand(mockFileSystem.Object);

            int returnCode = rebaseUriCommand.Run(options);
            returnCode.Should().Be(0);

            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(transformedContents.ToString());
            actualLog.Runs[0].Results[0].Locations[0].PhysicalLocation.Region.Snippet.Should().NotBeNull();
            actualLog.Runs[0].Results[0].Locations[0].PhysicalLocation.ContextRegion.Snippet.Should().NotBeNull();

            // Now test that this data is removed.
            inputSarifLog = JsonConvert.SerializeObject(actualLog);
            transformedContents.Length = 0;
            mockFileSystem = ArrangeMockFileSystem(inputSarifLog, logFilePath, transformedContents);
            rebaseUriCommand = new RebaseUriCommand(mockFileSystem.Object);

            options.DataToRemove = options.DataToInsert;
            options.DataToInsert = null;

            returnCode = rebaseUriCommand.Run(options);
            returnCode.Should().Be(0);

            actualLog = JsonConvert.DeserializeObject<SarifLog>(transformedContents.ToString());
            actualLog.Runs[0].Results[0].Locations[0].PhysicalLocation.Region.Snippet.Should().BeNull();
            actualLog.Runs[0].Results[0].Locations[0].PhysicalLocation.ContextRegion.Snippet.Should().BeNull();
        }


        [Fact]
        public void RebaseUriCommand_RebaseRunWithArtifacts()
        {
            string testFilePath = "RunWithArtifacts.sarif";

            this.options = CreateDefaultOptions();

            RunTest(testFilePath);
        }

        private static RebaseUriOptions CreateDefaultOptions()
        {
            return new RebaseUriOptions
            {
                BasePath = @"C:\vs\src\2\s\",
                BasePathToken = "SRCROOT",
                Inline = true,
                SarifOutputVersion = SarifVersion.Current,
                PrettyPrint = true
            };
        }

        protected override string ConstructTestOutputFromInputResource(string testFilePath, object parameter)
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

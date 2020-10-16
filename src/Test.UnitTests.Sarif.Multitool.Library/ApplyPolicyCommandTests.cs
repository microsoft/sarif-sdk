// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Moq;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ApplyPolicyCommandTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(ApplyPolicyCommandTests));

        [Fact]
        public void WhenOutputFormatOptionsAreInconsistent_Fails()
        {
            const string InputFilePath = "AnyFile.sarif";

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(_ => _.FileExists(InputFilePath)).Returns(true);

            var options = new ApplyPolicyOptions
            {
                InputFilePath = InputFilePath,
                Inline = true,
                PrettyPrint = true,
                Minify = true
            };

            int returnCode = new ApplyPolicyCommand(mockFileSystem.Object).Run(options);

            returnCode.Should().Be(1);

            mockFileSystem.Verify(_ => _.FileExists(InputFilePath), Times.Once);
            mockFileSystem.VerifyNoOtherCalls();
        }

        [Fact]
        public void WhenInputContainsOnePolicy_ShouldSucceed()
        {
            string path = "WithPolicy.sarif";
            File.WriteAllText(path, Extractor.GetResourceText($"ApplyPolicyCommand.{path}"));

            // Verify log loads, has correct Result count, and spot check a Result
            SarifLog log = ExecuteTest(path);
            log.Runs[0].Results.Count.Should().Be(1);
            log.Runs[0].Results[0].Level.Should().Be(FailureLevel.Error);

            File.Delete(path);
        }

        [Fact]
        public void WhenInputContainsMultiplePolicies_ShouldSucceed()
        {
            string path = "WithPolicy2.sarif";
            File.WriteAllText(path, Extractor.GetResourceText($"ApplyPolicyCommand.{path}"));

            // Verify log loads, has correct Result count, and spot check a Result
            SarifLog log = ExecuteTest(path);
            log.Runs[0].Results.Count.Should().Be(1);
            log.Runs[0].Results[0].Level.Should().Be(FailureLevel.Note);

            File.Delete(path);
        }

        private SarifLog ExecuteTest(string path)
        {
            var options = new ApplyPolicyOptions
            {
                InputFilePath = path,
                OutputFilePath = path,
                Force = true
            };

            // Verify command returned success
            int returnCode = new ApplyPolicyCommand().Run(options);
            returnCode.Should().Be(0);

            // Verify SARIF output log exists
            File.Exists(path).Should().BeTrue();

            return SarifLog.Load(path);
        }
    }
}

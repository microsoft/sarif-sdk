// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ApplyPolicyCommandTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(ApplyPolicyCommandTests));

        [Fact]
        public void WhenInputContainsOnePolicy_ShouldSucceed()
        {
            string path = "WithPolicy.sarif";
            File.WriteAllText(path, Extractor.GetResourceText($"ApplyPolicyCommand.{path}"));

            // Verify log loads, has correct Result count, and spot check a Result
            SarifLog log = ExecuteTest(path);
            log.Runs[0].Results.Count.Should().Be(1);
            log.Runs[0].Results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void WhenInputContainsMultiplePolicies_ShouldApplyPoliciesInOrder()
        {
            string path = "WithPolicy2.sarif";
            File.WriteAllText(path, Extractor.GetResourceText($"ApplyPolicyCommand.{path}"));

            // Verify log loads, has correct Result count, and spot check a Result
            SarifLog log = ExecuteTest(path);
            log.Runs[0].Results.Count.Should().Be(1);
            log.Runs[0].Results[0].Level.Should().Be(FailureLevel.Note);
        }

        private SarifLog ExecuteTest(string path)
        {
            var options = new ApplyPolicyOptions
            {
                InputFilePath = path,
                OutputFilePath = path,
                Force = true,
                SarifOutputVersion = SarifVersion.Current
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

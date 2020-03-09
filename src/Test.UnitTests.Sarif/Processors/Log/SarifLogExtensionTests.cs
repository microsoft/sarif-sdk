using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Processors.Log
{
    public class SarifLogExtensionTests
    {
        private readonly ITestOutputHelper output;

        public SarifLogExtensionTests(ITestOutputHelper testOutput)
        {
            this.output = testOutput;
        }

        [Fact]
        public void TestMerge_WorksAsExpected()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            List<SarifLog> logs = new List<SarifLog>();
            List<SarifLog> secondLogSet = new List<SarifLog>();
            int count = random.Next(10) + 1;
            for (int i = 0; i < count; i++)
            {
                SarifLog log = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, random.Next(1, 10));
                logs.Add(log);
                secondLogSet.Add(log.DeepClone());
            }

            SarifLog combinedLog = logs.Merge();

            combinedLog.Runs.Count.Should().Be(secondLogSet.Select(l => l.Runs == null ? 0 : l.Runs.Count).Sum());
        }

        [Fact]
        public void RebaseUri_WorksAsExpected()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            List<SarifLog> logs = new List<SarifLog>();

            int count = random.Next(10) + 1;
            for (int i = 0; i < count; i++)
            {
                SarifLog log = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, random.Next(10));
                logs.Add(log);
            }
            logs.RebaseUri("SRCROOT", false, new Uri(RandomSarifLogGenerator.GeneratorBaseUri));

            // All file URIs should be relative and the files dictionary should be rewritten.
            logs.All(
                log =>
                    log.Runs == null ||
                    log.Runs.All(
                        run =>
                            run.Results == null ||
                            run.Results.All(
                                result =>
                                    result.Locations == null ||
                                    result.Locations.All(
                                        location =>
                                            !location.PhysicalLocation.ArtifactLocation.Uri.IsAbsoluteUri
                                            && !string.IsNullOrEmpty(location.PhysicalLocation.ArtifactLocation.UriBaseId)))))
                .Should().BeTrue();
        }

        [Fact]
        public void AbsoluteUri_ReversesRebasedURIs()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            int count = random.Next(10) + 1;
            for (int i = 0; i < count; i++)
            {
                SarifLog log = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, random.Next(10));
                string inputSarifText = JsonConvert.SerializeObject(log, Formatting.Indented);

                log.RebaseUri("SRCROOT", false, new Uri(RandomSarifLogGenerator.GeneratorBaseUri)).MakeUrisAbsolute();

                string outputSarifText = JsonConvert.SerializeObject(log, Formatting.Indented);

                if (log.Runs == null) { continue; }

                log.Runs.All(
                    run =>
                        run.Results == null ||
                        run.Results.All(
                            result =>
                                result.Locations == null ||
                                result.Locations.All(
                                    location =>
                                        location.PhysicalLocation.ArtifactLocation.Uri.IsAbsoluteUri
                                        && string.IsNullOrEmpty(location.PhysicalLocation.ArtifactLocation.UriBaseId))))
                   .Should().BeTrue();
            }
        }

        [Fact]
        public void RebaseUri_WorksAsExpectedWithRebaseRelativeUris()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            List<SarifLog> logs = new List<SarifLog>();

            int count = random.Next(10) + 1;
            for (int i = 0; i < count; i++)
            {
                SarifLog log = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, random.Next(10));
                logs.Add(log);
            }
            logs.RebaseUri("SRCROOT", true, new Uri(RandomSarifLogGenerator.GeneratorBaseUri));

            // All file URIs should be relative and the files dictionary should be rewritten.
            logs.All(
                log =>
                    log.Runs == null ||
                    log.Runs.All(
                        run =>
                            run.Results == null ||
                            run.Results.All(
                                result =>
                                    result.Locations == null ||
                                    result.Locations.All(
                                        location =>
                                            !location.PhysicalLocation.ArtifactLocation.Uri.IsAbsoluteUri
                                            && !string.IsNullOrEmpty(location.PhysicalLocation.ArtifactLocation.UriBaseId)))))
                .Should().BeTrue();
        }
    }
}

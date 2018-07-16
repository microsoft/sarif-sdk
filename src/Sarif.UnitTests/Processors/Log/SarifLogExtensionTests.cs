using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
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
            logs.RebaseUri("%SRCROOT%", new Uri(RandomSarifLogGenerator.GeneratorBaseUri));

            // All file URIs should be relative and the files dictionary should be rewritten.
            logs.All(
                log => 
                    log.Runs == null ||
                    log.Runs.All(
                        run => 
                            run.Files == null ||
                            run.Files.Keys.All(
                                key => 
                                    run.Files[key].FileLocation.Uri.ToString() == key
                                    && !run.Files[key].FileLocation.Uri.IsAbsoluteUri
                                    && !string.IsNullOrEmpty(run.Files[key].FileLocation.UriBaseId))))
                .Should().BeTrue();
        }

        [Fact]
        public void AbsoluteUri_ReversesRebasedURIs()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            List<SarifLog> logs = new List<SarifLog>();
            int count = random.Next(10) + 1;
            for (int i = 0; i < count; i++)
            {
                SarifLog log = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, random.Next(10));
                logs.Add(log);
            }
            logs.RebaseUri("%SRCROOT%", new Uri(RandomSarifLogGenerator.GeneratorBaseUri)).MakeUrisAbsolute();

            // All file URIs should be absolute.
            logs.All(
                log =>
                    log.Runs == null ||
                    log.Runs.All(
                        run =>
                            run.Files == null ||
                            run.Files.Keys.All(
                                key =>
                                    run.Files[key].FileLocation.Uri.ToString() == key
                                    && run.Files[key].FileLocation.Uri.IsAbsoluteUri
                                    && string.IsNullOrEmpty(run.Files[key].FileLocation.UriBaseId))))
                .Should().BeTrue();
        }
    }
}

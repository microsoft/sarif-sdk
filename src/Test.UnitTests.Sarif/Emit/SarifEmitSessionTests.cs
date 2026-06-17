// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Test.UnitTests.Emit
{
    /// <summary>
    /// Exercises <see cref="SarifEmitSession"/> — the multi-run authoring surface that wraps N
    /// independent <see cref="RunEmitContext"/> shards and assembles them into a single log. Confirms
    /// run isolation (each run resolves its own index space), declared ordering, and that the
    /// in-memory <see cref="SarifEmitSession.Finalize"/> and streaming
    /// <see cref="SarifEmitSession.FinalizeToFile"/> paths agree.
    /// </summary>
    public class SarifEmitSessionTests
    {
        [Fact]
        public void Finalize_NoRuns_ProducesEmptyRunsArray()
        {
            using var session = new SarifEmitSession();

            SarifLog log = session.Finalize();

            log.Runs.Should().BeEmpty();
            log.SchemaUri.Should().NotBeNull();
            log.Version.Should().Be(SarifVersion.Current);
        }

        [Fact]
        public void Finalize_MultipleRuns_PreservesDeclaredOrder()
        {
            using var session = new SarifEmitSession();

            AddNamedRun(session, "first");
            AddNamedRun(session, "second");
            AddNamedRun(session, "third");

            SarifLog log = session.Finalize();

            log.Runs.Select(r => r.Tool.Driver.Name)
                .Should().Equal("first", "second", "third");
        }

        [Fact]
        public void Finalize_TwoRuns_ResolveIndependentRuleTables()
        {
            using var session = new SarifEmitSession();

            RunEmitContext first = AddNamedRun(session, "first");
            first.AddResults(Result("CWE-89/sql-injection")).Succeeded.Should().BeTrue();

            RunEmitContext second = AddNamedRun(session, "second");
            second.AddResults(Result("CWE-79/xss")).Succeeded.Should().BeTrue();

            SarifLog log = session.Finalize();

            // Each run carries its own rule table (the sub-id collapses to its CWE base rule) and
            // its own zero-based ruleIndex space.
            log.Runs[0].Tool.Driver.Rules.Should().ContainSingle().Which.Id.Should().Be("CWE-89");
            log.Runs[0].Results.Should().ContainSingle().Which.RuleIndex.Should().Be(0);
            log.Runs[1].Tool.Driver.Rules.Should().ContainSingle().Which.Id.Should().Be("CWE-79");
            log.Runs[1].Results.Should().ContainSingle().Which.RuleIndex.Should().Be(0);
        }

        [Fact]
        public void AddRun_WithHeader_SeedsRunSkeleton()
        {
            using var session = new SarifEmitSession();

            session.AddRun(Header("demo"));

            session.Runs.Should().ContainSingle();
            session.Finalize().Runs[0].Tool.Driver.Name.Should().Be("demo");
        }

        [Fact]
        public void FinalizeToFile_Compact_RoundTripsToSameRunsAsFinalize()
        {
            using var session = new SarifEmitSession();
            AddPopulatedRun(session, "first", "CWE-89/sql-injection");
            AddPopulatedRun(session, "second", "CWE-79/xss");

            SarifLog inMemory = session.Finalize();
            string path = Path.GetTempFileName();
            try
            {
                session.FinalizeToFile(path, prettyPrint: false);
                File.ReadAllText(path).Should().NotContain("\n");

                SarifLog fromFile = Load(path);
                AssertSameRuns(inMemory, fromFile);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void FinalizeToFile_Pretty_RoundTripsToSameRunsAsFinalize()
        {
            using var session = new SarifEmitSession();
            AddPopulatedRun(session, "first", "CWE-89/sql-injection");
            AddPopulatedRun(session, "second", "CWE-79/xss");

            SarifLog inMemory = session.Finalize();
            string path = Path.GetTempFileName();
            try
            {
                session.FinalizeToFile(path, prettyPrint: true);
                File.ReadAllText(path).Should().Contain("\n");

                SarifLog fromFile = Load(path);
                AssertSameRuns(inMemory, fromFile);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void FinalizeToFile_StampsCanonicalSchemaAndVersion()
        {
            using var session = new SarifEmitSession();
            AddNamedRun(session, "only");

            string path = Path.GetTempFileName();
            try
            {
                session.FinalizeToFile(path);

                var root = JObject.Parse(File.ReadAllText(path));
                root["$schema"].Value<string>()
                    .Should().Be(SarifVersion.Current.ConvertToSchemaUri().OriginalString);
                root["version"].Value<string>()
                    .Should().Be(SarifVersion.Current.ConvertToText());
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void CustomSinkFactory_IsInvokedOncePerRunWithRunIndex()
        {
            var indices = new List<int>();
            using var session = new SarifEmitSession(index =>
            {
                indices.Add(index);
                return new InMemoryEmitSink();
            });

            session.AddRun();
            session.AddRun();
            session.AddRun();

            indices.Should().Equal(0, 1, 2);
        }

        private static RunEmitContext AddNamedRun(SarifEmitSession session, string toolName)
            => session.AddRun(Header(toolName));

        private static void AddPopulatedRun(SarifEmitSession session, string toolName, string ruleId)
        {
            RunEmitContext context = AddNamedRun(session, toolName);
            context.AddResults(Result(ruleId)).Succeeded.Should().BeTrue();
        }

        private static void AssertSameRuns(SarifLog expected, SarifLog actual)
        {
            actual.Runs.Count.Should().Be(expected.Runs.Count);
            for (int i = 0; i < expected.Runs.Count; i++)
            {
                Serialize(actual.Runs[i]).Should().Be(Serialize(expected.Runs[i]));
            }
        }

        private static string Serialize(Run run)
            => JsonConvert.SerializeObject(run, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        private static SarifLog Load(string path)
            => JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(path));

        private static JObject Header(string toolName)
            => (JObject)JToken.Parse($@"{{ ""tool"": {{ ""driver"": {{ ""name"": ""{toolName}"" }} }} }}");

        private static JObject Result(string ruleId)
            => (JObject)JToken.Parse($@"{{ ""ruleId"": ""{ruleId}"", ""message"": {{ ""text"": ""finding"" }} }}");
    }
}

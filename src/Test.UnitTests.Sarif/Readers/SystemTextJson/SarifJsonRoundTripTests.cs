// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Readers.SystemTextJson;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Phase-0 spike for issue #3038: measures System.Text.Json (eager-path) parity
    /// against the existing Newtonsoft path on the SDK's golden-file corpus.
    /// </summary>
    /// <remarks>
    /// <para>These tests are deliberately additive — the Newtonsoft path is untouched
    /// and continues to be exercised by <see cref="RoundTrippingTests"/>. Failures here
    /// do not regress shipped behavior; they are the data the spike exists to produce.
    /// </para>
    /// <para>The parity bar is semantic equivalence (<c>JToken.DeepEquals</c>),
    /// the same bar the existing <c>FileDiffingUnitTests.AreEquivalent</c> applies.
    /// Byte-identity is not required: STJ and Newtonsoft differ on escaping, number
    /// formatting, and indentation, none of which are visible to a SARIF consumer.
    /// </para>
    /// </remarks>
    public class SarifJsonRoundTripTests
    {
        private readonly ITestOutputHelper _output;

        public SarifJsonRoundTripTests(ITestOutputHelper output) => _output = output;

        // -------------------------------------------------------------------
        // Per-file targeted tests (the existing RoundTrippingTests corpus).
        // -------------------------------------------------------------------

        [Theory]
        [InlineData("InvocationOverridesRuleDefaultConfigurationOfError.sarif")]
        [InlineData("PropertyBagComprehensiveValueTypes.sarif")]
        public void StjRoundTrip_IsSelfConsistent(string testFileName)
        {
            string input = ReadEmbeddedInput(testFileName);

            SarifLog log = LoadStj(input);
            string serialized = SarifJson.Serialize(log, prettyPrint: true);
            SarifLog reloaded = LoadStj(serialized);
            string reserialized = SarifJson.Serialize(reloaded, prettyPrint: true);

            // STJ → STJ round-trip must be a fixed point.
            serialized.Should().Be(reserialized, "STJ serialization must be idempotent");
        }

        [Theory]
        [InlineData("InvocationOverridesRuleDefaultConfigurationOfError.sarif")]
        [InlineData("PropertyBagComprehensiveValueTypes.sarif")]
        public void StjOutput_IsSemanticallyEquivalentToNewtonsoftOutput(string testFileName)
        {
            string input = ReadEmbeddedInput(testFileName);

            // The two serializers, fed the same input text, must agree on the
            // semantic JSON they emit. This is the cross-serializer parity bar
            // that makes a Newtonsoft → STJ swap invisible to consumers.
            string stj = SarifJson.Serialize(LoadStj(input), prettyPrint: true);
            string newtonsoft = JsonConvert.SerializeObject(
                JsonConvert.DeserializeObject<SarifLog>(input),
                Formatting.Indented);

            AssertSemanticEqual(newtonsoft, stj, testFileName);
        }

        [Fact]
        public void StjPropertyBag_RoundTripsAllValueTypes()
        {
            // The property-bag case #3038 flags as "harder". This is the same
            // assertion set as RoundTrippingTests.SarifLog_PropertyBagProperties_*
            // but driven through SarifJson.Load.
            string input = ReadEmbeddedInput("PropertyBagComprehensiveValueTypes.sarif");
            SarifLog log = LoadStj(input);

            PropertyBagHolder holder = log.Runs[0].Results[0];

            holder.GetProperty("string").Should().Be("Hello, string!");
            holder.GetProperty<int>("int").Should().Be(42);
            holder.GetProperty<long>("long").Should().Be(5000000000);
            holder.GetProperty<double>("double").Should().Be(3.14159265);
            holder.GetProperty<bool>("true").Should().BeTrue();
            holder.GetProperty<bool>("false").Should().BeFalse();
            holder.GetProperty("null").Should().BeNull();
            holder.GetProperty<string[]>("stringArray").Should().Equal("Thing One", "Thing Two");
            holder.GetProperty<int[]>("intArray").Should().Equal(54, -54);
            holder.GetProperty<bool[]>("boolArray").Should().Equal(true, false);
        }

        // -------------------------------------------------------------------
        // Corpus sweep: every SARIF v2 fixture in this test assembly.
        // -------------------------------------------------------------------

        public static IEnumerable<object[]> CorpusResources()
        {
            Assembly asm = typeof(SarifJsonRoundTripTests).Assembly;
            return asm.GetManifestResourceNames()
                .Where(n => n.EndsWith(".sarif", StringComparison.OrdinalIgnoreCase))
                // VersionOne fixtures use the v1 model; out of scope for the eager-v2 spike.
                .Where(n => !n.Contains("VersionOne", StringComparison.OrdinalIgnoreCase))
                .Where(n => !n.Contains(".v1.", StringComparison.OrdinalIgnoreCase))
                .Select(n => new object[] { n });
        }

        [Theory]
        [MemberData(nameof(CorpusResources))]
        public void StjCorpusSweep_LoadsAndIsSelfConsistent(string resourceName)
        {
            string input = ReadEmbeddedResource(resourceName);

            // Some fixtures are intentionally malformed (used by negative-path
            // validator tests). If Newtonsoft can't load it either, it's not a
            // parity signal.
            SarifLog newtonsoftLog;
            try { newtonsoftLog = JsonConvert.DeserializeObject<SarifLog>(input); }
            catch { _output.WriteLine($"skip (Newtonsoft also rejects): {resourceName}"); return; }
            if (newtonsoftLog?.Runs == null) { _output.WriteLine($"skip (not a v2 log): {resourceName}"); return; }

            SarifLog stjLog;
            try { stjLog = LoadStj(input); }
            catch (Exception ex)
            {
                Assert.Fail($"STJ failed to load '{resourceName}' that Newtonsoft accepts: {ex.GetType().Name}: {ex.Message}");
                return;
            }

            string round1 = SarifJson.Serialize(stjLog, prettyPrint: false);
            string round2 = SarifJson.Serialize(LoadStj(round1), prettyPrint: false);
            round1.Should().Be(round2, $"STJ round-trip of '{resourceName}' must be idempotent");
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        private static SarifLog LoadStj(string text)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return SarifJson.Load(ms);
        }

        private static void AssertSemanticEqual(string expected, string actual, string label)
        {
            JToken e = JToken.Parse(expected);
            JToken a = JToken.Parse(actual);
            if (!JToken.DeepEquals(e, a))
            {
                // Surface the first divergent path for an actionable failure message.
                string diff = FirstDiff(e, a, "$");
                Assert.Fail($"Semantic diff in '{label}': {diff}");
            }
        }

        private static string FirstDiff(JToken e, JToken a, string path)
        {
            if (JToken.DeepEquals(e, a)) { return null; }
            if (e.Type != a.Type) { return $"{path}: type {e.Type} vs {a.Type}"; }

            if (e is JObject eo && a is JObject ao)
            {
                foreach (string key in new HashSet<string>(eo.Properties().Select(p => p.Name).Concat(ao.Properties().Select(p => p.Name))))
                {
                    bool eHas = eo.TryGetValue(key, out JToken ev);
                    bool aHas = ao.TryGetValue(key, out JToken av);
                    if (eHas != aHas) { return $"{path}.{key}: {(eHas ? "missing in STJ" : "extra in STJ")}"; }
                    string d = FirstDiff(ev, av, $"{path}.{key}");
                    if (d != null) { return d; }
                }
            }
            else if (e is JArray ea && a is JArray aa)
            {
                if (ea.Count != aa.Count) { return $"{path}: array length {ea.Count} vs {aa.Count}"; }
                for (int i = 0; i < ea.Count; i++)
                {
                    string d = FirstDiff(ea[i], aa[i], $"{path}[{i}]");
                    if (d != null) { return d; }
                }
            }
            else
            {
                return $"{path}: '{e}' vs '{a}'";
            }

            return $"{path}: (unlocated)";
        }

        private static string ReadEmbeddedInput(string testFileName)
            => ReadEmbeddedResource(
                $"Test.UnitTests.Sarif.TestData.RoundTripping.Inputs.{testFileName}");

        private static string ReadEmbeddedResource(string resourceName)
        {
            Assembly asm = typeof(SarifJsonRoundTripTests).Assembly;
            using Stream s = asm.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");
            using var reader = new StreamReader(s);
            return reader.ReadToEnd();
        }
    }
}

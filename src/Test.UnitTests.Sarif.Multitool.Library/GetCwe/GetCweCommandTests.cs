// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class GetCweCommandTests : IDisposable
    {
        private readonly string _dir;

        public GetCweCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"get-cwe-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        private static (int exit, string stdout) RunCapturingStdout(GetCweOptions options)
        {
            TextWriter original = Console.Out;
            try
            {
                using var writer = new StringWriter();
                Console.SetOut(writer);
                int exit = new GetCweCommand().Run(options);
                return (exit, writer.ToString());
            }
            finally
            {
                Console.SetOut(original);
            }
        }

        private static JArray RunJson(string ids)
        {
            (int exit, string stdout) = RunCapturingStdout(new GetCweOptions { Ids = ids });
            exit.Should().Be(CommandBase.SUCCESS);
            return JArray.Parse(stdout);
        }

        private static JArray RunJsonConcise(GetCweOptions options)
        {
            options.Concise = true;
            (int exit, string stdout) = RunCapturingStdout(options);
            exit.Should().Be(CommandBase.SUCCESS);
            return JArray.Parse(stdout);
        }

        [Fact]
        public void GetCwe_NamedIds_EmitRecordsInFirstRequestedOrder()
        {
            JArray records = RunJson("89,79");

            records.Select(r => (string)r["id"]).Should().ContainInOrder("CWE-89", "CWE-79");
            records.Count.Should().Be(2);
        }

        [Fact]
        public void GetCwe_Cwe89_HasTheExpectedShape()
        {
            JObject record = RunJson("89").Single() as JObject;

            ((string)record["id"]).Should().Be("CWE-89");
            ((string)record["name"]).Should().Be("SqlInjection");
            ((string)record["slug"]).Should().Be("sql-injection");
            ((string)record["ruleIdFallback"]).Should().Be("CWE-89/sql-injection");
            ((string)record["status"]).Should().Be("Stable");
            ((string)record["abstraction"]).Should().Be("Base");
            ((string)record["parent"]).Should().Be("CWE-943");
            ((string)record["helpUri"]).Should().Be("https://cwe.mitre.org/data/definitions/89.html");
            ((string)record["title"]).Should().NotBeNullOrWhiteSpace();
            ((string)record["shortDescription"]).Should().NotBeNullOrWhiteSpace();
            ((string)record["help"]).Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GetCwe_Concise_OmitsHelpButKeepsHighLevelFields()
        {
            JObject record = RunJsonConcise(new GetCweOptions { Ids = "89" }).Single() as JObject;

            record["help"].Should().BeNull("--concise drops the full MITRE help markdown");
            ((string)record["id"]).Should().Be("CWE-89");
            ((string)record["name"]).Should().Be("SqlInjection");
            ((string)record["ruleIdFallback"]).Should().Be("CWE-89/sql-injection");
            ((string)record["status"]).Should().Be("Stable");
            ((string)record["helpUri"]).Should().Be("https://cwe.mitre.org/data/definitions/89.html");
            ((string)record["shortDescription"]).Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GetCwe_AllConcise_OmitsHelpForEveryRecord()
        {
            JArray records = RunJsonConcise(new GetCweOptions { All = true });

            records.Count.Should().BeGreaterThan(900);
            records.Any(r => r["help"] != null)
                .Should().BeFalse("--all --concise must omit the help field from every served record");
        }

        [Fact]
        public void GetCwe_ConciseMarkdown_OmitsHelpBodyButKeepsHeader()
        {
            (int exit, string stdout) = RunCapturingStdout(new GetCweOptions
            {
                Ids = "89",
                Concise = true,
                Format = CweOutputFormat.Markdown,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            stdout.Should().Contain("## CWE-89 — SqlInjection");
            stdout.Should().Contain("`CWE-89/sql-injection`");
        }

        [Fact]
        public void GetCwe_RuleIdFallback_AgreesWithAI1012ForEveryServedRecord()
        {
            (int exit, string stdout) = RunCapturingStdout(new GetCweOptions { All = true });
            exit.Should().Be(CommandBase.SUCCESS);

            JArray records = JArray.Parse(stdout);
            records.Count.Should().BeGreaterThan(900);

            var offenders = records
                .Select(r => new
                {
                    Id = (string)r["id"],
                    Name = (string)r["name"],
                    Slug = (string)r["slug"],
                    Fallback = (string)r["ruleIdFallback"],
                })
                .Where(r =>
                    string.IsNullOrEmpty(r.Id) ||
                    string.IsNullOrEmpty(r.Name) ||
                    string.IsNullOrEmpty(r.Slug) ||
                    r.Slug != ProvideRuleSubId.ToKebabCase(r.Name) ||
                    r.Fallback != r.Id + "/" + r.Slug)
                .Select(r => r.Id)
                .ToList();

            offenders.Should().BeEmpty(
                "every served record's slug and ruleIdFallback must match what AI1012 would derive " +
                "from the canonical CWE name: " + string.Join(", ", offenders));
        }

        [Fact]
        public void GetCwe_EveryServedRecord_CarriesACweMitreHelpUri()
        {
            (int exit, string stdout) = RunCapturingStdout(new GetCweOptions { All = true });
            exit.Should().Be(CommandBase.SUCCESS);

            var offenders = JArray.Parse(stdout)
                .Where(r =>
                {
                    string id = (string)r["id"];
                    string number = id.StartsWith("CWE-", StringComparison.Ordinal) ? id.Substring(4) : id;
                    string expected = $"https://cwe.mitre.org/data/definitions/{number}.html";
                    return (string)r["helpUri"] != expected;
                })
                .Select(r => (string)r["id"])
                .ToList();

            offenders.Should().BeEmpty(
                "the embedded taxonomy is expected to carry a MITRE help URI for every entry: " +
                string.Join(", ", offenders));
        }

        [Fact]
        public void GetCwe_LenientIdForms_NormalizeToTheSameRecord()
        {
            JArray records = RunJson("89, CWE-89, cwe-89, CWE-089");

            records.Count.Should().Be(1);
            ((string)records.Single()["id"]).Should().Be("CWE-89");
        }

        [Fact]
        public void GetCwe_DuplicateIds_CollapseToOneRecordPreservingOrder()
        {
            JArray records = RunJson("89,79,89");

            records.Select(r => (string)r["id"]).Should().ContainInOrder("CWE-89", "CWE-79");
            records.Count.Should().Be(2);
        }

        [Fact]
        public void GetCwe_DeprecatedCwe_ResolvesByExplicitId()
        {
            JObject record = RunJson("71").Single() as JObject;

            ((string)record["id"]).Should().Be("CWE-71");
            ((string)record["status"]).Should().Be("Deprecated");
        }

        [Fact]
        public void GetCwe_All_IncludesDeprecatedEntries()
        {
            (int exit, string stdout) = RunCapturingStdout(new GetCweOptions { All = true });
            exit.Should().Be(CommandBase.SUCCESS);

            JArray.Parse(stdout)
                .Any(r => (string)r["status"] == "Deprecated")
                .Should().BeTrue("--all serves the whole catalog, deprecated entries included");
        }

        [Fact]
        public void GetCwe_MalformedId_FailsAndEmitsNoRecords()
        {
            (int exit, string stdout) = RunCapturingStdout(new GetCweOptions { Ids = "banana" });

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().BeEmpty();
        }

        [Fact]
        public void GetCwe_EmptyToken_Fails()
        {
            (int exit, _) = RunCapturingStdout(new GetCweOptions { Ids = "89," });
            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void GetCwe_WellFormedButUnknownId_Fails()
        {
            (int exit, string stdout) = RunCapturingStdout(new GetCweOptions { Ids = "99999999" });

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().BeEmpty();
        }

        [Fact]
        public void GetCwe_NoIdsAndNoAll_Fails()
        {
            (int exit, _) = RunCapturingStdout(new GetCweOptions());
            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void GetCwe_IdsAndAllTogether_Fails()
        {
            (int exit, _) = RunCapturingStdout(new GetCweOptions { Ids = "89", All = true });
            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void GetCwe_MarkdownFormat_RendersASectionPerRecord()
        {
            (int exit, string stdout) = RunCapturingStdout(new GetCweOptions
            {
                Ids = "89",
                Format = CweOutputFormat.Markdown,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            stdout.Should().Contain("## CWE-89 — SqlInjection");
            stdout.Should().Contain("`CWE-89/sql-injection`");
        }

        [Fact]
        public void GetCwe_OutputExistsWithoutForce_FailsAndLeavesFileIntact()
        {
            string outPath = Path.Combine(_dir, "exists.json");
            File.WriteAllText(outPath, "occupied");

            int exit = new GetCweCommand().Run(new GetCweOptions { Ids = "89", OutputFilePath = outPath });

            exit.Should().Be(CommandBase.FAILURE);
            File.ReadAllText(outPath).Should().Be("occupied");
        }

        [Fact]
        public void GetCwe_Output_WritesParseableJsonToFile()
        {
            string outPath = Path.Combine(_dir, "cwe.json");

            int exit = new GetCweCommand().Run(new GetCweOptions { Ids = "79,89", OutputFilePath = outPath });

            exit.Should().Be(CommandBase.SUCCESS);
            JArray written = JArray.Parse(File.ReadAllText(outPath));
            written.Select(r => (string)r["id"]).Should().ContainInOrder("CWE-79", "CWE-89");
        }

        [Fact]
        public void GetCwe_TryNormalizeCweId_AcceptsLenientFormsAndRejectsJunk()
        {
            GetCweCommand.TryNormalizeCweId("89", out string a).Should().BeTrue();
            a.Should().Be("CWE-89");

            GetCweCommand.TryNormalizeCweId("CWE-089", out string b).Should().BeTrue();
            b.Should().Be("CWE-89");

            GetCweCommand.TryNormalizeCweId("cwe-79", out string c).Should().BeTrue();
            c.Should().Be("CWE-79");

            GetCweCommand.TryNormalizeCweId("", out _).Should().BeFalse();
            GetCweCommand.TryNormalizeCweId("CWE-", out _).Should().BeFalse();
            GetCweCommand.TryNormalizeCweId("banana", out _).Should().BeFalse();
            GetCweCommand.TryNormalizeCweId("8a9", out _).Should().BeFalse();
        }
    }
}

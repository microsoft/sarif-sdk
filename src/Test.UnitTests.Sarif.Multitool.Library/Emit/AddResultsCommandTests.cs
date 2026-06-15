// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class AddResultsCommandTests : IDisposable
    {
        private readonly string _dir;
        private readonly TextWriter _origStdOut;
        private readonly TextWriter _origStdErr;
        private readonly TextReader _origStdIn;

        public AddResultsCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"add-results-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);

            _origStdOut = Console.Out;
            _origStdErr = Console.Error;
            _origStdIn = Console.In;
        }

        public void Dispose()
        {
            Console.SetOut(_origStdOut);
            Console.SetError(_origStdErr);
            Console.SetIn(_origStdIn);

            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        private string OutPath => Path.Combine(_dir, "scan.sarif");
        private string WipPath => OutPath + ".wip.jsonl";
        private string InputPath => Path.Combine(_dir, "result.json");

        private void SeedRunHeader()
        {
            using var w = new SarifEventLogWriter(WipPath);
            w.Append(SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } });
        }

        // Runs add-results against the supplied payload, capturing the structured report (stdout)
        // and any pre-flight diagnostic (stderr). The batch report carries per-element rejections;
        // pre-flight failures (missing wip/input, empty/malformed JSON, top-level scalar) go to
        // stderr.
        private (int exit, string stdout, string stderr) RunAddResults(string payloadJson)
        {
            File.WriteAllText(InputPath, payloadJson);

            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            int exit = new AddResultsCommand().Run(new AddResultsOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            Console.SetOut(_origStdOut);
            Console.SetError(_origStdErr);

            return (exit, outWriter.ToString(), errWriter.ToString());
        }

        [Fact]
        public void Run_SingleObject_AppendsResult()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddResults(
                "{ \"ruleId\": \"CWE-79/template-xss\", \"message\": { \"text\": \"xss\" }, \"level\": \"error\" }");

            exit.Should().Be(CommandBase.SUCCESS);
            stdout.Should().Contain("\"appended\": 1");

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(2, "run-header + appended result");
            lines[1].Should().Contain("\"kind\":\"result\"");
            lines[1].Should().Contain("CWE-79/template-xss");
        }

        [Fact]
        public void Run_Batch_AppendsAllResultsInOrder()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddResults(
                "[ { \"ruleId\": \"CWE-79/template-xss\", \"message\": { \"text\": \"a\" } }, " +
                "{ \"ruleId\": \"NOVEL-prompt-injection\", \"message\": { \"text\": \"b\" } } ]");

            exit.Should().Be(CommandBase.SUCCESS);
            stdout.Should().Contain("\"appended\": 2");

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(3, "run-header + two appended results");
            lines[1].Should().Contain("CWE-79/template-xss");
            lines[2].Should().Contain("NOVEL-prompt-injection");
            lines.Skip(1).Should().OnlyContain(l => l.Contains("\"kind\":\"result\""));
        }

        [Fact]
        public void Run_Batch_AtomicReject_AppendsNothingAndReportsIndex()
        {
            // A single invalid element must abort the whole batch: nothing is appended, and the
            // report cites the offending index so the producer can correct and retry idempotently.
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddResults(
                "[ { \"ruleId\": \"CWE-79/template-xss\", \"message\": { \"text\": \"a\" } }, " +
                "{ \"ruleId\": \"CWE-79\", \"message\": { \"text\": \"b\" } } ]");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("\"appended\": 0");
            stdout.Should().Contain("\"index\": 1");
            stdout.Should().Contain(AIRuleIdConventionException.ErrorCode);

            File.ReadAllLines(WipPath).Should().HaveCount(1, "no element of a rejected batch may be appended");
        }

        [Fact]
        public void Run_Batch_NonObjectElement_RejectedByIndex()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddResults(
                "[ { \"ruleId\": \"CWE-79/template-xss\", \"message\": { \"text\": \"a\" } }, 5 ]");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("\"index\": 1");
            stdout.Should().Contain("must be a JSON object");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_PreservesDateLikeStringInProperties()
        {
            // The emit verbs read with DateParseHandling.None so producer-supplied timestamp
            // strings inside an arbitrary 'properties' bag round-trip verbatim.
            SeedRunHeader();
            const string OriginalTimestamp = "2026-02-14T08:30:00.1234567+00:00";

            (int exit, _, _) = RunAddResults(
                "{ \"ruleId\": \"CWE-79/x\", \"message\": { \"text\": \"x\" }, " +
                $"\"properties\": {{ \"observed\": \"{OriginalTimestamp}\" }} }}");

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllLines(WipPath).Last().Should().Contain(OriginalTimestamp,
                "the producer's timestamp must round-trip verbatim");
        }

        [Fact]
        public void Run_RejectsNonStringRuleId_WithSpecificDiagnostic()
        {
            // ruleId expressed as a JSON number (a typical AI generation mistake). The diagnostic
            // must name the actual problem rather than the generic "(empty ruleId)".
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddResults("{ \"ruleId\": 79, \"message\": { \"text\": \"x\" } }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain(AIRuleIdConventionException.ErrorCode);
            stdout.Should().Contain("must be a JSON string");
            stdout.Should().Contain("integer");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_RejectsBareTaxonomyRuleId_DoesNotAppend()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddResults("{ \"ruleId\": \"CWE-79\", \"message\": { \"text\": \"x\" } }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain(AIRuleIdConventionException.ErrorCode);
            stdout.Should().Contain("'CWE-79'");
            File.ReadAllLines(WipPath).Should().HaveCount(1, "rejected result must not pollute the event log");
        }

        [Fact]
        public void Run_RejectsNovelWithSlash_DoesNotAppend()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddResults("{ \"ruleId\": \"NOVEL-foo/bar\", \"message\": { \"text\": \"x\" } }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain(AIRuleIdConventionException.ErrorCode);
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenWipMissing()
        {
            // Do NOT seed the run header.
            (int exit, _, string stderr) = RunAddResults("{ \"ruleId\": \"CWE-79/x\", \"message\": { \"text\": \"x\" } }");

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("emit-run");
        }

        [Fact]
        public void Run_FailsWhenInputFileMissing()
        {
            SeedRunHeader();

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddResultsCommand().Run(new AddResultsOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = Path.Combine(_dir, "does-not-exist.json"),
            });

            Console.SetError(_origStdErr);

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("does not exist");
        }

        [Fact]
        public void Run_FailsOnMalformedJson()
        {
            SeedRunHeader();

            (int exit, _, string stderr) = RunAddResults("{ this is not json");

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("malformed");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsOnTopLevelScalar()
        {
            // A bare scalar is neither a single result object nor a batch array.
            SeedRunHeader();

            (int exit, _, string stderr) = RunAddResults("42");

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("object or an array");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_PreservesRichResultStructure()
        {
            // Nested SARIF result structure (codeFlows, properties, fixes, etc.) must survive the
            // JToken round-trip into the event log.
            SeedRunHeader();
            const string richJson = @"{
              ""ruleId"": ""CWE-89/string-concat"",
              ""message"": { ""text"": ""SQL injection"", ""markdown"": ""## SQL injection"" },
              ""level"": ""error"",
              ""rank"": 90.0,
              ""locations"": [
                {
                  ""physicalLocation"": {
                    ""artifactLocation"": { ""uri"": ""src/handler.cs"" },
                    ""region"": { ""startLine"": 42, ""startColumn"": 5 }
                  }
                }
              ],
              ""codeFlows"": [
                {
                  ""threadFlows"": [
                    {
                      ""locations"": [
                        { ""location"": { ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/handler.cs"" } } } }
                      ]
                    }
                  ]
                }
              ],
              ""properties"": {
                ""ai/exploitability"": ""demonstrated"",
                ""ai/attacker-position"": ""network""
              }
            }";

            (int exit, _, _) = RunAddResults(richJson);

            exit.Should().Be(CommandBase.SUCCESS);
            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain("codeFlows");
            appended.Should().Contain("threadFlows");
            appended.Should().Contain("ai/exploitability");
            appended.Should().Contain("\"rank\":90");
        }
    }
}

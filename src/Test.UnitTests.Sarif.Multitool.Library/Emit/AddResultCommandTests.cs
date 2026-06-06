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
    public class AddResultCommandTests : IDisposable
    {
        private readonly string _dir;
        private readonly TextWriter _origStdOut;
        private readonly TextWriter _origStdErr;
        private readonly TextReader _origStdIn;

        public AddResultCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"add-result-{Guid.NewGuid():N}");
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

        [Fact]
        public void Run_HappyPath_AppendsResultFromInputFile()
        {
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"ruleId\": \"CWE-79/template-xss\", \"message\": { \"text\": \"xss\" }, \"level\": \"error\" }");

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(2, "run-header + appended result");
            lines[1].Should().Contain("\"kind\":\"result\"");
            lines[1].Should().Contain("CWE-79/template-xss");
        }

        [Fact]
        public void Run_PrefersInputFileWhenSupplied()
        {
            // When --input is supplied, the verb reads from the file and ignores any stdin
            // pipe. (The stdin-fallback path of the verb is exercised end-to-end by the
            // sample-generation smoke script; xUnit doesn't redirect the OS-level stdin
            // handle even when Console.SetIn is called, so we cannot exercise that path
            // from a unit test.)
            SeedRunHeader();
            string json = "{ \"ruleId\": \"NOVEL-prompt-injection\", \"message\": { \"text\": \"x\" } }";
            File.WriteAllText(InputPath, json);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllText(WipPath).Should().Contain("NOVEL-prompt-injection");
        }

        [Fact]
        public void Run_PreservesDateLikeStringInProperties()
        {
            // Json.NET's JsonTextReader defaults to DateParseHandling.DateTime, which would
            // silently rewrite ISO-8601 strings inside an arbitrary 'properties' bag into
            // System.DateTime instances and re-emit them with a normalized format on the
            // way back out. The emit verbs use DateParseHandling.None so producer-supplied
            // timestamp strings round-trip verbatim. Pin that behavior.
            SeedRunHeader();
            const string OriginalTimestamp = "2026-02-14T08:30:00.1234567+00:00";
            string json = "{ \"ruleId\": \"CWE-79/x\", \"message\": { \"text\": \"x\" }, " +
                $"\"properties\": {{ \"observed\": \"{OriginalTimestamp}\" }} }}";
            File.WriteAllText(InputPath, json);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain(OriginalTimestamp,
                "the producer's timestamp must round-trip verbatim — Json.NET must not coerce it to a normalized DateTime form");
        }

        [Fact]
        public void Run_RejectsNonStringRuleId_WithSpecificDiagnostic()
        {
            SeedRunHeader();
            // ruleId expressed as a JSON number (a typical AI generation mistake — emitting
            // the descriptor index rather than the id string). We want the diagnostic to
            // name the actual problem ("must be a string, got integer") rather than the
            // generic "(empty ruleId)" produced when ruleId is missing or empty.
            File.WriteAllText(InputPath, "{ \"ruleId\": 79, \"message\": { \"text\": \"x\" } }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            string err = errWriter.ToString();
            err.Should().Contain(AIRuleIdConventionException.ErrorCode);
            err.Should().Contain("must be a JSON string");
            err.Should().Contain("integer");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_RejectsBareTaxonomyRuleId_DoesNotAppend()
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, "{ \"ruleId\": \"CWE-79\", \"message\": { \"text\": \"x\" } }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain(AIRuleIdConventionException.ErrorCode);
            errWriter.ToString().Should().Contain("'CWE-79'");

            // Critical: the event log must NOT have the rejected result appended.
            File.ReadAllLines(WipPath).Should().HaveCount(1, "rejected result must not pollute the event log");
        }

        [Fact]
        public void Run_RejectsNovelWithSlash_DoesNotAppend()
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, "{ \"ruleId\": \"NOVEL-foo/bar\", \"message\": { \"text\": \"x\" } }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain(AIRuleIdConventionException.ErrorCode);
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenWipMissing()
        {
            // Do NOT seed the run header.
            File.WriteAllText(InputPath, "{ \"ruleId\": \"CWE-79/x\", \"message\": { \"text\": \"x\" } }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("emit-run");
        }

        [Fact]
        public void Run_FailsWhenInputFileMissing()
        {
            SeedRunHeader();

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = Path.Combine(_dir, "does-not-exist.json"),
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("does not exist");
        }

        [Fact]
        public void Run_FailsOnMalformedJson()
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, "{ this is not json");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("malformed");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsOnNonObjectJson()
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, "[ 1, 2, 3 ]");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("object");
        }

        [Fact]
        public void Run_PreservesRichResultStructure()
        {
            // Verify that nested SARIF result structure (codeFlows, properties, fixes, etc.)
            // survives the JToken round-trip into the event log.
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
            File.WriteAllText(InputPath, richJson);

            int exit = new AddResultCommand().Run(new AddResultOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain("codeFlows");
            appended.Should().Contain("threadFlows");
            appended.Should().Contain("ai/exploitability");
            appended.Should().Contain("\"rank\":90");
        }
    }
}

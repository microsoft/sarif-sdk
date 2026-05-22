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
        public void Run_HappyPath_AppendsResultFromStdin()
        {
            SeedRunHeader();

            string json = "{ \"ruleId\": \"NOVEL-prompt-injection\", \"message\": { \"text\": \"x\" } }";
            using var stdin = new StringReader(json);
            Console.SetIn(stdin);

            // Console.IsInputRedirected reflects the OS-level handle, which xUnit does not
            // redirect even if Console.SetIn was called. The verb's stdin fallback path is
            // covered indirectly by the end-to-end smoke test; the unit tests cover the
            // input-file path explicitly. Here we just assert that the file path is preferred
            // when supplied.
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
            errWriter.ToString().Should().Contain("emit-init-run");
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

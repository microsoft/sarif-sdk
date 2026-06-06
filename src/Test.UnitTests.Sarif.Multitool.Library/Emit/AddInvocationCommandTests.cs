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
    public class AddInvocationCommandTests : IDisposable
    {
        private readonly string _dir;
        private readonly TextWriter _origStdOut;
        private readonly TextWriter _origStdErr;

        public AddInvocationCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"add-invoc-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);

            _origStdOut = Console.Out;
            _origStdErr = Console.Error;
        }

        public void Dispose()
        {
            Console.SetOut(_origStdOut);
            Console.SetError(_origStdErr);

            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        private string OutPath => Path.Combine(_dir, "scan.sarif");
        private string WipPath => OutPath + ".wip.jsonl";
        private string InputPath => Path.Combine(_dir, "invoc.json");

        private void SeedRunHeader()
        {
            using var w = new SarifEventLogWriter(WipPath);
            w.Append(SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } });
        }

        [Fact]
        public void Run_HappyPath_AppendsInvocationFromFile()
        {
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uri\": \"file:///work/\" } }");

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(2);
            lines[1].Should().Contain("\"kind\":\"invocation\"");
            lines[1].Should().Contain("demo --scan src");
        }

        [Fact]
        public void Run_FailsWhenExecutionSuccessfulMissing()
        {
            // The AI invocation profile (ai-invocation.schema.json) requires a boolean
            // executionSuccessful; the verb's receipt gate rejects a payload that omits it.
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uri\": \"file:///work/\" } }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("executionSuccessful");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenExecutionSuccessfulNotBoolean()
        {
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"executionSuccessful\": \"true\", \"commandLine\": \"demo --scan src\" }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("executionSuccessful");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenCommandLineMissing()
        {
            // The AI invocation profile requires a non-whitespace commandLine (the natural
            // identity of a launched process); the receipt gate rejects a payload without it.
            SeedRunHeader();
            File.WriteAllText(InputPath, "{ \"executionSuccessful\": true }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("commandLine");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenCommandLineIsWhitespace()
        {
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"executionSuccessful\": true, \"commandLine\": \"   \" }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("commandLine");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenWorkingDirectoryMissing()
        {
            // The AI invocation profile requires a workingDirectory artifactLocation (it anchors
            // the relative paths in the scan); the verb's receipt gate rejects a payload omitting it.
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\" }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("workingDirectory");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenWorkingDirectoryUriIsWhitespace()
        {
            // A workingDirectory whose uri is empty/whitespace carries no anchor, so the receipt
            // gate rejects it just as it rejects a whitespace commandLine.
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uri\": \"   \" } }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("workingDirectory");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_StampsEndTimeUtcWhenOmitted()
        {
            // The verb fills endTimeUtc at emit time when the producer left it unset, so the
            // finalized invocation always carries a completion timestamp.
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uri\": \"file:///work/\" } }");

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain("\"endTimeUtc\"");
        }

        [Fact]
        public void Run_RejectsInlineNotificationMissingTimeUtc()
        {
            // Notifications travel inline on the invocation, and each records when an event
            // occurred mid-flight. The producer owns that timeUtc (the verb never stamps it), so a
            // notification lacking one is rejected by the receipt gate.
            SeedRunHeader();
            const string json = @"{
              ""executionSuccessful"": true,
              ""commandLine"": ""demo --scan src"",
              ""workingDirectory"": { ""uri"": ""file:///work/"" },
              ""toolExecutionNotifications"": [
                { ""message"": { ""text"": ""no time"" } }
              ]
            }";
            File.WriteAllText(InputPath, json);

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("timeUtc");
            // The WIP holds only the run header; no invocation event was appended.
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_RejectsInlineConfigNotificationMissingTimeUtc()
        {
            SeedRunHeader();
            const string json = @"{
              ""executionSuccessful"": true,
              ""commandLine"": ""demo --scan src"",
              ""workingDirectory"": { ""uri"": ""file:///work/"" },
              ""toolConfigurationNotifications"": [
                { ""message"": { ""text"": ""config no time"" } }
              ]
            }";
            File.WriteAllText(InputPath, json);

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("timeUtc");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_PreservesSuppliedNotificationTimeUtc()
        {
            // When every inline notification supplies its own timeUtc the gate passes and the
            // producer-supplied values are preserved verbatim (the verb never rewrites them).
            SeedRunHeader();
            const string json = @"{
              ""executionSuccessful"": true,
              ""commandLine"": ""demo --scan src"",
              ""workingDirectory"": { ""uri"": ""file:///work/"" },
              ""toolExecutionNotifications"": [
                { ""message"": { ""text"": ""has time"" }, ""timeUtc"": ""2026-05-26T10:05:42.000Z"" }
              ],
              ""toolConfigurationNotifications"": [
                { ""message"": { ""text"": ""config has time"" }, ""timeUtc"": ""2026-05-26T10:05:43.000Z"" }
              ]
            }";
            File.WriteAllText(InputPath, json);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);

            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain("2026-05-26T10:05:42.000Z");
            appended.Should().Contain("2026-05-26T10:05:43.000Z");

            SarifLog log = SarifEventReplayer.Replay(WipPath);
            Invocation invocation = log.Runs[0].Invocations[0];
            invocation.ToolExecutionNotifications.Should().OnlyContain(n => n.TimeUtc != default(DateTime));
            invocation.ToolConfigurationNotifications.Should().OnlyContain(n => n.TimeUtc != default(DateTime));
        }

        [Fact]
        public void Run_FailsWhenWipMissing()
        {
            File.WriteAllText(InputPath, "{ \"executionSuccessful\": true }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("emit-run");
        }

        [Fact]
        public void Run_FailsOnMalformedJson()
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, "not-json");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("malformed");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenPayloadIsNotAnObject()
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, "[ { \"executionSuccessful\": true } ]");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("JSON object");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_PreservesRichInvocationStructure()
        {
            // AI producers may ship invocations with rich data: start/end timestamps,
            // command-line argument arrays, environment-variable dictionaries, properties bags.
            // Confirm these survive the JToken round-trip into the event log.
            SeedRunHeader();
            const string richJson = @"{
              ""startTimeUtc"": ""2026-05-26T10:00:00Z"",
              ""endTimeUtc"": ""2026-05-26T10:05:42Z"",
              ""executionSuccessful"": true,
              ""exitCode"": 0,
              ""commandLine"": ""demo --scan src --rule-kind AI"",
              ""arguments"": [ ""--scan"", ""src"", ""--rule-kind"", ""AI"" ],
              ""workingDirectory"": { ""uri"": ""file:///work/repo/"" },
              ""environmentVariables"": {
                ""TF_BUILD"": ""True"",
                ""BUILD_BUILDID"": ""12345""
              },
              ""properties"": {
                ""ai/origin"": ""generated"",
                ""ai/skills"": [ ""prompt-injection"", ""kql-injection"" ]
              }
            }";
            File.WriteAllText(InputPath, richJson);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain("2026-05-26T10:00:00Z");
            appended.Should().Contain("2026-05-26T10:05:42Z");
            appended.Should().Contain("TF_BUILD");
            appended.Should().Contain("ai/skills");
            appended.Should().Contain("prompt-injection");
        }

        [Fact]
        public void Run_AppendsMultipleInvocationsInOrder()
        {
            // The replayer appends invocation events to run.invocations[] in event order
            // (SarifEventReplayer.cs). Confirm the verb can be invoked repeatedly and that
            // each append produces a separate event line.
            SeedRunHeader();

            for (int i = 0; i < 3; i++)
            {
                File.WriteAllText(
                    InputPath,
                    $"{{ \"executionSuccessful\": true, \"commandLine\": \"phase-{i}\", \"workingDirectory\": {{ \"uri\": \"file:///work/\" }} }}");

                int exit = new AddInvocationCommand().Run(new AddInvocationOptions
                {
                    OutputFilePath = OutPath,
                    InputFilePath = InputPath,
                });

                exit.Should().Be(CommandBase.SUCCESS);
            }

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(4); // header + 3 invocations
            lines[1].Should().Contain("phase-0");
            lines[2].Should().Contain("phase-1");
            lines[3].Should().Contain("phase-2");
            lines.Skip(1).Should().OnlyContain(l => l.Contains("\"kind\":\"invocation\""));
        }

        [Fact]
        public void Run_FailsWhenInputFileMissing()
        {
            SeedRunHeader();

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath, // does not exist
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("does not exist");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenPayloadIsEmpty()
        {
            // Pointing --input at an empty file deterministically hits TryReadJsonPayload's
            // "is empty" diagnostic via the file branch. Falling through to the stdin branch
            // (InputFilePath = null) hangs under xUnit on the ADO Linux/Mac agents:
            // Console.IsInputRedirected reports true but the runner's stdin pipe never closes,
            // so Console.OpenStandardInput().ReadToEnd() never returns.
            SeedRunHeader();
            File.WriteAllText(InputPath, string.Empty);

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddInvocationCommand().Run(new AddInvocationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("Invocation");
            errWriter.ToString().Should().Contain("empty");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }
    }
}

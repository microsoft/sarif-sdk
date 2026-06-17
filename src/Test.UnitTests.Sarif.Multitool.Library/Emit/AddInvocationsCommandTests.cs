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
    public class AddInvocationsCommandTests : IDisposable
    {
        private readonly string _dir;
        private readonly TextWriter _origStdOut;
        private readonly TextWriter _origStdErr;

        public AddInvocationsCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"add-invocs-{Guid.NewGuid():N}");
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

        // Runs emit-invocations, capturing the structured report (stdout) and any pre-flight
        // diagnostic (stderr).
        private (int exit, string stdout, string stderr) RunAddInvocations(string payloadJson)
        {
            File.WriteAllText(InputPath, payloadJson);

            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            int exit = new AddInvocationsCommand().Run(new AddInvocationsOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            Console.SetOut(_origStdOut);
            Console.SetError(_origStdErr);

            return (exit, outWriter.ToString(), errWriter.ToString());
        }

        [Fact]
        public void Run_SingleObject_AppendsInvocation()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddInvocations(
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uri\": \"file:///work/\" } }");

            exit.Should().Be(CommandBase.SUCCESS);
            stdout.Should().Contain("\"appended\": 1");

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(2);
            lines[1].Should().Contain("\"kind\":\"invocation\"");
            lines[1].Should().Contain("demo --scan src");
        }

        [Fact]
        public void Run_FailsWhenExecutionSuccessfulMissing()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddInvocations(
                "{ \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uri\": \"file:///work/\" } }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("executionSuccessful");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenExecutionSuccessfulNotBoolean()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddInvocations(
                "{ \"executionSuccessful\": \"true\", \"commandLine\": \"demo --scan src\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("executionSuccessful");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenCommandLineMissing()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddInvocations("{ \"executionSuccessful\": true }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("commandLine");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenCommandLineIsWhitespace()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddInvocations(
                "{ \"executionSuccessful\": true, \"commandLine\": \"   \" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("commandLine");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenWorkingDirectoryMissing()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddInvocations(
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("workingDirectory");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenWorkingDirectoryUriIsWhitespace()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddInvocations(
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uri\": \"   \" } }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("workingDirectory");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_SucceedsWhenWorkingDirectoryAnchoredByUriBaseIdWithEmptyUri()
        {
            // emit-finalize rebases a repo-root workingDirectory to an empty 'uri' under a
            // 'uriBaseId'; that shape must round-trip rather than tripping the receipt gate.
            SeedRunHeader();

            (int exit, _, _) = RunAddInvocations(
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uri\": \"\", \"uriBaseId\": \"SRCROOT\" } }");

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllLines(WipPath).Should().HaveCount(2);
        }

        [Fact]
        public void Run_SucceedsWhenWorkingDirectoryAnchoredByUriBaseIdAlone()
        {
            SeedRunHeader();

            (int exit, _, _) = RunAddInvocations(
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uriBaseId\": \"SRCROOT\" } }");

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllLines(WipPath).Should().HaveCount(2);
        }

        [Fact]
        public void Run_SingleObject_StampsEndTimeUtcWhenOmitted()
        {
            // A lone object's write is roughly coincident with the invocation's conclusion, so the
            // verb fills endTimeUtc at receipt when the producer left it unset.
            SeedRunHeader();

            (int exit, _, _) = RunAddInvocations(
                "{ \"executionSuccessful\": true, \"commandLine\": \"demo --scan src\", \"workingDirectory\": { \"uri\": \"file:///work/\" } }");

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllLines(WipPath).Last().Should().Contain("\"endTimeUtc\"");
        }

        [Fact]
        public void Run_Batch_AppendsAllInvocationsInOrder()
        {
            // Batched invocations carry their own endTimeUtc (the receipt-time default is a
            // single-submission affordance).
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddInvocations(
                "[ { \"executionSuccessful\": true, \"commandLine\": \"phase-0\", \"workingDirectory\": { \"uri\": \"file:///work/\" }, \"endTimeUtc\": \"2026-05-26T10:00:00.000Z\" }, " +
                "{ \"executionSuccessful\": false, \"commandLine\": \"phase-1\", \"workingDirectory\": { \"uri\": \"file:///work/\" }, \"endTimeUtc\": \"2026-05-26T10:01:00.000Z\" } ]");

            exit.Should().Be(CommandBase.SUCCESS);
            stdout.Should().Contain("\"appended\": 2");

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(3);
            lines[1].Should().Contain("phase-0");
            lines[2].Should().Contain("phase-1");
            lines.Skip(1).Should().OnlyContain(l => l.Contains("\"kind\":\"invocation\""));
        }

        [Fact]
        public void Run_Batch_RejectsInvocationMissingEndTimeUtc()
        {
            // The receipt-time endTimeUtc default applies only to single submission; a batched
            // invocation that omits endTimeUtc is rejected by index rather than fabricated.
            SeedRunHeader();

            (int exit, string stdout, _) = RunAddInvocations(
                "[ { \"executionSuccessful\": true, \"commandLine\": \"phase-0\", \"workingDirectory\": { \"uri\": \"file:///work/\" }, \"endTimeUtc\": \"2026-05-26T10:00:00.000Z\" }, " +
                "{ \"executionSuccessful\": true, \"commandLine\": \"phase-1\", \"workingDirectory\": { \"uri\": \"file:///work/\" } } ]");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("\"appended\": 0");
            stdout.Should().Contain("\"index\": 1");
            stdout.Should().Contain("endTimeUtc");
            File.ReadAllLines(WipPath).Should().HaveCount(1, "no element of a rejected batch may be appended");
        }

        [Fact]
        public void Run_RejectsInlineNotificationMissingTimeUtc()
        {
            SeedRunHeader();
            const string json = @"{
              ""executionSuccessful"": true,
              ""commandLine"": ""demo --scan src"",
              ""workingDirectory"": { ""uri"": ""file:///work/"" },
              ""toolExecutionNotifications"": [
                { ""message"": { ""text"": ""no time"" } }
              ]
            }";

            (int exit, string stdout, _) = RunAddInvocations(json);

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("timeUtc");
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

            (int exit, string stdout, _) = RunAddInvocations(json);

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("timeUtc");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_PreservesSuppliedNotificationTimeUtc()
        {
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

            (int exit, _, _) = RunAddInvocations(json);

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
            (int exit, _, string stderr) = RunAddInvocations("{ \"executionSuccessful\": true }");

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("emit-run");
        }

        [Fact]
        public void Run_FailsOnMalformedJson()
        {
            SeedRunHeader();

            (int exit, _, string stderr) = RunAddInvocations("not-json");

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("malformed");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenPayloadIsTopLevelScalar()
        {
            SeedRunHeader();

            (int exit, _, string stderr) = RunAddInvocations("true");

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("object or an array");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_PreservesRichInvocationStructure()
        {
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

            (int exit, _, _) = RunAddInvocations(richJson);

            exit.Should().Be(CommandBase.SUCCESS);
            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain("2026-05-26T10:00:00Z");
            appended.Should().Contain("2026-05-26T10:05:42Z");
            appended.Should().Contain("TF_BUILD");
            appended.Should().Contain("ai/skills");
            appended.Should().Contain("prompt-injection");
        }

        [Fact]
        public void Run_AppendsMultipleInvocationsInOrderAcrossCalls()
        {
            SeedRunHeader();

            for (int i = 0; i < 3; i++)
            {
                (int exit, _, _) = RunAddInvocations(
                    $"{{ \"executionSuccessful\": true, \"commandLine\": \"phase-{i}\", \"workingDirectory\": {{ \"uri\": \"file:///work/\" }} }}");

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

            int exit = new AddInvocationsCommand().Run(new AddInvocationsOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath, // does not exist
            });

            Console.SetError(_origStdErr);

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("does not exist");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenPayloadIsEmpty()
        {
            SeedRunHeader();

            (int exit, _, string stderr) = RunAddInvocations(string.Empty);

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("Invocation");
            stderr.Should().Contain("empty");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }
    }
}

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
    public class AddNotificationCommandTests : IDisposable
    {
        private readonly string _dir;
        private readonly TextWriter _origStdOut;
        private readonly TextWriter _origStdErr;

        public AddNotificationCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"add-notif-{Guid.NewGuid():N}");
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
        private string InputPath => Path.Combine(_dir, "notif.json");

        private void SeedRunHeader()
        {
            using var w = new SarifEventLogWriter(WipPath);
            w.Append(SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } });
        }

        [Fact]
        public void Run_HappyPath_AppendsNotificationFromFile()
        {
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"level\": \"note\", \"message\": { \"text\": \"scan complete\" } }");

            int exit = new AddNotificationCommand().Run(new AddNotificationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(2);
            lines[1].Should().Contain("\"kind\":\"execution-notification\"");
            lines[1].Should().Contain("scan complete");
        }

        [Fact]
        public void Run_FailsWhenWipMissing()
        {
            File.WriteAllText(InputPath, "{ \"message\": { \"text\": \"x\" } }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddNotificationCommand().Run(new AddNotificationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("emit-init-run");
        }

        [Fact]
        public void Run_FailsOnMalformedJson()
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, "not-json");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddNotificationCommand().Run(new AddNotificationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("malformed");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_DoesNotEnforceAIRuleIdConvention()
        {
            // Notifications carry associatedRule.id which references a base descriptor id
            // per SARIF §3.49.3 — NOT the result-side hierarchical form. The AI ruleId
            // convention applies to result.ruleId only; this verb must accept a notification
            // whose associatedRule.id is a bare taxonomy base.
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"level\": \"warning\", \"message\": { \"text\": \"x\" }, " +
                "\"associatedRule\": { \"id\": \"CWE-79\" } }");

            int exit = new AddNotificationCommand().Run(new AddNotificationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);
        }

        [Fact]
        public void Run_PreservesRichNotificationStructure()
        {
            // AI producers are expected to emit notifications with potentially rich data:
            // full exception trees, descriptive markdown, properties bags, etc. Confirm
            // these survive the JToken round-trip into the event log.
            SeedRunHeader();
            const string richJson = @"{
              ""level"": ""error"",
              ""message"": { ""text"": ""Tool aborted"", ""markdown"": ""## Tool aborted\n\nDetails..."" },
              ""exception"": {
                ""kind"": ""System.InvalidOperationException"",
                ""message"": ""widget malfunction"",
                ""stack"": {
                  ""frames"": [
                    { ""location"": { ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/widget.cs"" } } } }
                  ]
                },
                ""innerExceptions"": [
                  { ""kind"": ""System.IO.IOException"", ""message"": ""disk full"" }
                ]
              },
              ""properties"": {
                ""ai/origin"": ""generated"",
                ""custom-trace-id"": ""abc-123""
              }
            }";
            File.WriteAllText(InputPath, richJson);

            int exit = new AddNotificationCommand().Run(new AddNotificationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain("innerExceptions");
            appended.Should().Contain("widget malfunction");
            appended.Should().Contain("ai/origin");
            appended.Should().Contain("custom-trace-id");
        }

        [Fact]
        public void Run_RoutesToConfigurationNotifications_WhenConfigFlagSet()
        {
            // The descriptor id no longer encodes placement; the --config switch does. Producers
            // selecting the configuration target write an event of kind 'configuration-notification'
            // that the replayer routes to toolConfigurationNotifications.
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"level\": \"warning\", \"message\": { \"text\": \"data source unreachable\" } }");

            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);

            int exit = new AddNotificationCommand().Run(new AddNotificationOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
                ConfigurationNotification = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(2);
            lines[1].Should().Contain("\"kind\":\"configuration-notification\"");
            lines[1].Should().Contain("data source unreachable");
            outWriter.ToString().Should().Contain("toolConfigurationNotifications");
        }
    }
}

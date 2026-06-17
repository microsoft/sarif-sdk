// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ReportingDescriptorCommandTests : IDisposable
    {
        private readonly string _dir;
        private readonly TextWriter _origStdOut;
        private readonly TextWriter _origStdErr;
        private readonly TextReader _origStdIn;

        public ReportingDescriptorCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"add-rdesc-{Guid.NewGuid():N}");
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
        private string InputPath => Path.Combine(_dir, "descriptor.json");

        private void SeedRunHeader(Run run = null)
        {
            run ??= new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } };
            using var w = new SarifEventLogWriter(WipPath);
            w.Append(SarifEventKinds.RunHeader, run);
        }

        private (int exit, string stdout, string stderr) RunRule(string payloadJson)
            => RunDescriptor(payloadJson, isRules: true);

        private (int exit, string stdout, string stderr) RunNotification(string payloadJson)
            => RunDescriptor(payloadJson, isRules: false);

        // Runs the descriptor verb, capturing the structured report (stdout) and any pre-flight
        // diagnostic (stderr).
        private (int exit, string stdout, string stderr) RunDescriptor(string payloadJson, bool isRules)
        {
            File.WriteAllText(InputPath, payloadJson);

            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            int exit = isRules
                ? new AddRuleReportingDescriptorsCommand().Run(new AddRuleReportingDescriptorsOptions
                {
                    OutputFilePath = OutPath,
                    InputFilePath = InputPath,
                })
                : new AddNotificationReportingDescriptorsCommand().Run(new AddNotificationReportingDescriptorsOptions
                {
                    OutputFilePath = OutPath,
                    InputFilePath = InputPath,
                });

            Console.SetOut(_origStdOut);
            Console.SetError(_origStdErr);

            return (exit, outWriter.ToString(), errWriter.ToString());
        }

        [Fact]
        public void Run_HappyPath_AppendsNotificationDescriptor()
        {
            SeedRunHeader();

            (int exit, _, _) = RunNotification(
                "{ \"id\": \"progress\", \"name\": \"Progress\", \"shortDescription\": { \"text\": \"Per-batch progress.\" } }");

            exit.Should().Be(CommandBase.SUCCESS);

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(2, "run-header + appended notification descriptor");
            lines[1].Should().Contain("\"kind\":\"notification-descriptor\"");
            lines[1].Should().Contain("\"progress\"");
        }

        [Fact]
        public void Run_HappyPath_AppendsRuleDescriptor()
        {
            SeedRunHeader();

            (int exit, _, _) = RunRule(
                "{ \"id\": \"NOVEL-prompt-injection\", \"name\": \"PromptInjection\", \"helpUri\": \"https://example.com/help\" }");

            exit.Should().Be(CommandBase.SUCCESS);

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(2);
            lines[1].Should().Contain("\"kind\":\"rule-descriptor\"");
            lines[1].Should().Contain("NOVEL-prompt-injection");
            lines[1].Should().Contain("https://example.com/help");
        }

        [Fact]
        public void Run_Batch_AppendsMultipleRuleDescriptors()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunRule(
                "[ { \"id\": \"NOVEL-foo\", \"name\": \"Foo\" }, { \"id\": \"NOVEL-bar\", \"name\": \"Bar\" } ]");

            exit.Should().Be(CommandBase.SUCCESS);
            stdout.Should().Contain("\"appended\": 2");

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(3);
            lines[1].Should().Contain("NOVEL-foo");
            lines[2].Should().Contain("NOVEL-bar");
        }

        [Fact]
        public void Run_Batch_IntraBatchDuplicateId_Rejected()
        {
            // Two elements of the same batch carrying the same id collide just as a cross-log
            // duplicate would; the whole batch is rejected and nothing is appended.
            SeedRunHeader();

            (int exit, string stdout, _) = RunRule(
                "[ { \"id\": \"NOVEL-foo\", \"name\": \"Foo\" }, { \"id\": \"NOVEL-foo\", \"name\": \"FooAgain\" } ]");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("\"appended\": 0");
            stdout.Should().Contain("\"index\": 1");
            stdout.Should().Contain("more than once in this batch");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_Batch_AtomicReject_AppendsNothing()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunRule(
                "[ { \"id\": \"NOVEL-foo\", \"name\": \"Foo\" }, { \"id\": \"CWE-89\", \"name\": \"NotNovel\" } ]");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("\"appended\": 0");
            stdout.Should().Contain("\"index\": 1");
            stdout.Should().Contain(AIRuleIdConventionException.ErrorCode);
            File.ReadAllLines(WipPath).Should().HaveCount(1, "no element of a rejected batch may be appended");
        }

        [Fact]
        public void Run_PreservesRichDescriptorPayloadVerbatim()
        {
            SeedRunHeader();
            const string Rich =
                "{ \"id\": \"NOVEL-system-prompt-leak\", \"name\": \"SystemPromptLeak\", " +
                "\"shortDescription\": { \"text\": \"System prompt leaked into model output.\" }, " +
                "\"fullDescription\": { \"text\": \"long form\", \"markdown\": \"long **form**\" }, " +
                "\"messageStrings\": { \"default\": { \"text\": \"Leak at {0}.\" } }, " +
                "\"defaultConfiguration\": { \"level\": \"error\", \"rank\": 90.0 }, " +
                "\"helpUri\": \"https://example.com/leak\", " +
                "\"properties\": { \"ai/family\": \"prompt\", \"observed\": \"2026-02-14T08:30:00+00:00\" } }";

            (int exit, _, _) = RunRule(Rich);

            exit.Should().Be(CommandBase.SUCCESS);
            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain("System prompt leaked");
            appended.Should().Contain("long **form**");
            appended.Should().Contain("Leak at {0}");
            appended.Should().Contain("\"ai/family\":\"prompt\"");
            appended.Should().Contain("2026-02-14T08:30:00+00:00",
                "date-like properties must round-trip verbatim");
        }

        [Fact]
        public void Run_RulesPath_RejectsTaxonomyIdNotInNovelForm()
        {
            AssertRulesPathRejects("{ \"id\": \"CWE-89\", \"name\": \"SqlInjection\" }", "CWE-89");
        }

        [Fact]
        public void Run_RulesPath_RejectsBareNovelPrefix()
        {
            AssertRulesPathRejects("{ \"id\": \"NOVEL-\", \"name\": \"Bare\" }", "NOVEL-");
        }

        [Fact]
        public void Run_RulesPath_RejectsNovelIdWithSlash()
        {
            AssertRulesPathRejects("{ \"id\": \"NOVEL-foo/bar\", \"name\": \"Slash\" }", "NOVEL-foo/bar");
        }

        [Fact]
        public void Run_RulesPath_RejectsNovelIdWithTrailingHyphen()
        {
            AssertRulesPathRejects("{ \"id\": \"NOVEL-trailing-\", \"name\": \"Trailing\" }", "NOVEL-trailing-");
        }

        [Fact]
        public void Run_RulesPath_RejectsNovelIdWithUppercaseTail()
        {
            AssertRulesPathRejects("{ \"id\": \"NOVEL-PromptInjection\", \"name\": \"Upper\" }", "NOVEL-PromptInjection");
        }

        [Fact]
        public void Run_RulesPath_RejectsNovelIdWithDoubleHyphen()
        {
            AssertRulesPathRejects("{ \"id\": \"NOVEL--double\", \"name\": \"Double\" }", "NOVEL--double");
        }

        private void AssertRulesPathRejects(string descriptorJson, string expectedIdInError)
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunRule(descriptorJson);

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain(AIRuleIdConventionException.ErrorCode);
            stdout.Should().Contain($"'{expectedIdInError}'");
            stdout.Should().Contain("NOVEL-");

            File.ReadAllLines(WipPath).Should().HaveCount(1, "rejected descriptor must not be appended");
        }

        [Fact]
        public void Run_NotificationsPath_AcceptsAnyId()
        {
            SeedRunHeader();

            (int exit, _, _) = RunNotification("{ \"id\": \"config-error\", \"name\": \"ConfigError\" }");

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllLines(WipPath).Should().HaveCount(2);
        }

        [Fact]
        public void Run_RejectsMissingId()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunNotification("{ \"name\": \"NoId\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("'id'");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_RejectsEmptyId()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunNotification("{ \"id\": \"\", \"name\": \"EmptyId\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("non-empty");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_RejectsNonStringId()
        {
            SeedRunHeader();

            (int exit, string stdout, _) = RunNotification("{ \"id\": 42, \"name\": \"NumericId\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("'id'");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_RejectsDuplicateRuleDescriptorId_DoesNotAppend()
        {
            SeedRunHeader();

            RunRule("{ \"id\": \"NOVEL-foo\", \"name\": \"Foo\" }").exit.Should().Be(CommandBase.SUCCESS);

            (int exit, string stdout, _) = RunRule("{ \"id\": \"NOVEL-foo\", \"name\": \"FooAgain\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("'NOVEL-foo'");
            stdout.Should().Contain("already");
            stdout.Should().Contain("rule descriptor");
            File.ReadAllLines(WipPath).Should().HaveCount(2, "only the first descriptor was appended");
        }

        [Fact]
        public void Run_RejectsDuplicateNotificationDescriptorId_DoesNotAppend()
        {
            SeedRunHeader();

            RunNotification("{ \"id\": \"progress\", \"name\": \"Progress\" }").exit.Should().Be(CommandBase.SUCCESS);

            (int exit, string stdout, _) = RunNotification("{ \"id\": \"progress\", \"name\": \"ProgressAgain\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("'progress'");
            stdout.Should().Contain("notification descriptor");
            File.ReadAllLines(WipPath).Should().HaveCount(2);
        }

        [Fact]
        public void Run_SameIdAcrossTargets_BothAccepted()
        {
            // Rules and notifications are separate descriptor lists per SARIF.
            SeedRunHeader();

            RunRule("{ \"id\": \"NOVEL-foo\", \"name\": \"Foo\" }").exit.Should().Be(CommandBase.SUCCESS);

            (int exit, _, _) = RunNotification("{ \"id\": \"NOVEL-foo\", \"name\": \"FooNotification\" }");

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllLines(WipPath).Should().HaveCount(3);
        }

        [Fact]
        public void Run_RejectsDuplicateAgainstHeaderPrePopulatedRules()
        {
            SeedRunHeader(new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = "demo",
                        Rules = new List<ReportingDescriptor>
                        {
                            new() { Id = "NOVEL-baked-in", Name = "BakedIn" },
                        },
                    },
                },
            });

            (int exit, string stdout, _) = RunRule("{ \"id\": \"NOVEL-baked-in\", \"name\": \"Override\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("'NOVEL-baked-in'");
            stdout.Should().Contain("already");
            stdout.Should().Contain("tool.driver.rules");
            File.ReadAllLines(WipPath).Should().HaveCount(1, "only the run-header event should be present");
        }

        [Fact]
        public void Run_RejectsDuplicateAgainstHeaderPrePopulatedNotifications()
        {
            SeedRunHeader(new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = "demo",
                        Notifications = new List<ReportingDescriptor>
                        {
                            new() { Id = "config-error", Name = "ConfigError" },
                        },
                    },
                },
            });

            (int exit, string stdout, _) = RunNotification("{ \"id\": \"config-error\", \"name\": \"Override\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stdout.Should().Contain("'config-error'");
            stdout.Should().Contain("tool.driver.notifications");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenWipMissing()
        {
            (int exit, _, string stderr) = RunNotification("{ \"id\": \"progress\" }");

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("emit-run");
        }

        [Fact]
        public void Run_FailsOnMalformedJson()
        {
            SeedRunHeader();

            (int exit, _, string stderr) = RunNotification("{ broken");

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("malformed");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsOnTopLevelScalar()
        {
            SeedRunHeader();

            (int exit, _, string stderr) = RunNotification("\"just-a-string\"");

            exit.Should().Be(CommandBase.FAILURE);
            stderr.Should().Contain("object or an array");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }
    }
}

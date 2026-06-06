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

        [Fact]
        public void Run_HappyPath_AppendsNotificationDescriptorByDefault()
        {
            SeedRunHeader();
            File.WriteAllText(
                InputPath,
                "{ \"id\": \"progress\", \"name\": \"Progress\", \"shortDescription\": { \"text\": \"Per-batch progress.\" } }");

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
                // notifications target
            });

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
            File.WriteAllText(
                InputPath,
                "{ \"id\": \"NOVEL-prompt-injection\", \"name\": \"PromptInjection\", \"helpUri\": \"https://example.com/help\" }");

            int exit = new AddRuleReportingDescriptorCommand().Run(new AddRuleReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);

            string[] lines = File.ReadAllLines(WipPath);
            lines.Length.Should().Be(2);
            lines[1].Should().Contain("\"kind\":\"rule-descriptor\"");
            lines[1].Should().Contain("NOVEL-prompt-injection");
            lines[1].Should().Contain("https://example.com/help");
        }

        [Fact]
        public void Run_PreservesRichDescriptorPayloadVerbatim()
        {
            // Producers may attach arbitrarily-rich content to a descriptor (messageStrings,
            // defaultConfiguration, relationships, properties). The JToken round-trip path
            // must not lose any of it.
            SeedRunHeader();
            const string Rich =
                "{ \"id\": \"NOVEL-system-prompt-leak\", \"name\": \"SystemPromptLeak\", " +
                "\"shortDescription\": { \"text\": \"System prompt leaked into model output.\" }, " +
                "\"fullDescription\": { \"text\": \"long form\", \"markdown\": \"long **form**\" }, " +
                "\"messageStrings\": { \"default\": { \"text\": \"Leak at {0}.\" } }, " +
                "\"defaultConfiguration\": { \"level\": \"error\", \"rank\": 90.0 }, " +
                "\"helpUri\": \"https://example.com/leak\", " +
                "\"properties\": { \"ai/family\": \"prompt\", \"observed\": \"2026-02-14T08:30:00+00:00\" } }";
            File.WriteAllText(InputPath, Rich);

            int exit = new AddRuleReportingDescriptorCommand().Run(new AddRuleReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            string appended = File.ReadAllLines(WipPath).Last();
            appended.Should().Contain("System prompt leaked");
            appended.Should().Contain("long **form**");
            appended.Should().Contain("Leak at {0}");
            appended.Should().Contain("\"ai/family\":\"prompt\"");
            appended.Should().Contain("2026-02-14T08:30:00+00:00",
                "date-like properties must round-trip verbatim — Json.NET must not coerce them to a normalized DateTime");
        }

        [Fact]
        public void Run_RulesPath_RejectsTaxonomyIdNotInNovelForm()
        {
            // add-rule-reporting-descriptor is reserved for NOVEL- novel-finding descriptors. Taxonomy
            // rule descriptors (e.g., CWE-89) come from the taxonomy enricher and MUST NOT
            // be authored via this verb.
            AssertRulesPathRejects("{ \"id\": \"CWE-89\", \"name\": \"SqlInjection\" }", "CWE-89");
        }

        [Fact]
        public void Run_RulesPath_RejectsBareNovelPrefix()
        {
            // 'NOVEL-' has the prefix but no kebab sub-id — the full grammar requires at
            // least one lowercase-alphanumeric segment after the hyphen.
            AssertRulesPathRejects("{ \"id\": \"NOVEL-\", \"name\": \"Bare\" }", "NOVEL-");
        }

        [Fact]
        public void Run_RulesPath_RejectsNovelIdWithSlash()
        {
            // The NOVEL- escape hatch is kebab-only; a slash (taxonomy sub-id punctuation)
            // is not legal in a novel id, so a descriptor id can never carry one.
            AssertRulesPathRejects("{ \"id\": \"NOVEL-foo/bar\", \"name\": \"Slash\" }", "NOVEL-foo/bar");
        }

        [Fact]
        public void Run_RulesPath_RejectsNovelIdWithTrailingHyphen()
        {
            // A trailing hyphen leaves an empty final kebab segment — rejected.
            AssertRulesPathRejects("{ \"id\": \"NOVEL-trailing-\", \"name\": \"Trailing\" }", "NOVEL-trailing-");
        }

        [Fact]
        public void Run_RulesPath_RejectsNovelIdWithUppercaseTail()
        {
            // The novel grammar is lowercase-kebab; an uppercase tail would not match the
            // result-side NOVEL- ruleId, so the descriptor id is rejected.
            AssertRulesPathRejects("{ \"id\": \"NOVEL-PromptInjection\", \"name\": \"Upper\" }", "NOVEL-PromptInjection");
        }

        [Fact]
        public void Run_RulesPath_RejectsNovelIdWithDoubleHyphen()
        {
            // Double hyphen yields an empty interior segment — rejected.
            AssertRulesPathRejects("{ \"id\": \"NOVEL--double\", \"name\": \"Double\" }", "NOVEL--double");
        }

        private void AssertRulesPathRejects(string descriptorJson, string expectedIdInError)
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, descriptorJson);

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddRuleReportingDescriptorCommand().Run(new AddRuleReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            string err = errWriter.ToString();
            err.Should().Contain(AIRuleIdConventionException.ErrorCode);
            err.Should().Contain($"'{expectedIdInError}'");
            err.Should().Contain("NOVEL-");

            // Critical: the rejected descriptor must NOT pollute the event log.
            File.ReadAllLines(WipPath).Should().HaveCount(1, "rejected descriptor must not be appended");
        }

        [Fact]
        public void Run_NotificationsPath_AcceptsAnyId()
        {
            // Notifications use opaque ids by convention — no NOVEL-/taxonomy gate fires.
            SeedRunHeader();
            File.WriteAllText(InputPath, "{ \"id\": \"config-error\", \"name\": \"ConfigError\" }");

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
                // notifications target; no id convention enforced
            });

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllLines(WipPath).Last().Should().Contain("config-error");
        }

        [Fact]
        public void Run_RejectsMissingId()
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, "{ \"name\": \"NoId\", \"shortDescription\": { \"text\": \"x\" } }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("'id'");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_RejectsEmptyId()
        {
            SeedRunHeader();
            File.WriteAllText(InputPath, "{ \"id\": \"\", \"name\": \"Empty\" }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("non-empty");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_RejectsNonStringId()
        {
            // Producer error: id supplied as a JSON number (e.g., descriptor index leaked
            // into the id field). Distinct diagnostic that names the actual problem.
            SeedRunHeader();
            File.WriteAllText(InputPath, "{ \"id\": 42, \"name\": \"BadId\" }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            string err = errWriter.ToString();
            err.Should().Contain("'id'");
            err.Should().Contain("integer");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_RejectsDuplicateRuleDescriptorId_DoesNotAppend()
        {
            SeedRunHeader();

            // First add: success.
            File.WriteAllText(InputPath, "{ \"id\": \"NOVEL-foo\", \"name\": \"Foo\" }");
            new AddRuleReportingDescriptorCommand().Run(new AddRuleReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            }).Should().Be(CommandBase.SUCCESS);

            // Second add of the SAME id: reject.
            File.WriteAllText(InputPath, "{ \"id\": \"NOVEL-foo\", \"name\": \"FooAgain\" }");
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddRuleReportingDescriptorCommand().Run(new AddRuleReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            string err = errWriter.ToString();
            err.Should().Contain("'NOVEL-foo'");
            err.Should().Contain("already");
            err.Should().Contain("rule descriptor");

            // Only the first descriptor was appended.
            File.ReadAllLines(WipPath).Should().HaveCount(2);
        }

        [Fact]
        public void Run_RejectsDuplicateNotificationDescriptorId_DoesNotAppend()
        {
            SeedRunHeader();

            File.WriteAllText(InputPath, "{ \"id\": \"progress\", \"name\": \"Progress\" }");
            new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            }).Should().Be(CommandBase.SUCCESS);

            File.WriteAllText(InputPath, "{ \"id\": \"progress\", \"name\": \"ProgressAgain\" }");
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("'progress'");
            errWriter.ToString().Should().Contain("notification descriptor");
            File.ReadAllLines(WipPath).Should().HaveCount(2);
        }

        [Fact]
        public void Run_SameIdAcrossTargets_BothAccepted()
        {
            // Rules and notifications are separate descriptor lists per SARIF — a notification
            // descriptor with id "X" does NOT collide with a rule descriptor with id "X".
            SeedRunHeader();

            File.WriteAllText(InputPath, "{ \"id\": \"NOVEL-foo\", \"name\": \"Foo\" }");
            new AddRuleReportingDescriptorCommand().Run(new AddRuleReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            }).Should().Be(CommandBase.SUCCESS);

            File.WriteAllText(InputPath, "{ \"id\": \"NOVEL-foo\", \"name\": \"FooNotification\" }");
            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
                // notifications target — different array
            });

            exit.Should().Be(CommandBase.SUCCESS);
            File.ReadAllLines(WipPath).Should().HaveCount(3);
        }

        [Fact]
        public void Run_RejectsDuplicateAgainstHeaderPrePopulatedRules()
        {
            // Producer pre-populated a rule descriptor on the run-header. Subsequent
            // add-rule-reporting-descriptor with the same id MUST fail at receipt.
            SeedRunHeader(new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = "demo",
                        Rules = new System.Collections.Generic.List<ReportingDescriptor>
                        {
                            new() { Id = "NOVEL-baked-in", Name = "BakedIn" },
                        },
                    },
                },
            });

            File.WriteAllText(InputPath, "{ \"id\": \"NOVEL-baked-in\", \"name\": \"Override\" }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddRuleReportingDescriptorCommand().Run(new AddRuleReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            string err = errWriter.ToString();
            err.Should().Contain("'NOVEL-baked-in'");
            err.Should().Contain("pre-populated");
            err.Should().Contain("tool.driver.rules");
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
                        Notifications = new System.Collections.Generic.List<ReportingDescriptor>
                        {
                            new() { Id = "config-error", Name = "ConfigError" },
                        },
                    },
                },
            });

            File.WriteAllText(InputPath, "{ \"id\": \"config-error\", \"name\": \"Override\" }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            string err = errWriter.ToString();
            err.Should().Contain("'config-error'");
            err.Should().Contain("tool.driver.notifications");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }

        [Fact]
        public void Run_FailsWhenWipMissing()
        {
            // Do NOT seed the run header.
            File.WriteAllText(InputPath, "{ \"id\": \"progress\", \"name\": \"Progress\" }");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
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
            File.WriteAllText(InputPath, "{ this is not json");

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
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

            int exit = new AddNotificationReportingDescriptorCommand().Run(new AddNotificationReportingDescriptorOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("JSON object");
            File.ReadAllLines(WipPath).Should().HaveCount(1);
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Test.UnitTests.Emit
{
    public class SarifEventReplayerTests
    {
        [Fact]
        public void Replay_BuildsSingleRunFromEvents()
        {
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-79/xss-via-template", Message = new Message { Text = "xss" } }),
                Event(SarifEventKinds.Invocation, new Invocation { ExecutionSuccessful = true }),
                Event(SarifEventKinds.Notification, new Notification { Message = new Message { Text = "n" } }),
            };

            SarifLog log = SarifEventReplayer.Replay(events);

            log.Runs.Should().HaveCount(1);
            Run run = log.Runs[0];
            run.Tool.Driver.Name.Should().Be("demo");
            run.Results.Should().HaveCount(1);
            run.Invocations.Should().HaveCount(1);
            run.Invocations[0].ToolExecutionNotifications.Should().HaveCount(1);
        }

        [Fact]
        public void Replay_AutoRegistersDescriptorForFirstSightingOfRuleId()
        {
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
                Event(SarifEventKinds.Result, new Result { RuleId = "NOVEL-rule-a" }),
                Event(SarifEventKinds.Result, new Result { RuleId = "NOVEL-rule-b" }),
                Event(SarifEventKinds.Result, new Result { RuleId = "NOVEL-rule-a" }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Tool.Driver.Rules.Should().HaveCount(2);
            run.Tool.Driver.Rules[0].Id.Should().Be("NOVEL-rule-a");
            run.Tool.Driver.Rules[1].Id.Should().Be("NOVEL-rule-b");
            run.Results[0].RuleIndex.Should().Be(0);
            run.Results[1].RuleIndex.Should().Be(1);
            run.Results[2].RuleIndex.Should().Be(0);
        }

        [Fact]
        public void Replay_RegistersBaseDescriptorForHierarchicalRuleIds()
        {
            // Per SARIF §3.49.3 descriptor ids are base-only. A hierarchical result.ruleId
            // such as "CWE-79/dom-xss-bypass" registers a descriptor with the base id
            // "CWE-79"; the full hierarchical form stays on result.ruleId per §3.52.4.
            // Multiple hierarchical results sharing a base reuse the same descriptor.
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-79/dom-xss-bypass" }),
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-79/stored-template-xss" }),
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-89/string-concat-query" }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Tool.Driver.Rules.Should().HaveCount(2);
            run.Tool.Driver.Rules[0].Id.Should().Be("CWE-79");
            run.Tool.Driver.Rules[1].Id.Should().Be("CWE-89");

            run.Results[0].RuleId.Should().Be("CWE-79/dom-xss-bypass");
            run.Results[0].RuleIndex.Should().Be(0);
            run.Results[1].RuleId.Should().Be("CWE-79/stored-template-xss");
            run.Results[1].RuleIndex.Should().Be(0);
            run.Results[2].RuleId.Should().Be("CWE-89/string-concat-query");
            run.Results[2].RuleIndex.Should().Be(1);
        }

        [Fact]
        public void Replay_HierarchicalRuleIdReusesPreRegisteredBaseDescriptor()
        {
            // If the producer pre-registered the base descriptor on the run header,
            // a hierarchical result.ruleId resolves to it — no duplicate descriptor created,
            // pre-registered metadata (name, helpUri, etc.) preserved.
            var preRegistered = new ReportingDescriptor { Id = "CWE-79", Name = "Pre-registered" };
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run
                {
                    Tool = new Tool
                    {
                        Driver = new ToolComponent
                        {
                            Name = "demo",
                            Rules = new List<ReportingDescriptor> { preRegistered },
                        },
                    },
                }),
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-79/dom-xss-bypass" }),
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-79/secondary" }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Tool.Driver.Rules.Should().HaveCount(1);
            run.Tool.Driver.Rules[0].Name.Should().Be("Pre-registered");
            run.Results[0].RuleIndex.Should().Be(0);
            run.Results[1].RuleIndex.Should().Be(0);
        }

        [Fact]
        public void Replay_RejectsBareTaxonomyRuleId()
        {
            // A bare taxonomy id (no sub-id) violates the AI ruleId convention. The
            // replayer surfaces this as AIRuleIdConventionException so emit-finalize can
            // print a structured, AI-consumable error and the orchestrator can retry.
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run()),
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-79" }),
            };

            System.Action act = () => SarifEventReplayer.Replay(events);

            AIRuleIdConventionException ex = act.Should().Throw<AIRuleIdConventionException>().Which;
            ex.OffendingRuleIds.Should().ContainSingle().Which.Should().Be("CWE-79");
            ex.Message.Should().Contain("AI-RULEID-001");
            ex.Message.Should().Contain("CWE-89/kql-injection-from-config");
            ex.Message.Should().Contain("NOVEL-");
        }

        [Fact]
        public void Replay_CollectsAllRuleIdViolationsInOneException()
        {
            // Multiple violators are reported in a single exception so an AI orchestrator
            // can fix every offender in one retry rather than one-at-a-time.
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run()),
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-79" }),                  // bare taxonomy
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-89/x" }),                // OK (interleaved)
                Event(SarifEventKinds.Result, new Result { RuleId = "my-custom-rule" }),          // not taxonomy, not NOVEL-
                Event(SarifEventKinds.Result, new Result { RuleId = "NOVEL-foo/bar" }),           // NOVEL- with slash
                Event(SarifEventKinds.Result, new Result { RuleId = string.Empty }),              // empty
            };

            System.Action act = () => SarifEventReplayer.Replay(events);

            AIRuleIdConventionException ex = act.Should().Throw<AIRuleIdConventionException>().Which;
            ex.OffendingRuleIds.Should().BeEquivalentTo(new[] { "CWE-79", "my-custom-rule", "NOVEL-foo/bar", string.Empty });
        }

        [Fact]
        public void Replay_SynthesizesInvocationToHoldNotificationsWhenNoneProvided()
        {
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
                Event(SarifEventKinds.Notification, new Notification { Message = new Message { Text = "orphan" } }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Invocations.Should().HaveCount(1);
            run.Invocations[0].ExecutionSuccessful.Should().BeTrue();
            run.Invocations[0].ToolExecutionNotifications.Should().HaveCount(1);
        }

        [Fact]
        public void Replay_RoutesNotificationsToLastInvocation()
        {
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run()),
                Event(SarifEventKinds.Invocation, new Invocation { CommandLine = "first" }),
                Event(SarifEventKinds.Invocation, new Invocation { CommandLine = "second" }),
                Event(SarifEventKinds.Notification, new Notification { Message = new Message { Text = "n" } }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Invocations[0].ToolExecutionNotifications.Should().BeNull();
            run.Invocations[1].ToolExecutionNotifications.Should().HaveCount(1);
        }

        [Fact]
        public void Replay_StampsTimeUtcOnNotificationsLackingTimestamp()
        {
            // AI2019 expects every notification to carry timeUtc; the replayer fills the gap
            // so producers don't have to remember. The exact value isn't asserted (it is
            // the moment of replay) but it MUST fall between observations bracketing the call.
            DateTime before = DateTime.UtcNow.AddSeconds(-1);
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run()),
                Event(SarifEventKinds.Notification, new Notification { Message = new Message { Text = "n" } }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];
            DateTime after = DateTime.UtcNow.AddSeconds(1);

            Notification stamped = run.Invocations[0].ToolExecutionNotifications[0];
            stamped.TimeUtc.Should().NotBe(default(DateTime));
            stamped.TimeUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Fact]
        public void Replay_PreservesProducerSuppliedTimeUtcOnNotifications()
        {
            // Producer-supplied timeUtc wins. The replayer only fills the gap; it never
            // rewrites a stamp the producer already chose.
            var supplied = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run()),
                Event(SarifEventKinds.Notification, new Notification { Message = new Message { Text = "n" }, TimeUtc = supplied }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Invocations[0].ToolExecutionNotifications[0].TimeUtc.Should().Be(supplied);
        }

        [Fact]
        public void Replay_TolleratesAbsenceOfRunHeader()
        {
            var events = new[]
            {
                Event(SarifEventKinds.Result, new Result { RuleId = "NOVEL-x" }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Tool.Driver.Name.Should().Be("Unknown");
            run.Tool.Driver.Rules.Should().HaveCount(1);
            run.Results.Should().HaveCount(1);
        }

        [Fact]
        public void Replay_ThrowsWhenSecondRunHeaderEncountered()
        {
            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, new Run()),
                Event(SarifEventKinds.RunHeader, new Run()),
            };

            System.Action act = () => SarifEventReplayer.Replay(events);

            act.Should().Throw<SarifEventLogException>().WithMessage("*more than one*");
        }

        [Fact]
        public void Replay_DiscardsResultsAndInvocationsOnRunHeader()
        {
            // The header carries a partial Run shape only; results/invocations on a header are
            // ignored so events remain the source of truth.
            var seed = new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "demo" } },
                Results = new List<Result> { new() { RuleId = "STALE" } },
                Invocations = new List<Invocation> { new() { CommandLine = "stale" } },
            };

            var events = new[]
            {
                Event(SarifEventKinds.RunHeader, seed),
                Event(SarifEventKinds.Result, new Result { RuleId = "NOVEL-real" }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Results.Should().HaveCount(1);
            run.Results[0].RuleId.Should().Be("NOVEL-real");
            run.Invocations.Should().BeNull();
        }

        // Reflection-enforced coverage: every public string constant declared on
        // SarifEventKinds must be reached by a non-default case in
        // SarifEventReplayer's switch. If a new kind is added to SarifEventKinds
        // without a matching case, the replayer's default branch throws and this
        // test fails for that kind.
        [Theory]
        [MemberData(nameof(EveryDeclaredEventKind))]
        public void Replay_HasSwitchCoverageFor(string kind)
        {
            // Empty {} deserializes to a default instance for every payload type the
            // replayer consumes today (Run, Result, Invocation, Notification). New kinds
            // that need a richer minimal payload can extend MinimalPayloadFor.
            JToken payload = MinimalPayloadFor(kind);
            var ev = new SarifEvent { Kind = kind, Version = SarifEventKinds.CurrentSchemaVersion, Payload = payload };

            System.Action act = () => SarifEventReplayer.Replay(new[] { ev });

            act.Should().NotThrow<SarifEventLogException>(
                because: $"every public const in SarifEventKinds must be handled by the replayer switch; '{kind}' is missing a case.");
        }

        public static IEnumerable<object[]> EveryDeclaredEventKind() =>
            typeof(SarifEventKinds)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => new object[] { (string)f.GetRawConstantValue() });

        private static JToken MinimalPayloadFor(string kind) => new JObject();

        private static SarifEvent Event(string kind, object payload) =>
            new SarifEvent
            {
                Kind = kind,
                Version = SarifEventKinds.CurrentSchemaVersion,
                Payload = JToken.FromObject(payload),
            };
    }
}

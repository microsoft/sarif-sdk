// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

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
                Event(SarifEventKinds.Result, new Result { RuleId = "CWE-79", Message = new Message { Text = "xss" } }),
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
                Event(SarifEventKinds.Result, new Result { RuleId = "RULE-A" }),
                Event(SarifEventKinds.Result, new Result { RuleId = "RULE-B" }),
                Event(SarifEventKinds.Result, new Result { RuleId = "RULE-A" }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Tool.Driver.Rules.Should().HaveCount(2);
            run.Tool.Driver.Rules[0].Id.Should().Be("RULE-A");
            run.Tool.Driver.Rules[1].Id.Should().Be("RULE-B");
            run.Results[0].RuleIndex.Should().Be(0);
            run.Results[1].RuleIndex.Should().Be(1);
            run.Results[2].RuleIndex.Should().Be(0);
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
        public void Replay_TolleratesAbsenceOfRunHeader()
        {
            var events = new[]
            {
                Event(SarifEventKinds.Result, new Result { RuleId = "X" }),
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
                Event(SarifEventKinds.Result, new Result { RuleId = "REAL" }),
            };

            Run run = SarifEventReplayer.Replay(events).Runs[0];

            run.Results.Should().HaveCount(1);
            run.Results[0].RuleId.Should().Be("REAL");
            run.Invocations.Should().BeNull();
        }

        private static SarifEvent Event(string kind, object payload) =>
            new SarifEvent
            {
                Kind = kind,
                Version = SarifEventKinds.CurrentSchemaVersion,
                Payload = JToken.FromObject(payload),
            };
    }
}

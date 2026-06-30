// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Test.UnitTests.Emit
{
    /// <summary>
    /// Exercises <see cref="RunEmitContext"/> directly over an <see cref="InMemoryEmitSink"/>,
    /// confirming the emit pipeline is usable in-process — no file, no process spawn — and that the
    /// returned <see cref="EmitReport"/> carries the same validation and all-or-none semantics the
    /// CLI verbs expose.
    /// </summary>
    public class RunEmitContextTests
    {
        [Fact]
        public void AddResults_SingleValid_AppendsOneResultEvent()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            EmitReport report = context.AddResults(Obj(@"{ ""ruleId"": ""CWE-89/sql-injection"" }"));

            report.Succeeded.Should().BeTrue();
            report.Appended.Should().Be(1);
            sink.Events.Should().ContainSingle(e => e.Kind == SarifEventKinds.Result);
        }

        [Fact]
        public void AddResults_ValidArray_AppendsEveryElement()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            EmitReport report = context.AddResults(Arr(
                @"[ { ""ruleId"": ""CWE-89/sql-injection"" }, { ""ruleId"": ""NOVEL-prompt-injection"" } ]"));

            report.Succeeded.Should().BeTrue();
            report.Appended.Should().Be(2);
            sink.Events.Count(e => e.Kind == SarifEventKinds.Result).Should().Be(2);
        }

        [Fact]
        public void AddResults_NonStringRuleId_RejectsWithAi1012AndAppendsNothing()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            EmitReport report = context.AddResults(Obj(@"{ ""ruleId"": 7 }"));

            report.Succeeded.Should().BeFalse();
            report.Appended.Should().Be(0);
            report.Rejected.Should().ContainSingle()
                .Which.ErrorCode.Should().Be(AIRuleIdConventionException.ErrorCode);
            sink.Events.Should().BeEmpty();
        }

        [Fact]
        public void AddResults_BatchWithInvalidLastElement_RejectsAtomically()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            // The first element is valid; the second (the Nth) is not — the whole batch is refused.
            EmitReport report = context.AddResults(Arr(
                @"[ { ""ruleId"": ""CWE-89/sql-injection"" }, { ""ruleId"": ""not a valid id"" } ]"));

            report.Succeeded.Should().BeFalse();
            report.Appended.Should().Be(0);
            report.Rejected.Should().ContainSingle().Which.Index.Should().Be(1);
            sink.Events.Should().BeEmpty();
        }

        [Fact]
        public void AddInvocations_SingleOmittingEndTimeUtc_LeavesEndTimeUtcAbsent()
        {
            (EmitReport report, InMemoryEmitSink sink) = EmitSingleInvocation(
                @"{ ""executionSuccessful"": true, ""commandLine"": ""scan"", ""workingDirectory"": { ""uri"": ""file:///repo/"" } }");

            report.Succeeded.Should().BeTrue();
            report.Appended.Should().Be(1);
            JToken appended = sink.Events.Single(e => e.Kind == SarifEventKinds.Invocation).Payload;
            appended["endTimeUtc"].Should().BeNull();
        }

        [Fact]
        public void AddInvocations_BatchOmittingEndTimeUtc_IsAccepted()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            EmitReport report = context.AddInvocations(Arr(
                @"[ { ""executionSuccessful"": true, ""commandLine"": ""scan"", ""workingDirectory"": { ""uri"": ""file:///repo/"" } } ]"));

            report.Succeeded.Should().BeTrue();
            report.Appended.Should().Be(1);
            sink.Events.Should().ContainSingle(e => e.Kind == SarifEventKinds.Invocation);
        }

        [Fact]
        public void AddInvocations_EndTimeUtcInFuture_IsRejected()
        {
            (EmitReport report, _) = EmitSingleInvocation(InvocationWith("endTimeUtc", Future));

            report.Succeeded.Should().BeFalse();
            report.Rejected.Should().ContainSingle()
                .Which.Message.Should().Contain("future");
        }

        [Fact]
        public void AddInvocations_StartTimeUtcInFuture_IsRejected()
        {
            (EmitReport report, _) = EmitSingleInvocation(InvocationWith("startTimeUtc", Future));

            report.Succeeded.Should().BeFalse();
            report.Rejected.Should().ContainSingle()
                .Which.Message.Should().Contain("future");
        }

        [Fact]
        public void AddInvocations_EndTimeUtcWithinSkewGrace_IsAccepted()
        {
            (EmitReport report, _) = EmitSingleInvocation(InvocationWith("endTimeUtc", WithinGrace));

            report.Succeeded.Should().BeTrue();
            report.Appended.Should().Be(1);
        }

        [Fact]
        public void AddInvocations_PastTimes_AreAccepted()
        {
            (EmitReport report, _) = EmitSingleInvocation(InvocationWith("endTimeUtc", Past));

            report.Succeeded.Should().BeTrue();
            report.Appended.Should().Be(1);
        }

        [Fact]
        public void AddInvocations_NotificationTimeUtcInFuture_IsRejected()
        {
            (EmitReport report, _) = EmitSingleInvocation(
                $@"{{ ""executionSuccessful"": true, ""commandLine"": ""scan"", ""workingDirectory"": {{ ""uri"": ""file:///repo/"" }}, ""toolExecutionNotifications"": [ {{ ""message"": {{ ""text"": ""x"" }}, ""timeUtc"": ""{Future}"" }} ] }}");

            report.Succeeded.Should().BeFalse();
            report.Rejected.Should().ContainSingle()
                .Which.Message.Should().Contain("future");
        }

        [Fact]
        public void AddInvocations_NotificationTimeUtcInPast_IsAccepted()
        {
            (EmitReport report, _) = EmitSingleInvocation(
                $@"{{ ""executionSuccessful"": true, ""commandLine"": ""scan"", ""workingDirectory"": {{ ""uri"": ""file:///repo/"" }}, ""toolExecutionNotifications"": [ {{ ""message"": {{ ""text"": ""x"" }}, ""timeUtc"": ""{Past}"" }} ] }}");

            report.Succeeded.Should().BeTrue();
            report.Appended.Should().Be(1);
        }

        [Fact]
        public void AddRuleDescriptors_NovelId_Appends()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            EmitReport report = context.AddRuleDescriptors(Obj(@"{ ""id"": ""NOVEL-prompt-injection"" }"));

            report.Succeeded.Should().BeTrue();
            sink.Events.Should().ContainSingle(e => e.Kind == SarifEventKinds.RuleDescriptor);
        }

        [Fact]
        public void AddRuleDescriptors_NonNovelId_RejectsWithAi1012()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            EmitReport report = context.AddRuleDescriptors(Obj(@"{ ""id"": ""CWE-89"" }"));

            report.Succeeded.Should().BeFalse();
            report.Rejected.Should().ContainSingle()
                .Which.ErrorCode.Should().Be(AIRuleIdConventionException.ErrorCode);
            sink.Events.Should().BeEmpty();
        }

        [Fact]
        public void AddRuleDescriptors_DuplicateIdInBatch_RejectsAtomically()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            EmitReport report = context.AddRuleDescriptors(Arr(
                @"[ { ""id"": ""NOVEL-a"" }, { ""id"": ""NOVEL-a"" } ]"));

            report.Succeeded.Should().BeFalse();
            report.Appended.Should().Be(0);
            report.Rejected.Should().ContainSingle().Which.Index.Should().Be(1);
            sink.Events.Should().BeEmpty();
        }

        [Fact]
        public void AddRuleDescriptors_DuplicateOfPriorlyAppendedId_IsRejected()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            context.AddRuleDescriptors(Obj(@"{ ""id"": ""NOVEL-a"" }")).Succeeded.Should().BeTrue();

            EmitReport report = context.AddRuleDescriptors(Obj(@"{ ""id"": ""NOVEL-a"" }"));

            report.Succeeded.Should().BeFalse();
            report.Rejected.Should().ContainSingle()
                .Which.Message.Should().Contain("already present");
            sink.Events.Count(e => e.Kind == SarifEventKinds.RuleDescriptor).Should().Be(1);
        }

        [Fact]
        public void AddResults_ScalarPayload_IsWholePayloadRejection()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            EmitReport report = context.AddResults(JToken.Parse("42"));

            report.Succeeded.Should().BeFalse();
            report.PayloadError.Should().NotBeNull();
            report.Rejected.Should().BeEmpty();
            sink.Events.Should().BeEmpty();
        }

        [Fact]
        public void ResolveToLog_MaterializesAppendedEventsIntoSingleRun()
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);

            context.SetRunHeader(Obj(@"{ ""tool"": { ""driver"": { ""name"": ""demo"" } } }"));
            context.AddResults(Obj(
                @"{ ""ruleId"": ""CWE-89/sql-injection"", ""message"": { ""text"": ""sql injection"" } }"))
                .Succeeded.Should().BeTrue();

            SarifLog log = context.ResolveToLog();

            log.Runs.Should().ContainSingle();
            log.Runs[0].Tool.Driver.Name.Should().Be("demo");
            log.Runs[0].Results.Should().ContainSingle();
        }

        private static JObject Obj(string json) => (JObject)JToken.Parse(json);

        private static JArray Arr(string json) => (JArray)JToken.Parse(json);

        private static string Future => DateTime.UtcNow.AddHours(1).ToString("o", CultureInfo.InvariantCulture);

        private static string WithinGrace => DateTime.UtcNow.AddMinutes(2).ToString("o", CultureInfo.InvariantCulture);

        private static string Past => DateTime.UtcNow.AddMinutes(-5).ToString("o", CultureInfo.InvariantCulture);

        private static string InvocationWith(string propertyName, string timeValue) =>
            $@"{{ ""executionSuccessful"": true, ""commandLine"": ""scan"", ""workingDirectory"": {{ ""uri"": ""file:///repo/"" }}, ""{propertyName}"": ""{timeValue}"" }}";

        private static (EmitReport report, InMemoryEmitSink sink) EmitSingleInvocation(string invocationJson)
        {
            var sink = new InMemoryEmitSink();
            var context = new RunEmitContext(sink);
            EmitReport report = context.AddInvocations(ObjPreservingDates(invocationJson));
            return (report, sink);
        }

        // Mirror the production reader: keep timeUtc/startTimeUtc/endTimeUtc as string tokens rather
        // than letting JToken.Parse coerce them into Date tokens, so the receipt gate sees the same
        // shape it sees at emit time.
        private static JObject ObjPreservingDates(string json)
        {
            using (var reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None })
            {
                return (JObject)JToken.ReadFrom(reader);
            }
        }
    }
}

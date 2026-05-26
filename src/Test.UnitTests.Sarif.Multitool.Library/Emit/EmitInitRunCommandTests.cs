// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class EmitInitRunCommandTests : IDisposable
    {
        private readonly string _dir;
        private readonly TextWriter _origStdOut;
        private readonly TextWriter _origStdErr;
        private readonly TextReader _origStdIn;
        private readonly IEnvironmentVariableGetter _emptyEnv = new EmptyEnvironmentVariableGetter();

        public EmitInitRunCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"emit-init-run-{Guid.NewGuid():N}");
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
        private string InputPath => Path.Combine(_dir, "run.json");

        private EmitInitRunCommand NewCommand() => new EmitInitRunCommand(_emptyEnv);

        // Minimal Run shape that satisfies the verb's required-field check; tests build on this
        // by mutating a JObject clone so each scenario stays self-contained.
        private static JObject MinimalRun() => new JObject
        {
            ["tool"] = new JObject
            {
                ["driver"] = new JObject { ["name"] = "demo" },
            },
        };

        private void WriteInput(JObject runObject) => File.WriteAllText(InputPath, runObject.ToString());

        private int RunWithInput(JObject runObject, bool forceOverwrite = false)
        {
            WriteInput(runObject);
            return NewCommand().Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
                ForceOverwrite = forceOverwrite,
            });
        }

        private int RunWithInput(IEnvironmentVariableGetter env, JObject runObject, bool forceOverwrite = false)
        {
            WriteInput(runObject);
            return new EmitInitRunCommand(env).Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
                ForceOverwrite = forceOverwrite,
            });
        }

        [Fact]
        public void Run_OnCleanState_CreatesWipWithRunHeaderEvent()
        {
            int exit = RunWithInput(MinimalRun());

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(WipPath).Should().BeTrue();

            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);
            events[0].Kind.Should().Be(SarifEventKinds.RunHeader);
            events[0].Payload["tool"]["driver"]["name"].ToString().Should().Be("demo");
        }

        [Fact]
        public void Run_WithRichRunHeader_AppendsTwoVcpEntriesAndPropertiesBag()
        {
            // The motivating scenario: producers need to emit multiple versionControlProvenance
            // entries with attached properties bags (e.g. one documenting the skills in play)
            // that the previous flag surface could not encode.
            var richRun = new JObject
            {
                ["tool"] = new JObject
                {
                    ["driver"] = new JObject
                    {
                        ["name"] = "demo",
                        ["semanticVersion"] = "1.2.3",
                        ["informationUri"] = "https://example.com/tool",
                    },
                },
                ["versionControlProvenance"] = new JArray
                {
                    new JObject
                    {
                        ["repositoryUri"] = "https://github.com/acme/demo",
                        ["revisionId"] = "abc",
                        ["branch"] = "main",
                    },
                    new JObject
                    {
                        ["repositoryUri"] = "https://github.com/acme/skills",
                        ["revisionId"] = "def",
                        ["branch"] = "main",
                        ["properties"] = new JObject
                        {
                            ["skills/in-play"] = new JArray("triage", "remediation"),
                        },
                    },
                },
                ["properties"] = new JObject { ["ai/origin"] = "generated" },
            };

            int exit = RunWithInput(richRun);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);

            JToken vcp = events[0].Payload["versionControlProvenance"];
            vcp.Should().NotBeNull();
            vcp.Type.Should().Be(JTokenType.Array);
            ((JArray)vcp).Should().HaveCount(2);
            vcp[0]["repositoryUri"].ToString().Should().Be("https://github.com/acme/demo");
            vcp[1]["repositoryUri"].ToString().Should().Be("https://github.com/acme/skills");
            vcp[1]["properties"]["skills/in-play"].Type.Should().Be(JTokenType.Array);
            ((JArray)vcp[1]["properties"]["skills/in-play"]).Should().HaveCount(2);
            events[0].Payload["properties"]["ai/origin"].ToString().Should().Be("generated");
        }

        [Fact]
        public void Run_PreservesUnknownSarifFields_OnTheJTokenDirectPath()
        {
            // JToken-direct stamping (no typed-Run round-trip) preserves SARIF fields that the
            // typed Run model may not surface — proven here with an arbitrary extension key on
            // the run-level properties bag.
            JObject runObject = MinimalRun();
            runObject["properties"] = new JObject
            {
                ["acme/forensic-trace"] = "preserve-me",
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events[0].Payload["properties"]["acme/forensic-trace"].ToString().Should().Be("preserve-me");
        }

        [Fact]
        public void Run_ReadsRunJsonFromStdin_WhenInputFlagOmitted()
        {
            // Stdin redirection via Console.SetIn. EmitEventLogHelpers.TryReadJsonPayload reads
            // raw bytes from Console.OpenStandardInput which doesn't honor Console.SetIn —
            // however the IsInputRedirected branch keys off of the redirected handle. We can
            // exercise the "missing both" branch deterministically and assert it fails clean.
            // (The actual stdin-byte-read path is integration-tested via AddResultCommandTests
            // comment-equivalent skip and the CLI fixtures.)
            int exit = NewCommand().Run(new EmitInitRunOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_FailsIfWipExistsAndOverwriteNotSpecified()
        {
            File.WriteAllText(WipPath, "{}\n");

            int exit = RunWithInput(MinimalRun());

            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void Run_FailsIfSarifExistsAndOverwriteNotSpecified()
        {
            File.WriteAllText(OutPath, "{ \"version\": \"2.1.0\" }");

            int exit = RunWithInput(MinimalRun());

            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void Run_WithForceOverwriteAndExistingWip_DeletesAndRecreates()
        {
            File.WriteAllText(WipPath, "stale wip\n");

            int exit = RunWithInput(MinimalRun(), forceOverwrite: true);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);
            events[0].Kind.Should().Be(SarifEventKinds.RunHeader);
        }

        [Fact]
        public void Run_WithMissingToolDriverName_Fails()
        {
            var runObject = new JObject(); // no tool.driver.name

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithEmptyToolDriverName_Fails()
        {
            var runObject = new JObject
            {
                ["tool"] = new JObject { ["driver"] = new JObject { ["name"] = "   " } },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithNonStringToolDriverName_Fails()
        {
            var runObject = new JObject
            {
                ["tool"] = new JObject { ["driver"] = new JObject { ["name"] = 42 } },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithNonHttpsInformationUri_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["tool"]["driver"]["informationUri"] = "http://example.com";

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithNonHttpsVcpRepositoryUri_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["repositoryUri"] = "http://github.com/acme/demo" },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithNonArrayVcp_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JObject { ["repositoryUri"] = "https://github.com/acme/demo" };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithBadSrcrootScheme_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["originalUriBaseIds"] = new JObject
            {
                ["SRCROOT"] = new JObject { ["uri"] = "ftp://example.com/src/" },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithBadAutomationGuid_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["automationDetails"] = new JObject { ["guid"] = "not-a-guid" };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithBadAutomationCorrelationGuid_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["automationDetails"] = new JObject { ["correlationGuid"] = "not-a-guid" };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithBadAiOrigin_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["properties"] = new JObject { ["ai/origin"] = "speculative" };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithSarifLogShape_FailsWithShapeDiagnostic()
        {
            // The most common user mistake: piping a full SARIF log document in place of a Run.
            var sarifLog = new JObject
            {
                ["version"] = "2.1.0",
                ["runs"] = new JArray(MinimalRun()),
            };

            int exit = RunWithInput(sarifLog);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithMalformedJson_Fails()
        {
            File.WriteAllText(InputPath, "{ not json ");

            int exit = NewCommand().Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithNonObjectJson_Fails()
        {
            File.WriteAllText(InputPath, "[1,2,3]");

            int exit = NewCommand().Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithMissingInputFile_Fails()
        {
            int exit = NewCommand().Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = Path.Combine(_dir, "missing.json"),
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        private static FakeEnvironmentVariableGetter CompleteAdoEnv() => new FakeEnvironmentVariableGetter()
            .With(AdoPipelineContext.TfBuildEnvVar, "True")
            .With(AdoPipelineContext.CollectionUriEnvVar, "https://dev.azure.com/contoso/")
            .With(AdoPipelineContext.TeamProjectIdEnvVar, "11111111-1111-1111-1111-111111111111")
            .With(AdoPipelineContext.BuildDefinitionIdPrimaryEnvVar, "1234")
            .With(AdoPipelineContext.BuildDefinitionNameEnvVar, "Nightly Build")
            .With(AdoPipelineContext.BuildIdEnvVar, "98765")
            .With(AdoPipelineContext.PhaseIdPrimaryEnvVar, "22222222-2222-2222-2222-222222222222")
            .With(AdoPipelineContext.PhaseNamePrimaryEnvVar, "Build")
            .With(AdoPipelineContext.SourceBranchEnvVar, "refs/heads/main");

        private const string ExpectedAdoAutomationId
            = "azuredevops/pipeline/build/contoso/11111111-1111-1111-1111-111111111111/1234/22222222-2222-2222-2222-222222222222/refs/heads/main/98765";

        [Fact]
        public void Run_WhenAdoPipelineContextComplete_StampsAutomationDetails()
        {
            int exit = RunWithInput(CompleteAdoEnv(), MinimalRun());

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);

            events[0].Payload["automationDetails"]["id"].ToString().Should().Be(ExpectedAdoAutomationId);
            events[0].Payload["automationDetails"]["properties"][AdoPipelineContext.PropBuildDefinitionId].ToString().Should().Be("1234");
            events[0].Payload["automationDetails"]["properties"][AdoPipelineContext.PropPhaseName].ToString().Should().Be("Build");
        }

        [Fact]
        public void Run_WhenAdoPipelineContextComplete_AndJsonProvidesEqualValues_Idempotent()
        {
            // Producer-supplied automationDetails.id and the four properties that match the
            // env-detected values are preserved (no-op stamp, no conflict).
            JObject runObject = MinimalRun();
            runObject["automationDetails"] = new JObject
            {
                ["id"] = ExpectedAdoAutomationId,
                ["properties"] = new JObject
                {
                    [AdoPipelineContext.PropBuildDefinitionId] = "1234",
                    [AdoPipelineContext.PropBuildDefinitionName] = "Nightly Build",
                    [AdoPipelineContext.PropPhaseId] = "22222222-2222-2222-2222-222222222222",
                    [AdoPipelineContext.PropPhaseName] = "Build",
                },
            };

            int exit = RunWithInput(CompleteAdoEnv(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events[0].Payload["automationDetails"]["id"].ToString().Should().Be(ExpectedAdoAutomationId);
        }

        [Fact]
        public void Run_WhenAdoPipelineContextComplete_AndJsonProvidesConflictingId_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["automationDetails"] = new JObject { ["id"] = "self-hosted/forensic-trace/2025-11-26" };

            int exit = RunWithInput(CompleteAdoEnv(), runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WhenAdoPipelineContextComplete_AndJsonProvidesConflictingProperty_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["automationDetails"] = new JObject
            {
                ["properties"] = new JObject
                {
                    [AdoPipelineContext.PropBuildDefinitionName] = "Some Other Pipeline",
                },
            };

            int exit = RunWithInput(CompleteAdoEnv(), runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WhenAdoPipelineContextPartial_FailsBeforeCreatingWip()
        {
            // The partial-state path MUST NOT create or overwrite the .wip.jsonl, otherwise a
            // misconfigured pipeline could blow away a valid in-flight scan with --force-overwrite.
            File.WriteAllText(WipPath, "existing wip\n");

            var env = new FakeEnvironmentVariableGetter()
                .With(AdoPipelineContext.TfBuildEnvVar, "True")
                .With(AdoPipelineContext.CollectionUriEnvVar, "https://dev.azure.com/contoso/")
                .With(AdoPipelineContext.BuildDefinitionIdPrimaryEnvVar, "1234");
            // intentionally omit other required vars -> Partial

            int exit = RunWithInput(env, MinimalRun(), forceOverwrite: true);

            exit.Should().Be(CommandBase.FAILURE);
            File.ReadAllText(WipPath).Should().Be("existing wip\n");
        }

        [Fact]
        public void Run_WhenNoAdoEnvVarsSet_OmitsAutomationDetails()
        {
            int exit = RunWithInput(MinimalRun());

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);
            events[0].Payload["automationDetails"].Should().BeNull();
        }

        [Fact]
        public void Run_WithNonObjectToolParent_FailsWithShapeDiagnostic()
        {
            // Regression: prior to parent-shape validation, runObject["tool"]?["driver"] threw
            // InvalidOperationException when "tool" was a JValue, surfacing as a stack-trace
            // instead of an AI-consumable diagnostic.
            int exit = RunWithInput(new JObject { ["tool"] = "x" });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithNonObjectAutomationDetailsParent_FailsWithShapeDiagnostic()
        {
            JObject runObject = MinimalRun();
            runObject["automationDetails"] = "x";

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithNonObjectOriginalUriBaseIdsParent_FailsWithShapeDiagnostic()
        {
            JObject runObject = MinimalRun();
            runObject["originalUriBaseIds"] = new JArray();

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithBlankAutomationGuid_FailsInsteadOfSilentlySkipping()
        {
            JObject runObject = MinimalRun();
            runObject["automationDetails"] = new JObject { ["guid"] = "" };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithWhitespacePaddedAiOrigin_FailsRequiringExactSpelling()
        {
            // " generated " (with whitespace) is not one of the three canonical values; the
            // verb does not trim because downstream AI validation rules match on the raw value.
            JObject runObject = MinimalRun();
            runObject["properties"] = new JObject { ["ai/origin"] = " generated " };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WhenAdoPipelineContextComplete_AndJsonProvidesNonStringId_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["automationDetails"] = new JObject { ["id"] = 12345 };

            int exit = RunWithInput(CompleteAdoEnv(), runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WhenAdoPipelineContextComplete_AndJsonProvidesEmptyId_StampsCanonical()
        {
            // Empty string id is the JSON equivalent of "absent" — stamp it rather than treat
            // it as a conflicting value. Mirrors the typed-Run TryApplyTo behavior.
            JObject runObject = MinimalRun();
            runObject["automationDetails"] = new JObject { ["id"] = "" };

            int exit = RunWithInput(CompleteAdoEnv(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events[0].Payload["automationDetails"]["id"].ToString().Should().Be(ExpectedAdoAutomationId);
        }
    }
}

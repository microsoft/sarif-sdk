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
    public class EmitRunCommandTests : IDisposable
    {
        private readonly string _dir;
        private readonly TextWriter _origStdOut;
        private readonly TextWriter _origStdErr;
        private readonly TextReader _origStdIn;
        private readonly IEnvironmentVariableGetter _emptyEnv = new EmptyEnvironmentVariableGetter();

        public EmitRunCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"emit-run-{Guid.NewGuid():N}");
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

        private EmitRunCommand NewCommand() => new EmitRunCommand(_emptyEnv);

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
            return NewCommand().Run(new EmitRunOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
                ForceOverwrite = forceOverwrite,
            });
        }

        private int RunWithInput(IEnvironmentVariableGetter env, JObject runObject, bool forceOverwrite = false)
        {
            WriteInput(runObject);
            return new EmitRunCommand(env).Run(new EmitRunOptions
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
            // Producers can emit multiple versionControlProvenance entries, each with an
            // attached properties bag (e.g. one documenting the skills in play).
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
        public void Run_FailsWhenPayloadIsEmpty()
        {
            // Pointing --input at an empty file deterministically hits TryReadJsonPayload's
            // "is empty" diagnostic via the file branch. Falling through to the stdin branch
            // (InputFilePath = null) hangs under xUnit on the ADO Linux/Mac agents:
            // Console.IsInputRedirected reports true but the runner's stdin pipe never closes,
            // so Console.OpenStandardInput().ReadToEnd() never returns.
            File.WriteAllText(InputPath, string.Empty);

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = NewCommand().Run(new EmitRunOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = InputPath,
            });

            exit.Should().Be(CommandBase.FAILURE);
            errWriter.ToString().Should().Contain("Run");
            errWriter.ToString().Should().Contain("empty");
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
        public void Run_WithUnsupportedVcpHost_Fails()
        {
            // https passes the header scheme check, but gitlab.com is not a derivable portable-root
            // host, so the receipt-time shape gate rejects it.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["repositoryUri"] = "https://gitlab.com/acme/demo" },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithMalformedAdoVcpRepositoryUri_Fails()
        {
            // A dev.azure.com URL missing its project segment is a valid https URI but not a
            // derivable Azure DevOps repository root, so the receipt-time shape gate rejects it.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["repositoryUri"] = "https://dev.azure.com/contoso/_git/widgets" },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithCredentialBearingVcpRepositoryUri_Fails()
        {
            // A repositoryUri carrying embedded credentials (account@ here; a PAT or user:password@
            // would be worse) is rejected at the receipt-time shape gate so it never reaches finalize.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["repositoryUri"] = "https://x-access-token@github.com/acme/demo" },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithSupportedGheComVcpRepositoryUri_Succeeds()
        {
            // <slug>.ghe.com is the GitHub data-residency / EMU host and is an allow-listed
            // portable-root host, so the receipt-time shape gate accepts it.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["repositoryUri"] = "https://octocorp.ghe.com/acme/demo" },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(WipPath).Should().BeTrue();
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
        public void Run_WithNonExistentFileSrcroot_Fails()
        {
            JObject runObject = MinimalRun();
            string missingDir = Path.Combine(_dir, "no-such-checkout");
            runObject["originalUriBaseIds"] = new JObject
            {
                ["SRCROOT"] = new JObject { ["uri"] = new Uri(missingDir).AbsoluteUri },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithExistingFileSrcroot_Succeeds()
        {
            JObject runObject = MinimalRun();
            runObject["originalUriBaseIds"] = new JObject
            {
                ["SRCROOT"] = new JObject { ["uri"] = new Uri(_dir).AbsoluteUri },
            };

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(WipPath).Should().BeTrue();
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

            int exit = NewCommand().Run(new EmitRunOptions
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

            int exit = NewCommand().Run(new EmitRunOptions
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
            int exit = NewCommand().Run(new EmitRunOptions
            {
                OutputFilePath = OutPath,
                InputFilePath = Path.Combine(_dir, "missing.json"),
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WhenHeaderCarriesResults_WarnsToStderrButStillWritesHeader()
        {
            // results on the run header are dropped at replay (they belong in emit-results), so the
            // verb warns rather than silently discarding them; the header is still written.
            JObject runObject = MinimalRun();
            runObject["results"] = new JArray
            {
                new JObject { ["ruleId"] = "TEST0001", ["message"] = new JObject { ["text"] = "x" } },
            };

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            errWriter.ToString().Should().Contain("results[1]");
            File.Exists(WipPath).Should().BeTrue();
        }

        [Fact]
        public void Run_WhenHeaderCarriesInvocations_WarnsToStderrButStillWritesHeader()
        {
            // invocations on the run header are dropped at replay (they belong in emit-invocations),
            // so the verb warns rather than silently discarding them; the header is still written.
            JObject runObject = MinimalRun();
            runObject["invocations"] = new JArray
            {
                new JObject { ["executionSuccessful"] = true },
                new JObject { ["executionSuccessful"] = false },
            };

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            errWriter.ToString().Should().Contain("invocations[2]");
            File.Exists(WipPath).Should().BeTrue();
        }

        [Fact]
        public void Run_WhenHeaderHasNoResultsOrInvocations_DoesNotWarn()
        {
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = RunWithInput(MinimalRun());

            exit.Should().Be(CommandBase.SUCCESS);
            errWriter.ToString().Should().NotContain("ignored at replay");
        }

        [Fact]
        public void Run_WhenHeaderHasEmptyResultsArray_DoesNotWarn()
        {
            // An empty array carries no data, so it is not worth a warning.
            JObject runObject = MinimalRun();
            runObject["results"] = new JArray();

            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            int exit = RunWithInput(runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            errWriter.ToString().Should().NotContain("ignored at replay");
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

        private const string AdoRepoUriValue = "https://dev.azure.com/contoso/example/_git/example";
        private const string AdoRevisionIdValue = "0123456789abcdef0123456789abcdef01234567";

        private static FakeEnvironmentVariableGetter CompleteAdoEnvWithVcp()
            => CompleteAdoEnv()
                .With(AdoPipelineContext.RepositoryUriEnvVar, AdoRepoUriValue)
                .With(AdoPipelineContext.SourceVersionEnvVar, AdoRevisionIdValue);

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
        public void Run_WhenAdoVcpFieldsPresent_AndJsonOmitsVcp_AppendsSynthesizedEntry()
        {
            int exit = RunWithInput(CompleteAdoEnvWithVcp(), MinimalRun());

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            JToken vcp = events[0].Payload["versionControlProvenance"];
            vcp.Should().NotBeNull();
            vcp.Type.Should().Be(JTokenType.Array);
            ((JArray)vcp).Should().HaveCount(1);
            vcp[0]["repositoryUri"].ToString().Should().Be(AdoRepoUriValue);
            vcp[0]["revisionId"].ToString().Should().Be(AdoRevisionIdValue);
            vcp[0]["branch"].ToString().Should().Be("refs/heads/main");
        }

        [Fact]
        public void Run_WhenAdoVcpFieldsPresent_AndJsonHasEmptyVcpArray_AppendsSynthesizedEntry()
        {
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray();

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["repositoryUri"].ToString().Should().Be(AdoRepoUriValue);
        }

        [Fact]
        public void Run_WhenAdoVcpFieldsPresent_AndJsonHasOneEntryMissingFields_EnrichesMissing()
        {
            // Producer supplies repositoryUri only; verb fills revisionId + branch.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["repositoryUri"] = AdoRepoUriValue },
            };

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["repositoryUri"].ToString().Should().Be(AdoRepoUriValue);
            vcp[0]["revisionId"].ToString().Should().Be(AdoRevisionIdValue);
            vcp[0]["branch"].ToString().Should().Be("refs/heads/main");
        }

        [Fact]
        public void Run_WhenAdoVcpFieldsPresent_AndJsonHasEqualFields_Idempotent()
        {
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject
                {
                    ["repositoryUri"] = AdoRepoUriValue,
                    ["revisionId"] = AdoRevisionIdValue,
                    ["branch"] = "refs/heads/main",
                    ["properties"] = new JObject { ["custom"] = "preserved" },
                },
            };

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp[0]["properties"]["custom"].ToString().Should().Be("preserved");
        }

        [Fact]
        public void Run_WhenAdoVcpFieldsPresent_AndSingleEntryNamesDifferentRepo_LeavesUntouched()
        {
            // A single entry whose repositoryUri names a repo other than the pipeline's enlistment
            // is caller-authored for a different repository (the normal case for a scanner whose
            // scan target differs from the pipeline source repo). The verb must not assume it is
            // the pipeline primary repo and must neither stamp pipeline revisionId/branch onto it
            // nor fail; the entry passes through verbatim.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["repositoryUri"] = "https://github.com/microsoft/sarif-sdk" },
            };

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["repositoryUri"].ToString().Should().Be("https://github.com/microsoft/sarif-sdk");
            vcp[0]["branch"].Should().BeNull();
            vcp[0]["mappedTo"].Should().BeNull();
        }

        [Fact]
        public void Run_WhenAdoVcpFieldsPresent_AndSingleEntryForDifferentRepoHasOwnFields_PreservesThem()
        {
            // The scan-target entry carries its own revisionId/branch that diverge from the
            // pipeline's. Because the repositoryUri identifies a different repo, those values are
            // authoritative for that repo and must survive verbatim rather than colliding with the
            // pipeline values.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject
                {
                    ["repositoryUri"] = "https://github.com/microsoft/sarif-sdk",
                    ["revisionId"] = "feedfacefeedfacefeedfacefeedfacefeedface",
                    ["branch"] = "release/v5.0.2",
                },
            };

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["repositoryUri"].ToString().Should().Be("https://github.com/microsoft/sarif-sdk");
            vcp[0]["revisionId"].ToString().Should().Be("feedfacefeedfacefeedfacefeedfacefeedface");
            vcp[0]["branch"].ToString().Should().Be("release/v5.0.2");
        }

        [Fact]
        public void Run_WhenAdoVcpFieldsPresent_AndJsonHasConflictingRevisionId_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["revisionId"] = "feedfacefeedfacefeedfacefeedfacefeedface" },
            };

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WhenAdoVcpFieldsPresent_AndJsonHasConflictingBranch_Fails()
        {
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["branch"] = "release/v5.0.2" },
            };

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WhenAdoVcpFieldsPresent_AndJsonHasMultipleEntries_LeavesUntouched()
        {
            // Multi-entry: caller has declared a multi-repo shape and we refuse to guess which
            // entry names the pipeline's source repo. Both entries pass through verbatim.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject
                {
                    ["repositoryUri"] = "https://github.com/microsoft/sarif-sdk",
                    ["revisionId"] = "feedfacefeedfacefeedfacefeedfacefeedface",
                    ["branch"] = "release/v5.0.2",
                },
                new JObject
                {
                    ["repositoryUri"] = "https://github.com/microsoft/sarif-pattern-matcher",
                    ["branch"] = "main",
                },
            };

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(2);
            vcp[0]["repositoryUri"].ToString().Should().Be("https://github.com/microsoft/sarif-sdk");
            vcp[1]["repositoryUri"].ToString().Should().Be("https://github.com/microsoft/sarif-pattern-matcher");
        }

        [Fact]
        public void Run_WhenAdoVcpFieldsAbsent_DoesNotTouchVcp()
        {
            // Only the pipeline-identity vars are set (no BUILD_REPOSITORY_URI / BUILD_SOURCEVERSION);
            // VCP is untouched and the run continues to lack any versionControlProvenance entry.
            int exit = RunWithInput(CompleteAdoEnv(), MinimalRun());

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events[0].Payload["versionControlProvenance"].Should().BeNull();
        }

        [Fact]
        public void Run_WhenSrcRootDeclared_AndVcpSynthesized_BindsMappedToSourceRoot()
        {
            // The run names a local SRCROOT, so the auto-stamped source-repo entry binds to it via
            // mappedTo. emit-finalize relies on this binding to deconstruct local paths.
            JObject runObject = MinimalRun();
            runObject["originalUriBaseIds"] = new JObject
            {
                ["SRCROOT"] = new JObject { ["uri"] = new Uri(_dir).AbsoluteUri },
            };
            runObject["versionControlProvenance"] = new JArray();

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["mappedTo"]["uriBaseId"].ToString().Should().Be(EmitRunCommand.SourceRootBaseId);
        }

        [Fact]
        public void Run_WhenSrcRootDeclared_AndCallerSuppliesMappedTo_DoesNotOverride()
        {
            // A caller-authored mappedTo is authoritative; stamping fills missing fields but never
            // rebinds the source root the producer already chose.
            JObject runObject = MinimalRun();
            runObject["originalUriBaseIds"] = new JObject
            {
                ["SRCROOT"] = new JObject { ["uri"] = new Uri(_dir).AbsoluteUri },
            };
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject
                {
                    ["repositoryUri"] = AdoRepoUriValue,
                    ["mappedTo"] = new JObject { ["uriBaseId"] = "REPO_ROOT" },
                },
            };

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp[0]["mappedTo"]["uriBaseId"].ToString().Should().Be("REPO_ROOT");
        }

        [Fact]
        public void Run_WhenNoSrcRootDeclared_StampedVcpHasNoMappedTo()
        {
            // Without a declared SRCROOT there is nothing for mappedTo to resolve against, so the
            // stamped entry carries no binding rather than a dangling one.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray();

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["mappedTo"].Should().BeNull();
        }

        [Fact]
        public void Run_WhenAdoRepositoryUriHostCasingDiffers_IsNotConflict()
        {
            // URI equality treats scheme/host case-insensitively (per RFC 3986). The
            // VcpFieldValuesAgree path normalizes via Uri.TryCreate so a producer-supplied
            // host with different casing matches the detected value without conflict.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["repositoryUri"] = "https://DEV.azure.com/contoso/example/_git/example" },
            };

            int exit = RunWithInput(CompleteAdoEnvWithVcp(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
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

        private const string GhaRepoUriValue = "https://github.com/microsoft/sarif-sdk";
        private const string GhaRevisionIdValue = "fedcba9876543210fedcba9876543210fedcba98";

        private static FakeEnvironmentVariableGetter CompleteGhaEnv() => new FakeEnvironmentVariableGetter()
            .With(GitHubActionsContext.GitHubActionsEnvVar, "true")
            .With(GitHubActionsContext.ServerUrlEnvVar, "https://github.com")
            .With(GitHubActionsContext.RepositoryEnvVar, "microsoft/sarif-sdk")
            .With(GitHubActionsContext.ShaEnvVar, GhaRevisionIdValue)
            .With(GitHubActionsContext.RefEnvVar, "refs/heads/main");

        [Fact]
        public void Run_WhenGhaContextComplete_StampsVcpFromGhaEnv()
        {
            // GHA-only env (no ADO sentinels): VCP is synthesized from the GitHub Actions
            // env vars. automationDetails is NOT stamped (no GHA pipeline-identity contract
            // in scope for this verb today).
            int exit = RunWithInput(CompleteGhaEnv(), MinimalRun());

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events[0].Payload["automationDetails"].Should().BeNull();

            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["repositoryUri"].ToString().Should().Be(GhaRepoUriValue);
            vcp[0]["revisionId"].ToString().Should().Be(GhaRevisionIdValue);
            vcp[0]["branch"].ToString().Should().Be("refs/heads/main");
        }

        [Fact]
        public void Run_WhenAdoAndGhaBothCompleteWithDistinctValues_FailsCrossSource()
        {
            // Both ADO and GHA env populated with their own canonical (distinct) values;
            // the cross-source merge MUST refuse to stamp rather than guess which source
            // names the build's source repo.
            var env = CompleteAdoEnvWithVcp();
            env.With(GitHubActionsContext.GitHubActionsEnvVar, "true")
               .With(GitHubActionsContext.ServerUrlEnvVar, "https://github.com")
               .With(GitHubActionsContext.RepositoryEnvVar, "microsoft/sarif-sdk")
               .With(GitHubActionsContext.ShaEnvVar, GhaRevisionIdValue)
               .With(GitHubActionsContext.RefEnvVar, "refs/heads/main");

            int exit = RunWithInput(env, MinimalRun());

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WhenAdoAndGhaAgreeOnEveryVcpField_StampsOnce()
        {
            // Caller intentionally staged matching values across both source envs (hand-built
            // test env, or AI exercising both shapes). Agreement on every field is fine; we
            // stamp once with the merged triple.
            var env = CompleteAdoEnv()
                .With(AdoPipelineContext.RepositoryUriEnvVar, GhaRepoUriValue)
                .With(AdoPipelineContext.SourceVersionEnvVar, GhaRevisionIdValue)
                .With(GitHubActionsContext.GitHubActionsEnvVar, "true")
                .With(GitHubActionsContext.ServerUrlEnvVar, "https://github.com")
                .With(GitHubActionsContext.RepositoryEnvVar, "microsoft/sarif-sdk")
                .With(GitHubActionsContext.ShaEnvVar, GhaRevisionIdValue)
                .With(GitHubActionsContext.RefEnvVar, "refs/heads/main");

            int exit = RunWithInput(env, MinimalRun());

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["repositoryUri"].ToString().Should().Be(GhaRepoUriValue);
            vcp[0]["revisionId"].ToString().Should().Be(GhaRevisionIdValue);
            vcp[0]["branch"].ToString().Should().Be("refs/heads/main");
        }

        [Fact]
        public void Run_WhenAdoSilentOnRepoUri_GhaFillsTheGap()
        {
            // ADO publishes revision + branch only (BUILD_REPOSITORY_URI absent); GHA
            // publishes a repo URI. Lower-priority GHA fills the gap and the synthesized
            // entry uses all three fields. This is the cross-source gap-fill path.
            var env = CompleteAdoEnv()
                .With(AdoPipelineContext.SourceVersionEnvVar, AdoRevisionIdValue)
                .With(GitHubActionsContext.GitHubActionsEnvVar, "true")
                .With(GitHubActionsContext.ServerUrlEnvVar, "https://github.com")
                .With(GitHubActionsContext.RepositoryEnvVar, "microsoft/sarif-sdk");

            int exit = RunWithInput(env, MinimalRun());

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["repositoryUri"].ToString().Should().Be(GhaRepoUriValue);
            vcp[0]["revisionId"].ToString().Should().Be(AdoRevisionIdValue);
            vcp[0]["branch"].ToString().Should().Be("refs/heads/main");
        }

        [Fact]
        public void Run_WhenGhaContextPartial_FailsBeforeCreatingWip()
        {
            // Same partial-state contract as ADO: malformed GHA env vars MUST NOT produce
            // a wip on disk. Mirrors Run_WhenAdoPipelineContextPartial_FailsBeforeCreatingWip.
            File.WriteAllText(WipPath, "existing wip\n");

            var env = new FakeEnvironmentVariableGetter()
                .With(GitHubActionsContext.GitHubActionsEnvVar, "true")
                .With(GitHubActionsContext.ShaEnvVar, "not-a-sha");

            int exit = RunWithInput(env, MinimalRun(), forceOverwrite: true);

            exit.Should().Be(CommandBase.FAILURE);
            File.ReadAllText(WipPath).Should().Be("existing wip\n");
        }

        [Fact]
        public void Run_WhenGhaContextComplete_AndSingleEntryNamesDifferentRepo_LeavesUntouched()
        {
            // The single-entry VCP repositoryUri-gate applies to GHA env identically to ADO: a
            // producer-supplied repoUri that names a different repo than the detected GHA value is
            // a separate scan target, left untouched rather than reconciled or failed.
            JObject runObject = MinimalRun();
            runObject["versionControlProvenance"] = new JArray
            {
                new JObject { ["repositoryUri"] = "https://github.com/other/repo" },
            };

            int exit = RunWithInput(CompleteGhaEnv(), runObject);

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            var vcp = (JArray)events[0].Payload["versionControlProvenance"];
            vcp.Should().HaveCount(1);
            vcp[0]["repositoryUri"].ToString().Should().Be("https://github.com/other/repo");
            vcp[0]["revisionId"].Should().BeNull();
            vcp[0]["branch"].Should().BeNull();
        }

        [Fact]
        public void Run_WhenAdoAndGhaDisagreeOnRevisionId_FailsCrossSource()
        {
            // ADO and GHA both publish a revision; the values disagree. The cross-source
            // merge errors out before we touch the JSON.
            var env = CompleteAdoEnvWithVcp()
                .With(GitHubActionsContext.GitHubActionsEnvVar, "true")
                .With(GitHubActionsContext.ServerUrlEnvVar, "https://dev.azure.com")
                .With(GitHubActionsContext.RepositoryEnvVar, "contoso/example/_git/example")
                .With(GitHubActionsContext.ShaEnvVar, GhaRevisionIdValue)
                .With(GitHubActionsContext.RefEnvVar, "refs/heads/main");

            int exit = RunWithInput(env, MinimalRun());

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }
    }
}

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
    public class EmitInitRunCommandTests : IDisposable
    {
        private readonly string _dir;
        private readonly IEnvironmentVariableGetter _emptyEnv = new EmptyEnvironmentVariableGetter();

        public EmitInitRunCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"emit-init-run-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        private string OutPath => Path.Combine(_dir, "scan.sarif");
        private string WipPath => OutPath + ".wip.jsonl";

        private EmitInitRunCommand NewCommand() => new EmitInitRunCommand(_emptyEnv);

        [Fact]
        public void Run_OnCleanState_CreatesWipWithRunHeaderEvent()
        {
            int exit = NewCommand().Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
                ToolVersion = "1.0.0",
                InformationUri = "https://example.com",
            });

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(WipPath).Should().BeTrue();

            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);
            events[0].Kind.Should().Be(SarifEventKinds.RunHeader);
            events[0].Payload["tool"]["driver"]["name"].ToString().Should().Be("demo");
            events[0].Payload["tool"]["driver"]["version"].ToString().Should().Be("1.0.0");
        }

        [Fact]
        public void Run_FailsIfWipExistsAndOverwriteNotSpecified()
        {
            File.WriteAllText(WipPath, "{}\n");

            int exit = NewCommand().Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
            });

            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void Run_FailsIfSarifExistsAndOverwriteNotSpecified()
        {
            File.WriteAllText(OutPath, "{ \"version\": \"2.1.0\" }");

            int exit = NewCommand().Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
            });

            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void Run_WithForceOverwriteAndExistingWip_DeletesAndRecreates()
        {
            File.WriteAllText(WipPath, "stale wip\n");

            int exit = NewCommand().Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
                ForceOverwrite = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);
            events[0].Kind.Should().Be(SarifEventKinds.RunHeader);
        }

        [Fact]
        public void Run_FailsIfToolNameMissing()
        {
            int exit = NewCommand().Run(new EmitInitRunOptions { OutputFilePath = OutPath });
            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Theory]
        [InlineData("InformationUri", "ht!tp://not a uri")]       // malformed
        [InlineData("InformationUri", "docs/tool")]                // relative
        [InlineData("InformationUri", "http://example.com/tool")]  // disallowed scheme (https-only)
        [InlineData("InformationUri", "file:///etc/tool/docs")]    // file scheme rejected for informationUri
        [InlineData("SourceRoot", "ftp://example.com/src/")]   // disallowed scheme for --srcroot
        public void Run_FailsOnInvalidUriFlag(string slot, string value)
        {
            // EmitEventLogHelpers.TryValidateUri rejects malformed URIs, relative URIs,
            // and any scheme outside the per-slot allow-list. Each rejection must
            // surface as FAILURE *before* the wip file is created so a typo never
            // leaves a partially-initialized scan state on disk.
            var options = new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
            };
            switch (slot)
            {
                case "InformationUri": options.InformationUri = value; break;
                case "SourceRoot": options.SourceRoot = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(slot));
            }

            int exit = NewCommand().Run(options);

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void BuildRunHeader_PopulatesVersionControlProvenanceFromFlags()
        {
            Run run = EmitInitRunCommand.BuildRunHeader(new EmitInitRunOptions
            {
                ToolName = "demo",
                RepositoryUri = "https://github.com/acme/demo",
                RevisionId = "abc",
                Branch = "main",
            });

            run.VersionControlProvenance.Should().HaveCount(1);
            run.VersionControlProvenance[0].RepositoryUri.ToString().Should().Be("https://github.com/acme/demo");
            run.VersionControlProvenance[0].RevisionId.Should().Be("abc");
            run.VersionControlProvenance[0].Branch.Should().Be("main");
        }

        [Fact]
        public void BuildRunHeader_OmitsVersionControlProvenanceWhenNoFlagsSupplied()
        {
            Run run = EmitInitRunCommand.BuildRunHeader(new EmitInitRunOptions { ToolName = "demo" });
            run.VersionControlProvenance.Should().BeNull();
        }

        [Fact]
        public void BuildRunHeader_AddsSourceRootUnderSrcrootBaseId()
        {
            Run run = EmitInitRunCommand.BuildRunHeader(new EmitInitRunOptions
            {
                ToolName = "demo",
                SourceRoot = "file:///D:/work/demo",
            });

            run.OriginalUriBaseIds.Should().ContainKey("SRCROOT");
            run.OriginalUriBaseIds["SRCROOT"].Uri.ToString().Should().Be("file:///D:/work/demo");
        }

        [Fact]
        public void Run_WhenAdoPipelineContextComplete_StampsAutomationDetails()
        {
            // Wire up: env-driven Complete pipeline context flows into the on-disk run header.
            var env = new FakeEnvironmentVariableGetter()
                .With(AdoPipelineContext.TfBuildEnvVar, "True")
                .With(AdoPipelineContext.CollectionUriEnvVar, "https://dev.azure.com/contoso/")
                .With(AdoPipelineContext.TeamProjectIdEnvVar, "11111111-1111-1111-1111-111111111111")
                .With(AdoPipelineContext.BuildDefinitionIdPrimaryEnvVar, "1234")
                .With(AdoPipelineContext.BuildDefinitionNameEnvVar, "Nightly Build")
                .With(AdoPipelineContext.BuildIdEnvVar, "98765")
                .With(AdoPipelineContext.PhaseIdPrimaryEnvVar, "22222222-2222-2222-2222-222222222222")
                .With(AdoPipelineContext.PhaseNamePrimaryEnvVar, "Build")
                .With(AdoPipelineContext.SourceBranchEnvVar, "refs/heads/main");

            int exit = new EmitInitRunCommand(env).Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
            });

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);

            string id = events[0].Payload["automationDetails"]["id"].ToString();
            id.Should().Be("azuredevops/pipeline/build/contoso/11111111-1111-1111-1111-111111111111/1234/22222222-2222-2222-2222-222222222222/refs/heads/main/98765");

            events[0].Payload["automationDetails"]["properties"][AdoPipelineContext.PropBuildDefinitionId].ToString().Should().Be("1234");
            events[0].Payload["automationDetails"]["properties"][AdoPipelineContext.PropPhaseName].ToString().Should().Be("Build");
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

            int exit = new EmitInitRunCommand(env).Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
                ForceOverwrite = true,
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.ReadAllText(WipPath).Should().Be("existing wip\n");
        }

        [Fact]
        public void Run_WhenNoAdoEnvVarsSet_OmitsAutomationDetails()
        {
            int exit = new EmitInitRunCommand(_emptyEnv).Run(new EmitInitRunOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
            });

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);
            events[0].Payload["automationDetails"].Should().BeNull();
        }
    }
}

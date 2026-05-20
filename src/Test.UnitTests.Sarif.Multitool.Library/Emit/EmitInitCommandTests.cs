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
    public class EmitInitCommandTests : IDisposable
    {
        private readonly string _dir;

        public EmitInitCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"emit-init-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        private string OutPath => Path.Combine(_dir, "scan.sarif");
        private string WipPath => OutPath + ".wip.jsonl";

        [Fact]
        public void Run_OnCleanState_CreatesWipWithRunHeaderEvent()
        {
            int exit = new EmitInitCommand().Run(new EmitInitOptions
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

            int exit = new EmitInitCommand().Run(new EmitInitOptions
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

            int exit = new EmitInitCommand().Run(new EmitInitOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
            });

            exit.Should().Be(CommandBase.FAILURE);
        }

        [Fact]
        public void Run_WithAllowOverwriteAndExistingWip_DeletesAndRecreates()
        {
            File.WriteAllText(WipPath, "stale wip\n");

            int exit = new EmitInitCommand().Run(new EmitInitOptions
            {
                OutputFilePath = OutPath,
                ToolName = "demo",
                AllowOverwrite = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            var events = new SarifEventLogReader().Read(WipPath).ToList();
            events.Should().HaveCount(1);
            events[0].Kind.Should().Be(SarifEventKinds.RunHeader);
        }

        [Fact]
        public void Run_FailsIfToolNameMissing()
        {
            int exit = new EmitInitCommand().Run(new EmitInitOptions { OutputFilePath = OutPath });
            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(WipPath).Should().BeFalse();
        }

        [Fact]
        public void BuildRunHeader_PopulatesVersionControlProvenanceFromFlags()
        {
            Run run = EmitInitCommand.BuildRunHeader(new EmitInitOptions
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
            Run run = EmitInitCommand.BuildRunHeader(new EmitInitOptions { ToolName = "demo" });
            run.VersionControlProvenance.Should().BeNull();
        }

        [Fact]
        public void BuildRunHeader_AddsSourceRootUnderSrcrootBaseId()
        {
            Run run = EmitInitCommand.BuildRunHeader(new EmitInitOptions
            {
                ToolName = "demo",
                SourceRoot = "file:///D:/work/demo",
            });

            run.OriginalUriBaseIds.Should().ContainKey("SRCROOT");
            run.OriginalUriBaseIds["SRCROOT"].Uri.ToString().Should().Be("file:///D:/work/demo");
        }
    }
}

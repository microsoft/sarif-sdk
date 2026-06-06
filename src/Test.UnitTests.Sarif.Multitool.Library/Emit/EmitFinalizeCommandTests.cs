// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class EmitFinalizeCommandTests : IDisposable
    {
        private readonly string _dir;

        public EmitFinalizeCommandTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"emit-finalize-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir)) { Directory.Delete(_dir, recursive: true); }
        }

        private string OutPath => Path.Combine(_dir, "scan.sarif");
        private string WipPath => OutPath + ".wip.jsonl";

        private void SeedWip(params (string kind, object payload)[] events)
        {
            using var w = new SarifEventLogWriter(WipPath);
            foreach ((string kind, object payload) in events) { w.Append(kind, payload); }
        }

        private SarifLog LoadSarif()
        {
            using var sr = new StreamReader(OutPath);
            using var jr = new JsonTextReader(sr);
            return JsonSerializer.CreateDefault().Deserialize<SarifLog>(jr);
        }

        private const string FrozenSha = "1234567890abcdef1234567890abcdef12345678";

        // emit-finalize now requires every run to declare versionControlProvenance with a
        // mappedTo-bound local root so it can deconstruct local paths into portable permalinks.
        private static Run RunHeader()
            => new Run
            {
                Tool = new Tool { Driver = new ToolComponent { Name = "demo" } },
                VersionControlProvenance = new[]
                {
                    new VersionControlDetails
                    {
                        RepositoryUri = new Uri("https://github.com/microsoft/sarif-sdk", UriKind.Absolute),
                        RevisionId = FrozenSha,
                        Branch = "refs/heads/main",
                        MappedTo = new ArtifactLocation { UriBaseId = "SRCROOT" },
                    },
                },
                OriginalUriBaseIds = new System.Collections.Generic.Dictionary<string, ArtifactLocation>
                {
                    ["SRCROOT"] = new ArtifactLocation { Uri = new Uri("file:///d:/repo/", UriKind.Absolute) },
                },
            };

        [Fact]
        public void Run_HappyPath_WritesSarifWithEnrichedCweDescriptorsAndRemovesWip()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }),
                (SarifEventKinds.Result, new Result { RuleId = "NOVEL-custom", Message = new Message { Text = "n/a" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(OutPath).Should().BeTrue();
            File.Exists(WipPath).Should().BeFalse();

            SarifLog log = LoadSarif();
            log.Runs[0].Tool.Driver.Rules.Should().HaveCount(2);
            log.Runs[0].Tool.Driver.Rules[0].Id.Should().Be("CWE-79");
            log.Runs[0].Tool.Driver.Rules[0].HelpUri.Should().NotBeNull();
            log.Runs[0].Tool.Driver.Rules[0].Name.Should().NotBeNullOrEmpty();
            log.Runs[0].Tool.Driver.Rules[1].Id.Should().Be("NOVEL-custom");
            log.Runs[0].Tool.Driver.Rules[1].HelpUri.Should().BeNull();
        }

        [Fact]
        public void Run_FailsIfWipDoesNotExist()
        {
            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });
            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(OutPath).Should().BeFalse();
        }

        [Fact]
        public void Run_WithNoCweEnrichment_LeavesCweDescriptorBare()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79/template-xss", Message = new Message { Text = "xss" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                NoCweEnrichment = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            SarifLog log = LoadSarif();
            ReportingDescriptor descriptor = log.Runs[0].Tool.Driver.Rules[0];
            descriptor.Id.Should().Be("CWE-79");
            descriptor.HelpUri.Should().BeNull();
            descriptor.Name.Should().BeNull();
        }

        [Fact]
        public void Run_WithKeepWip_RetainsWipAfterSuccess()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                KeepWip = true,
            });

            exit.Should().Be(CommandBase.SUCCESS);
            File.Exists(WipPath).Should().BeTrue();
            File.Exists(OutPath).Should().BeTrue();
        }

        [Fact]
        public void Run_AtomicallyReplacesExistingSarif()
        {
            // Pre-existing SARIF on disk should be replaced wholesale, not appended to.
            File.WriteAllText(OutPath, "{ \"stale\": true }");

            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result { RuleId = "NOVEL-rule-1", Message = new Message { Text = "x" } }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            string contents = File.ReadAllText(OutPath);
            contents.Should().NotContain("stale");
            contents.Should().Contain("NOVEL-rule-1");
        }
        [Fact]
        public void Run_RejectsNonCompliantRuleId_WritesAIRuleIdEnvelopeToStderr()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
                (SarifEventKinds.Result, new Result { RuleId = "CWE-79", Message = new Message { Text = "xss" } }));

            string capturedStderr;
            int exit;
            using (var writer = new StringWriter())
            {
                TextWriter original = Console.Error;
                Console.SetError(writer);
                try
                {
                    exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });
                }
                finally
                {
                    Console.SetError(original);
                }
                capturedStderr = writer.ToString();
            }

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(OutPath).Should().BeFalse();
            capturedStderr.Should().Contain(AIRuleIdConventionException.ErrorCode);
            capturedStderr.Should().Contain("'CWE-79'");
            capturedStderr.Should().NotContain("at Microsoft.CodeAnalysis.Sarif", "the catch block should write the envelope, not a stack trace");
        }

        [Fact]
        public void Run_RebasesSrcRootToPortableGitHubPermalink()
        {
            // Producer emits with a local file:// SRCROOT so InsertOptionalDataVisitor can
            // resolve sources; finalize deconstructs that local anchor into a portable GitHub
            // blob permalink derived from versionControlProvenance. The shipped artifact must
            // carry the post-rebase value and keep result URIs relative.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()),
                (SarifEventKinds.Result, new Result
                {
                    RuleId = "NOVEL-test",
                    Message = new Message { Text = "x" },
                    Locations = new[]
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    Uri = new Uri("src/a.cs", UriKind.Relative),
                                    UriBaseId = "SRCROOT",
                                },
                            },
                        },
                    },
                }));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });

            exit.Should().Be(CommandBase.SUCCESS);
            SarifLog log = LoadSarif();
            log.Runs[0].OriginalUriBaseIds.Should().ContainKey("SRCROOT");
            log.Runs[0].OriginalUriBaseIds["SRCROOT"].Uri
                .Should().Be(new Uri($"https://github.com/microsoft/sarif-sdk/blob/{FrozenSha}/", UriKind.Absolute));

            ArtifactLocation shipped = log.Runs[0].Results[0].Locations[0].PhysicalLocation.ArtifactLocation;
            shipped.Uri.OriginalString.Should().Be("src/a.cs");
            shipped.UriBaseId.Should().Be("SRCROOT");
        }

        [Fact]
        public void Run_FailsWhenRunHasNoVersionControlProvenance()
        {
            // The finalize contract requires versionControlProvenance so local paths can be
            // rebased to portable permalinks; a run without it is refused before any file ships.
            SeedWip(
                (SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
                (SarifEventKinds.Result, new Result { RuleId = "NOVEL-test", Message = new Message { Text = "x" } }));

            string capturedStderr;
            int exit;
            using (var writer = new StringWriter())
            {
                TextWriter original = Console.Error;
                Console.SetError(writer);
                try
                {
                    exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions { OutputFilePath = OutPath });
                }
                finally
                {
                    Console.SetError(original);
                }
                capturedStderr = writer.ToString();
            }

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(OutPath).Should().BeFalse();
            capturedStderr.Should().Contain("versionControlProvenance");
        }

        [Fact]
        public void Run_WithValidateFlag_ReturnsFailureWhenErrorFindingsPresent()
        {
            // A run that carries versionControlProvenance (so finalize can rebase) but is
            // otherwise bare of AI-profile metadata fires several AI* error-level findings
            // (AI1006 missing ai/origin, automationDetails, etc.). The --validate gate should
            // propagate this as a FAILURE exit code and leave the report file on disk for
            // forensics. The clean-input success path is covered with higher fidelity by the
            // CweGenerateSample.ps1 + CweSample.sarif integration fixture.
            SeedWip(
                (SarifEventKinds.RunHeader, RunHeader()));

            int exit = new EmitFinalizeCommand().Run(new EmitFinalizeOptions
            {
                OutputFilePath = OutPath,
                Validate = true,
            });

            exit.Should().Be(CommandBase.FAILURE);
            File.Exists(OutPath).Should().BeTrue();
            File.Exists(Path.Combine(_dir, "scan.validate-report.sarif")).Should().BeTrue();
        }
    }
}

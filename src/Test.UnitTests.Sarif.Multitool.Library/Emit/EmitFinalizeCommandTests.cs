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

        [Fact]
        public void Run_HappyPath_WritesSarifWithEnrichedCweDescriptorsAndRemovesWip()
        {
            SeedWip(
                (SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
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
                (SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
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
                (SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }));

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
                (SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } }),
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
    }
}

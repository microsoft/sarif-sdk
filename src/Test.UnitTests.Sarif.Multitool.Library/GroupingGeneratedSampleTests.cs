// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Gates for the checked-in finding-grouping worked sample
    /// (docs/ai/samples/grouping-findings/). The sample is the two-tier
    /// generated/synthesized log described in docs/ai/grouping-findings.md, and
    /// it doubles as an end-to-end integration test of multi-run
    /// <c>emit-finalize --inputs</c>.
    ///
    /// Two gates, layered:
    ///
    ///   1. <see cref="GroupingSample_ValidatesCleanUnderSarifAndAi"/> - an
    ///      always-on, in-process check that the checked-in fixture reports zero
    ///      Error- or Warning-level results under --rule-kind Sarif;AI (the bar
    ///      emit-finalize --validate itself gates on). This needs no external
    ///      tooling, so it runs on every CI image.
    ///   2. <see cref="GroupingSample_IsByteIdenticalToGenerateScriptOutput"/> -
    ///      the fidelity gate: re-runs GroupingGenerateSample.ps1 and asserts the
    ///      fixture is byte-identical to genuine multitool emit output. A
    ///      published "Sample" is a teacher; this proves it is exactly what the
    ///      verbs emit, not a hand-authored approximation. Soft-skips where pwsh
    ///      or the built multitool are unavailable.
    /// </summary>
    public class GroupingGeneratedSampleTests
    {
        private const string ScriptRelativePath = @"docs/ai/samples/grouping-findings/GroupingGenerateSample.ps1";
        private const string FixtureRelativePath = @"docs/ai/samples/grouping-findings/GroupingSample.sarif";

        private readonly ITestOutputHelper _output;

        public GroupingGeneratedSampleTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GroupingSample_ValidatesCleanUnderSarifAndAi()
        {
            string repoRoot = FindRepoRoot(Path.GetDirectoryName(typeof(GroupingGeneratedSampleTests).Assembly.Location));
            repoRoot.Should().NotBeNull("the test must run from within the repository working tree");

            string fixturePath = Path.Combine(repoRoot, FixtureRelativePath);
            File.Exists(fixturePath).Should().BeTrue($"the checked-in fixture must exist at '{fixturePath}'");

            string outputPath = Path.Combine(
                Path.GetTempPath(),
                "grouping-sample-validation-" + Guid.NewGuid().ToString("N") + ".sarif");

            try
            {
                var options = new ValidateOptions
                {
                    TargetFileSpecifiers = new[] { fixturePath },
                    OutputFilePath = outputPath,
                    OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    RuleKindOption = new List<RuleKind> { RuleKind.Sarif, RuleKind.AI },
                    Kind = new List<ResultKind> { ResultKind.Fail },

                    // emit-finalize --validate gates a sample at Error and Warning;
                    // Note-level rules (e.g. AI2012 'ai/handoff', SARIF2006 URL
                    // reachability) are advisory and legitimately fire on a minimal,
                    // placeholder-URI synthetic sample. We assert the same bar the
                    // producing verb does so the fixture stays pure verb output.
                    Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error },
                };

                var context = new SarifValidationContext { FileSystem = FileSystem.Instance };
                int returnCode = new ValidateCommand().Run(options, ref context);

                (context.RuntimeErrors & ~RuntimeConditions.Nonfatal).Should().Be(
                    RuntimeConditions.None,
                    "the worked sample must validate without any fatal condition");
                returnCode.Should().Be(0);

                SarifLog validationLog = SarifLog.Load(outputPath);
                List<Result> results = validationLog.Runs
                    .Where(r => r.Results != null)
                    .SelectMany(r => r.Results)
                    .ToList();

                results.Should().BeEmpty(
                    "the worked grouping sample must report zero Error- or Warning-level results under Sarif;AI, but the validator emitted: " +
                    string.Join("; ", results.Select(r => $"{r.RuleId} ({r.Level}): {r.Message?.Text}")));
            }
            finally
            {
                File.Delete(outputPath);
            }
        }

        [Fact]
        public void GroupingSample_IsByteIdenticalToGenerateScriptOutput()
        {
            string pwsh = FindOnPath("pwsh") ?? FindOnPath("pwsh.exe");
            if (pwsh == null)
            {
                _output.WriteLine("pwsh not found on PATH; skipping grouping sample regeneration gate.");
                return;
            }

            string testAssemblyDirectory = Path.GetDirectoryName(typeof(GroupingGeneratedSampleTests).Assembly.Location);
            string repoRoot = FindRepoRoot(testAssemblyDirectory);
            if (repoRoot == null)
            {
                _output.WriteLine($"Could not locate repo root from '{testAssemblyDirectory}'; skipping.");
                return;
            }

            string scriptPath = Path.Combine(repoRoot, ScriptRelativePath);
            string fixturePath = Path.Combine(repoRoot, FixtureRelativePath);
            File.Exists(scriptPath).Should().BeTrue($"the generator script must exist at '{scriptPath}'");
            File.Exists(fixturePath).Should().BeTrue($"the checked-in fixture must exist at '{fixturePath}'");

            string configuration = InferConfigurationFromAssemblyPath(testAssemblyDirectory) ?? "Release";
            string multitoolPath = Path.Combine(
                repoRoot, "bld", "bin", $"AnyCPU_{configuration}", "Sarif.Multitool", "net8.0", "Sarif.Multitool.dll");
            if (!File.Exists(multitoolPath))
            {
                _output.WriteLine($"Sarif.Multitool.dll not found at '{multitoolPath}'; skipping (build the multitool to enable this gate).");
                return;
            }

            byte[] expectedBytes = File.ReadAllBytes(fixturePath);
            string expectedHash = ComputeSha256(expectedBytes);

            int exitCode = RunPwsh(pwsh, scriptPath, new[] { "-Configuration", configuration }, out string stdout, out string stderr);
            if (exitCode != 0)
            {
                Assert.Fail(
                    $"GroupingGenerateSample.ps1 exited with code {exitCode}.{Environment.NewLine}" +
                    $"stdout:{Environment.NewLine}{stdout}{Environment.NewLine}" +
                    $"stderr:{Environment.NewLine}{stderr}");
            }

            byte[] actualBytes = File.ReadAllBytes(fixturePath);
            string actualHash = ComputeSha256(actualBytes);

            if (expectedHash != actualHash)
            {
                Assert.Fail(
                    $"{Path.GetFileName(fixturePath)} drifted from GroupingGenerateSample.ps1 output. The working tree now " +
                    $"contains the regenerated fixture - review with `git diff {FixtureRelativePath}` and commit alongside " +
                    "the source change if intended, or `git checkout` to revert." + Environment.NewLine +
                    $"Expected (checked-in) sha256: {expectedHash}" + Environment.NewLine +
                    $"Actual   (regenerated) sha256: {actualHash}" + Environment.NewLine +
                    $"Expected length: {expectedBytes.Length} bytes; Actual length: {actualBytes.Length} bytes.");
            }
        }

        private static string ComputeSha256(byte[] bytes)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) { sb.Append(b.ToString("x2")); }
            return sb.ToString();
        }

        private static int RunPwsh(string pwshPath, string scriptPath, string[] args, out string stdout, out string stderr)
        {
            var psi = new ProcessStartInfo
            {
                FileName = pwshPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("-NoProfile");
            psi.ArgumentList.Add("-NonInteractive");
            psi.ArgumentList.Add("-File");
            psi.ArgumentList.Add(scriptPath);
            foreach (string a in args) { psi.ArgumentList.Add(a); }

            using var process = new Process { StartInfo = psi };
            var stdoutBuilder = new StringBuilder();
            var stderrBuilder = new StringBuilder();
            process.OutputDataReceived += (_, e) => { if (e.Data != null) { stdoutBuilder.AppendLine(e.Data); } };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) { stderrBuilder.AppendLine(e.Data); } };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            stdout = stdoutBuilder.ToString();
            stderr = stderrBuilder.ToString();
            return process.ExitCode;
        }

        private static string FindOnPath(string fileName)
        {
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv)) { return null; }
            foreach (string entry in pathEnv.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(entry)) { continue; }
                string candidate;
                try { candidate = Path.Combine(entry, fileName); }
                catch (ArgumentException) { continue; }
                if (File.Exists(candidate)) { return candidate; }
            }
            return null;
        }

        private static string FindRepoRoot(string startDirectory)
        {
            var dir = new DirectoryInfo(startDirectory);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")) ||
                    File.Exists(Path.Combine(dir.FullName, "Sarif.Sdk.sln")))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            return null;
        }

        private static string InferConfigurationFromAssemblyPath(string assemblyDirectory)
        {
            var dir = new DirectoryInfo(assemblyDirectory);
            while (dir != null)
            {
                if (dir.Name.StartsWith("AnyCPU_", StringComparison.OrdinalIgnoreCase))
                {
                    return dir.Name.Substring("AnyCPU_".Length);
                }
                dir = dir.Parent;
            }
            return null;
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Taxonomies
{
    /// <summary>
    /// Regression gate for the checked-in CWE taxonomy sample.
    /// CweSample.sarif ships next to CweGenerateSample.ps1 so reviewers can see
    /// the shape of a fully-enriched CWE log without building anything. This
    /// test hashes the fixture, re-runs the generator script (which overwrites
    /// the fixture in-place), and re-hashes. A drift means an output-shape
    /// change hit the emit chain, the visitor enrichment, the CWE enricher,
    /// or SampleCode.cs. The working tree now carries the regenerated fixture;
    /// review with <c>git diff src/Sarif/Taxonomies/CweSample.sarif</c>, commit
    /// alongside the source change if intended, or <c>git checkout</c> to
    /// revert.
    /// </summary>
    public class CweGeneratedSampleTests
    {
        private const string ScriptRelativePath = @"src/Sarif/Taxonomies/CweGenerateSample.ps1";
        private const string FixtureRelativePath = @"src/Sarif/Taxonomies/CweSample.sarif";

        private readonly ITestOutputHelper _output;

        public CweGeneratedSampleTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CweSample_Sarif_IsByteIdenticalToCweGenerateSampleScriptOutput()
        {
            // The script is PowerShell. CI images that lack pwsh on PATH can
            // skip without failing the build; the fixture stays guarded by
            // every developer who does have pwsh locally.
            string pwsh = FindOnPath("pwsh") ?? FindOnPath("pwsh.exe");
            if (pwsh == null)
            {
                _output.WriteLine("pwsh not found on PATH; skipping CweSample fixture regression test.");
                return;
            }

            string testAssemblyDirectory = Path.GetDirectoryName(typeof(CweGeneratedSampleTests).Assembly.Location);
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

            // The script needs the multitool DLL built into the same bld/bin
            // layout the test assembly itself was loaded from. If the build
            // skipped Sarif.Multitool we soft-skip rather than fail; this test
            // is a fixture guard, not a build-output guard.
            string multitoolPath = Path.Combine(Path.GetDirectoryName(testAssemblyDirectory) ?? string.Empty, "..", "Sarif.Multitool", "net8.0", "Sarif.Multitool.dll");
            multitoolPath = Path.GetFullPath(multitoolPath);
            if (!File.Exists(multitoolPath))
            {
                _output.WriteLine($"Sarif.Multitool.dll not found at '{multitoolPath}'; skipping (build the multitool to enable this gate).");
                return;
            }

            string configuration = InferConfigurationFromAssemblyPath(testAssemblyDirectory) ?? "Release";

            string expectedHash = ComputeSha256(fixturePath);

            int exitCode = RunPwsh(
                pwsh,
                scriptPath,
                new[] { "-Configuration", configuration },
                out string stdout,
                out string stderr);

            if (exitCode != 0)
            {
                Assert.Fail(
                    $"CweGenerateSample.ps1 exited with code {exitCode}.{Environment.NewLine}" +
                    $"stdout:{Environment.NewLine}{stdout}{Environment.NewLine}" +
                    $"stderr:{Environment.NewLine}{stderr}");
            }

            string actualHash = ComputeSha256(fixturePath);

            if (expectedHash != actualHash)
            {
                Assert.Fail(
                    "CweSample.sarif drifted from CweGenerateSample.ps1 output. The working tree now contains " +
                    "the regenerated fixture — review with `git diff src/Sarif/Taxonomies/CweSample.sarif` and " +
                    "commit alongside the source change if intended, or `git checkout` to revert." + Environment.NewLine +
                    $"Expected (checked-in) sha256: {expectedHash}" + Environment.NewLine +
                    $"Actual   (regenerated) sha256: {actualHash}");
            }
        }

        private static string ComputeSha256(string path)
        {
            using var sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            byte[] hash = sha.ComputeHash(stream);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
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
            foreach (string a in args)
            {
                psi.ArgumentList.Add(a);
            }

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
                try
                {
                    candidate = Path.Combine(entry, fileName);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            return null;
        }

        private static string FindRepoRoot(string startDirectory)
        {
            DirectoryInfo dir = new DirectoryInfo(startDirectory);
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
            // assemblyDirectory looks like .../bld/bin/AnyCPU_Release/Test.UnitTests.Sarif/net8.0
            // Walk up looking for an "AnyCPU_<Config>" segment.
            DirectoryInfo dir = new DirectoryInfo(assemblyDirectory);
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

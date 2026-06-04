// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
    /// Regression gate for the checked-in CWE taxonomy samples.
    /// CweSample.sarif (default, AI-shape) and CweGHAzDoSample.sarif (the
    /// -GHAzDO variant, AI + ADO pipeline ingestion shape) ship next to
    /// CweGenerateSample.ps1 so reviewers can see the shape of a fully-
    /// enriched CWE log without building anything. Each test re-runs the
    /// generator in the relevant mode (which overwrites the fixture
    /// in-place) and asserts it's byte-identical to what's checked in.
    /// A drift means an output-shape change hit the emit chain, the
    /// visitor enrichment, the CWE enricher, or SampleCode.cs. The
    /// working tree now carries the regenerated fixture; review with
    /// <c>git diff src/Sarif/Taxonomies/</c>, commit alongside the
    /// source change if intended, or <c>git checkout</c> to revert.
    /// </summary>
    public class CweGeneratedSampleTests
    {
        private const string ScriptRelativePath = @"src/Sarif/Taxonomies/CweGenerateSample.ps1";
        private const string DefaultFixtureRelativePath = @"src/Sarif/Taxonomies/CweSample.sarif";
        private const string GHAzDoFixtureRelativePath = @"src/Sarif/Taxonomies/CweGHAzDoSample.sarif";

        private readonly ITestOutputHelper _output;

        public CweGeneratedSampleTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CweSample_Sarif_IsByteIdenticalToCweGenerateSampleScriptOutput()
        {
            VerifyFixtureIsByteIdenticalToScriptOutput(
                extraScriptArgs: Array.Empty<string>(),
                fixtureRelativePath: DefaultFixtureRelativePath);
        }

        [Fact]
        public void CweGHAzDoSample_Sarif_IsByteIdenticalToCweGenerateSampleScriptOutput()
        {
            VerifyFixtureIsByteIdenticalToScriptOutput(
                extraScriptArgs: new[] { "-GHAzDO" },
                fixtureRelativePath: GHAzDoFixtureRelativePath);
        }

        [Fact]
        public void CweGHAzDoSample_RegenerationSucceeds_WhenAmbientAdoFallbackEnvVarsConflict()
        {
            // Regression gate for the mseng pipeline break: a real ADO agent
            // injects SYSTEM_DEFINITIONID (the fallback for BUILD_DEFINITIONID)
            // with the genuine pipeline id, which disagrees with the fixed
            // '1234' the script stamps on BUILD_DEFINITIONID. Without the
            // script also overriding the fallback, AdoPipelineContext.TryDetect
            // raises "disagrees with" and emit-run exits non-zero before
            // any fixture bytes are written. Set conflicting values for every
            // fallback env var the verb reads to ensure the script scrubs
            // them all.
            VerifyScriptIsIsolatedFromAmbientFallbackEnv(
                new Dictionary<string, string>
                {
                    { "SYSTEM_DEFINITIONID", "9978" },
                    { "SYSTEM_JOBID", "00000000-0000-0000-0000-deadbeefdead" },
                    { "SYSTEM_JOBNAME", "AmbientAgentJob" },
                });
        }

        [Fact]
        public void CweGHAzDoSample_RegenerationSucceeds_WhenAmbientGitHubActionsEnvVarsConflict()
        {
            // Regression gate for the macOS CI break that surfaced when
            // GitHubActionsContext landed: a GitHub Actions runner sets
            // GITHUB_ACTIONS=true + GITHUB_SHA=<real commit sha>; without the
            // script scrubbing those, GitHubActionsContext.TryDetect returns
            // Complete with a revisionId that conflicts with the zero-SHA the
            // script writes into the supplied VCP entry, and emit-run
            // exits non-zero before any fixture bytes are written. Set
            // conflicting values for every GHA env var the verb reads.
            VerifyScriptIsIsolatedFromAmbientFallbackEnv(
                new Dictionary<string, string>
                {
                    { "GITHUB_ACTIONS", "true" },
                    { "GITHUB_SERVER_URL", "https://github.example.com" },
                    { "GITHUB_REPOSITORY", "ambient/other" },
                    { "GITHUB_SHA", "deadbeefdeadbeefdeadbeefdeadbeefdeadbeef" },
                    { "GITHUB_REF_NAME", "feature/ambient" },
                    { "GITHUB_REF", "refs/heads/feature/ambient" },
                });
        }

        private void VerifyScriptIsIsolatedFromAmbientFallbackEnv(IDictionary<string, string> conflictingEnv)
        {
            var saved = new Dictionary<string, string>();
            try
            {
                foreach (KeyValuePair<string, string> kvp in conflictingEnv)
                {
                    saved[kvp.Key] = Environment.GetEnvironmentVariable(kvp.Key);
                    Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
                }

                VerifyFixtureIsByteIdenticalToScriptOutput(
                    extraScriptArgs: new[] { "-GHAzDO" },
                    fixtureRelativePath: GHAzDoFixtureRelativePath);
            }
            finally
            {
                foreach (KeyValuePair<string, string> kvp in saved)
                {
                    Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
                }
            }
        }

        private void VerifyFixtureIsByteIdenticalToScriptOutput(
            string[] extraScriptArgs,
            string fixtureRelativePath)
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
            string fixturePath = Path.Combine(repoRoot, fixtureRelativePath);
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

            byte[] expectedBytes = File.ReadAllBytes(fixturePath);
            string expectedHash = ComputeSha256(expectedBytes);

            var scriptArgs = new List<string> { "-Configuration", configuration };
            scriptArgs.AddRange(extraScriptArgs);

            int exitCode = RunPwsh(
                pwsh,
                scriptPath,
                scriptArgs.ToArray(),
                out string stdout,
                out string stderr);

            if (exitCode != 0)
            {
                Assert.Fail(
                    $"CweGenerateSample.ps1 (args: {string.Join(" ", scriptArgs)}) exited with code {exitCode}.{Environment.NewLine}" +
                    $"stdout:{Environment.NewLine}{stdout}{Environment.NewLine}" +
                    $"stderr:{Environment.NewLine}{stderr}");
            }

            byte[] actualBytes = File.ReadAllBytes(fixturePath);
            string actualHash = ComputeSha256(actualBytes);

            if (expectedHash != actualHash)
            {
                Assert.Fail(
                    $"{Path.GetFileName(fixturePath)} drifted from CweGenerateSample.ps1 output. The working tree now contains " +
                    $"the regenerated fixture — review with `git diff {fixtureRelativePath}` and " +
                    "commit alongside the source change if intended, or `git checkout` to revert." + Environment.NewLine +
                    $"Expected (checked-in) sha256: {expectedHash}" + Environment.NewLine +
                    $"Actual   (regenerated) sha256: {actualHash}" + Environment.NewLine +
                    DescribeDrift(expectedBytes, actualBytes));
            }
        }

        /// <summary>
        /// Produces a precise, CI-log-friendly diagnostic of how two byte streams differ.
        /// Reports total sizes, line-ending counts, the first divergent byte offset with
        /// a hex+ASCII window of surrounding context, and the line numbers + content of
        /// the first few diverging lines. Designed to make cross-platform fixture drift
        /// (e.g., line-ending or Unicode-escape differences) immediately visible in the
        /// xUnit test output without requiring a separate local repro.
        /// </summary>
        private static string DescribeDrift(byte[] expected, byte[] actual)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== Drift diagnostic ===");
            sb.AppendLine($"Expected length: {expected.Length} bytes");
            sb.AppendLine($"Actual   length: {actual.Length} bytes");
            sb.AppendLine($"Expected line endings: CRLF={CountCrlf(expected)} LF-only={CountLfOnly(expected)}");
            sb.AppendLine($"Actual   line endings: CRLF={CountCrlf(actual)} LF-only={CountLfOnly(actual)}");

            int firstDiff = -1;
            int min = Math.Min(expected.Length, actual.Length);
            for (int i = 0; i < min; i++)
            {
                if (expected[i] != actual[i]) { firstDiff = i; break; }
            }
            if (firstDiff == -1 && expected.Length != actual.Length) { firstDiff = min; }

            if (firstDiff >= 0)
            {
                sb.AppendLine();
                sb.AppendLine($"First divergence at byte offset {firstDiff} (0x{firstDiff:X}).");
                int contextStart = Math.Max(0, firstDiff - 32);
                int contextEnd = Math.Min(Math.Max(expected.Length, actual.Length), firstDiff + 32);
                sb.AppendLine("Expected window (hex / ASCII):");
                sb.AppendLine(HexDumpWindow(expected, contextStart, contextEnd));
                sb.AppendLine("Actual   window (hex / ASCII):");
                sb.AppendLine(HexDumpWindow(actual, contextStart, contextEnd));
            }

            // Line-level diff. Decode both as UTF-8 and walk lines; report the first
            // few diverging line pairs so a reviewer can see, e.g., "\u2014" vs "—"
            // or property-order swaps in plain text.
            string expectedText;
            string actualText;
            try
            {
                expectedText = new UTF8Encoding(false, true).GetString(expected);
                actualText = new UTF8Encoding(false, true).GetString(actual);
            }
            catch (DecoderFallbackException)
            {
                sb.AppendLine();
                sb.AppendLine("Skipped line-level diff: byte stream is not valid UTF-8.");
                return sb.ToString();
            }

            string[] expectedLines = expectedText.Split('\n');
            string[] actualLines = actualText.Split('\n');
            sb.AppendLine();
            sb.AppendLine($"Expected line count: {expectedLines.Length}");
            sb.AppendLine($"Actual   line count: {actualLines.Length}");

            const int maxLineReports = 5;
            int reported = 0;
            int common = Math.Min(expectedLines.Length, actualLines.Length);
            for (int i = 0; i < common && reported < maxLineReports; i++)
            {
                if (!string.Equals(expectedLines[i], actualLines[i], StringComparison.Ordinal))
                {
                    sb.AppendLine();
                    sb.AppendLine($"Line {i + 1} differs:");
                    sb.AppendLine($"  expected: {Truncate(expectedLines[i], 240)}");
                    sb.AppendLine($"  actual:   {Truncate(actualLines[i], 240)}");
                    reported++;
                }
            }
            if (reported == 0 && expectedLines.Length != actualLines.Length)
            {
                sb.AppendLine();
                sb.AppendLine("Lines differ only at tail; counts above show which side has extra lines.");
            }

            return sb.ToString();
        }

        private static int CountCrlf(byte[] bytes)
        {
            int count = 0;
            for (int i = 1; i < bytes.Length; i++)
            {
                if (bytes[i] == 0x0A && bytes[i - 1] == 0x0D) { count++; }
            }
            return count;
        }

        private static int CountLfOnly(byte[] bytes)
        {
            int count = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0x0A && (i == 0 || bytes[i - 1] != 0x0D)) { count++; }
            }
            return count;
        }

        private static string HexDumpWindow(byte[] bytes, int start, int end)
        {
            var sb = new StringBuilder();
            for (int rowStart = start; rowStart < end; rowStart += 16)
            {
                int rowEnd = Math.Min(rowStart + 16, end);
                sb.Append($"  {rowStart:X8}  ");
                for (int i = rowStart; i < rowEnd; i++)
                {
                    if (i < bytes.Length)
                    {
                        sb.Append(bytes[i].ToString("x2"));
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append("   ");
                    }
                }
                // Pad to align ASCII gutter
                for (int i = rowEnd; i < rowStart + 16; i++) { sb.Append("   "); }
                sb.Append(' ');
                for (int i = rowStart; i < rowEnd; i++)
                {
                    if (i < bytes.Length)
                    {
                        byte b = bytes[i];
                        sb.Append((b >= 0x20 && b < 0x7F) ? (char)b : '.');
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string Truncate(string s, int max)
        {
            if (s.Length <= max) { return s; }
            return s.Substring(0, max) + "…[truncated]";
        }

        private static string ComputeSha256(byte[] bytes)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(bytes);
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

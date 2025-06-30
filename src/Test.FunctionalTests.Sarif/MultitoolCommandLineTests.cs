// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO;

using FluentAssertions;

#if DEBUG
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis.Sarif.Driver;
#endif

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class MultitoolCommandLineTests
    {
        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1487")]
        public void Multitool_LaunchesAndRunsSuccessfully()
        {
            string multitoolPath = Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    @"..\..\Sarif.Multitool\net8.0\Sarif.Multitool.exe"));

            var startInfo = new ProcessStartInfo(multitoolPath, @"validate v2\ConverterTestData\ContrastSecurity\WebGoat.xml.sarif")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();

                process.ExitCode.Should().Be(0);
            }
        }

#if DEBUG
        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void Multitool_LaunchesAndRunsSuccessfully_WithNumberOfFilesExceedingChannelCapacity()
        {
            using var assertionScope = new AssertionScope();

            string multitoolPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),
                @"..\..\Sarif.Multitool\net8.0\Sarif.Multitool.exe"));

            string directoryPath = Path.Combine(Path.GetTempPath(), "SarifMultitoolTestFilesWithNumberOfFilesExceedingChannelCapacity");

            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            Directory.CreateDirectory(directoryPath);

            int fileCount = OrderedFileSpecifier.ChannelCapacity + 1;

            for (int i = 1; i <= fileCount; i++)
            {
                string filename = $"file_{i}.txt";
                string filepath = Path.Combine(directoryPath, filename);
                File.WriteAllText(filepath, " ");
            }

            var startInfo = new ProcessStartInfo(multitoolPath, $@"analyze-test {directoryPath}\*")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(startInfo))
            {
                var timer = new System.Timers.Timer(30000); // 30 seconds.
                timer.Elapsed += (sender, e) =>
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                };
                timer.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                timer.Stop();
                process.ExitCode.Should().Be(0);
                output.Should().Contain(
                    $"Done. {fileCount:n0} files scanned.",
                    $"analyzing {fileCount:n0} small files should not result in freezing and should finish within 30 seconds, " +
                    "typically completing in just 5 seconds");
            }

            Directory.Delete(directoryPath, true);
        }
#endif
    }
}

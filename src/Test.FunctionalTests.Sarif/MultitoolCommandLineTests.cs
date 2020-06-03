// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.FunctionalTests.Sarif
{
    public class MultitoolCommandLineTests
    {
        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1487")]
        public void Multitool_LaunchesAndRunsSuccessfully()
        {
            string multitoolPath = Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    @"..\..\Sarif.Multitool\net461\Sarif.Multitool.exe"));

            ProcessStartInfo startInfo = new ProcessStartInfo(multitoolPath, @"validate v2\ConverterTestData\ContrastSecurity\WebGoat.xml.sarif")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();

                process.ExitCode.Should().Be(0);
            }
        }
    }
}

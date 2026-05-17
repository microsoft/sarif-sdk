// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Mcp.Server;

using Xunit;

namespace Test.UnitTests.Sarif.Mcp.Server
{
    public class ProgramTests
    {
        [Fact]
        public async Task Main_ReturnsZero_OnEmptyArgs()
        {
            (int exitCode, _, _) = await RunMainAsync(Array.Empty<string>());

            exitCode.Should().Be(0);
        }

        [Fact]
        public async Task Main_WritesDiagnosticMessage_ToStderr()
        {
            (_, _, string stderr) = await RunMainAsync(Array.Empty<string>());

            stderr.Should().Contain("Sarif.Mcp.Server");
        }

        // Pins the stdio MCP invariant: stdout is reserved for JSON-RPC traffic;
        // diagnostics, banners, and logging must go to stderr.
        [Fact]
        public async Task Main_DoesNotWriteAnything_ToStdout()
        {
            (_, string stdout, _) = await RunMainAsync(Array.Empty<string>());

            stdout.Should().BeEmpty();
        }

        private static async Task<(int ExitCode, string Stdout, string Stderr)> RunMainAsync(string[] args)
        {
            TextWriter originalOut = Console.Out;
            TextWriter originalError = Console.Error;
            try
            {
                using var stdoutWriter = new StringWriter();
                using var stderrWriter = new StringWriter();
                Console.SetOut(stdoutWriter);
                Console.SetError(stderrWriter);

                int exitCode = await Program.Main(args);

                return (exitCode, stdoutWriter.ToString(), stderrWriter.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }
    }
}

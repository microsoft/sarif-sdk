// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (!args.Contains("--stdio"))
            {
                await Console.Error.WriteLineAsync(
                    "Sarif.Mcp.Server: Pass --stdio to run as a Model Context Protocol server over stdio.")
                    .ConfigureAwait(false);
                return 0;
            }

            // Stdio MCP host: stdout is reserved for JSON-RPC traffic, so all
            // logging providers must be cleared. Diagnostics go to stderr only.
            HostApplicationBuilder hb = Host.CreateApplicationBuilder(args);
            hb.Logging.ClearProviders();
            hb.Services.Configure<SarifMcpOptions>(hb.Configuration.GetSection("SarifMcp"));
            hb.Services.AddSingleton<SarifRunStore>();
            hb.Services.AddSingleton<CweNameResolver>();
            hb.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

            await hb.Build().RunAsync().ConfigureAwait(false);
            return 0;
        }
    }
}


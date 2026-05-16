// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools;

using ModelContextProtocol.Server;

using Xunit;

namespace Test.UnitTests.Sarif.Mcp.Server
{
    /// <summary>
    /// Pins the MCP tool surface of the server: the seven tool names exposed,
    /// their descriptions, and the explicit absence of the redaction parameter
    /// removed from FinalizeRunTool when porting from the AI plug-in.
    /// </summary>
    public class ToolSurfaceTests
    {
        /// <summary>
        /// Canonical wire names every MCP client should be able to discover.
        /// Reordering or renaming any of these is a breaking change for clients
        /// already invoking by name.
        /// </summary>
        private static readonly string[] s_expectedToolNames =
        {
            "sarif_create_run",
            "sarif_start_invocation",
            "sarif_end_invocation",
            "sarif_add_result",
            "sarif_add_notification",
            "sarif_finalize",
            "sarif_merge"
        };

        [Fact]
        public void Assembly_ExposesExactlyTheExpectedToolNames()
        {
            string[] discoveredNames = DiscoverMcpToolNames()
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();

            discoveredNames.Should().BeEquivalentTo(
                s_expectedToolNames.OrderBy(n => n, StringComparer.Ordinal).ToArray(),
                "MCP tool wire names are the public contract; renames or omissions break clients");
        }

        [Fact]
        public void EveryMcpTool_HasANonEmptyDescription()
        {
            foreach ((string toolName, MethodInfo method) in DiscoverMcpToolMethods())
            {
                DescriptionAttribute? description = method.GetCustomAttribute<DescriptionAttribute>();
                description.Should().NotBeNull(
                    $"MCP tool '{toolName}' is exposed to clients; tools/list requires a description");

                description!.Description.Should().NotBeNullOrWhiteSpace(
                    $"MCP tool '{toolName}' description must be non-empty");
            }
        }

        [Fact]
        public void EveryParameterOfEveryMcpTool_HasADescription()
        {
            foreach ((string toolName, MethodInfo method) in DiscoverMcpToolMethods())
            {
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    DescriptionAttribute? description = parameter.GetCustomAttribute<DescriptionAttribute>();
                    description.Should().NotBeNull(
                        $"MCP tool '{toolName}' parameter '{parameter.Name}' needs a description so " +
                        "tools/list reports a usable input schema");
                }
            }
        }

        /// <summary>
        /// Pins the redaction-removal contract: the original AI plug-in's
        /// FinalizeRunTool accepted <c>produceRedacted</c> and returned a
        /// <c>redacted_path</c>. Per the punt-redaction decision, neither
        /// belongs on the public surface of this server.
        /// </summary>
        [Fact]
        public void FinalizeRunTool_HasNoProduceRedactedParameter()
        {
            MethodInfo finalize = typeof(FinalizeRunTool).GetMethods()
                .Single(m => m.GetCustomAttribute<McpServerToolAttribute>() != null);

            finalize.GetParameters()
                .Select(p => p.Name)
                .Should().NotContain(
                    "produceRedacted",
                    "the AI profile carries no redaction surface (aip0 PR 91 punt); " +
                    "sarif_finalize must not accept a produceRedacted parameter");
        }

        private static IEnumerable<string> DiscoverMcpToolNames()
        {
            foreach ((string name, _) in DiscoverMcpToolMethods())
            {
                yield return name;
            }
        }

        private static IEnumerable<(string Name, MethodInfo Method)> DiscoverMcpToolMethods()
        {
            Assembly serverAssembly = typeof(CreateRunTool).Assembly;

            IEnumerable<Type> toolTypes = serverAssembly.GetTypes()
                .Where(t => t.GetCustomAttribute<McpServerToolTypeAttribute>() != null);

            foreach (Type toolType in toolTypes)
            {
                foreach (MethodInfo method in toolType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    McpServerToolAttribute? toolAttribute = method.GetCustomAttribute<McpServerToolAttribute>();
                    if (toolAttribute == null)
                    {
                        continue;
                    }

                    yield return (toolAttribute.Name ?? method.Name, method);
                }
            }
        }
    }
}

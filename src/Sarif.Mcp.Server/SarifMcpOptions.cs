// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// Server-wide configuration for the SARIF MCP server. Bound from the
    /// "SarifMcp" configuration section.
    /// </summary>
    public sealed class SarifMcpOptions
    {
        /// <summary>
        /// Server-controlled root directory for SARIF output files. When set,
        /// caller-supplied output paths are treated as relative filenames and
        /// combined with this root, preventing arbitrary file writes. When
        /// null or empty (default for stdio), caller paths are used as-is.
        /// </summary>
        public string? OutputRoot { get; set; }
    }
}

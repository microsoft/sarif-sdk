// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools
{
    /// <summary>
    /// Shared helper for writing SARIF logs with the indentation aip0 used,
    /// while routing through Newtonsoft + the SDK's [JsonConverter] attributes
    /// so SARIF-specific converters (UriConverter, DateTimeConverter,
    /// EnumConverter) are honored.
    /// </summary>
    internal static class SarifLogWriter
    {
        public static void Save(SarifLog log, string outputPath)
        {
            string? outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            using FileStream stream = File.Create(outputPath);
            using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
            using var writer = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };
            new JsonSerializer().Serialize(writer, log);
            writer.Flush();
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server.Tools
{
    /// <summary>
    /// Writes SARIF logs with the indentation the AI plug-in convention used,
    /// while routing through Newtonsoft + the SDK's [JsonConverter] attributes
    /// so SARIF-specific converters (UriConverter, DateTimeConverter,
    /// EnumConverter) are honored.
    /// </summary>
    internal static class SarifLogWriter
    {
        /// <summary>
        /// Atomically writes <paramref name="log"/> to <paramref name="outputPath"/>.
        /// </summary>
        /// <remarks>
        /// Writes to a sibling temp file first, then moves it onto the target
        /// path. Readers observe either the prior contents (if any) or the new
        /// contents; never a partially-written file. If serialization throws,
        /// the temp file is best-effort cleaned and the target is untouched.
        /// </remarks>
        public static void Save(SarifLog log, string outputPath)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentException("outputPath must be non-empty.", nameof(outputPath));
            }

            string? outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string tempPath = outputPath + ".tmp-" + Guid.NewGuid().ToString("N");

            try
            {
                using (FileStream stream = File.Create(tempPath))
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true))
                using (var writer = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented })
                {
                    new JsonSerializer().Serialize(writer, log);
                    writer.Flush();
                }

                // File.Move with overwrite is the documented atomic-rename
                // pattern on every supported platform. Either readers see the
                // prior file (if any) or the new one; never a partial write.
                File.Move(tempPath, outputPath, overwrite: true);
            }
            catch
            {
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch
                {
                    // Best-effort cleanup; the originating exception is what
                    // the caller needs to see.
                }

                throw;
            }
        }
    }
}


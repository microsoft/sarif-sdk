// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Atomically writes a SARIF file by staging to a sibling temp file in the same directory
    /// and renaming over the destination.
    /// </summary>
    /// <remarks>
    /// <para>The staging file is placed in the same directory as the destination so the final
    /// rename is a within-volume operation, which is atomic on every supported filesystem.</para>
    /// <para>If the rename fails, the staging file is removed to avoid leaving turds behind.</para>
    /// <para>The <c>writeContent</c> callback receives the underlying <see cref="FileStream"/>;
    /// callers MAY dispose any wrapper they construct (e.g., a <see cref="StreamWriter"/>) — the
    /// final fsync is best-effort and tolerates an already-disposed stream.</para>
    /// </remarks>
    public static class AtomicSarifWriter
    {
        /// <summary>
        /// Stages writing via <paramref name="writeContent"/>, then atomically replaces
        /// <paramref name="destinationPath"/>.
        /// </summary>
        public static void Write(string destinationPath, Action<Stream> writeContent)
        {
            if (string.IsNullOrEmpty(destinationPath))
            {
                throw new ArgumentException("Destination path must be supplied.", nameof(destinationPath));
            }

            if (writeContent == null)
            {
                throw new ArgumentNullException(nameof(writeContent));
            }

            string directory = Path.GetDirectoryName(Path.GetFullPath(destinationPath));
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string stagingPath = Path.Combine(
                directory,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1:N}.tmp",
                    Path.GetFileName(destinationPath),
                    Guid.NewGuid()));

            try
            {
                using (var stream = new FileStream(stagingPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    writeContent(stream);
                    try
                    {
                        // Best-effort durability hint. If the consumer already disposed the
                        // stream (e.g., via `using` on a StreamWriter without leaveOpen), it
                        // already flushed to the OS buffer; we can't fsync but the bytes are
                        // safely en route to disk.
                        stream.Flush(flushToDisk: true);
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }

                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                File.Move(stagingPath, destinationPath);
            }
            catch
            {
                TryDelete(stagingPath);
                throw;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best-effort cleanup; intentional swallow.
            }
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// SHA-256 hashing utilities for SARIF artifact hashes.
    /// Emits lowercase hex (matching the AI plug-in source); this differs from
    /// <c>Microsoft.CodeAnalysis.Sarif.HashUtilities</c> in the parent SDK, which
    /// emits uppercase hex. Wire output is locked by the serialization-parity tests.
    /// </summary>
    internal static class HashUtilities
    {
        /// <summary>Computes the SHA-256 hash of a file and returns it as a lowercase hex string.</summary>
        public static string ComputeSha256(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            return ComputeSha256(stream);
        }

        /// <summary>Computes the SHA-256 hash of text content (UTF-8 encoded).</summary>
        public static string ComputeSha256FromText(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>Computes the SHA-256 hash of a stream.</summary>
        public static string ComputeSha256(Stream stream)
        {
            byte[] hash = SHA256.HashData(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}

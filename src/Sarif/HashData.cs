// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A bag of hex-encoded hash values for a single artifact, keyed by algorithm.
    /// Populate the algorithm-specific properties via object initializer syntax:
    /// <c>new HashData { Sha256 = ..., GitBlobSha1 = ... }</c>. Unset properties are
    /// omitted from <see cref="ToDictionary"/>.
    /// </summary>
    public class HashData
    {
        /// <summary>
        /// SHA-1 (uppercase hex). Emitted under the dictionary key <c>sha-1</c>.
        /// </summary>
        public string Sha1 { get; set; }

        /// <summary>
        /// SHA-256 (uppercase hex). Emitted under the dictionary key <c>sha-256</c>.
        /// </summary>
        public string Sha256 { get; set; }

        /// <summary>
        /// SHA-512 (uppercase hex). Emitted under the dictionary key <c>sha-512</c>.
        /// </summary>
        public string Sha512 { get; set; }

        /// <summary>
        /// The GitHub blob SHA-1 of the file content, computed as
        /// <c>SHA1("blob " + length + "\0" + content)</c> over the raw bytes of the file.
        /// Emitted under the SARIF artifact hashes dictionary key <c>git-blob-sha-1</c>
        /// (lowercase hex, matching git's canonical form).
        /// </summary>
        public string GitBlobSha1 { get; set; }

        public IDictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(Sha1)) { result["sha-1"] = Sha1; }
            if (!string.IsNullOrEmpty(Sha256)) { result["sha-256"] = Sha256; }
            if (!string.IsNullOrEmpty(Sha512)) { result["sha-512"] = Sha512; }
            if (!string.IsNullOrEmpty(GitBlobSha1)) { result["git-blob-sha-1"] = GitBlobSha1; }

            return result;
        }
    }
}

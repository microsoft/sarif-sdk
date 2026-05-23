// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class HashData
    {
        public HashData(string sha1, string sha256)
            : this(sha1, sha256, gitBlobSha1: null)
        {
        }

        public HashData(string sha1, string sha256, string gitBlobSha1)
        {
            Sha1 = sha1;
            Sha256 = sha256;
            GitBlobSha1 = gitBlobSha1;
        }

        public string Sha1 { get; }

        public string Sha256 { get; }

        /// <summary>
        /// The GitHub blob SHA-1 of the file content, computed as
        /// <c>SHA1("blob " + length + "\0" + content)</c> over the raw bytes of the file.
        /// Emitted under the SARIF artifact hashes dictionary key <c>git-blob-sha-1</c>.
        /// </summary>
        public string GitBlobSha1 { get; }

        public IDictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(Sha1))
            {
                result["sha-1"] = Sha1;
            }

            if (!string.IsNullOrEmpty(Sha256))
            {
                result["sha-256"] = Sha256;
            }

            if (!string.IsNullOrEmpty(GitBlobSha1))
            {
                result["git-blob-sha-1"] = GitBlobSha1;
            }

            return result;
        }
    }
}

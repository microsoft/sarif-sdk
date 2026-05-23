// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Specifies the set of hash algorithms to compute for files persisted into a SARIF log.
    /// Used in conjunction with <see cref="OptionallyEmittedData.Hashes"/>, which acts as the
    /// on/off switch for emitting any hashes at all; this enum selects which algorithms are
    /// produced.
    /// </summary>
    /// <remarks>
    /// SHA-1 is no longer included in the default set. To preserve legacy behavior, callers
    /// must explicitly request <see cref="HashAlgorithms.Sha1"/>.
    ///
    /// <see cref="HashAlgorithms.GitBlobSha1"/> emits a hash with the dictionary key
    /// <c>git-blob-sha-1</c>, computed as <c>SHA1("blob " + length + "\0" + content)</c>
    /// over the raw bytes of the file on disk. This hash is byte-for-byte sensitive,
    /// including line-ending configuration. For the value to match a server-persisted git
    /// blob SHA, the on-disk bytes must match what git stored (typically LF line endings
    /// for text files in a normalized repository).
    ///
    /// When a caller of <see cref="Writers.SarifLogger"/> supplies an explicit
    /// <see cref="FileRegionsCache"/>, the algorithm set configured on that cache wins
    /// and the logger's <c>hashAlgorithms</c> parameter is not consulted. Configure
    /// algorithms on the cache in that scenario.
    /// </remarks>
    [Flags]
    public enum HashAlgorithms
    {
        /// <summary>
        /// Compute no hashes.
        /// </summary>
        None = 0,

        /// <summary>
        /// Compute SHA-1. Emitted under the dictionary key <c>sha-1</c>.
        /// </summary>
        Sha1 = 0x1,

        /// <summary>
        /// Compute SHA-256. Emitted under the dictionary key <c>sha-256</c>.
        /// </summary>
        Sha256 = 0x2,

        /// <summary>
        /// Compute a GitHub blob SHA-1 over the raw bytes of the file. Emitted under the
        /// dictionary key <c>git-blob-sha-1</c>. The value matches what
        /// <c>git hash-object &lt;file&gt;</c> would produce for the same bytes.
        /// </summary>
        GitBlobSha1 = 0x4,

        /// <summary>
        /// The default set of algorithms computed by <see cref="SarifLogger"/> and related
        /// infrastructure when only <see cref="OptionallyEmittedData.Hashes"/> is requested.
        /// </summary>
        Default = Sha256,
    }
}

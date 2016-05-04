// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Values specifying different hashing algorithms.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public enum AlgorithmKind
    {
        Unknown,
        Authentihash,
        Blake256,
        Blake512,
        Ecoh,
        Fsb,
        Gost,
        Groestl,
        Has160,
        Haval,
        JH,
        MD2,
        MD4,
        MD5,
        MD6,
        RadioGatun,
        RipeMD,
        RipeMD128,
        RipeMD160,
        RipeMD320,
        Sdhash,
        Sha1,
        Sha224,
        Sha256,
        Sha384,
        Sha512,
        Sha3,
        Skein,
        Snefru,
        SpectralHash,
        Ssdeep,
        Swifft,
        Tiger,
        Tlsh,
        Whirlpool
    }
}
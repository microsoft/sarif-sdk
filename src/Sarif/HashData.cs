// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public class HashData
    {
        public HashData(string md5, string sha1, string sha256)
        {
            MD5 = md5;
            Sha1 = sha1;
            Sha256 = sha256;
        }

        public string MD5 { get; }

        public string Sha1 { get; }

        public string Sha256 { get; }

    }
}

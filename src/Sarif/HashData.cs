// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class HashData
    {
        public HashData(string sha1, string sha256)
        {
            Sha1 = sha1;
            Sha256 = sha256;
        }

        public string Sha1 { get; }

        public string Sha256 { get; }

        public IDictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(this?.Sha1))
            {
                result["sha-1"] = this?.Sha1;
            }

            if (!string.IsNullOrEmpty(this?.Sha256))
            {
                result["sha-256"] = this?.Sha256;
            }

            return result;
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class UriUtilities
    {
        public const string FileScheme = "file";

        // The in-log object-addressing scheme: a 'sarif:' URI (e.g.
        // "sarif:/runs/<r>/results/<m>") addresses another object in the same
        // log - used by relatedLocations cross-links and partitioning - and is
        // not a physical artifact. See Writers/PartitionFunctions.cs.
        public const string SarifScheme = "sarif";

        public static string WithColon(this string scheme)
        {
            scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            return $"{scheme}:";
        }
    }
}

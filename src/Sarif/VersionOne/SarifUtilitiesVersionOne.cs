// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class SarifUtilitiesVersionOne
    {
        public static string ConvertToText(this SarifVersionVersionOne sarifVersion)
        {
            switch (sarifVersion)
            {
                case SarifVersionVersionOne.OneZeroZero:
                    return SarifUtilities.V1_0_0;
                default:
                    throw new ArgumentException("Unsupported SARIF version", nameof(sarifVersion));
            }
        }

        public static Uri ConvertToSchemaUri(this SarifVersionVersionOne sarifVersion)
        {
            return new Uri(SarifUtilities.SarifSchemaUriBase + sarifVersion.ConvertToText() + ".json", UriKind.Absolute);
        }
    }
}

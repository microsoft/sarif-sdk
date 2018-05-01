// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class SarifUtilitiesVersionOne
    {
        private const string V1_0_0 = "1.0.0";

        public static string ConvertToText(this SarifVersionVersionOne sarifVersion)
        {
            switch (sarifVersion)
            {
                case SarifVersionVersionOne.OneZeroZero:
                    return V1_0_0;
                default:
                    throw new ArgumentException("Unsupported SARIF version", nameof(sarifVersion));
            }
        }

        public static Uri ConvertToSchemaUri(this SarifVersionVersionOne sarifVersion)
        {
            return new Uri("http://json.schemastore.org/sarif-" + sarifVersion.ConvertToText(), UriKind.Absolute);
        }
    }
}

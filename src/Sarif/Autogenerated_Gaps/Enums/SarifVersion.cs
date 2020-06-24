// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Possible values for the SARIF schema version.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public enum SarifVersion
    {
        Unknown,

        [EnumMember(Value = SarifUtilities.V1_0_0)]
        OneZeroZero,

        [EnumMember(Value = VersionConstants.StableSarifVersion)]
        Current
    }
}
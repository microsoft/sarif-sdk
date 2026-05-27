// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Identifies the validator family that applies a SARIF reporting-descriptor rule.
    /// </summary>
    /// <remarks>
    /// Not a <c>[Flags]</c> enum; combinations are expressed via <see cref="RuleKindSet"/>.
    /// </remarks>
    public enum RuleKind
    {
        None = 0,
        Sarif = 1,
        Gh = 2,
        Ghas = 3,
        GHAzDO = 4,
        AI = 5,

        [Obsolete("Use RuleKind.GHAzDO instead.")]
        Ado = GHAzDO,
    }
}


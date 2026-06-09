// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Identifies the validator family that applies a SARIF reporting-descriptor rule.
    /// </summary>
    /// <remarks>
    /// <c>[Flags]</c>: each kind is an independent bit. The active kinds for an
    /// analysis are expressed as a <see cref="RuleKindSet"/>.
    /// </remarks>
    [Flags]
    public enum RuleKind
    {
        None = 0,
        Sarif = 1,
        Ghas = 2,
        GHAzDO = 4,
        AI = 8,

        [Obsolete("Use RuleKind.GHAzDO instead.")]
        Ado = GHAzDO,
    }
}


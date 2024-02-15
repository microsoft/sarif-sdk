// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public enum RuleKind
    {
        None = 0,
        Sarif = 1,
        Gh = 2,
        Ghas = 4,
        Ado = 8,
    }
}

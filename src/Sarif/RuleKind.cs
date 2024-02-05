// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public enum RuleKind
    {
        None = 0,
        Sarif = 1,
        Ghas = 2,
        Ado = 4,
    }
}

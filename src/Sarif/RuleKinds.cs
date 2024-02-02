// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Flags]
    public enum RuleKinds
    {
        None = 0,
        Sarif = 1,
        Ghas = 2,
        Ado = 4,
    }
}

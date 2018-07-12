// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    // Mapping Actions
    public enum SarifLogAction
    {
        None = 0,
        RebaseUri,
        MakeUrisAbsolute,
        Merge,
        // Future work...
        Sort,
        MakeDeterministic,
    }
}

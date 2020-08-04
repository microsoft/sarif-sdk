// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    // Mapping Actions
    public enum SarifLogAction
    {
        None = 0,
        Sort, // NOT_IMPL
        Merge,
        RebaseUri,
        MakeUrisAbsolute,
        MakeDeterministic, // NOT_IMPL
        InsertOptionalData,
        RemoveOptionalData,
    }
}

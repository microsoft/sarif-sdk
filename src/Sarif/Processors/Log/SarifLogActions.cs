// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    // Mapping Actions
    public enum SarifLogAction
    {
        None = 0,
        RebaseUri,
        Merge,
        // Future work...
        Sort,
        MakeDeterministic,
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public abstract class SarifWorkItemModelTransformer
    {
        // TODO: make this async in the next iteration of the pipeline design
        //
        // 
        public abstract void Transform(SarifWorkItemModel workItemModel);
    }
}

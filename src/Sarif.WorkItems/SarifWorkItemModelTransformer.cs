// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.WorkItems;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public abstract class SarifWorkItemModelTransformer : IWorkItemModelTransformer<SarifWorkItemContext>
    {
        public abstract void Transform(WorkItemModel<SarifWorkItemContext> workItemModel);
    }
}

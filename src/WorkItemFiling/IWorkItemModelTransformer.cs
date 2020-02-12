// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WorkItemFiling;

namespace Microsoft.WorkItemFiling
{
    public interface IWorkItemModelTransformer<T>
    {
        void Transform(WorkItemModel<T> workItemModel);
    }
}
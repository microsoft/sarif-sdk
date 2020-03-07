// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.WorkItems
{
    public interface IWorkItemModelTransformer<T> where T : WorkItemModel
    {
        void Transform(T workItemModel);
    }
}
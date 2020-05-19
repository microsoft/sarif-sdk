// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WorkItems;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public abstract class SarifWorkItemModelTransformer
    {
        public SarifWorkItemModelTransformer()
        {
            this.Logger = ServiceProviderFactory.ServiceProvider.GetService<ILogger>();
        }

        protected ILogger Logger { get; }

        // TODO: make this async in the next iteration of the pipeline design
        //
        // 
        public abstract SarifWorkItemModel Transform(SarifWorkItemModel workItemModel);
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Abstract base for classes that represents a system (for example, GitHub or Azure DevOps)
    /// to which work items can be filed.
    /// </summary>
    public abstract class FilingTarget
    {
        /// <summary>
        /// Connect to the project in which work items will be filed.
        /// </summary>
        /// <param name="projectUri">
        /// The URI of the project.
        /// </param>
        /// <param name="personalAccessToken">
        /// TEMPORARY: Specifes the personal access used to access the project. Default: null.
        /// </param>
        /// <remarks>
        /// We provide an empty implementation because not all systems require an explicit connection step.
        /// </remarks>
        public virtual Task Connect(Uri projectUri, string personalAccessToken = null)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Asynchronously file work items for the specified results.
        /// </summary>
        /// <param name="workItemMetadata">
        /// Describes the work items to be filed.
        /// </param>
        /// <returns>
        /// An object that can be awaited to see the result groups that were actually filed.
        /// </returns>
        public abstract Task<IEnumerable<WorkItemMetadata>> FileWorkItems(IEnumerable<WorkItemMetadata> workItemMetadata);
    }
}

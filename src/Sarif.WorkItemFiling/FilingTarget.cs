// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Abstract base for classes that represents a system (for example, GitHub or AzureDevOps)
    /// to which work items can be filed.
    /// </summary>
    public abstract class FilingTarget
    {
        /// <summary>
        /// Asynchronously file work items for the specified results.
        /// </summary>
        /// <param name="resultGroups">
        /// The sets of SARIF <see cref="Result"/> objects for which work items are to be filed.
        /// </param>
        /// <returns>
        /// An object that can be awaited to see the result groups that were actually filed.
        /// </returns>
        public abstract Task<IEnumerable<ResultGroup>> FileWorkItems(IEnumerable<ResultGroup> resultGroups);
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public abstract class FilingTargetBase
    {
        /// <summary>
        /// Asynchronously file work items for the specified results.
        /// </summary>
        /// <param name="results">
        /// The SARIF <see cref="Result"/> objects for which work items are to be filed.
        /// </param>
        /// <returns>
        /// An object that can be awaited to see the set of results that were actually filed.
        /// </returns>
        public abstract Task<IEnumerable<Result>> FileWorkItems(IEnumerable<Result> results);
    }
}

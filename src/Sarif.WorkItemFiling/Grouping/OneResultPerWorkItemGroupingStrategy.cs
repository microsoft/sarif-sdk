// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// A grouping strategy that files each SARIF result as a separate work item.
    /// </summary>
    public class OneResultPerWorkItemGroupingStrategy : GroupingStrategy
    {
        public override IList<WorkItemFilingMetadata> GroupResults(IEnumerable<Result> results)
        {
            if (results == null) { throw new ArgumentNullException(nameof(results)); }

            return results
                .Select(r => new WorkItemFilingMetadata { Results = new List<Result> { r } })
                .ToList();
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Filtering
{
    /// <summary>
    /// A filtering strategy that selects only new results.
    /// </summary>
    public class NewResultsFilteringStrategy : FilteringStrategy
    {
        public override IList<Result> FilterResults(IEnumerable<Result> results)
        {
            if (results == null) { throw new ArgumentNullException(nameof(results)); }

            return results
                .Where(r => r.BaselineState == BaselineState.New)
                .ToList();
        }
    }
}

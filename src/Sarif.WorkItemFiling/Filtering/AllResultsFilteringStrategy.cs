// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Filtering
{
    /// <summary>
    /// A filtering strategy that selects all results.
    /// </summary>
    public class AllResultsFilteringStrategy : IFilteringStrategy
    {
        public IList<Result> FilterResults(IEnumerable<Result> results)
        {
            // Note that this creates a new list (which is intentional; it allows you
            // to manipulate the two lists independently). However, since Result is a reference
            // type, the elements of the two lists will refer to the same objects.
            return results.ToList();
        }
    }
}

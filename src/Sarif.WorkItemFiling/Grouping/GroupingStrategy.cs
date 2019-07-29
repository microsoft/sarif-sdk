// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Abstract base class for strategies that grouping SARIF results into sets that
    /// should each be filed together as a single work item.
    /// </summary>
    public abstract class GroupingStrategy
    {
        public abstract IList<WorkItemFilingMetadata> GroupResults(IEnumerable<Result> results);
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Interface that defines a strategy for grouping SARIF results into sets that
    /// should each be filed together as a single work item.
    /// </summary>
    public interface IGroupingStrategy
    {
        ResultGroup GroupResults(IEnumerable<Result> results);
    }
}

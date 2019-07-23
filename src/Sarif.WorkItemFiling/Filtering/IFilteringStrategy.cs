// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Filtering
{
    /// <summary>
    /// Interface that defines a strategy for selecting only those SARIF results that should
    /// be filed as work items.
    /// </summary>
    public interface IFilteringStrategy
    {
        IList<Result> FilterResults(IEnumerable<Result> results);
    }
}

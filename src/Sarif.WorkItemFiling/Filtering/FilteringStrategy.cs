// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Filtering
{
    /// <summary>
    /// Abstract base class for strategies that select only those SARIF results that should
    /// be filed as work items.
    /// </summary>
    public abstract class FilteringStrategy
    {
        public abstract IList<Result> FilterResults(IEnumerable<Result> results);
    }
}

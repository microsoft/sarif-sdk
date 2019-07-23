// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Grouping
{
    /// <summary>
    /// Represents a collection of SARIF results that should be filed as a single work item.
    /// </summary>
    /// <remarks>
    /// We represent this as a class, rather than simply working with lists of results, so
    /// we can in future associated bug filing metadata with a list of results.
    /// </remarks>
    public class ResultGroup
    {
        /// <summary>
        /// The results that should be filed as a single work item.
        /// </summary>
        public IList<Result> Results { get; set; }
    }
}

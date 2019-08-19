// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Enumerates the grouping strategies provided out of the box by the WorkItemFiler.
    /// </summary>
    public enum GroupingStrategyKind
    {
        /// <summary>
        /// No grouping strategy was specified.
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// A grouping strategy that creates one work item per SARIF result.
        /// </summary>
        PerResult = 1,
    }
}

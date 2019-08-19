// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    /// <summary>
    /// Enumerates the filtering strategies provided out of the box by the WorkItemFiler.
    /// </summary>
    public enum FilteringStrategyKind
    {
        /// <summary>
        /// No filtering strategy was specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// A filtering strategy that selects all results.
        /// </summary>
        AllResults = 1,

        /// <summary>
        /// A filtering strategy that selects new results.
        /// </summary>
        NewResults = 2,
    }
}

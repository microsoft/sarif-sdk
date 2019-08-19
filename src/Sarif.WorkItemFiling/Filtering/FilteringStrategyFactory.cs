// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public static class FilteringStrategyFactory
    {
        public static FilteringStrategy CreateFilteringStrategy(FilteringStrategyKind kind)
        {
            switch (kind)
            {
                case FilteringStrategyKind.AllResults:
                    return new AllResultsFilteringStrategy();

                case FilteringStrategyKind.NewResults:
                    return new NewResultsFilteringStrategy();

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public static class GroupingStrategyFactory
    {
        public static GroupingStrategy CreateGroupingStrategy(GroupingStrategyKind kind)
        {
            switch (kind)
            {
                case GroupingStrategyKind.PerResult:
                    return new OneResultPerWorkItemGroupingStrategy();

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class LogicalLocation
    {
        public LogicalLocation Resolve(Run run)
        {
            return Index >= 0 && Index < run?.LogicalLocations?.Count
                ? run.LogicalLocations[Index]
                : this;
        }
    }
}

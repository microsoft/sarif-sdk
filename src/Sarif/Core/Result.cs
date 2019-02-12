// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Result
    {
        public bool ShouldSerializeWorkItemUris() { return this.WorkItemUris != null && this.WorkItemUris.Any((s) => s != null); }

        public bool ShouldSerializeLevel() { return this.Level != FailureLevel.Warning; }
    }
}

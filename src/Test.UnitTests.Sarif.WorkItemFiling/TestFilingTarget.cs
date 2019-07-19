// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling
{
    internal class TestFilingTarget : WorkItemFilingTargetBase
    {
        public override async Task<IEnumerable<Result>> FileWorkItems(IEnumerable<Result> results)
        {
            return await Task.FromResult(results);
        }
    }
}

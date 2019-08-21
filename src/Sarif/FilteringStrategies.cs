// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class FilteringStrategies
    {
        public static Func<Result, bool> NewOrUnbaselined =
            (result) =>
            {
                return result.BaselineState == BaselineState.New || result.BaselineState == BaselineState.None;
            };
    }
}

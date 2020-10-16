// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public interface IResultMatcher
    {
        IList<MatchedResults> Match(IList<ExtractedResult> previousResults, IList<ExtractedResult> currentResults);
    }
}

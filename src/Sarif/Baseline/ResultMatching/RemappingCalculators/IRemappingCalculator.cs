// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    public interface IRemappingCalculator
    {
        IEnumerable<SarifLogRemapping> CalculatePossibleRemappings(IEnumerable<ExtractedResult> baseline, IEnumerable<ExtractedResult> current);
    }
}
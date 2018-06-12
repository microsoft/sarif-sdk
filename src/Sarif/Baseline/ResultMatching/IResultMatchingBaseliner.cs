// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    public interface IResultMatchingBaseliner
    {
        SarifLog BaselineSarifLogs(SarifLog[] baseline, SarifLog[] current);
    }
}

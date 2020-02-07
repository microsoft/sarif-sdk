// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    public interface ISarifLogMatcher
    {
        IEnumerable<SarifLog> Match(IEnumerable<SarifLog> previousLogs, IEnumerable<SarifLog> currentLogs);
    }
}

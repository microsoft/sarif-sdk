// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Extensions
{
    public static class ResultExtensions
    {
        public static bool IsSuppressed(this Result result)
        {
            result = result ?? throw new ArgumentNullException(nameof(result));

            IList<Suppression> suppressions = result.Suppressions;
            if (suppressions == null || suppressions.Count == 0)
            {
                return false;
            }

            return suppressions.Any(s => s.Status == SuppressionStatus.Accepted)
                && !suppressions.Any(s => s.Status == SuppressionStatus.Rejected || s.Status == SuppressionStatus.UnderReview);
        }
    }
}

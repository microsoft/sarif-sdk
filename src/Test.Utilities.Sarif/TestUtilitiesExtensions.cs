// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Test.Utilities.Sarif
{
    public static class TestUtilitiesExtensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list, Random random)
        {
            if (list == null)
            {
                return null;
            }

            if (random == null)
            {
                // Random object with seed logged in test is required.
                throw new ArgumentNullException(nameof(random));
            }

            return list.OrderBy(item => random.Next()).ToList();
        }
    }
}

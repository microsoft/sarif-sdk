// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal static class IntegerExtensions
    {
        public static string ToInvariantString(this int n)
        {
            return n.ToString(CultureInfo.InvariantCulture);
        }
    }
}

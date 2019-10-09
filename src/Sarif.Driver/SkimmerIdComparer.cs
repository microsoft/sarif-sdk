// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// This partly implemented Skimmer comparer simply orders skimmers by their Ids and Names.
    /// This provides for some basic sort ordering to help produce deterministically ordered analysis.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class SkimmerIdComparer<TContext> : IComparer<Skimmer<TContext>>
    {
        public static SkimmerIdComparer<TContext> Instance = new SkimmerIdComparer<TContext>();

        public int Compare(Skimmer<TContext> left, Skimmer<TContext> right)
        {
            if (left == null && right == null) { return 0; }
            if (left == null) { return -1; }
            if (right == null) { return 1; }

            int comparison = StringComparer.Ordinal.Compare(left.Id, right.Id);
            if (comparison != 0) { return comparison; }

            return StringComparer.Ordinal.Compare(left.Name, right.Name);
        }
    }
}

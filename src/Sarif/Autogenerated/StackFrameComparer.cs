// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type StackFrame for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class StackFrameComparer : IComparer<StackFrame>
    {
        internal static readonly StackFrameComparer Instance = new StackFrameComparer();

        public int Compare(StackFrame left, StackFrame right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = LocationComparer.Instance.Compare(left.Location, right.Location);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Module, right.Module);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ThreadId.CompareTo(right.ThreadId);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Parameters.ListCompares(right.Parameters);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Properties.DictionaryCompares(right.Properties, SerializedPropertyInfoComparer.Instance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
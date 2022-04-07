// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ExceptionData for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class ExceptionDataComparer : IComparer<ExceptionData>
    {
        internal static readonly ExceptionDataComparer Instance = new ExceptionDataComparer();

        public int Compare(ExceptionData left, ExceptionData right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Kind, right.Kind);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Message, right.Message);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = StackComparer.Instance.Compare(left.Stack, right.Stack);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.InnerExceptions.ListCompares(right.InnerExceptions, ExceptionDataComparer.Instance);
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
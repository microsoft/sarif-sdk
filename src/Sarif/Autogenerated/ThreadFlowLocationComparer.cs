// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ThreadFlowLocation for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class ThreadFlowLocationComparer : IComparer<ThreadFlowLocation>
    {
        internal static readonly ThreadFlowLocationComparer Instance = new ThreadFlowLocationComparer();

        public int Compare(ThreadFlowLocation left, ThreadFlowLocation right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.Index.CompareTo(right.Index);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = LocationComparer.Instance.Compare(left.Location, right.Location);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = StackComparer.Instance.Compare(left.Stack, right.Stack);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Kinds.ListCompares(right.Kinds);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Taxa.ListCompares(right.Taxa, ReportingDescriptorReferenceComparer.Instance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Module, right.Module);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.State.DictionaryCompares(right.State, MultiformatMessageStringComparer.Instance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.NestingLevel.CompareTo(right.NestingLevel);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ExecutionOrder.CompareTo(right.ExecutionOrder);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ExecutionTimeUtc.CompareTo(right.ExecutionTimeUtc);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Importance.CompareTo(right.Importance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = WebRequestComparer.Instance.Compare(left.WebRequest, right.WebRequest);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = WebResponseComparer.Instance.Compare(left.WebResponse, right.WebResponse);
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
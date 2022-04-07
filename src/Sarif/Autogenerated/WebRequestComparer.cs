// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type WebRequest for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class WebRequestComparer : IComparer<WebRequest>
    {
        internal static readonly WebRequestComparer Instance = new WebRequestComparer();

        public int Compare(WebRequest left, WebRequest right)
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

            compareResult = string.Compare(left.Protocol, right.Protocol);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Version, right.Version);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Target, right.Target);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Method, right.Method);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Headers.DictionaryCompares(right.Headers);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Parameters.DictionaryCompares(right.Parameters);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = ArtifactContentComparer.Instance.Compare(left.Body, right.Body);
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
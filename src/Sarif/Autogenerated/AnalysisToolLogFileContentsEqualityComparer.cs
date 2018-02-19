// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type AnalysisToolLogFileContents for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class AnalysisToolLogFileContentsEqualityComparer : IEqualityComparer<AnalysisToolLogFileContents>
    {
        internal static readonly AnalysisToolLogFileContentsEqualityComparer Instance = new AnalysisToolLogFileContentsEqualityComparer();

        public bool Equals(AnalysisToolLogFileContents left, AnalysisToolLogFileContents right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Region.ValueComparer.Equals(left.Region, right.Region))
            {
                return false;
            }

            if (left.Snippet != right.Snippet)
            {
                return false;
            }

            if (left.AnalysisToolLogFileUri != right.AnalysisToolLogFileUri)
            {
                return false;
            }

            if (left.AnalysisToolLogFileUriBaseId != right.AnalysisToolLogFileUriBaseId)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(AnalysisToolLogFileContents obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Region != null)
                {
                    result = (result * 31) + obj.Region.ValueGetHashCode();
                }

                if (obj.Snippet != null)
                {
                    result = (result * 31) + obj.Snippet.GetHashCode();
                }

                if (obj.AnalysisToolLogFileUri != null)
                {
                    result = (result * 31) + obj.AnalysisToolLogFileUri.GetHashCode();
                }

                if (obj.AnalysisToolLogFileUriBaseId != null)
                {
                    result = (result * 31) + obj.AnalysisToolLogFileUriBaseId.GetHashCode();
                }
            }

            return result;
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Conversion for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class ConversionEqualityComparer : IEqualityComparer<Conversion>
    {
        internal static readonly ConversionEqualityComparer Instance = new ConversionEqualityComparer();

        public bool Equals(Conversion left, Conversion right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Tool.ValueComparer.Equals(left.Tool, right.Tool))
            {
                return false;
            }

            if (!Invocation.ValueComparer.Equals(left.Invocation, right.Invocation))
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

        public int GetHashCode(Conversion obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Tool != null)
                {
                    result = (result * 31) + obj.Tool.ValueGetHashCode();
                }

                if (obj.Invocation != null)
                {
                    result = (result * 31) + obj.Invocation.ValueGetHashCode();
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
// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Region for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    internal sealed class RegionEqualityComparer : IEqualityComparer<Region>
    {
        internal static readonly RegionEqualityComparer Instance = new RegionEqualityComparer();

        public bool Equals(Region left, Region right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.StartLine != right.StartLine)
            {
                return false;
            }

            if (left.StartColumn != right.StartColumn)
            {
                return false;
            }

            if (left.EndLine != right.EndLine)
            {
                return false;
            }

            if (left.EndColumn != right.EndColumn)
            {
                return false;
            }

            if (left.Offset != right.Offset)
            {
                return false;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Region obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.StartLine.GetHashCode();
                result = (result * 31) + obj.StartColumn.GetHashCode();
                result = (result * 31) + obj.EndLine.GetHashCode();
                result = (result * 31) + obj.EndColumn.GetHashCode();
                result = (result * 31) + obj.Offset.GetHashCode();
                result = (result * 31) + obj.Length.GetHashCode();
            }

            return result;
        }
    }
}
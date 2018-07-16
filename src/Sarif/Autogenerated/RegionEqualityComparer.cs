// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Region for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.55.0.0")]
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

            if (left.CharOffset != right.CharOffset)
            {
                return false;
            }

            if (left.CharLength != right.CharLength)
            {
                return false;
            }

            if (left.ByteOffset != right.ByteOffset)
            {
                return false;
            }

            if (left.ByteLength != right.ByteLength)
            {
                return false;
            }

            if (!FileContent.ValueComparer.Equals(left.Snippet, right.Snippet))
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Message, right.Message))
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
                result = (result * 31) + obj.CharOffset.GetHashCode();
                result = (result * 31) + obj.CharLength.GetHashCode();
                result = (result * 31) + obj.ByteOffset.GetHashCode();
                result = (result * 31) + obj.ByteLength.GetHashCode();
                if (obj.Snippet != null)
                {
                    result = (result * 31) + obj.Snippet.ValueGetHashCode();
                }

                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}
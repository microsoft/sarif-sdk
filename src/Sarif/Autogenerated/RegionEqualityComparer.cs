// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Region for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
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

            if (!ArtifactContent.ValueComparer.Equals(left.Snippet, right.Snippet))
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Message, right.Message))
            {
                return false;
            }

            if (left.SourceLanguage != right.SourceLanguage)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Properties)
                {
                    SerializedPropertyInfo value_1;
                    if (!right.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!object.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
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

                if (obj.SourceLanguage != null)
                {
                    result = (result * 31) + obj.SourceLanguage.GetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_2 in obj.Properties)
                    {
                        xor_0 ^= value_2.Key.GetHashCode();
                        if (value_2.Value != null)
                        {
                            xor_0 ^= value_2.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}
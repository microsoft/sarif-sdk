// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type FileData for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class FileDataEqualityComparer : IEqualityComparer<FileData>
    {
        internal static readonly FileDataEqualityComparer Instance = new FileDataEqualityComparer();

        public bool Equals(FileData left, FileData right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.FileLocation, right.FileLocation))
            {
                return false;
            }

            if (left.ParentKey != right.ParentKey)
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

            if (left.MimeType != right.MimeType)
            {
                return false;
            }

            if (!FileContent.ValueComparer.Equals(left.Contents, right.Contents))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Hashes, right.Hashes))
            {
                if (left.Hashes == null || right.Hashes == null)
                {
                    return false;
                }

                if (left.Hashes.Count != right.Hashes.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Hashes.Count; ++index_0)
                {
                    if (!Hash.ValueComparer.Equals(left.Hashes[index_0], right.Hashes[index_0]))
                    {
                        return false;
                    }
                }
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

        public int GetHashCode(FileData obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.FileLocation != null)
                {
                    result = (result * 31) + obj.FileLocation.ValueGetHashCode();
                }

                if (obj.ParentKey != null)
                {
                    result = (result * 31) + obj.ParentKey.GetHashCode();
                }

                result = (result * 31) + obj.Offset.GetHashCode();
                result = (result * 31) + obj.Length.GetHashCode();
                if (obj.MimeType != null)
                {
                    result = (result * 31) + obj.MimeType.GetHashCode();
                }

                if (obj.Contents != null)
                {
                    result = (result * 31) + obj.Contents.ValueGetHashCode();
                }

                if (obj.Hashes != null)
                {
                    foreach (var value_2 in obj.Hashes)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_3 in obj.Properties)
                    {
                        xor_0 ^= value_3.Key.GetHashCode();
                        if (value_3.Value != null)
                        {
                            xor_0 ^= value_3.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}
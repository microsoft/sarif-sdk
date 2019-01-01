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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
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

            if (left.ParentIndex != right.ParentIndex)
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

            if (left.Roles != right.Roles)
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

            if (left.Encoding != right.Encoding)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Hashes, right.Hashes))
            {
                if (left.Hashes == null || right.Hashes == null || left.Hashes.Count != right.Hashes.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Hashes)
                {
                    string value_1;
                    if (!right.Hashes.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (left.LastModifiedTimeUtc != right.LastModifiedTimeUtc)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_2 in left.Properties)
                {
                    SerializedPropertyInfo value_3;
                    if (!right.Properties.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (!object.Equals(value_2.Value, value_3))
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

                result = (result * 31) + obj.ParentIndex.GetHashCode();
                result = (result * 31) + obj.Offset.GetHashCode();
                result = (result * 31) + obj.Length.GetHashCode();
                result = (result * 31) + obj.Roles.GetHashCode();
                if (obj.MimeType != null)
                {
                    result = (result * 31) + obj.MimeType.GetHashCode();
                }

                if (obj.Contents != null)
                {
                    result = (result * 31) + obj.Contents.ValueGetHashCode();
                }

                if (obj.Encoding != null)
                {
                    result = (result * 31) + obj.Encoding.GetHashCode();
                }

                if (obj.Hashes != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.Hashes)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                result = (result * 31) + obj.LastModifiedTimeUtc.GetHashCode();
                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_5 in obj.Properties)
                    {
                        xor_1 ^= value_5.Key.GetHashCode();
                        if (value_5.Value != null)
                        {
                            xor_1 ^= value_5.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}
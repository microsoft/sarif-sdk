// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Attachment for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class AttachmentEqualityComparer : IEqualityComparer<Attachment>
    {
        internal static readonly AttachmentEqualityComparer Instance = new AttachmentEqualityComparer();

        public bool Equals(Attachment left, Attachment right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Description, right.Description))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.FileLocation, right.FileLocation))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Regions, right.Regions))
            {
                if (left.Regions == null || right.Regions == null)
                {
                    return false;
                }

                if (left.Regions.Count != right.Regions.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Regions.Count; ++index_0)
                {
                    if (!Region.ValueComparer.Equals(left.Regions[index_0], right.Regions[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Rectangles, right.Rectangles))
            {
                if (left.Rectangles == null || right.Rectangles == null)
                {
                    return false;
                }

                if (left.Rectangles.Count != right.Rectangles.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.Rectangles.Count; ++index_1)
                {
                    if (!Rectangle.ValueComparer.Equals(left.Rectangles[index_1], right.Rectangles[index_1]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(Attachment obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Description != null)
                {
                    result = (result * 31) + obj.Description.ValueGetHashCode();
                }

                if (obj.FileLocation != null)
                {
                    result = (result * 31) + obj.FileLocation.ValueGetHashCode();
                }

                if (obj.Regions != null)
                {
                    foreach (var value_0 in obj.Regions)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Rectangles != null)
                {
                    foreach (var value_1 in obj.Rectangles)
                    {
                        result = result * 31;
                        if (value_1 != null)
                        {
                            result = (result * 31) + value_1.ValueGetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type PhysicalLocation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class PhysicalLocationEqualityComparer : IEqualityComparer<PhysicalLocation>
    {
        internal static readonly PhysicalLocationEqualityComparer Instance = new PhysicalLocationEqualityComparer();

        public bool Equals(PhysicalLocation left, PhysicalLocation right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Id != right.Id)
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.FileLocation, right.FileLocation))
            {
                return false;
            }

            if (!Region.ValueComparer.Equals(left.Region, right.Region))
            {
                return false;
            }

            if (!Region.ValueComparer.Equals(left.ContextRegion, right.ContextRegion))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(PhysicalLocation obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Id.GetHashCode();
                if (obj.FileLocation != null)
                {
                    result = (result * 31) + obj.FileLocation.ValueGetHashCode();
                }

                if (obj.Region != null)
                {
                    result = (result * 31) + obj.Region.ValueGetHashCode();
                }

                if (obj.ContextRegion != null)
                {
                    result = (result * 31) + obj.ContextRegion.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}
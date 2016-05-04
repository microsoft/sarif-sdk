// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type PhysicalLocation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
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

            if (left.Uri != right.Uri)
            {
                return false;
            }

            if (!Region.ValueComparer.Equals(left.Region, right.Region))
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
                if (obj.Uri != null)
                {
                    result = (result * 31) + obj.Uri.GetHashCode();
                }

                if (obj.Region != null)
                {
                    result = (result * 31) + obj.Region.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}
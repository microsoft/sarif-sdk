// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Replacement for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.55.0.0")]
    internal sealed class ReplacementEqualityComparer : IEqualityComparer<Replacement>
    {
        internal static readonly ReplacementEqualityComparer Instance = new ReplacementEqualityComparer();

        public bool Equals(Replacement left, Replacement right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Region.ValueComparer.Equals(left.DeletedRegion, right.DeletedRegion))
            {
                return false;
            }

            if (!FileContent.ValueComparer.Equals(left.InsertedContent, right.InsertedContent))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Replacement obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.DeletedRegion != null)
                {
                    result = (result * 31) + obj.DeletedRegion.ValueGetHashCode();
                }

                if (obj.InsertedContent != null)
                {
                    result = (result * 31) + obj.InsertedContent.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Replacement for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
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

            if (left.Offset != right.Offset)
            {
                return false;
            }

            if (left.DeletedLength != right.DeletedLength)
            {
                return false;
            }

            if (left.InsertedBytes != right.InsertedBytes)
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
                result = (result * 31) + obj.Offset.GetHashCode();
                result = (result * 31) + obj.DeletedLength.GetHashCode();
                if (obj.InsertedBytes != null)
                {
                    result = (result * 31) + obj.InsertedBytes.GetHashCode();
                }
            }

            return result;
        }
    }
}
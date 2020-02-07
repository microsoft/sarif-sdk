// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ExceptionDataVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class ExceptionDataVersionOneEqualityComparer : IEqualityComparer<ExceptionDataVersionOne>
    {
        internal static readonly ExceptionDataVersionOneEqualityComparer Instance = new ExceptionDataVersionOneEqualityComparer();

        public bool Equals(ExceptionDataVersionOne left, ExceptionDataVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Kind != right.Kind)
            {
                return false;
            }

            if (left.Message != right.Message)
            {
                return false;
            }

            if (!StackVersionOne.ValueComparer.Equals(left.Stack, right.Stack))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.InnerExceptions, right.InnerExceptions))
            {
                if (left.InnerExceptions == null || right.InnerExceptions == null)
                {
                    return false;
                }

                if (left.InnerExceptions.Count != right.InnerExceptions.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.InnerExceptions.Count; ++index_0)
                {
                    if (!ExceptionDataVersionOne.ValueComparer.Equals(left.InnerExceptions[index_0], right.InnerExceptions[index_0]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(ExceptionDataVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Kind != null)
                {
                    result = (result * 31) + obj.Kind.GetHashCode();
                }

                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.GetHashCode();
                }

                if (obj.Stack != null)
                {
                    result = (result * 31) + obj.Stack.ValueGetHashCode();
                }

                if (obj.InnerExceptions != null)
                {
                    foreach (var value_0 in obj.InnerExceptions)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.ValueGetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}
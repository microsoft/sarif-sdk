// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Request for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    internal sealed class RequestEqualityComparer : IEqualityComparer<Request>
    {
        internal static readonly RequestEqualityComparer Instance = new RequestEqualityComparer();

        public bool Equals(Request left, Request right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Index != right.Index)
            {
                return false;
            }

            if (left.Protocol != right.Protocol)
            {
                return false;
            }

            if (left.Version != right.Version)
            {
                return false;
            }

            if (left.Uri != right.Uri)
            {
                return false;
            }

            if (left.Method != right.Method)
            {
                return false;
            }

            if (!object.Equals(left.Headers, right.Headers))
            {
                return false;
            }

            if (!object.Equals(left.Parameters, right.Parameters))
            {
                return false;
            }

            if (left.Body != right.Body)
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

        public int GetHashCode(Request obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Index.GetHashCode();
                if (obj.Protocol != null)
                {
                    result = (result * 31) + obj.Protocol.GetHashCode();
                }

                if (obj.Version != null)
                {
                    result = (result * 31) + obj.Version.GetHashCode();
                }

                if (obj.Uri != null)
                {
                    result = (result * 31) + obj.Uri.GetHashCode();
                }

                if (obj.Method != null)
                {
                    result = (result * 31) + obj.Method.GetHashCode();
                }

                if (obj.Headers != null)
                {
                    result = (result * 31) + obj.Headers.GetHashCode();
                }

                if (obj.Parameters != null)
                {
                    result = (result * 31) + obj.Parameters.GetHashCode();
                }

                if (obj.Body != null)
                {
                    result = (result * 31) + obj.Body.GetHashCode();
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
// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Response for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    internal sealed class ResponseEqualityComparer : IEqualityComparer<Response>
    {
        internal static readonly ResponseEqualityComparer Instance = new ResponseEqualityComparer();

        public bool Equals(Response left, Response right)
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

            if (left.StatusCode != right.StatusCode)
            {
                return false;
            }

            if (left.ReasonPhrase != right.ReasonPhrase)
            {
                return false;
            }

            if (!object.Equals(left.Headers, right.Headers))
            {
                return false;
            }

            if (!ArtifactContent.ValueComparer.Equals(left.Body, right.Body))
            {
                return false;
            }

            if (left.NoResponseReceived != right.NoResponseReceived)
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

        public int GetHashCode(Response obj)
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

                result = (result * 31) + obj.StatusCode.GetHashCode();
                if (obj.ReasonPhrase != null)
                {
                    result = (result * 31) + obj.ReasonPhrase.GetHashCode();
                }

                if (obj.Headers != null)
                {
                    result = (result * 31) + obj.Headers.GetHashCode();
                }

                if (obj.Body != null)
                {
                    result = (result * 31) + obj.Body.ValueGetHashCode();
                }

                result = (result * 31) + obj.NoResponseReceived.GetHashCode();
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
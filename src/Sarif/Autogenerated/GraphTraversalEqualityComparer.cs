// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type GraphTraversal for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class GraphTraversalEqualityComparer : IEqualityComparer<GraphTraversal>
    {
        internal static readonly GraphTraversalEqualityComparer Instance = new GraphTraversalEqualityComparer();

        public bool Equals(GraphTraversal left, GraphTraversal right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.RunGraphIndex != right.RunGraphIndex)
            {
                return false;
            }

            if (left.ResultGraphIndex != right.ResultGraphIndex)
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Description, right.Description))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.InitialState, right.InitialState))
            {
                if (left.InitialState == null || right.InitialState == null || left.InitialState.Count != right.InitialState.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.InitialState)
                {
                    MultiformatMessageString value_1;
                    if (!right.InitialState.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!MultiformatMessageString.ValueComparer.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
            }

            if (!object.Equals(left.ImmutableState, right.ImmutableState))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.EdgeTraversals, right.EdgeTraversals))
            {
                if (left.EdgeTraversals == null || right.EdgeTraversals == null)
                {
                    return false;
                }

                if (left.EdgeTraversals.Count != right.EdgeTraversals.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.EdgeTraversals.Count; ++index_0)
                {
                    if (!EdgeTraversal.ValueComparer.Equals(left.EdgeTraversals[index_0], right.EdgeTraversals[index_0]))
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

        public int GetHashCode(GraphTraversal obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.RunGraphIndex.GetHashCode();
                result = (result * 31) + obj.ResultGraphIndex.GetHashCode();
                if (obj.Description != null)
                {
                    result = (result * 31) + obj.Description.ValueGetHashCode();
                }

                if (obj.InitialState != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.InitialState)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.ImmutableState != null)
                {
                    result = (result * 31) + obj.ImmutableState.GetHashCode();
                }

                if (obj.EdgeTraversals != null)
                {
                    foreach (var value_5 in obj.EdgeTraversals)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_6 in obj.Properties)
                    {
                        xor_1 ^= value_6.Key.GetHashCode();
                        if (value_6.Value != null)
                        {
                            xor_1 ^= value_6.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type AnnotatedCodeLocationVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class AnnotatedCodeLocationVersionOneEqualityComparer : IEqualityComparer<AnnotatedCodeLocationVersionOne>
    {
        internal static readonly AnnotatedCodeLocationVersionOneEqualityComparer Instance = new AnnotatedCodeLocationVersionOneEqualityComparer();

        public bool Equals(AnnotatedCodeLocationVersionOne left, AnnotatedCodeLocationVersionOne right)
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

            if (left.Step != right.Step)
            {
                return false;
            }

            if (!PhysicalLocationVersionOne.ValueComparer.Equals(left.PhysicalLocation, right.PhysicalLocation))
            {
                return false;
            }

            if (left.FullyQualifiedLogicalName != right.FullyQualifiedLogicalName)
            {
                return false;
            }

            if (left.LogicalLocationKey != right.LogicalLocationKey)
            {
                return false;
            }

            if (left.Module != right.Module)
            {
                return false;
            }

            if (left.ThreadId != right.ThreadId)
            {
                return false;
            }

            if (left.Message != right.Message)
            {
                return false;
            }

            if (left.Kind != right.Kind)
            {
                return false;
            }

            if (left.TaintKind != right.TaintKind)
            {
                return false;
            }

            if (left.Target != right.Target)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Values, right.Values))
            {
                if (left.Values == null || right.Values == null)
                {
                    return false;
                }

                if (left.Values.Count != right.Values.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Values.Count; ++index_0)
                {
                    if (left.Values[index_0] != right.Values[index_0])
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.State, right.State))
            {
                if (left.State == null || right.State == null || left.State.Count != right.State.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.State)
                {
                    string value_1;
                    if (!right.State.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (left.TargetKey != right.TargetKey)
            {
                return false;
            }

            if (left.Essential != right.Essential)
            {
                return false;
            }

            if (left.Importance != right.Importance)
            {
                return false;
            }

            if (left.Snippet != right.Snippet)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Annotations, right.Annotations))
            {
                if (left.Annotations == null || right.Annotations == null)
                {
                    return false;
                }

                if (left.Annotations.Count != right.Annotations.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.Annotations.Count; ++index_1)
                {
                    if (!AnnotationVersionOne.ValueComparer.Equals(left.Annotations[index_1], right.Annotations[index_1]))
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

        public int GetHashCode(AnnotatedCodeLocationVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Id.GetHashCode();
                result = (result * 31) + obj.Step.GetHashCode();
                if (obj.PhysicalLocation != null)
                {
                    result = (result * 31) + obj.PhysicalLocation.ValueGetHashCode();
                }

                if (obj.FullyQualifiedLogicalName != null)
                {
                    result = (result * 31) + obj.FullyQualifiedLogicalName.GetHashCode();
                }

                if (obj.LogicalLocationKey != null)
                {
                    result = (result * 31) + obj.LogicalLocationKey.GetHashCode();
                }

                if (obj.Module != null)
                {
                    result = (result * 31) + obj.Module.GetHashCode();
                }

                result = (result * 31) + obj.ThreadId.GetHashCode();
                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.GetHashCode();
                }

                result = (result * 31) + obj.Kind.GetHashCode();
                result = (result * 31) + obj.TaintKind.GetHashCode();
                if (obj.Target != null)
                {
                    result = (result * 31) + obj.Target.GetHashCode();
                }

                if (obj.Values != null)
                {
                    foreach (var value_2 in obj.Values)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }

                if (obj.State != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.State)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.TargetKey != null)
                {
                    result = (result * 31) + obj.TargetKey.GetHashCode();
                }

                result = (result * 31) + obj.Essential.GetHashCode();
                result = (result * 31) + obj.Importance.GetHashCode();
                if (obj.Snippet != null)
                {
                    result = (result * 31) + obj.Snippet.GetHashCode();
                }

                if (obj.Annotations != null)
                {
                    foreach (var value_3 in obj.Annotations)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.Properties)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}
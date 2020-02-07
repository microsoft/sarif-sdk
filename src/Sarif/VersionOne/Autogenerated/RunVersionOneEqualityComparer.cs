// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type RunVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class RunVersionOneEqualityComparer : IEqualityComparer<RunVersionOne>
    {
        internal static readonly RunVersionOneEqualityComparer Instance = new RunVersionOneEqualityComparer();

        public bool Equals(RunVersionOne left, RunVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!ToolVersionOne.ValueComparer.Equals(left.Tool, right.Tool))
            {
                return false;
            }

            if (!InvocationVersionOne.ValueComparer.Equals(left.Invocation, right.Invocation))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Files, right.Files))
            {
                if (left.Files == null || right.Files == null || left.Files.Count != right.Files.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Files)
                {
                    FileDataVersionOne value_1;
                    if (!right.Files.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!FileDataVersionOne.ValueComparer.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.LogicalLocations, right.LogicalLocations))
            {
                if (left.LogicalLocations == null || right.LogicalLocations == null || left.LogicalLocations.Count != right.LogicalLocations.Count)
                {
                    return false;
                }

                foreach (var value_2 in left.LogicalLocations)
                {
                    LogicalLocationVersionOne value_3;
                    if (!right.LogicalLocations.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (!LogicalLocationVersionOne.ValueComparer.Equals(value_2.Value, value_3))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Results, right.Results))
            {
                if (left.Results == null || right.Results == null)
                {
                    return false;
                }

                if (left.Results.Count != right.Results.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Results.Count; ++index_0)
                {
                    if (!ResultVersionOne.ValueComparer.Equals(left.Results[index_0], right.Results[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.ToolNotifications, right.ToolNotifications))
            {
                if (left.ToolNotifications == null || right.ToolNotifications == null)
                {
                    return false;
                }

                if (left.ToolNotifications.Count != right.ToolNotifications.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.ToolNotifications.Count; ++index_1)
                {
                    if (!NotificationVersionOne.ValueComparer.Equals(left.ToolNotifications[index_1], right.ToolNotifications[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.ConfigurationNotifications, right.ConfigurationNotifications))
            {
                if (left.ConfigurationNotifications == null || right.ConfigurationNotifications == null)
                {
                    return false;
                }

                if (left.ConfigurationNotifications.Count != right.ConfigurationNotifications.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < left.ConfigurationNotifications.Count; ++index_2)
                {
                    if (!NotificationVersionOne.ValueComparer.Equals(left.ConfigurationNotifications[index_2], right.ConfigurationNotifications[index_2]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Rules, right.Rules))
            {
                if (left.Rules == null || right.Rules == null || left.Rules.Count != right.Rules.Count)
                {
                    return false;
                }

                foreach (var value_4 in left.Rules)
                {
                    RuleVersionOne value_5;
                    if (!right.Rules.TryGetValue(value_4.Key, out value_5))
                    {
                        return false;
                    }

                    if (!RuleVersionOne.ValueComparer.Equals(value_4.Value, value_5))
                    {
                        return false;
                    }
                }
            }

            if (left.Id != right.Id)
            {
                return false;
            }

            if (left.StableId != right.StableId)
            {
                return false;
            }

            if (left.AutomationId != right.AutomationId)
            {
                return false;
            }

            if (left.BaselineId != right.BaselineId)
            {
                return false;
            }

            if (left.Architecture != right.Architecture)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_6 in left.Properties)
                {
                    SerializedPropertyInfo value_7;
                    if (!right.Properties.TryGetValue(value_6.Key, out value_7))
                    {
                        return false;
                    }

                    if (!object.Equals(value_6.Value, value_7))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(RunVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Tool != null)
                {
                    result = (result * 31) + obj.Tool.ValueGetHashCode();
                }

                if (obj.Invocation != null)
                {
                    result = (result * 31) + obj.Invocation.ValueGetHashCode();
                }

                if (obj.Files != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_8 in obj.Files)
                    {
                        xor_0 ^= value_8.Key.GetHashCode();
                        if (value_8.Value != null)
                        {
                            xor_0 ^= value_8.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.LogicalLocations != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_9 in obj.LogicalLocations)
                    {
                        xor_1 ^= value_9.Key.GetHashCode();
                        if (value_9.Value != null)
                        {
                            xor_1 ^= value_9.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (obj.Results != null)
                {
                    foreach (var value_10 in obj.Results)
                    {
                        result = result * 31;
                        if (value_10 != null)
                        {
                            result = (result * 31) + value_10.ValueGetHashCode();
                        }
                    }
                }

                if (obj.ToolNotifications != null)
                {
                    foreach (var value_11 in obj.ToolNotifications)
                    {
                        result = result * 31;
                        if (value_11 != null)
                        {
                            result = (result * 31) + value_11.ValueGetHashCode();
                        }
                    }
                }

                if (obj.ConfigurationNotifications != null)
                {
                    foreach (var value_12 in obj.ConfigurationNotifications)
                    {
                        result = result * 31;
                        if (value_12 != null)
                        {
                            result = (result * 31) + value_12.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Rules != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_2 = 0;
                    foreach (var value_13 in obj.Rules)
                    {
                        xor_2 ^= value_13.Key.GetHashCode();
                        if (value_13.Value != null)
                        {
                            xor_2 ^= value_13.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_2;
                }

                if (obj.Id != null)
                {
                    result = (result * 31) + obj.Id.GetHashCode();
                }

                if (obj.StableId != null)
                {
                    result = (result * 31) + obj.StableId.GetHashCode();
                }

                if (obj.AutomationId != null)
                {
                    result = (result * 31) + obj.AutomationId.GetHashCode();
                }

                if (obj.BaselineId != null)
                {
                    result = (result * 31) + obj.BaselineId.GetHashCode();
                }

                if (obj.Architecture != null)
                {
                    result = (result * 31) + obj.Architecture.GetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_3 = 0;
                    foreach (var value_14 in obj.Properties)
                    {
                        xor_3 ^= value_14.Key.GetHashCode();
                        if (value_14.Value != null)
                        {
                            xor_3 ^= value_14.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_3;
                }
            }

            return result;
        }
    }
}
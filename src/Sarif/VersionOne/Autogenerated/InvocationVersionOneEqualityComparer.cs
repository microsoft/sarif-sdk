// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type InvocationVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class InvocationVersionOneEqualityComparer : IEqualityComparer<InvocationVersionOne>
    {
        internal static readonly InvocationVersionOneEqualityComparer Instance = new InvocationVersionOneEqualityComparer();

        public bool Equals(InvocationVersionOne left, InvocationVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.CommandLine != right.CommandLine)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.ResponseFiles, right.ResponseFiles))
            {
                if (left.ResponseFiles == null || right.ResponseFiles == null || left.ResponseFiles.Count != right.ResponseFiles.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.ResponseFiles)
                {
                    string value_1;
                    if (!right.ResponseFiles.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (left.StartTime != right.StartTime)
            {
                return false;
            }

            if (left.EndTime != right.EndTime)
            {
                return false;
            }

            if (left.Machine != right.Machine)
            {
                return false;
            }

            if (left.Account != right.Account)
            {
                return false;
            }

            if (left.ProcessId != right.ProcessId)
            {
                return false;
            }

            if (left.FileName != right.FileName)
            {
                return false;
            }

            if (left.WorkingDirectory != right.WorkingDirectory)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.EnvironmentVariables, right.EnvironmentVariables))
            {
                if (left.EnvironmentVariables == null || right.EnvironmentVariables == null || left.EnvironmentVariables.Count != right.EnvironmentVariables.Count)
                {
                    return false;
                }

                foreach (var value_2 in left.EnvironmentVariables)
                {
                    string value_3;
                    if (!right.EnvironmentVariables.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (value_2.Value != value_3)
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

                foreach (var value_4 in left.Properties)
                {
                    SerializedPropertyInfo value_5;
                    if (!right.Properties.TryGetValue(value_4.Key, out value_5))
                    {
                        return false;
                    }

                    if (!object.Equals(value_4.Value, value_5))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(InvocationVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.CommandLine != null)
                {
                    result = (result * 31) + obj.CommandLine.GetHashCode();
                }

                if (obj.ResponseFiles != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_6 in obj.ResponseFiles)
                    {
                        xor_0 ^= value_6.Key.GetHashCode();
                        if (value_6.Value != null)
                        {
                            xor_0 ^= value_6.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                result = (result * 31) + obj.StartTime.GetHashCode();
                result = (result * 31) + obj.EndTime.GetHashCode();
                if (obj.Machine != null)
                {
                    result = (result * 31) + obj.Machine.GetHashCode();
                }

                if (obj.Account != null)
                {
                    result = (result * 31) + obj.Account.GetHashCode();
                }

                result = (result * 31) + obj.ProcessId.GetHashCode();
                if (obj.FileName != null)
                {
                    result = (result * 31) + obj.FileName.GetHashCode();
                }

                if (obj.WorkingDirectory != null)
                {
                    result = (result * 31) + obj.WorkingDirectory.GetHashCode();
                }

                if (obj.EnvironmentVariables != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_7 in obj.EnvironmentVariables)
                    {
                        xor_1 ^= value_7.Key.GetHashCode();
                        if (value_7.Value != null)
                        {
                            xor_1 ^= value_7.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_2 = 0;
                    foreach (var value_8 in obj.Properties)
                    {
                        xor_2 ^= value_8.Key.GetHashCode();
                        if (value_8.Value != null)
                        {
                            xor_2 ^= value_8.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_2;
                }
            }

            return result;
        }
    }
}
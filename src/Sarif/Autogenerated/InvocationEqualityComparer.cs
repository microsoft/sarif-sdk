// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Invocation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class InvocationEqualityComparer : IEqualityComparer<Invocation>
    {
        internal static readonly InvocationEqualityComparer Instance = new InvocationEqualityComparer();

        public bool Equals(Invocation left, Invocation right)
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

            if (!object.ReferenceEquals(left.Arguments, right.Arguments))
            {
                if (left.Arguments == null || right.Arguments == null)
                {
                    return false;
                }

                if (left.Arguments.Count != right.Arguments.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Arguments.Count; ++index_0)
                {
                    if (left.Arguments[index_0] != right.Arguments[index_0])
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.ResponseFiles, right.ResponseFiles))
            {
                if (left.ResponseFiles == null || right.ResponseFiles == null)
                {
                    return false;
                }

                if (left.ResponseFiles.Count != right.ResponseFiles.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.ResponseFiles.Count; ++index_1)
                {
                    if (!ArtifactLocation.ValueComparer.Equals(left.ResponseFiles[index_1], right.ResponseFiles[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (left.StartTimeUtc != right.StartTimeUtc)
            {
                return false;
            }

            if (left.EndTimeUtc != right.EndTimeUtc)
            {
                return false;
            }

            if (left.ExitCode != right.ExitCode)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.RuleConfigurationOverrides, right.RuleConfigurationOverrides))
            {
                if (left.RuleConfigurationOverrides == null || right.RuleConfigurationOverrides == null)
                {
                    return false;
                }

                if (left.RuleConfigurationOverrides.Count != right.RuleConfigurationOverrides.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < left.RuleConfigurationOverrides.Count; ++index_2)
                {
                    if (!ConfigurationOverride.ValueComparer.Equals(left.RuleConfigurationOverrides[index_2], right.RuleConfigurationOverrides[index_2]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.NotificationConfigurationOverrides, right.NotificationConfigurationOverrides))
            {
                if (left.NotificationConfigurationOverrides == null || right.NotificationConfigurationOverrides == null)
                {
                    return false;
                }

                if (left.NotificationConfigurationOverrides.Count != right.NotificationConfigurationOverrides.Count)
                {
                    return false;
                }

                for (int index_3 = 0; index_3 < left.NotificationConfigurationOverrides.Count; ++index_3)
                {
                    if (!ConfigurationOverride.ValueComparer.Equals(left.NotificationConfigurationOverrides[index_3], right.NotificationConfigurationOverrides[index_3]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.ToolExecutionNotifications, right.ToolExecutionNotifications))
            {
                if (left.ToolExecutionNotifications == null || right.ToolExecutionNotifications == null)
                {
                    return false;
                }

                if (left.ToolExecutionNotifications.Count != right.ToolExecutionNotifications.Count)
                {
                    return false;
                }

                for (int index_4 = 0; index_4 < left.ToolExecutionNotifications.Count; ++index_4)
                {
                    if (!Notification.ValueComparer.Equals(left.ToolExecutionNotifications[index_4], right.ToolExecutionNotifications[index_4]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.ToolConfigurationNotifications, right.ToolConfigurationNotifications))
            {
                if (left.ToolConfigurationNotifications == null || right.ToolConfigurationNotifications == null)
                {
                    return false;
                }

                if (left.ToolConfigurationNotifications.Count != right.ToolConfigurationNotifications.Count)
                {
                    return false;
                }

                for (int index_5 = 0; index_5 < left.ToolConfigurationNotifications.Count; ++index_5)
                {
                    if (!Notification.ValueComparer.Equals(left.ToolConfigurationNotifications[index_5], right.ToolConfigurationNotifications[index_5]))
                    {
                        return false;
                    }
                }
            }

            if (left.ExitCodeDescription != right.ExitCodeDescription)
            {
                return false;
            }

            if (left.ExitSignalName != right.ExitSignalName)
            {
                return false;
            }

            if (left.ExitSignalNumber != right.ExitSignalNumber)
            {
                return false;
            }

            if (left.ProcessStartFailureMessage != right.ProcessStartFailureMessage)
            {
                return false;
            }

            if (left.ExecutionSuccessful != right.ExecutionSuccessful)
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

            if (!ArtifactLocation.ValueComparer.Equals(left.ExecutableLocation, right.ExecutableLocation))
            {
                return false;
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.WorkingDirectory, right.WorkingDirectory))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.EnvironmentVariables, right.EnvironmentVariables))
            {
                if (left.EnvironmentVariables == null || right.EnvironmentVariables == null || left.EnvironmentVariables.Count != right.EnvironmentVariables.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.EnvironmentVariables)
                {
                    string value_1;
                    if (!right.EnvironmentVariables.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.Stdin, right.Stdin))
            {
                return false;
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.Stdout, right.Stdout))
            {
                return false;
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.Stderr, right.Stderr))
            {
                return false;
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.StdoutStderr, right.StdoutStderr))
            {
                return false;
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

        public int GetHashCode(Invocation obj)
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

                if (obj.Arguments != null)
                {
                    foreach (var value_4 in obj.Arguments)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.GetHashCode();
                        }
                    }
                }

                if (obj.ResponseFiles != null)
                {
                    foreach (var value_5 in obj.ResponseFiles)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                result = (result * 31) + obj.StartTimeUtc.GetHashCode();
                result = (result * 31) + obj.EndTimeUtc.GetHashCode();
                result = (result * 31) + obj.ExitCode.GetHashCode();
                if (obj.RuleConfigurationOverrides != null)
                {
                    foreach (var value_6 in obj.RuleConfigurationOverrides)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.ValueGetHashCode();
                        }
                    }
                }

                if (obj.NotificationConfigurationOverrides != null)
                {
                    foreach (var value_7 in obj.NotificationConfigurationOverrides)
                    {
                        result = result * 31;
                        if (value_7 != null)
                        {
                            result = (result * 31) + value_7.ValueGetHashCode();
                        }
                    }
                }

                if (obj.ToolExecutionNotifications != null)
                {
                    foreach (var value_8 in obj.ToolExecutionNotifications)
                    {
                        result = result * 31;
                        if (value_8 != null)
                        {
                            result = (result * 31) + value_8.ValueGetHashCode();
                        }
                    }
                }

                if (obj.ToolConfigurationNotifications != null)
                {
                    foreach (var value_9 in obj.ToolConfigurationNotifications)
                    {
                        result = result * 31;
                        if (value_9 != null)
                        {
                            result = (result * 31) + value_9.ValueGetHashCode();
                        }
                    }
                }

                if (obj.ExitCodeDescription != null)
                {
                    result = (result * 31) + obj.ExitCodeDescription.GetHashCode();
                }

                if (obj.ExitSignalName != null)
                {
                    result = (result * 31) + obj.ExitSignalName.GetHashCode();
                }

                result = (result * 31) + obj.ExitSignalNumber.GetHashCode();
                if (obj.ProcessStartFailureMessage != null)
                {
                    result = (result * 31) + obj.ProcessStartFailureMessage.GetHashCode();
                }

                result = (result * 31) + obj.ExecutionSuccessful.GetHashCode();
                if (obj.Machine != null)
                {
                    result = (result * 31) + obj.Machine.GetHashCode();
                }

                if (obj.Account != null)
                {
                    result = (result * 31) + obj.Account.GetHashCode();
                }

                result = (result * 31) + obj.ProcessId.GetHashCode();
                if (obj.ExecutableLocation != null)
                {
                    result = (result * 31) + obj.ExecutableLocation.ValueGetHashCode();
                }

                if (obj.WorkingDirectory != null)
                {
                    result = (result * 31) + obj.WorkingDirectory.ValueGetHashCode();
                }

                if (obj.EnvironmentVariables != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_10 in obj.EnvironmentVariables)
                    {
                        xor_0 ^= value_10.Key.GetHashCode();
                        if (value_10.Value != null)
                        {
                            xor_0 ^= value_10.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.Stdin != null)
                {
                    result = (result * 31) + obj.Stdin.ValueGetHashCode();
                }

                if (obj.Stdout != null)
                {
                    result = (result * 31) + obj.Stdout.ValueGetHashCode();
                }

                if (obj.Stderr != null)
                {
                    result = (result * 31) + obj.Stderr.ValueGetHashCode();
                }

                if (obj.StdoutStderr != null)
                {
                    result = (result * 31) + obj.StdoutStderr.ValueGetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_11 in obj.Properties)
                    {
                        xor_1 ^= value_11.Key.GetHashCode();
                        if (value_11.Value != null)
                        {
                            xor_1 ^= value_11.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}
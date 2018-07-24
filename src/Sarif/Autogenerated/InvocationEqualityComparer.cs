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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.56.0.0")]
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

            if (!object.ReferenceEquals(left.Attachments, right.Attachments))
            {
                if (left.Attachments == null || right.Attachments == null)
                {
                    return false;
                }

                if (left.Attachments.Count != right.Attachments.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Attachments.Count; ++index_0)
                {
                    if (!Attachment.ValueComparer.Equals(left.Attachments[index_0], right.Attachments[index_0]))
                    {
                        return false;
                    }
                }
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

                for (int index_1 = 0; index_1 < left.Arguments.Count; ++index_1)
                {
                    if (left.Arguments[index_1] != right.Arguments[index_1])
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

                for (int index_2 = 0; index_2 < left.ResponseFiles.Count; ++index_2)
                {
                    if (!FileLocation.ValueComparer.Equals(left.ResponseFiles[index_2], right.ResponseFiles[index_2]))
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

            if (left.ExitCode != right.ExitCode)
            {
                return false;
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

                for (int index_3 = 0; index_3 < left.ToolNotifications.Count; ++index_3)
                {
                    if (!Notification.ValueComparer.Equals(left.ToolNotifications[index_3], right.ToolNotifications[index_3]))
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

                for (int index_4 = 0; index_4 < left.ConfigurationNotifications.Count; ++index_4)
                {
                    if (!Notification.ValueComparer.Equals(left.ConfigurationNotifications[index_4], right.ConfigurationNotifications[index_4]))
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

            if (left.ToolExecutionSuccessful != right.ToolExecutionSuccessful)
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

            if (!FileLocation.ValueComparer.Equals(left.ExecutableLocation, right.ExecutableLocation))
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

            if (!FileLocation.ValueComparer.Equals(left.Stdin, right.Stdin))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.Stdout, right.Stdout))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.Stderr, right.Stderr))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.StdoutStderr, right.StdoutStderr))
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
                if (obj.Attachments != null)
                {
                    foreach (var value_4 in obj.Attachments)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.ValueGetHashCode();
                        }
                    }
                }

                if (obj.CommandLine != null)
                {
                    result = (result * 31) + obj.CommandLine.GetHashCode();
                }

                if (obj.Arguments != null)
                {
                    foreach (var value_5 in obj.Arguments)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.GetHashCode();
                        }
                    }
                }

                if (obj.ResponseFiles != null)
                {
                    foreach (var value_6 in obj.ResponseFiles)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.ValueGetHashCode();
                        }
                    }
                }

                result = (result * 31) + obj.StartTime.GetHashCode();
                result = (result * 31) + obj.EndTime.GetHashCode();
                result = (result * 31) + obj.ExitCode.GetHashCode();
                if (obj.ToolNotifications != null)
                {
                    foreach (var value_7 in obj.ToolNotifications)
                    {
                        result = result * 31;
                        if (value_7 != null)
                        {
                            result = (result * 31) + value_7.ValueGetHashCode();
                        }
                    }
                }

                if (obj.ConfigurationNotifications != null)
                {
                    foreach (var value_8 in obj.ConfigurationNotifications)
                    {
                        result = result * 31;
                        if (value_8 != null)
                        {
                            result = (result * 31) + value_8.ValueGetHashCode();
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

                result = (result * 31) + obj.ToolExecutionSuccessful.GetHashCode();
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
                    result = (result * 31) + obj.WorkingDirectory.GetHashCode();
                }

                if (obj.EnvironmentVariables != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_9 in obj.EnvironmentVariables)
                    {
                        xor_0 ^= value_9.Key.GetHashCode();
                        if (value_9.Value != null)
                        {
                            xor_0 ^= value_9.Value.GetHashCode();
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
                    foreach (var value_10 in obj.Properties)
                    {
                        xor_1 ^= value_10.Key.GetHashCode();
                        if (value_10.Value != null)
                        {
                            xor_1 ^= value_10.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}
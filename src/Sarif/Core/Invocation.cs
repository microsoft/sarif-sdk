// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Invocation
    {
        private IEnumerable<string> _propertiesToLog;
        private bool _suppressNonDeterministicProperties;

        public static Invocation Create(
            bool emitMachineEnvironment = false,
            bool emitTimestamps = true,
            IEnumerable<string> propertiesToLog = null)
        {
            var invocation = new Invocation
            {
                _suppressNonDeterministicProperties = !emitTimestamps,
                _propertiesToLog = propertiesToLog?.Select(p => p.ToUpperInvariant()).ToList()
            };

            if (emitTimestamps)
            {
                invocation.StartTimeUtc = DateTime.UtcNow;
            }

            if (invocation.ShouldLog(nameof(ProcessId)))
            {
                invocation.ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            }

            if (invocation.ShouldLog(nameof(WorkingDirectory)))
            {
                invocation.WorkingDirectory = new ArtifactLocation { Uri = new Uri(Environment.CurrentDirectory) };
            }

            if (invocation.ShouldLog(nameof(CommandLine)))
            {
                invocation.CommandLine = Environment.CommandLine;
            }

            if (emitMachineEnvironment)
            {
                invocation.Machine = Environment.MachineName;
                invocation.Account = Environment.UserName;
                invocation.EnvironmentVariables = CopyEnvironmentVariables();
            }

            if (invocation.ShouldLog(nameof(ExecutableLocation)))
            {
                Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                invocation.ExecutableLocation.Uri = new Uri(assembly.Location);
            }

            invocation.ExecutionSuccessful = true;

            return invocation;
        }

        private static IDictionary<string, string> CopyEnvironmentVariables()
        {
            var result = new Dictionary<string, string>();
            IDictionary variables = Environment.GetEnvironmentVariables();

            foreach (DictionaryEntry entry in variables)
            {
                result[(string)entry.Key] = (string)entry.Value;
            }

            return result;
        }

        private bool ShouldLog(string propertyName)
        {
            return _propertiesToLog != null && _propertiesToLog.Contains(propertyName.ToUpperInvariant());
        }

        public bool ShouldSerializeArguments()
        {
            return this.Arguments != null &&
                (this.Arguments.Where((e) => { return e != null; }).Count() == this.Arguments.Count);
        }

        public bool ShouldSerializeToolExecutionNotifications()
        {
            return this.ToolExecutionNotifications.HasAtLeastOneNonNullValue();
        }

        public bool ShouldSerializeToolConfigurationNotifications()
        {
            return this.ToolConfigurationNotifications.HasAtLeastOneNonNullValue();
        }

        public bool ShouldSerializeStartTimeUtc()
        {
            return !_suppressNonDeterministicProperties;
        }

        public bool ShouldSerializeEndTimeUtc()
        {
            return !_suppressNonDeterministicProperties;
        }
    }
}

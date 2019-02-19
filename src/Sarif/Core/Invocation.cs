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
        private IEnumerable<string> PropertiesToLog {get; set; }

        public static Invocation Create(
            bool emitMachineEnvironment = false,
            IEnumerable<string> propertiesToLog = null)
        {
            var invocation = new Invocation
            {
                PropertiesToLog = propertiesToLog?.Select(p => p.ToUpperInvariant()).ToList()
            };

            invocation.StartTimeUtc = DateTime.UtcNow;

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

            return invocation;
        }

        private static IDictionary<string, string> CopyEnvironmentVariables()
        {
            var result = new Dictionary<string, string>();
            var variables = Environment.GetEnvironmentVariables();

            foreach (DictionaryEntry entry in variables)
            {
                result[(string)entry.Key] = (string)entry.Value;
            }

            return result;
        }

        private bool ShouldLog(string propertyName)
        {
            return PropertiesToLog != null && PropertiesToLog.Contains(propertyName.ToUpperInvariant());
        }

        public bool ShouldSerializeArguments()
        {
            return this.Arguments != null &&
                (this.Arguments.Where((e) => { return e != null; }).Count() == this.Arguments.Count);
        }

        public bool ShouldSerializeToolNotifications()
        {
            return this.ToolNotifications.HasAtLeastOneNonNullValue();
        }

        public bool ShouldSerializeConfigurationNotifications()
        {
            return this.ConfigurationNotifications.HasAtLeastOneNonNullValue();
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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

            invocation.StartTime = DateTime.UtcNow;

            if (invocation.ShouldLog(nameof(ProcessId)))
            {
                invocation.ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            }

            if (invocation.ShouldLog(nameof(WorkingDirectory)))
            {
                invocation.WorkingDirectory = Environment.CurrentDirectory;
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

            if (invocation.ShouldLog(nameof(FileName)))
            {
                Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                invocation.FileName = assembly.Location;
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
    }
}

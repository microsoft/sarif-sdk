// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Invocation
    {
        public static Invocation Create(bool emitMachineEnvironment = false)
        {
            var invocation = new Invocation();

            invocation.StartTime = DateTime.UtcNow;
            invocation.ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            invocation.WorkingDirectory = Environment.CurrentDirectory;
            invocation.CommandLine = Environment.CommandLine;

            if (emitMachineEnvironment)
            {
                invocation.Machine = Environment.MachineName;
                invocation.Account = Environment.UserName;
                invocation.EnvironmentVariables = CopyEnvironmentVariables();
            }

            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            invocation.FileName = assembly.Location;

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
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Invocation
    {
        public static Invocation Create()
        {
            return new Invocation
            {
                StartTime = DateTime.UtcNow,
                EnvironmentVariables = CopyEnvironmentVariables(),
                Parameters = Environment.CommandLine,
                Machine = Environment.MachineName,
                ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id,
                WorkingDirectory = Environment.CurrentDirectory
            };
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

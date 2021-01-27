// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class OptionsInterpretter
    {
        //  Poor man's dependency injection
        public OptionsInterpretter() : this (new EnvironmentVariableGetter())
        {

        }
        
        public OptionsInterpretter(IEnvironmentVariableGetter environmentVariableGetter)
        {
            if (environmentVariableGetter == null)
            {
                throw new ArgumentException("Required parameter", nameof(environmentVariableGetter));
            }

            _environmentVariableGetter = environmentVariableGetter;
        }

        private readonly IEnvironmentVariableGetter _environmentVariableGetter;

        public void ConsumeEnvVarsAndInterpretOptions(AnalyzeOptionsBase analyzeOptionsBase)
        {
            //  Level and Kind are the only two properties we're reading from env variables for now.  See if they are populated.
            IEnumerable<ResultKind> kindsFromEnv = getOptionEnumFromEnvVar<ResultKind>(nameof(analyzeOptionsBase.Kind));
            IEnumerable<FailureLevel> levelsFromEnv = getOptionEnumFromEnvVar<FailureLevel>(nameof(analyzeOptionsBase.Level));

            //  Union in C# is distinct
            analyzeOptionsBase.Kind = analyzeOptionsBase.Kind.Union(kindsFromEnv);
            analyzeOptionsBase.Level = analyzeOptionsBase.Level.Union(levelsFromEnv);
        }

        private IEnumerable<T> getOptionEnumFromEnvVar<T>(string propertyName)
        {
            return _environmentVariableGetter.GetEnvironmentVariable(FormEnvironmentVariableName(propertyName))?.Split(';')?.Select(x => Enum.Parse(typeof(T), x, ignoreCase: true))?.ToList()?.Cast<T>();
        }

        private static string FormEnvironmentVariableName(string propertyName)
        {
            return string.Format(SdkResources.EnvironmentVariable_Additive_Format, propertyName.ToUpperInvariant());
        }
    }
}

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
        public OptionsInterpretter() : this(new EnvironmentVariableGetter())
        {

        }

        public OptionsInterpretter(IEnvironmentVariableGetter environmentVariableGetter)
        {
            _environmentVariableGetter = environmentVariableGetter ?? throw new ArgumentNullException(nameof(environmentVariableGetter));
        }

        private readonly IEnvironmentVariableGetter _environmentVariableGetter;

        public void ConsumeEnvVarsAndInterpretOptions(AnalyzeOptionsBase analyzeOptionsBase)
        {
            //  Level and Kind are the only two properties we're reading from env variables for now.  See if they are populated.
            IEnumerable<ResultKind> kindsFromEnv = GetOptionEnumFromEnvVar<ResultKind>(nameof(analyzeOptionsBase.Kind));
            IEnumerable<FailureLevel> levelsFromEnv = GetOptionEnumFromEnvVar<FailureLevel>(nameof(analyzeOptionsBase.Level));

            //  Union in C# is distinct
            if (kindsFromEnv != null)
            {
                analyzeOptionsBase.Kind = analyzeOptionsBase?.Kind.Union(kindsFromEnv);
            }

            if (levelsFromEnv != null)
            {
                analyzeOptionsBase.Level = analyzeOptionsBase?.Level.Union(levelsFromEnv);
            }
        }

        private IEnumerable<T> GetOptionEnumFromEnvVar<T>(string propertyName)
        {
            return _environmentVariableGetter
                .GetEnvironmentVariable(FormEnvironmentVariableName(propertyName))?
                .Split(';')?
                .Select(x => Enum.Parse(typeof(T), x, ignoreCase: true))?
                .ToList()?
                .Cast<T>();
        }

        private static string FormEnvironmentVariableName(string propertyName)
        {
            return string.Format(SdkResources.EnvironmentVariable_Additive_Format, propertyName.ToUpperInvariant());
        }
    }
}

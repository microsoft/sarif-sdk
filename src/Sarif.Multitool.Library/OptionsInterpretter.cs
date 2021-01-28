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

        public void ConsumeEnvVarsAndInterpretOptions(CommonOptionsBase commonOptionsBase)
        {
            IEnumerable<OptionallyEmittedData> toInsertFromEnvVar =
                GetOptionEnumFromEnvVar<OptionallyEmittedData>(nameof(commonOptionsBase.DataToInsert));
            IEnumerable<OptionallyEmittedData> toRemoveFromEnvVar =
                GetOptionEnumFromEnvVar<OptionallyEmittedData>(nameof(commonOptionsBase.DataToRemove));

            commonOptionsBase.DataToInsert = NullCheckAndDistinctUnionLists(commonOptionsBase.DataToInsert, toInsertFromEnvVar);
            commonOptionsBase.DataToRemove = NullCheckAndDistinctUnionLists(commonOptionsBase.DataToRemove, toRemoveFromEnvVar);
        }

        public void ConsumeEnvVarsAndInterpretOptions(AnalyzeOptionsBase analyzeOptionsBase)
        {
            ConsumeEnvVarsAndInterpretOptions((CommonOptionsBase)analyzeOptionsBase);

            //  Level and Kind are the only two properties we're reading from env variables for now.  See if they are populated.
            IEnumerable<ResultKind> kindsFromEnv = GetOptionEnumFromEnvVar<ResultKind>(nameof(analyzeOptionsBase.Kind));
            IEnumerable<FailureLevel> levelsFromEnv = GetOptionEnumFromEnvVar<FailureLevel>(nameof(analyzeOptionsBase.Level));

            analyzeOptionsBase.Kind = NullCheckAndDistinctUnionLists(analyzeOptionsBase.Kind, kindsFromEnv);
            analyzeOptionsBase.Level = NullCheckAndDistinctUnionLists(analyzeOptionsBase.Level, levelsFromEnv);
        }

        private static IEnumerable<T> NullCheckAndDistinctUnionLists<T>(IEnumerable<T> firstList, IEnumerable<T> secondList)
        {
            if (secondList != null)
            {
                //  Union in C# is distinct
                return firstList.Union(secondList);
            }

            return firstList;
        }

        private IEnumerable<T> GetOptionEnumFromEnvVar<T>(string propertyName)
        {
            return _environmentVariableGetter
                .GetEnvironmentVariable(FormEnvironmentVariableName(propertyName))?
                .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)?
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

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

        //  Protected methods for abstract classes and public methods for concrete classes to ensure proper roll up and no 
        //  redundant execution.  Only leaves of the class diagram should have public methods and be called outside this class
        protected void ConsumeEnvVarsAndInterpretOptions(CommonOptionsBase commonOptionsBase)
        {
            IEnumerable<OptionallyEmittedData> toInsertFromEnvVar =
                GetOptionEnumFromEnvVar<OptionallyEmittedData>(nameof(commonOptionsBase.DataToInsert));
            IEnumerable<OptionallyEmittedData> toRemoveFromEnvVar =
                GetOptionEnumFromEnvVar<OptionallyEmittedData>(nameof(commonOptionsBase.DataToRemove));

            commonOptionsBase.DataToInsert = NullCheckAndDistinctUnionLists(commonOptionsBase.DataToInsert, toInsertFromEnvVar);
            commonOptionsBase.DataToRemove = NullCheckAndDistinctUnionLists(commonOptionsBase.DataToRemove, toRemoveFromEnvVar);
        }

#pragma warning disable IDE0060 // Ignore unused parameter for now
        protected void ConsumeEnvVarsAndInterpretOptions(ExportConfigurationOptions exportConfigurationOptions)
#pragma warning restore IDE0060
        {

        }

#pragma warning disable IDE0060 // Ignore unused parameter for now
        protected void ConsumeEnvVarsAndInterpretOptions(ExportRulesMetadataOptions exportConfigurationOptions)
#pragma warning restore IDE0060
        {

        }

        protected void ConsumeEnvVarsAndInterpretOptions(AnalyzeOptionsBase analyzeOptionsBase)
        {
            ConsumeEnvVarsAndInterpretOptions((CommonOptionsBase)analyzeOptionsBase);

            //  Level and Kind are the only two properties we're reading from env variables for now.  See if they are populated.
            IEnumerable<ResultKind> kindsFromEnv = GetOptionEnumFromEnvVar<ResultKind>(nameof(analyzeOptionsBase.Kind));
            IEnumerable<FailureLevel> levelsFromEnv = GetOptionEnumFromEnvVar<FailureLevel>(nameof(analyzeOptionsBase.Level));

            analyzeOptionsBase.Kind = NullCheckAndDistinctUnionLists(analyzeOptionsBase.Kind, kindsFromEnv);
            analyzeOptionsBase.Level = NullCheckAndDistinctUnionLists(analyzeOptionsBase.Level, levelsFromEnv);
        }

        protected void ConsumeEnvVarsAndInterpretOptions(MultipleFilesOptionsBase multipleFilesOptionsBase)
        {
            ConsumeEnvVarsAndInterpretOptions((CommonOptionsBase)multipleFilesOptionsBase);
        }

        protected void ConsumeEnvVarsAndInterpretOptions(SingleFileOptionsBase singleFileOptionsBase)
        {
            ConsumeEnvVarsAndInterpretOptions((CommonOptionsBase)singleFileOptionsBase);
        }

        public void ConsumeEnvVarsAndInterpretOptions(AbsoluteUriOptions absoluteUriOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((MultipleFilesOptionsBase)absoluteUriOptions);
        }

#if DEBUG
        public void ConsumeEnvVarsAndInterpretOptions(AnalyzeTestOptions analyzeTestOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((AnalyzeOptionsBase)analyzeTestOptions);
        }
#endif

        public void ConsumeEnvVarsAndInterpretOptions(ApplyPolicyOptions applyPolicyOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((SingleFileOptionsBase)applyPolicyOptions);
        }

        public void ConsumeEnvVarsAndInterpretOptions(ConvertOptions convertOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((SingleFileOptionsBase)convertOptions);
        }

        public void ConsumeEnvVarsAndInterpretOptions(ExportValidationConfigurationOptions exportValidationConfigurationOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((ExportConfigurationOptions)exportValidationConfigurationOptions);
        }

        public void ConsumeEnvVarsAndInterpretOptions(ExportValidationRulesMetadataOptions exportValidationRulesMetadataOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((ExportRulesMetadataOptions)exportValidationRulesMetadataOptions);
        }

        public void ConsumeEnvVarsAndInterpretOptions(FileWorkItemsOptions fileWorkItemsOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((SingleFileOptionsBase)fileWorkItemsOptions);
        }

        public void ConsumeEnvVarsAndInterpretOptions(ResultMatchingOptions resultMatchingOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((CommonOptionsBase)resultMatchingOptions);
        }

        public void ConsumeEnvVarsAndInterpretOptions(MergeOptions mergeOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((MultipleFilesOptionsBase)mergeOptions);
        }

#pragma warning disable IDE0060 // Ignore unused parameter for now
        public void ConsumeEnvVarsAndInterpretOptions(PageOptions pageOptions)
#pragma warning restore IDE0060
        {

        }

#pragma warning disable IDE0060 // Ignore unused parameter for now
        public void ConsumeEnvVarsAndInterpretOptions(QueryOptions queryOptions)
#pragma warning restore IDE0060
        {

        }

        public void ConsumeEnvVarsAndInterpretOptions(RebaseUriOptions rebaseUriOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((MultipleFilesOptionsBase)rebaseUriOptions);
        }

        public void ConsumeEnvVarsAndInterpretOptions(RewriteOptions rewriteOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((SingleFileOptionsBase)rewriteOptions);
        }

        public void ConsumeEnvVarsAndInterpretOptions(ValidateOptions validateOptions)
        {
            ConsumeEnvVarsAndInterpretOptions((AnalyzeOptionsBase)validateOptions);
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

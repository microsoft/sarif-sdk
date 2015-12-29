// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class ErrorDescriptors
    {
        private const string ERR0997 = "ERR0997";
        private const string ERR0998 = "ERR0998";
        private const string ERR0999 = "ERR0998";
        private const string ERR1001 = "ERR1001";

        public static IRuleDescriptor InvalidConfiguration = new RuleDescriptor()
        {
            Id = ERR0997,
            Name = nameof(InvalidConfiguration),
            FullDescription = SdkResources.ERR0997_InvalidConfiguration_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ERR0997_ExceptionCreatingLogFile),
                    nameof(SdkResources.ERR0997_ExceptionLoadingAnalysisPlugIn),
                    nameof(SdkResources.ERR0997_ExceptionLoadingAnalysisTarget),
                    nameof(SdkResources.ERR0997_MissingRuleConfiguration)
                }, ERR0997)
        };

        public static IRuleDescriptor RuleDisabled = new RuleDescriptor()
        {
            Id = ERR0998,
            Name = nameof(RuleDisabled),
            FullDescription = SdkResources.ERR0998_RuleDisabled_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ERR0998_ExceptionInCanAnalyze),
                    nameof(SdkResources.ERR0998_ExceptionInInitialize),
                    nameof(SdkResources.ERR0998_ExceptionInAnalyze)
                }, ERR0998)
        };

        public static IRuleDescriptor AnalysisHalted = new RuleDescriptor()
        {
            Id = ERR0999,
            Name = nameof(AnalysisHalted),
            FullDescription = SdkResources.ERR0999_AnalysisHalted_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ERR0999_UnhandledEngineException)
                }, ERR0999)
        };

        public static IRuleDescriptor ParseError = new RuleDescriptor()
        {
            Id = ERR1001,
            Name = nameof(ParseError),
            FullDescription = SdkResources.ERR1001_ParseError_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ERR1001_Default)
                }, ERR1001)
        };
    }
}
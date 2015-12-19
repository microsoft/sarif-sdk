// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class ErrorDescriptors
    {
        public static IRuleDescriptor InvalidConfiguration = new RuleDescriptor()
        {
            Id = "ERR0997",
            Name = nameof(InvalidConfiguration),
            FullDescription = SdkResources.InvalidConfiguration_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ExceptionCreatingLogFile),
                    nameof(SdkResources.ExceptionLoadingAnalysisPlugIn),
                    nameof(SdkResources.ExceptionLoadingAnalysisTarget)
                })
        };

        public static IRuleDescriptor UnhandledRuleException = new RuleDescriptor()
        {
            Id = "ERR0998",
            Name = nameof(UnhandledRuleException),
            FullDescription = SdkResources.ExceptionInRule_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ExceptionCheckingApplicability),
                    nameof(SdkResources.ExceptionInitializingRule),
                    nameof(SdkResources.ExceptionAnalyzingTarget)
                })
        };

        public static IRuleDescriptor UnhandledEngineException = new RuleDescriptor()
        {
            Id = "ERR0999",
            Name = nameof(UnhandledEngineException),
            FullDescription = SdkResources.ExceptionInAnalysisEngine_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ExceptionInAnalysisEngine)
                })
        };


        public static IRuleDescriptor ParseError = new RuleDescriptor()
        {
            Id = "ERR1001",
            Name = nameof(ParseError),
            FullDescription = SdkResources.ParseError_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.ParseError)
                })
        };
    }
}
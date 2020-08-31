// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public static class RuleId
    {
        public const string RuleIdentifiersMustBeValid = "SARIF1001";
        public const string UrisMustBeValid = "SARIF1002";
        public const string ExpressUriBaseIdsCorrectly = "SARIF1004";
        public const string UriMustBeAbsolute = "SARIF1005";

        public const string InvocationPropertiesMustBeConsistent = "SARIF1006";
        public const string RegionPropertiesMustBeConsistent = "SARIF1007";
        public const string PhysicalLocationPropertiesMustBeConsistent = "SARIF1008";
        public const string IndexPropertiesMustBeConsistentWithArrays = "SARIF1009";
        public const string RuleIdMustBeConsistent = "SARIF1010";

        public const string ReferenceFinalSchema = "SARIF1011";
        public const string MessageArgumentsMustBeConsistentWithRule = "SARIF1012";

        public const string TerminateMessagesWithPeriod = "SARIF2001";
        public const string ProvideMessageArguments = "SARIF2002";
        public const string ProvideVersionControlProvenance = "SARIF2003";
        public const string OptimizeFileSize = "SARIF2004";
        public const string ProvideToolProperties = "SARIF2005";

        public const string UrisShouldBeReachable = "SARIF2006"; 
        public const string ExpressPathsRelativeToRepoRoot = "SARIF2007";
        public const string ProvideSchema = "SARIF2008";
        public const string ConsiderConventionalIdentifierValues = "SARIF2009";
        public const string ProvideCodeSnippets = "SARIF2010";

        public const string ProvideContextRegion = "SARIF2011";
        public const string ProvideHelpUris = "SARIF2012";
        public const string ProvideEmbeddedFileContent = "SARIF2013";
        public const string ProvideDynamicMessageContent = "SARIF2014";
        public const string EnquoteDynamicMessageContent = "SARIF2015";
        public const string FileUrisShouldBeRelative = "SARIF2016";

        // Rules required to upload SARIF files to the GitHub Developer Security Portal.
        // These rules are disabled by default. The can be enabled by running the MultiTool
        // with the configuration file policies/github-dsp.sarif-external-properties.
        public const string ProvideRequiredLocationProperties = "DSP1001";
        public const string InlineThreadFlowLocations = "DSP1002";
        public const string ProvideRequiredRegionProperties = "DSP1003";
        public const string ReviewArraysThatExceedConfigurableDefaults = "DSP1004";
        public const string LocationsMustBeRelativeUrisOrFilePaths = "DSP1005";
        public const string ProvideCheckoutPath = "DSP1006";
        public const string ProvideRequiredRelatedLocationProperties = "DSP1007";

        // TEMPLATE:
        // public const string RuleFriendlyName = "SARIFnnnn";
    }
}

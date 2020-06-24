// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideHelpfulToolInformation : SarifValidationSkimmerBase
    {
        private static readonly Regex s_versionRegex = new Regex(@"^\d+\.\d+.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF2005_ProvideHelpfulToolInformation_FullDescription_Text
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        public override string Id => RuleId.ProvideHelpfulToolInformation;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF2005_ProvideHelpfulToolInformation_Warning_ProvideToolVersion_Text),
            nameof(RuleResources.SARIF2005_ProvideHelpfulToolInformation_Warning_ProvideConciseToolName_Text),
            nameof(RuleResources.SARIF2005_ProvideHelpfulToolInformation_Warning_UseNumericToolVersions_Text)
        };

        protected override void Analyze(Tool tool, string toolPointer)
        {
            if (tool.Driver != null)
            {
                AnalyzeToolDriver(tool.Driver, toolPointer.AtProperty(SarifPropertyName.Driver));
            }
        }

        private void AnalyzeToolDriver(ToolComponent toolComponent, string toolDriverPointer)
        {
            // ProvideConciseToolName: Ensure that tool.driver.name isn't more than 3 words long
            if (!string.IsNullOrEmpty(toolComponent.Name))
            {
                const int MaxWords = 3;
                int wordCount = toolComponent.Name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (wordCount > MaxWords)
                {
                    string driverNamePointer = toolDriverPointer.AtProperty(SarifPropertyName.Name);
                    LogResult(
                        driverNamePointer,
                        nameof(RuleResources.SARIF2005_ProvideHelpfulToolInformation_Warning_ProvideConciseToolName_Text),
                        toolComponent.Name,
                        wordCount.ToString(),
                        MaxWords.ToString());
                }
            }

            // ProvideToolVersion: Either tool.driver.version or tool.driver.semanticVersion should be there.
            if (string.IsNullOrWhiteSpace(toolComponent.Version) && string.IsNullOrWhiteSpace(toolComponent.SemanticVersion))
            {
                LogResult(
                        toolDriverPointer,
                        nameof(RuleResources.SARIF2005_ProvideHelpfulToolInformation_Warning_ProvideToolVersion_Text));
            }
            else
            {
                // UseNumericToolVersions
                if (!string.IsNullOrWhiteSpace(toolComponent.Version))
                {
                    AnalyzeVersion(toolComponent.Version, toolDriverPointer.AtProperty(SarifPropertyName.Version));
                }
            }
        }

        private void AnalyzeVersion(string version, string pointer)
        {
            if (!s_versionRegex.IsMatch(version))
            {
                LogResult(
                        pointer,
                        nameof(RuleResources.SARIF2005_ProvideHelpfulToolInformation_Warning_UseNumericToolVersions_Text),
                        version);
            }
        }
    }
}

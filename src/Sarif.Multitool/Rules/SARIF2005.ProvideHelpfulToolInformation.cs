// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideHelpfulToolInformation : SarifValidationSkimmerBase
    {
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
            if (tool == null)
            {
                return;
            }

            if (tool.Driver != null)
            {
                AnalyzeToolDriver(tool.Driver, toolPointer.AtProperty(SarifPropertyName.Driver));
            }
        }

        private void AnalyzeToolDriver(ToolComponent toolComponent, string driverPointer)
        {
            // ProvideConciseToolName: Ensure that tool.driver.name isn't more than 3 words long
            if (!string.IsNullOrEmpty(toolComponent.Name))
            {
                const int maxWordCount = 3;
                int wordCount = toolComponent.Name.Split(' ').Length;
                if (wordCount > maxWordCount)
                {
                    string namePointer = driverPointer.AtProperty(SarifPropertyName.Name);
                    LogResult(
                        namePointer,
                        nameof(RuleResources.SARIF2005_ProvideHelpfulToolInformation_Warning_ProvideConciseToolName_Text),
                        wordCount.ToString(),
                        maxWordCount.ToString());
                }
            }

            // ProvideToolVersion: Either tool.driver.version or tool.driver.semanticVersion should be there.
            if (string.IsNullOrWhiteSpace(toolComponent.Version) && string.IsNullOrWhiteSpace(toolComponent.SemanticVersion))
            {
                LogResult(
                        driverPointer,
                        nameof(RuleResources.SARIF2005_ProvideHelpfulToolInformation_Warning_ProvideToolVersion_Text));
            }
            else
            {
                // UseNumericToolVersions
                if (!string.IsNullOrWhiteSpace(toolComponent.Version))
                {
                    AnalyzeVersion(toolComponent.Version, driverPointer.AtProperty(SarifPropertyName.Version));
                }
                else
                {
                    AnalyzeVersion(toolComponent.SemanticVersion, driverPointer.AtProperty(SarifPropertyName.SemanticVersion));
                }
            }
        }

        private void AnalyzeVersion(string version, string pointer)
        {
            Regex regex = new Regex(@"^\d+\.\d+.*");
            Match match = regex.Match(version);
            if (!match.Success)
            {
                LogResult(
                        pointer,
                        nameof(RuleResources.SARIF2005_ProvideHelpfulToolInformation_Warning_UseNumericToolVersions_Text),
                        version);
            }
        }
    }
}

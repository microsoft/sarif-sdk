// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideHelpUris : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2012
        /// </summary>
        public override string Id => RuleId.ProvideHelpUris;

        /// <summary>
        /// Placeholder
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2012_ProvideHelpUris_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2012_ProvideHelpUris_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        protected override void Analyze(Tool tool, string toolPointer)
        {
            if (tool.Driver != null)
            {
                AnalyzeToolDriver(tool.Driver, toolPointer.AtProperty(SarifPropertyName.Driver));
            }
        }

        private void AnalyzeToolDriver(ToolComponent toolComponent, string toolDriverPointer)
        {
            if (toolComponent.Rules != null)
            {
                string rulesPointer = toolDriverPointer.AtProperty(SarifPropertyName.Rules);
                for (int i = 0; i < toolComponent.Rules.Count; i++)
                {
                    AnalyzeReportingDescriptor(toolComponent.Rules[i], rulesPointer.AtIndex(i));
                }
            }
        }

        private void AnalyzeReportingDescriptor(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor.HelpUri == null)
            {
                string ruleMoniker = reportingDescriptor.Id;
                if (!string.IsNullOrWhiteSpace(reportingDescriptor.Name))
                {
                    ruleMoniker += $".{reportingDescriptor.Name}";
                }

                // {0}: Placeholder '{1}'
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF2012_ProvideHelpUris_Note_Default_Text),
                    ruleMoniker);
            }
        }
    }
}

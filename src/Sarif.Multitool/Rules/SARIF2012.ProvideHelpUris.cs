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
        /// For each rule, provide a URI where users can find detailed information about the rule.
        /// This information should include a detailed description of the invalid pattern, an
        /// explanation of why the pattern is poor practice (particularly in contexts such as
        /// security or accessibility where driving considerations might not be readily apparent),
        /// guidance for resolving the problem (including describing circumstances in which ignoring
        /// the problem altogether might be appropriate), examples of invalid and valid patterns,
        /// and special considerations (such as noting when a violation should never be ignored or
        /// suppressed, noting when a violation could cause downstream tool noise, and noting when
        /// a rule can be configured in some way to refine or alter the analysis).
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2012_ProvideHelpUris_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2012_ProvideHelpUris_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        protected override void Analyze(Run run, string runPointer)
        {
            AnalyzeTool(run.Tool, runPointer.AtProperty(SarifPropertyName.Tool));
        }

        private void AnalyzeTool(Tool tool, string toolPointer)
        {
            AnalyzeToolDriver(tool.Driver, toolPointer.AtProperty(SarifPropertyName.Driver));
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

                // {0}: The rule '{1}' does not provide a help URI. Providing a URI where users can
                // find detailed information about the rule helps users to understand the result and
                // how they can best address it.
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF2012_ProvideHelpUris_Note_Default_Text),
                    ruleMoniker);
            }
        }
    }
}

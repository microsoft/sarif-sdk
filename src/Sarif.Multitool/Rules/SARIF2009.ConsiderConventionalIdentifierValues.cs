// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ConsiderConventionalIdentifierValues : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2009
        /// </summary>
        public override string Id => RuleId.ConsiderConventionalIdentifierValues;

        /// <summary>
        /// Adopt uniform naming conventions for rule ids.
        ///
        /// Many tools follow a conventional format for the 'reportingDescriptor.id' property:
        /// a short string identifying the tool concatenated with a numeric rule number, for 
        /// example, 'CS2001' for a diagnostic from the Roslyn C# compiler. For uniformity of 
        /// experience across tools, we recommend this format.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2009_ConsiderConventionalIdentifierValues_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2009_ConsiderConventionalIdentifierValues_Note_UseConventionalRuleIds_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        private static readonly Regex s_conventionalIdRegex = new Regex(@"^[A-Z]{1,5}[0-9]{1,4}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
            if (string.IsNullOrWhiteSpace(reportingDescriptor.Id))
            {
                return;
            }

            if (!s_conventionalIdRegex.IsMatch(reportingDescriptor.Id))
            {
                // {0}: The 'id' property of the rule '{1}' does not follow the recommended format:
                // a short string identifying the tool concatenated with a numeric rule number, for
                // example, 'CS2001'. Using a conventional format for the rule id provides a more
                // uniform experience across tools.
                LogResult(
                    reportingDescriptorPointer.AtProperty(SarifPropertyName.Id),
                    nameof(RuleResources.SARIF2009_ConsiderConventionalIdentifierValues_Note_UseConventionalRuleIds_Text),
                    reportingDescriptor.Id);
            }
        }
    }
}

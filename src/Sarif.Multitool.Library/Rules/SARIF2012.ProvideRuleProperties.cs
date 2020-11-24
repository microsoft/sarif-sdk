// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideRuleProperties : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2012
        /// </summary>
        public override string Id => RuleId.ProvideRuleProperties;

        /// <summary>
        /// Rule metadata should provide information that makes it easy to understand and fix the problem.
        /// 
        /// Provide the 'name' property, which contains a "friendly name" that helps users see at a glance
        /// the purpose of the rule.For uniformity of experience across all tools that produce SARIF, the
        /// friendly name should be a single Pascal identifier, for example, 'ProvideRuleFriendlyName'.
        /// 
        /// Provide the 'helpUri' property, which contains a URI where users can find detailed information
        /// about the rule.This information should include a detailed description of the invalid pattern,
        /// an explanation of why the pattern is poor practice (particularly in contexts such as security
        /// or accessibility where driving considerations might not be readily apparent), guidance for
        /// resolving the problem(including describing circumstances in which ignoring the problem
        /// altogether might be appropriate), examples of invalid and valid patterns, and special considerations
        /// (such as noting when a violation should never be ignored or suppressed, noting when a violation
        /// could cause downstream tool noise, and noting when a rule can be configured in some way to refine
        /// or alter the analysis).
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2012_ProvideRuleProperties_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_FriendlyNameNotAPascalIdentifier_Text),
            nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_ProvideFriendlyName_Text),
            nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_ProvideHelpUri_Text),
            nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_ProvideMetadataForAllViolatedRules_Text),
            nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_ProvideRuleMetadata_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        private static readonly Regex s_pascalCaseRegex = new Regex(@"^(\p{Lu}[\p{Ll}\p{Nd}]+)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private HashSet<string> currentRules;
        private Run currentRun;
        private string toolName;

        protected override void Analyze(Run run, string runPointer)
        {
            currentRun = run;
            AnalyzeTool(run.Tool, runPointer.AtProperty(SarifPropertyName.Tool));
        }

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (string.IsNullOrWhiteSpace(reportingDescriptor.Name))
            {
                // {0}: The rule '{1}' does not provide a "friendly name" in its 'name'
                // property. The friendly name should be a single Pascal identifier, for
                // example, 'ProvideRuleFriendlyName', that helps users see at a glance
                // the purpose of the rule.
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_ProvideFriendlyName_Text),
                    reportingDescriptor.Id);
                return;
            }

            if (!s_pascalCaseRegex.IsMatch(reportingDescriptor.Name))
            {
                // {0}: '{1}' is not a Pascal identifier. For uniformity of experience across all tools that
                // produce SARIF, the friendly name should be a single Pascal identifier, for example,
                // 'ProvideRuleFriendlyName'.
                LogResult(
                    reportingDescriptorPointer.AtProperty(SarifPropertyName.Name),
                    nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_FriendlyNameNotAPascalIdentifier_Text),
                    reportingDescriptor.Name);
                return;
            }
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            if (currentRules.Count != 0 && !currentRules.Contains(result.ResolvedRuleId(currentRun)))
            {
                // '{0}' does not provide metadata for rule '{1}'. Rule metadata contains information
                // that helps the user understand why each rule fires and what the user can do to fix it.
                LogResult(
                    resultPointer,
                    nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_ProvideRuleMetadata_Text),
                    toolName,
                    result.ResolvedRuleId(currentRun));
            }
        }

        private void AnalyzeTool(Tool tool, string toolPointer)
        {
            currentRules = AnalyzeToolDriver(tool.Driver, toolPointer.AtProperty(SarifPropertyName.Driver));
        }

        private HashSet<string> AnalyzeToolDriver(ToolComponent toolComponent, string toolDriverPointer)
        {
            var rules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            toolName = toolComponent.Name;
            if (toolComponent.Rules != null && toolComponent.Rules.Count != 0)
            {
                string rulesPointer = toolDriverPointer.AtProperty(SarifPropertyName.Rules);
                
                for (int i = 0; i < toolComponent.Rules.Count; i++)
                {
                    AnalyzeReportingDescriptor(toolComponent.Rules[i], rulesPointer.AtIndex(i));
                    rules.Add(toolComponent.Rules[i].Id);
                }
            }
            else
            {
                // '{0}' does not provide a 'rules' property. 'rules' contain information that helps
                // users understand why each rule fires and what the user can do to fix it.
                LogResult(
                    toolDriverPointer,
                    nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_ProvideMetadataForAllViolatedRules_Text),
                    toolName);
            }

            return rules;
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
                    nameof(RuleResources.SARIF2012_ProvideRuleProperties_Note_ProvideHelpUri_Text),
                    ruleMoniker);
            }
        }
    }
}

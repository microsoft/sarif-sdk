// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UseConventionalSymbolicNames : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2009
        /// </summary>
        public override string Id => RuleId.UseConventionalSymbolicNames;

        /// <summary>
        /// Adopt uniform naming conventions for the symbolic names that SARIF uses various contexts.
        /// 
        /// Many tools follow a conventional format for the 'reportingDescriptor.id' property: a short string identifying the tool concatenated with a numeric rule number,
        /// for example, 'CS2001' for a diagnostic from the Roslyn C# compiler. For uniformity of experience across tools, we recommend this format.
        /// 
        /// Many tool use similar names for 'uriBaseId' symbols.We suggest 'REPOROOT' for the root of a repository, 'SRCROOT' for the root of the directory containing all source code, 'TESTROOT' for the root of the directory containing all test code(if your repository is organized in that way), and 'BINROOT' for the root of the directory containing build output(if your project places all build output in a common directory).
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2009_UseConventionalSymbolicNames_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2009_UseConventionalSymbolicNames_Warning_UseConventionalRuleIds_Text),
            nameof(RuleResources.SARIF2009_UseConventionalSymbolicNames_Warning_UseConventionalUriBaseIdNames_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        private static readonly string[] s_conventionalSymbols = new string[] { "REPOROOT", "SRCROOT", "TESTROOT", "BINROOT" };
        private static readonly Regex s_conventionalIdRegex = new Regex(@"^[A-Z]{1,5}[0-9]{1,4}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.OriginalUriBaseIds != null)
            {
                AnalyzeOriginalUriBaseIds(run.OriginalUriBaseIds, runPointer.AtProperty(SarifPropertyName.OriginalUriBaseIds));
            }
        }

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
            if (string.IsNullOrWhiteSpace(reportingDescriptor.Id))
            {
                return;
            }

            if (!s_conventionalIdRegex.IsMatch(reportingDescriptor.Id))
            {
                // {0}: The 'id' property of the rule '{1}' does not follow the recommended format:
                // a short string identifying the tool concatenated with a numeric rule number, for example, `CS2001`.
                // Using a conventional format for the rule id provides a more uniform experience across tools.
                LogResult(
                    reportingDescriptorPointer.AtProperty(SarifPropertyName.Id),
                    nameof(RuleResources.SARIF2001_AuthorHighQualityMessages_Warning_IncludeDynamicContent_Text),
                    reportingDescriptor.Name);
            }
        }

        private void AnalyzeOriginalUriBaseIds(IDictionary<string, ArtifactLocation> originalUriBaseIds, string originalUriBaseIdsPointer)
        {
            foreach (KeyValuePair<string, ArtifactLocation> originalUriBaseId in originalUriBaseIds)
            {
                if (!s_conventionalSymbols.Contains(originalUriBaseId.Key))
                {
                    // {0}: The 'originalUriBaseIds' symbol '{1}' is not one of the conventional symbols. 
                    // We suggest 'REPOROOT' for the root of a repository, 'SRCROOT' for the root of the directory containing all source code,
                    // 'TESTROOT' for the root of the directory containing all test code (if your repository is organized in that way),
                    // and 'BINROOT' for the root of the directory containing build output (if your project places all build output in a common directory).
                    LogResult(
                        originalUriBaseIdsPointer.AtProperty(originalUriBaseId.Key),
                        nameof(RuleResources.SARIF2001_AuthorHighQualityMessages_Warning_IncludeDynamicContent_Text),
                        originalUriBaseId.Key);
                }
            }
        }
    }
}

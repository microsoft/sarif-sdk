// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public class DoNotUseFriendlyNameAsRuleId : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SV0001_DoNotUseFriendlyNameAsRuleIdDescription;

        /// <summary>
        /// SV0001
        /// </summary>
        public override string Id => RuleId.DoNotUseFriendlyNameAsRuleId;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SV0001_DefaultFormatId)
                };
            }
        }

        protected override void AnalyzeCore(SarifValidationContext context)
        {
            SarifLog log = context.InputLog;
            if (log.Runs != null)
            {
                Run[] runs = log.Runs.ToArray();
                for (int iRun = 0; iRun < runs.Length; ++iRun)
                {
                    Run run = runs[iRun];
                    if (run.Rules != null)
                    {
                        Rule[] rules = run.Rules.Values.ToArray();
                        for (int iRule = 0; iRule < rules.Length; ++iRule)
                        {
                            Rule rule = rules[iRule];
                            if (rule.Id != null
                                && rule.Name != null 
                                && rule.Id.Equals(rule.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                string jPointerValue = $"/runs/{iRun}/rules/{rule.Id}";
                                Region region = GetRegionFromJPointer(jPointerValue, context);

                                context.Logger.Log(this,
                                    RuleUtilities.BuildResult(ResultLevel.Warning, context, region, nameof(RuleResources.SV0001_DefaultFormatId), rule.Id));
                            }
                        }
                    }
                }
            }
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideConventionalGroupingDirection : SarifValidationSkimmerBase
    {
        private const string Synthesized = "synthesized";
        private const string Generated = "generated";
        private const string Annotated = "annotated";

        public ProvideConventionalGroupingDirection()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// AI2020
        /// </summary>
        public override string Id => RuleId.AIProvideConventionalGroupingDirection;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2020_ProvideConventionalGroupingDirection_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2020_ProvideConventionalGroupingDirection_Warning_Default_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            string sourceOrigin = GetRunOrigin(Context.CurrentRunIndex);

            foreach (GroupingRelationshipHelper.GroupingEdge edge in GroupingRelationshipHelper.GetGroupingEdges(result))
            {
                // Inspect only the 'includes' direction; the reciprocal 'isIncludedBy'
                // edge describes the same grouping and would double-report.
                if (edge.Kind != GroupingRelationshipHelper.Includes)
                {
                    continue;
                }

                string targetOrigin = GetRunOrigin(edge.RunIndex);

                // 'ai/origin' presence and validity are owned by AI1006; assess direction
                // only when both endpoints declare an origin.
                if (sourceOrigin == null || targetOrigin == null)
                {
                    continue;
                }

                bool conventional = sourceOrigin == Synthesized
                    && (targetOrigin == Generated || targetOrigin == Annotated);

                if (!conventional)
                {
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.AI2020_ProvideConventionalGroupingDirection_Warning_Default_Text),
                        edge.TargetUri,
                        sourceOrigin,
                        targetOrigin);
                }
            }
        }

        private string GetRunOrigin(int runIndex)
        {
            IList<Run> runs = Context.InputLog?.Runs;
            if (runs == null || runIndex < 0 || runIndex >= runs.Count)
            {
                return null;
            }

            return runs[runIndex].TryGetProperty("ai/origin", out string origin) ? origin : null;
        }
    }
}

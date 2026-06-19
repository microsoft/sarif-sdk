// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideReciprocalGroupingRelationships : SarifValidationSkimmerBase
    {
        public ProvideReciprocalGroupingRelationships()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1015
        /// </summary>
        public override string Id => RuleId.AIProvideReciprocalGroupingRelationships;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1015_ProvideReciprocalGroupingRelationships_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1015_ProvideReciprocalGroupingRelationships_Error_Default_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            int runIndex = Context.CurrentRunIndex;
            int resultIndex = Context.CurrentResultIndex;

            foreach (GroupingRelationshipHelper.GroupingEdge edge in GroupingRelationshipHelper.GetGroupingEdges(result))
            {
                string inverse = GroupingRelationshipHelper.Inverse(edge.Kind);

                Result target = ResolveResult(edge.RunIndex, edge.ResultIndex);

                // A grouping pointer that does not resolve to a result is reported by
                // SARIF1013; reciprocity cannot be assessed against a missing target, so
                // skip cleanly here rather than double-reporting.
                if (target == null)
                {
                    continue;
                }

                bool reciprocated = GroupingRelationshipHelper.GetGroupingEdges(target)
                    .Any(e => e.Kind == inverse && e.RunIndex == runIndex && e.ResultIndex == resultIndex);

                if (!reciprocated)
                {
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.AI1015_ProvideReciprocalGroupingRelationships_Error_Default_Text),
                        edge.Kind,
                        edge.TargetUri,
                        inverse);
                }
            }
        }

        private Result ResolveResult(int runIndex, int resultIndex)
        {
            IList<Run> runs = Context.InputLog?.Runs;
            if (runs == null || runIndex < 0 || runIndex >= runs.Count)
            {
                return null;
            }

            IList<Result> results = runs[runIndex].Results;
            if (results == null || resultIndex < 0 || resultIndex >= results.Count)
            {
                return null;
            }

            return results[resultIndex];
        }
    }
}

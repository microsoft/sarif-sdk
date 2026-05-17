// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// AI1015: when a run declares <c>ai/origin</c> on <c>run.properties</c>
    /// (i.e., this is an AI-emitted run), every <c>result</c> MUST carry
    /// <c>result.rank</c>. Confidence is first-class on AI findings: it
    /// drives triage priority and downstream filtering. Without <c>rank</c>
    /// the consumer has no signal of how strongly the producer believes in
    /// the result. This rule does not fire on non-AI runs; the SARIF-general
    /// recommendation in <c>AI2010.ProvideResultRank</c> covers those at
    /// note severity.
    /// </summary>
    public class RequireResultRankOnAIRun : SarifValidationSkimmerBase
    {
        public RequireResultRankOnAIRun()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1015
        /// </summary>
        public override string Id => RuleId.AIRequireResultRankOnAIRun;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1015_RequireResultRankOnAIRun_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1015_RequireResultRankOnAIRun_Error_Default_Text)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            // Gate: only fire when the run is AI-emitted. Keegan PR 97 L128:
            // ai/origin is run-level (not per-result) with three string values
            // (generated/annotated/synthesized), not a boolean. AI1006 enforces
            // its presence and shape; we only need its presence here.
            if (!run.TryGetProperty("ai/origin", out string _))
            {
                return;
            }

            if (run.Results == null)
            {
                return;
            }

            string resultsPointer = runPointer.AtProperty(SarifPropertyName.Results);

            for (int i = 0; i < run.Results.Count; i++)
            {
                Result result = run.Results[i];

                // result.Rank uses -1.0 as the unset sentinel
                // (DefaultValue(-1.0) + IgnoreAndPopulate on the property).
                if (result.Rank < 0)
                {
                    LogResult(
                        resultsPointer.AtIndex(i),
                        nameof(RuleResources.AI1015_RequireResultRankOnAIRun_Error_Default_Text));
                }
            }
        }
    }
}

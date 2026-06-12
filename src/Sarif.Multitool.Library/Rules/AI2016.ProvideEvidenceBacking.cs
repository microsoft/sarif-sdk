// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideEvidenceBacking : SarifValidationSkimmerBase
    {
        public ProvideEvidenceBacking()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// AI2016
        /// </summary>
        public override string Id => RuleId.AIProvideEvidenceBacking;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2016_ProvideEvidenceBacking_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2016_ProvideEvidenceBacking_Warning_MissingBacking_Text),
            nameof(RuleResources.AI2016_ProvideEvidenceBacking_Warning_Inconsistent_Text),
            nameof(RuleResources.AI2016_ProvideEvidenceBacking_Warning_MalformedEvidence_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!result.TryGetSerializedPropertyValue("ai/evidence", out string evidenceJson))
            {
                return;
            }

            // AI2016 owns the 'ai/evidence' well-formedness check: a present-but-malformed
            // value is a producer defect worth a single, explicit diagnostic. Other evidence
            // rules skip silently on the same failure and defer the report to this one.
            if (!EvidenceJsonReader.TryParseEvidenceArray(evidenceJson, out JArray evidenceArray))
            {
                LogResult(
                    resultPointer,
                    nameof(RuleResources.AI2016_ProvideEvidenceBacking_Warning_MalformedEvidence_Text));
                return;
            }

            bool hasDemonstratedWithBacking = false;

            for (int i = 0; i < evidenceArray.Count; i++)
            {
                if (!(evidenceArray[i] is JObject entry))
                {
                    continue;
                }

                // Defensive reads. 'strength' is expected to be a string per the
                // AI profile; 'backing' is emitted in the wild as either a single
                // string or an array of strings. JObject.Value<string>() throws
                // InvalidCastException on a JArray and disables the rule mid-run
                // (issue #2908).
                string strength = EvidenceJsonReader.ReadString(entry, "strength");
                IReadOnlyList<string> backings = EvidenceJsonReader.ReadStrings(entry, "backing");

                if (strength == "demonstrated")
                {
                    if (backings.Count == 0)
                    {
                        LogResult(
                            resultPointer,
                            nameof(RuleResources.AI2016_ProvideEvidenceBacking_Warning_MissingBacking_Text),
                            i.ToString());
                    }
                    else
                    {
                        hasDemonstratedWithBacking = true;
                    }
                }
            }

            if (result.TryGetProperty("ai/exploitability", out string exploitability)
                && exploitability == "demonstrated"
                && !hasDemonstratedWithBacking)
            {
                LogResult(
                    resultPointer,
                    nameof(RuleResources.AI2016_ProvideEvidenceBacking_Warning_Inconsistent_Text));
            }
        }
    }
}

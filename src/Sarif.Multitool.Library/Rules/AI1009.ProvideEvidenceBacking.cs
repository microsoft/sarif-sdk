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
        /// AI1009
        /// </summary>
        public override string Id => RuleId.AIProvideEvidenceBacking;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1009_ProvideEvidenceBacking_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1009_ProvideEvidenceBacking_Warning_MissingBacking_Text),
            nameof(RuleResources.AI1009_ProvideEvidenceBacking_Warning_Inconsistent_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!result.TryGetSerializedPropertyValue("ai/evidence", out string evidenceJson))
            {
                return;
            }

            JArray evidenceArray;
            try
            {
                evidenceArray = JArray.Parse(evidenceJson);
            }
            catch
            {
                return;
            }

            bool hasDemonstratedWithBacking = false;

            for (int i = 0; i < evidenceArray.Count; i++)
            {
                if (!(evidenceArray[i] is JObject entry))
                {
                    continue;
                }

                string strength = entry.Value<string>("strength");
                string backing = entry.Value<string>("backing");

                if (strength == "demonstrated")
                {
                    if (string.IsNullOrEmpty(backing))
                    {
                        LogResult(
                            resultPointer,
                            nameof(RuleResources.AI1009_ProvideEvidenceBacking_Warning_MissingBacking_Text),
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
                    nameof(RuleResources.AI1009_ProvideEvidenceBacking_Warning_Inconsistent_Text));
            }
        }
    }
}

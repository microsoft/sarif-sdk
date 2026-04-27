// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideAttackerPosition : SarifValidationSkimmerBase
    {
        public ProvideAttackerPosition()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// AI2015
        /// </summary>
        public override string Id => RuleId.AIProvideAttackerPosition;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2015_ProvideAttackerPosition_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2015_ProvideAttackerPosition_Warning_Missing_Text),
            nameof(RuleResources.AI2015_ProvideAttackerPosition_Warning_Inconsistent_Text)
        };

        // All-or-nothing check: if any result has ai/attackerPosition, all must.
        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Results == null || run.Results.Count == 0)
            {
                return;
            }

            int withAttackerPosition = 0;
            int withoutAttackerPosition = 0;

            foreach (Result result in run.Results)
            {
                if (result.TryGetProperty("ai/attackerPosition", out string _))
                {
                    withAttackerPosition++;
                }
                else
                {
                    withoutAttackerPosition++;
                }
            }

            if (withAttackerPosition > 0 && withoutAttackerPosition > 0)
            {
                string resultsPointer = runPointer.AtProperty(SarifPropertyName.Results);
                LogResult(
                    resultsPointer,
                    nameof(RuleResources.AI2015_ProvideAttackerPosition_Warning_Inconsistent_Text),
                    withAttackerPosition.ToString(),
                    withoutAttackerPosition.ToString());
            }
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!result.TryGetProperty("ai/attackerPosition", out string _))
            {
                LogResult(
                    resultPointer,
                    nameof(RuleResources.AI2015_ProvideAttackerPosition_Warning_Missing_Text));
            }
        }
    }
}

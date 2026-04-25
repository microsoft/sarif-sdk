// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RedactedRunMarker : SarifValidationSkimmerBase
    {
        public RedactedRunMarker()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// AI2013
        /// </summary>
        public override string Id => RuleId.AIRedactedRunMarker;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2013_RedactedRunMarker_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2013_RedactedRunMarker_Warning_FalseValue_Text),
            nameof(RuleResources.AI2013_RedactedRunMarker_Warning_MissingRedactionTokens_Text),
            nameof(RuleResources.AI2013_RedactedRunMarker_Warning_FullLogWithoutRedaction_Text)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            bool hasRedacted = run.TryGetProperty("ai/redacted", out string redactedValue);
            bool isRedactedTrue = hasRedacted && redactedValue == "true";

            if (hasRedacted && redactedValue == "false")
            {
                // ai/redacted should be true or absent, never false.
                LogResult(
                    runPointer.AtProperty(SarifPropertyName.Properties),
                    nameof(RuleResources.AI2013_RedactedRunMarker_Warning_FalseValue_Text));
            }

            if (isRedactedTrue && (run.RedactionTokens == null || run.RedactionTokens.Count == 0))
            {
                // ai/redacted is true but no redaction tokens are defined.
                LogResult(
                    runPointer,
                    nameof(RuleResources.AI2013_RedactedRunMarker_Warning_MissingRedactionTokens_Text));
            }

            if (run.TryGetProperty("ai/fullLogLocation", out string _) && !isRedactedTrue)
            {
                // ai/fullLogLocation is present but ai/redacted is not true.
                LogResult(
                    runPointer.AtProperty(SarifPropertyName.Properties),
                    nameof(RuleResources.AI2013_RedactedRunMarker_Warning_FullLogWithoutRedaction_Text));
            }
        }
    }
}

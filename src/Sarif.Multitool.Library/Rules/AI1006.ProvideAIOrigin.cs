// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideAIOrigin : SarifValidationSkimmerBase
    {
        private static readonly HashSet<string> s_validOrigins = new HashSet<string>(StringComparer.Ordinal)
        {
            "generated",
            "annotated",
            "synthesized"
        };

        public ProvideAIOrigin()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI1006
        /// </summary>
        public override string Id => RuleId.AIProvideAIOrigin;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1006_ProvideAIOrigin_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1006_ProvideAIOrigin_Error_Missing_Text),
            nameof(RuleResources.AI1006_ProvideAIOrigin_Error_InvalidValue_Text)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            if (!run.TryGetProperty("ai/origin", out string origin))
            {
                // {0}: This run does not declare 'ai/origin' in 'run.properties'.
                LogResult(
                    runPointer,
                    nameof(RuleResources.AI1006_ProvideAIOrigin_Error_Missing_Text));
                return;
            }

            if (origin == null || !s_validOrigins.Contains(origin))
            {
                // {0}: The 'ai/origin' value '{1}' is not a recognized origin.
                LogResult(
                    runPointer.AtProperty(SarifPropertyName.Properties),
                    nameof(RuleResources.AI1006_ProvideAIOrigin_Error_InvalidValue_Text),
                    origin ?? "(null)");
            }
        }
    }
}

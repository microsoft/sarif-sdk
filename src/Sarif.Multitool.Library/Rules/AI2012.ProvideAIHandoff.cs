// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideAIHandoff : SarifValidationSkimmerBase
    {
        public ProvideAIHandoff()
        {
            this.DefaultConfiguration.Level = FailureLevel.Note;
        }

        /// <summary>
        /// AI2012
        /// </summary>
        public override string Id => RuleId.AIProvideAiHandoff;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2012_ProvideAiHandoff_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2012_ProvideAiHandoff_Note_Default_Text)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            if (!run.TryGetProperty("ai/handoff", out string handoff) || string.IsNullOrWhiteSpace(handoff))
            {
                LogResult(
                    runPointer,
                    nameof(RuleResources.AI2012_ProvideAiHandoff_Note_Default_Text));
            }
        }
    }
}

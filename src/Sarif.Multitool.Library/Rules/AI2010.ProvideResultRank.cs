// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideResultRank : SarifValidationSkimmerBase
    {
        public ProvideResultRank()
        {
            this.DefaultConfiguration.Level = FailureLevel.Note;
        }

        /// <summary>
        /// AI2010
        /// </summary>
        public override string Id => RuleId.AIProvideResultRank;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2010_ProvideResultRank_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2010_ProvideResultRank_Note_Default_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            // result.Rank defaults to -1.0 when not set
            if (result.Rank < 0)
            {
                LogResult(
                    resultPointer,
                    nameof(RuleResources.AI2010_ProvideResultRank_Note_Default_Text));
            }
        }
    }
}

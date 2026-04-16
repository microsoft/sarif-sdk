// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideMessageMarkdown : SarifValidationSkimmerBase
    {
        public ProvideMessageMarkdown()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// AI2006
        /// </summary>
        public override string Id => RuleId.AIProvideMessageMarkdown;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2006_ProvideMessageMarkdown_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2006_ProvideMessageMarkdown_Error_Default_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.Message == null ||
                string.IsNullOrWhiteSpace(result.Message.Markdown))
            {
                LogResult(
                    resultPointer.AtProperty(SarifPropertyName.Message),
                    nameof(RuleResources.AI2006_ProvideMessageMarkdown_Error_Default_Text));
            }
        }
    }
}

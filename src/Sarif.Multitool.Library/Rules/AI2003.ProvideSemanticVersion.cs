// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideSemanticVersion : SarifValidationSkimmerBase
    {
        public ProvideSemanticVersion()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// AI2003
        /// </summary>
        public override string Id => RuleId.AIProvideSemanticVersion;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2003_ProvideSemanticVersion_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2003_ProvideSemanticVersion_Warning_Default_Text)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Tool?.Driver == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(run.Tool.Driver.SemanticVersion))
            {
                LogResult(
                    runPointer
                        .AtProperty(SarifPropertyName.Tool)
                        .AtProperty(SarifPropertyName.Driver),
                    nameof(RuleResources.AI2003_ProvideSemanticVersion_Warning_Default_Text),
                    run.Tool.Driver.Name ?? "(unnamed)");
            }
        }
    }
}

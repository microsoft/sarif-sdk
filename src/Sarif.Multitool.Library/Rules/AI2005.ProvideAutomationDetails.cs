// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideAutomationDetails : SarifValidationSkimmerBase
    {
        public ProvideAutomationDetails()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// AI2005
        /// </summary>
        public override string Id => RuleId.AIProvideAutomationDetails;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI2005_ProvideAutomationDetails_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI2005_ProvideAutomationDetails_Warning_Missing_Text),
            nameof(RuleResources.AI2005_ProvideAutomationDetails_Warning_MissingGuid_Text)
        };

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.AutomationDetails == null)
            {
                LogResult(
                    runPointer,
                    nameof(RuleResources.AI2005_ProvideAutomationDetails_Warning_Missing_Text));
                return;
            }

            if (run.AutomationDetails.Guid == null)
            {
                LogResult(
                    runPointer.AtProperty(SarifPropertyName.AutomationDetails),
                    nameof(RuleResources.AI2005_ProvideAutomationDetails_Warning_MissingGuid_Text));
            }
        }
    }
}

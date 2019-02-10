// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DoNotUseFriendlyNameAsRuleId : SarifValidationSkimmerBase
    {
        private readonly Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1001_DoNotUseFriendlyNameAsRuleIdDescription
        };

        public override Message FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        /// <summary>
        /// SARIF1001
        /// </summary>
        public override string Id => RuleId.DoNotUseFriendlyNameAsRuleId;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1001_Default)
        };

        protected override void Analyze(MessageDescriptor messageDescriptor, string messageDescriptorPointer)
        {
            if (messageDescriptor.Id != null &&
                messageDescriptor.Name != null &&
                messageDescriptor.Name.Text != null &&
                messageDescriptor.Id.Equals(messageDescriptor.Name.Text, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(
                    messageDescriptorPointer,
                    nameof(RuleResources.SARIF1001_Default),
                    messageDescriptor.Id);
            }
        }
    }
}

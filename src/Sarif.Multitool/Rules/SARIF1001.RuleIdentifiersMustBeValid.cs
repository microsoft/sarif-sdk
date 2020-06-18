// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RuleIdentifiersMustBeValid : SarifValidationSkimmerBase
    {
        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1001_DistinguishRuleIdFromRuleName
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        public override string Id => RuleId.RuleIdentifiersMustBeValid;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1001_RuleIdentifiersMustBeValid)
        };

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor.Id != null &&
                reportingDescriptor.Name != null &&
                reportingDescriptor.Id.Equals(reportingDescriptor.Name, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF1001_RuleIdentifiersMustBeValid),
                    reportingDescriptor.Id);
            }
        }
    }
}

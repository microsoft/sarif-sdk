// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RuleIdentifiersMustBeValid : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2001
        /// </summary>
        public override string Id => RuleId.RuleIdentifiersMustBeValid;

        /// <summary>
        /// The two identity-related properties of a SARIF rule must be consistent. The required 'id'
        /// property must be a "stable, opaque identifier" (the SARIF specification
        /// ([3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839))
        /// explains the reasons for this). The optional 'name' property
        /// ([3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843))
        /// is an identifer that is understandable to an end user. Therefore if both 'id' and 'name'
        /// are present, they must be different. If both 'name' and 'id' are opaque identifiers,
        /// omit the 'name' property. If both 'name' and 'id' are human-readable identifiers, then
        /// consider assigning an opaque identifier to each rule, but in the meantime, omit the 'name'
        /// property.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1001_RuleIdentifiersMustBeValid_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1001_RuleIdentifiersMustBeValid_Error_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor.Id != null &&
                reportingDescriptor.Name != null &&
                reportingDescriptor.Id.Equals(reportingDescriptor.Name, StringComparison.OrdinalIgnoreCase))
            {
                // {0}: The rule '{1}' has a 'name' property that is identical to its 'id' property.
                // The required 'id' property must be a "stable, opaque identifier" (the SARIF specification
                // ([3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839))
                // explains the reasons for this). The optional 'name' property
                // ([3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843))
                // is an identifer that is understandable to an end user. Therefore if both 'id' and
                // 'name' are present, they must be different. If they are identical, the tool must
                // omit the 'name' property.
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF1001_RuleIdentifiersMustBeValid_Error_Default_Text),
                    reportingDescriptor.Id);
            }
        }
    }
}

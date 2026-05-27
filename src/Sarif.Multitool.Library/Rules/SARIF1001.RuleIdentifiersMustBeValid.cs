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
        /// is an identifier that is understandable to an end user. Therefore if both 'id' and 'name'
        /// are present, they must be different. If both 'name' and 'id' are opaque identifiers,
        /// omit the 'name' property. If both 'name' and 'id' are human-readable identifiers, then
        /// consider assigning an opaque identifier to each rule, but in the meantime, omit the 'name'
        /// property.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1001_RuleIdentifiersMustBeValid_FullDescription_Text };

        protected override ICollection<string> MessageResourceNames => new List<string> {
            nameof(RuleResources.SARIF1001_RuleIdentifiersMustBeValid_Warning_Default_Text)
        };

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor.Id == null || reportingDescriptor.Name == null) { return; }

            bool isStrictlyIdentical = reportingDescriptor.Id.Equals(reportingDescriptor.Name, StringComparison.Ordinal);

            // Spec MUST (§3.49.7): if both 'id' and 'name' are present, they SHALL NOT be the
            // same string. Strict-byte equality fires for every descriptor in every run.
            if (isStrictlyIdentical)
            {
                LogIdNameCollision(reportingDescriptor, reportingDescriptorPointer);
                return;
            }

            // Authorial convention SHOULD: case-fold-equal 'id'/'name' pairs (e.g. 'LogLevel' / 'loglevel')
            // are almost always a hand-authoring slip. AI notification taxonomies (see #2952) deliberately
            // pair a SCREAMING-CAPS opaque id with the corresponding PascalCase end-user name
            // (e.g. 'DECISION' / 'Decision'); for those descriptors the convention check is suppressed.
            // The intersection (AI-origin AND notification descriptor) is the cut: AI rule ids are
            // constrained by AI1012 to 'BASE/sub-id' or 'NOVEL-<sub-id>' forms whose hyphens / slashes
            // can't case-fold-collide with any PascalCase name, so the carve-out is unnecessary for
            // the rules/taxa kinds and we keep the typo heuristic engaged there.
            if (IsAIOriginRun()
                && Context?.CurrentReportingDescriptorKind == SarifValidationContext.ReportingDescriptorKind.Notification)
            {
                return;
            }

            if (reportingDescriptor.Id.Equals(reportingDescriptor.Name, StringComparison.OrdinalIgnoreCase))
            {
                LogIdNameCollision(reportingDescriptor, reportingDescriptorPointer);
            }
        }

        private void LogIdNameCollision(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            // {0}: The rule '{1}' has a 'name' property that is identical to its 'id' property.
            // The required 'id' property must be a "stable, opaque identifier" (the SARIF specification
            // ([3.49.3](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317839))
            // explains the reasons for this). The optional 'name' property
            // ([3.49.7](https://docs.oasis-open.org/sarif/sarif/v2.1.0/os/sarif-v2.1.0-os.html#_Toc34317843))
            // is an identifier that is understandable to an end user. Therefore if both 'id' and
            // 'name' are present, they must be different. If they are identical, the tool must
            // omit the 'name' property.
            LogResult(
                reportingDescriptorPointer,
                nameof(RuleResources.SARIF1001_RuleIdentifiersMustBeValid_Warning_Default_Text),
                reportingDescriptor.Id);
        }
    }
}

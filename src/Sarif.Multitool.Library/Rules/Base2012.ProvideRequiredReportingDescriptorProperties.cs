﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class Base2012ProvideRequiredReportingDescriptorProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.Base2012_ProvideRequiredReportingDescriptorProperties_Error_MissingIdProperty_Text)
        };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>();

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor != null && string.IsNullOrWhiteSpace(reportingDescriptor.Id))
            {
                // {0}: This 'reportingDescriptor' object does not provide an 'id' value. This property is required by the {1} service.
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.Base2012_ProvideRequiredReportingDescriptorProperties_Error_MissingIdProperty_Text));
            }
        }
    }
}

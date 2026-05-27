// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseProvideRequiredReportingDescriptorProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        private readonly List<string> _baseMessageResourceNames = new List<string>
        {
            nameof(RuleResources.Base2012_ProvideRequiredReportingDescriptorProperties_Error_MissingIdProperty_Text)
        };

        protected ICollection<string> BaseMessageResourceNames => _baseMessageResourceNames;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>();

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor != null && string.IsNullOrWhiteSpace(reportingDescriptor.Id))
            {
                // {0}: This 'reportingDescriptor' object does not provide an 'Id' value. This property is required by the {1} service.
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.Base2012_ProvideRequiredReportingDescriptorProperties_Error_MissingIdProperty_Text));
            }
        }
    }
}

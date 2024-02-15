// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AdoProvideRequiredReportingDescriptorProperties
        : BaseProvideRequiredResultProperties
    {
        /// <summary>
        /// ADO2012
        /// </summary>
        public override string Id => RuleId.ADOProvideRequiredReportingDescriptorProperties;

        protected override IEnumerable<string> MessageResourceNames => [
            nameof(RuleResources.ADO2012_ProvideRequiredResultProperties_Error_MissingName_Text)
        ];

        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.ADO2012_ProvideRequiredReportingDescriptorProperties_FullDescription_Text };

        public override HashSet<RuleKind> RuleKinds => new([RuleKind.Ado]);

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        public AdoProvideRequiredReportingDescriptorProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            base.Analyze(reportingDescriptor, reportingDescriptorPointer);

            if (reportingDescriptor != null && string.IsNullOrWhiteSpace(reportingDescriptor.Name))
            {
                // {0}: This 'reportingDescriptor' object does not provide a 'name' value. This property is required by the {1} service.
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.ADO2012_ProvideRequiredResultProperties_Error_MissingName_Text),
                    this.ServiceName);
            }
        }
    }
}

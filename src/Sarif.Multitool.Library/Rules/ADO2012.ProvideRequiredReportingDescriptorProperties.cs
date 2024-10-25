// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AdoProvideRequiredReportingDescriptorProperties
        : BaseProvideRequiredReportingDescriptorProperties
    {
        /// <summary>
        /// ADO2012
        /// </summary>
        public override string Id => RuleId.ADOProvideRequiredReportingDescriptorProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.ADO2012_ProvideRequiredReportingDescriptorProperties_FullDescription_Text };

        private readonly List<string> _messageResourceNames = new List<string>()
        {
            nameof(RuleResources.ADO2012_ProvideRequiredResultProperties_Error_MissingName_Text)
        };

        protected override ICollection<string> MessageResourceNames => _messageResourceNames.Concat(BaseMessageResourceNames).ToList();

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Ado });

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
                    nameof(RuleResources.ADO2012_ProvideRequiredResultProperties_Error_MissingName_Text));
            }
        }
    }
}

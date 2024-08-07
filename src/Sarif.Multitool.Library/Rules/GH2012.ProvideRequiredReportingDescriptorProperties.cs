﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GhasProvideRequiredReportingDescriptorProperties
        : BaseProvideRequiredResultProperties
    {
        /// <summary>
        /// GH2012
        /// </summary>
        public override string Id => RuleId.GHASProvideRequiredReportingDescriptorProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString() { Text = RuleResources.GH2012_ProvideRequiredReportingDescriptorProperties_FullDescription_Text };

        private readonly List<string> _messageResourceNames = new List<string>()
        {
            nameof(RuleResources.GH2012_ProvideRequiredReportingDescriptorProperties_Error_MissingFullDescription_Text),
            nameof(RuleResources.GH2012_ProvideRequiredReportingDescriptorProperties_Error_MissingHelpText_Text),
            nameof(RuleResources.GH2012_ProvideRequiredReportingDescriptorProperties_Error_MissingHelp_Text),
            nameof(RuleResources.GH2012_ProvideRequiredReportingDescriptorProperties_Error_MissingShortDescription_Text)
        };

        protected override ICollection<string> MessageResourceNames => _messageResourceNames.Concat(BaseMessageResourceNames).ToList();

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Ghas });

        protected override string ServiceName => RuleResources.ServiceName_GHAS;

        public GhasProvideRequiredReportingDescriptorProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            base.Analyze(reportingDescriptor, reportingDescriptorPointer);

            if (reportingDescriptor != null)
            {
                if (string.IsNullOrWhiteSpace(reportingDescriptor.ShortDescription?.Text))
                {
                    // {0}: This 'reportingDescriptor' object does not provide a 'shortDescription' value. This property is required by the {1} service.
                    LogResult(
                        reportingDescriptorPointer,
                        nameof(RuleResources.GH2012_ProvideRequiredReportingDescriptorProperties_Error_MissingShortDescription_Text));
                }

                if (string.IsNullOrWhiteSpace(reportingDescriptor.FullDescription?.Text))
                {
                    // {0}: This 'reportingDescriptor' object does not provide a 'fullDescription' value. This property is required by the {1} service.
                    LogResult(
                        reportingDescriptorPointer,
                        nameof(RuleResources.GH2012_ProvideRequiredReportingDescriptorProperties_Error_MissingFullDescription_Text));
                }

                if (reportingDescriptor.Help == null)
                {
                    // {0}: This 'reportingDescriptor' object does not provide a 'help' object. This property is required by the {1} service.
                    LogResult(
                        reportingDescriptorPointer,
                        nameof(RuleResources.GH2012_ProvideRequiredReportingDescriptorProperties_Error_MissingHelp_Text));
                }
                else if (string.IsNullOrWhiteSpace(reportingDescriptor.Help.Text))
                {
                    // {0}: This 'help' object does not provide a 'text' value. This property is required by the {1} service.
                    LogResult(
                        reportingDescriptorPointer.AtProperty(SarifPropertyName.Help),
                        nameof(RuleResources.GH2012_ProvideRequiredReportingDescriptorProperties_Error_MissingHelpText_Text));
                }
            }
        }
    }
}

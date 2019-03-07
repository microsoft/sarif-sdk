﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DoNotUseFriendlyNameAsRuleId : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString _fullDescription = new MultiformatMessageString
        {
            Text = RuleResources.SARIF1001_DoNotUseFriendlyNameAsRuleIdDescription
        };

        public override MultiformatMessageString FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        /// <summary>
        /// SARIF1001
        /// </summary>
        public override string Id => RuleId.DoNotUseFriendlyNameAsRuleId;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1001_Default)
        };

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor.Id != null &&
                reportingDescriptor.Name != null &&
                reportingDescriptor.Id.Equals(reportingDescriptor.Name, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF1001_Default),
                    reportingDescriptor.Id);
            }
        }
    }
}

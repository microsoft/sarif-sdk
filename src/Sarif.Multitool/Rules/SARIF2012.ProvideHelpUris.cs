// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideHelpUris : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2012
        /// </summary>
        public override string Id => RuleId.ProvideHelpUris;

        /// <summary>
        /// Placeholder
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2012_ProvideHelpUris_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2012_ProvideHelpUris_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor.HelpUri == null)
            {
                // {0}: Placeholder
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF2011_ProvideContextRegion_Note_Default_Text));
            }
        }
    }
}

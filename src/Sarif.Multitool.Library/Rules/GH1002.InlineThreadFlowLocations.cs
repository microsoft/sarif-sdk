// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class InlineThreadFlowLocations : SarifValidationSkimmerBase
    {
        public InlineThreadFlowLocations() : base()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        /// <summary>
        /// GH1002
        /// </summary>
        public override string Id => RuleId.InlineThreadFlowLocations;

        /// <summary>
        /// Results that include codeFlows must specify each threadFlowLocation directly within
        /// the codeFlow, rather than relying on threadFlowLocation.index to refer to an element
        /// of the run.threadFlowLocations array. GitHub Advanced Security code scanning will not
        /// display a result that uses such threadFlowLocations.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.GH1002_InlineThreadFlowLocations_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.GH1002_InlineThreadFlowLocations_Error_Default_Text)
        };

        public override bool EnabledByDefault => false;

        protected override void Analyze(ThreadFlowLocation threadFlowLocation, string threadFlowLocationPointer)
        {
            if (threadFlowLocation.Index >= 0)
            {
                // {0}: This 'threadFlowLocation' uses its 'index' property to refer to information
                // in the 'run.threadFlowLocations' array. GitHub Advanced Security code scanning
                // will not display a result that includes such a 'threadFlowLocation'.
                LogResult(
                    threadFlowLocationPointer.AtProperty(SarifPropertyName.Index),
                    nameof(RuleResources.GH1002_InlineThreadFlowLocations_Error_Default_Text));
                return;
            }
        }
    }
}

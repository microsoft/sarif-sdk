// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ADOValidateToolComponent : Base1003ToolComponent
    {
        /// <summary>
        /// ADO1003
        /// </summary>
        public override string Id => RuleId.ADOProvideToolDriverProperties;

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        protected override void Analyze(Run run, string runPointer)
        {
            /// run.tool is chcked by the base class.
            base.Analyze(run, runPointer);

            if (run.Tool != null)
            {
                AnalyzeFullName(run.Tool.FullName, runPointer
                    .AtProperty(SarifPropertyName.Tool)
                    .AtProperty(SarifPropertyName.FullName));
            }
        }

        private void AnalyzeFullName(ToolComponent toolComponent, string toolFullNamePointer)
        {
            if (string.IsNullOrEmpty(toolComponent.FullName))
            {
                // {0}: The 'tool' object in this run does not provide a 'fullName' value.
                LogResult(
                    toolComponentPointer,
                    nameof(RuleResources.ADO1003_ProvideFullName_Note_Default_Text));
            }
        }
    }
}

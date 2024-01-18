// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class Base1003ProvideToolDriver : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Tool == null)
            {
                // {0}: The 'tool' object in this run does not provide a value.
                LogResult(
                    runPointer,
                    nameof(RuleResources.Base1003_ProvideTool_Note_Default_Text),
                    this.ServiceName);
            }
            else
            {
                Analyze(run.Tool.Driver, runPointer
                    .AtProperty(SarifPropertyName.Tool)
                    .AtProperty(SarifPropertyName.Driver));
            }
        }

        protected override void Analyze(ToolComponent toolComponent, string toolComponentPointer)
        {
            if (toolComponent == null)
            {
                // {0}: The 'tool' object in this run does not provide a 'driver' value.
                LogResult(
                    toolComponentPointer,
                    nameof(RuleResources.Base1003_ProvideDriver_Note_Default_Text),
                    this.ServiceName);
            }
            else
            {
                AnalyzeToolDriver(toolComponent, toolComponentPointer);
            }
        }

        private void AnalyzeToolDriver(ToolComponent toolComponent, string toolDriverPointer)
        {
            if (string.IsNullOrWhiteSpace(toolComponent.FullName))
            {
                // {0}: The 'tool' object in this run does not provide a 'fullName' value.
                LogResult(
                    toolDriverPointer,
                    nameof(RuleResources.Base1003_ProvideFullName_Note_Default_Text),
                    this.ServiceName);
            }

            if (toolComponent.Rules == null)
            {
                // {0}: The 'tool' object in this run does not provide a 'rules' value.
                LogResult(
                    toolDriverPointer,
                    nameof(RuleResources.Base1003_ProvideRules_Note_Default_Text),
                    this.ServiceName);
            }
        }
    }
}

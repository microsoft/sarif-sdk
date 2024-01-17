// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ADO1002ProvideRun : Base1002ProvideRun
    {
        /// <summary>
        /// ADO1002
        /// </summary>
        public override string Id => RuleId.ADOProvideRunProperties;

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        protected override void Analyze(Run run, string runPointer)
        {
            /// run.results is chcked by the base class.
            base.Analyze(run, runPointer);

            if (run != null)
            {
                if (run.AutomationDetails == null)
                {
                    // {0}: The 'automationDetails' object in this run does not provide a value.
                    LogResult(
                        runPointer,
                        nameof(RuleResources.ADO1002_ProvideAutomationDetails_Note_Default_Text),
                        this.ServiceName);
                }
                else if (string.IsNullOrWhiteSpace(run.AutomationDetails?.Id))
                {
                    // {0}: The 'id' property in this automationDetails object does not provide a value.
                    LogResult(
                        runPointer,
                        nameof(RuleResources.ADO2010_ProvideAutomationDetailsId_Note_Default_Text),
                        this.ServiceName);
                }
            }
        }
    }
}

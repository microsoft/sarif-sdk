// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ADO1003ProvideToolDriver : Base1003ProvideToolDriver
    {
        /// <summary>
        /// ADO1003
        /// </summary>
        public override string Id => RuleId.ADOProvideToolDriverProperties;

        public override RuleKinds Kinds => RuleKinds.Ado;

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        protected override void Analyze(Run run, string runPointer)
        {
            /// run.tool is chcked by the base class.
            base.Analyze(run, runPointer);

            if (run.Tool != null)
            {
                if (run.Tool.Driver == null)
                {
                    string toolPointer = $"{runPointer}/{SarifPropertyName.Tool}";
                    // {0}: The 'tool' object in this run does not provide a 'driver' value.
                    LogResult(
                        toolPointer,
                        nameof(RuleResources.ADO1003_ProvideDriver_Note_Default_Text),
                        this.ServiceName);
                }
                else if (string.IsNullOrWhiteSpace(run.Tool.Driver.FullName))
                {
                    string driverPointer = $"{runPointer}/{SarifPropertyName.Tool}/{SarifPropertyName.Driver}";

                    // {0}: The 'driver' object in this 'tool' does not provide a 'fullName' value.
                    LogResult(
                        driverPointer,
                        nameof(RuleResources.ADO1003_ProvideFullName_Note_Default_Text),
                        this.ServiceName);
                }
            }
        }
    }
}

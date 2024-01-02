// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class Base1002ProvideRun : SarifValidationSkimmerBase
    {
        protected override string ServiceName => RuleResources.ServiceName_Base;

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Results == null)
            {
                // {0}: The 'results' list in this run does not provide a value.
                LogResult(
                    runPointer,
                    nameof(RuleResources.Base1002_ProvideResultsArray_Note_Default_Text));
            }
        }
    }
}

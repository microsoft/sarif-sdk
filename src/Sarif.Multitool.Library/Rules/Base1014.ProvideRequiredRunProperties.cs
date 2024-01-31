// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseProvideRequiredRunProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        protected override void Analyze(Run run, string runPointer)
        {
            if (run.Results == null)
            {
                // {0}: This 'run' object does not provide a 'results' array property. This property is required by the {1} service.
                LogResult(
                    runPointer.AtProperty(SarifPropertyName.Results),
                    nameof(RuleResources.Base1014_ProvideRequiredRunProperties_Error_MissingResultsArray_Text),
                    this.ServiceName);
            }

            if (run.Tool == null)
            {
                // {0}: This 'run' object does not provide a 'tool' object. This property is required by the {1} service.
                LogResult(
                    runPointer.AtProperty(SarifPropertyName.Tool),
                    nameof(RuleResources.Base1014_ProvideRequiredRunProperties_Error_MissingTool_Text),
                    this.ServiceName);
            }
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseProvideRequiredToolProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        private readonly List<string> _baseMessageResourceNames = new List<string>
        {
            nameof(RuleResources.Base1018_ProvideRequiredToolProperties_Error_MissingDriverName_Text),
            nameof(RuleResources.Base1018_ProvideRequiredToolProperties_Error_MissingDriverRules_Text),
            nameof(RuleResources.Base1018_ProvideRequiredToolProperties_Error_MissingDriver_Text),
        };

        protected ICollection<string> BaseMessageResourceNames => _baseMessageResourceNames;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>();

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        protected override void Analyze(Tool tool, string toolPointer)
        {
            if (tool != null)
            {
                if (tool.Driver == null)
                {
                    // {0}: This 'tool' object does not provide a 'driver' object. This property is required by the {1} service.
                    LogResult(
                        toolPointer,
                        nameof(RuleResources.Base1018_ProvideRequiredToolProperties_Error_MissingDriver_Text));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(tool.Driver.Name))
                    {
                        // {0}: The 'driver' object in this tool does not provide a 'name' value. This property is required by the {1} service.
                        LogResult(
                            toolPointer
                                .AtProperty(SarifPropertyName.Driver),
                            nameof(RuleResources.Base1018_ProvideRequiredToolProperties_Error_MissingDriverName_Text));
                    }

                    if (tool.Driver.Rules == null)
                    {
                        // {0}: The 'driver' object in this tool does not provide a 'rules' array. This property is required by the {1} service.
                        LogResult(
                            toolPointer
                                .AtProperty(SarifPropertyName.Driver),
                            nameof(RuleResources.Base1018_ProvideRequiredToolProperties_Error_MissingDriverRules_Text));
                    }
                }
            }
        }
    }
}

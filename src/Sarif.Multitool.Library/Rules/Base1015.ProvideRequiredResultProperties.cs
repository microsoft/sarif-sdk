// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class BaseProvideRequiredResultProperties
        : SarifValidationSkimmerBase
    {
        public override string Id => string.Empty;

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_EmptyLocationsArray_Text),
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingLocationsArray_Text),
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingMessageText_Text),
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingMessage_Text),
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingPartialFingerprints_Text)
        };

        public override HashSet<RuleKind> RuleKinds => new();

        public override MultiformatMessageString FullDescription => new MultiformatMessageString();

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.Message == null)
            {
                // {0}: This 'result' object does not provide a 'message' object. This property is required by the {1} service.
                LogResult(
                    resultPointer,
                    nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingMessage_Text));
            }
            else if (string.IsNullOrWhiteSpace(result.Message.Text))
            {
                // {0}: The 'message' object on this 'result' object does not provide a 'text' property. This property is required by the {1} service.
                LogResult(
                    resultPointer.AtProperty(SarifPropertyName.Message),
                    nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingMessageText_Text));
            }

            if (result.Locations == null)
            {
                // {0}: This 'result' object does not provide a 'locations' array. This property is required by the {1} service.
                LogResult(
                    resultPointer,
                    nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingLocationsArray_Text));
            }
            else if (result.Locations.Count == 0)
            {
                // {0}: The 'locations' array on this 'result' object is empty. This property is required by the {1} service.
                LogResult(
                    resultPointer,
                    nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_EmptyLocationsArray_Text));
            }

            if (result.PartialFingerprints == null)
            {
                // {0}: This 'result' object does not provide a 'partialFingerprints' dictionary. This property is required by the {1} service.
                LogResult(
                    resultPointer,
                    nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingPartialFingerprints_Text));
            }
        }
    }
}

﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class GhasProvideRequiredPhysicalLocationProperties
        : BaseProvideRequiredResultProperties
    {
        /// <summary>
        /// GH1017
        /// </summary>
        public override string Id => RuleId.GHASProvideRequiredPhysicalLocationProperties;

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.GH1017_ProvideRequiredPhysicalLocationProperties_Error_MissingArtifactLocationUri_Text),
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_EmptyLocationsArray_Text),
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingLocationsArray_Text),
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingMessageText_Text),
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingMessage_Text),
            nameof(RuleResources.Base1015_ProvideRequiredResultProperties_Error_MissingPartialFingerprints_Text)
        };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Ghas });

        protected override string ServiceName => RuleResources.ServiceName_GHAS;

        public GhasProvideRequiredPhysicalLocationProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            base.Analyze(physicalLocation, physicalLocationPointer);

            if (physicalLocation.ArtifactLocation != null && physicalLocation.ArtifactLocation.Uri == null)
            {
                // {0}: The 'artifactLocation' object on this 'physicalLocation' object does not provide a 'uri' object. This property is required by the {1} service.
                LogResult(
                    physicalLocationPointer.AtProperty(SarifPropertyName.ArtifactLocation),
                    nameof(RuleResources.GH1017_ProvideRequiredPhysicalLocationProperties_Error_MissingArtifactLocationUri_Text));
            }
        }
    }
}

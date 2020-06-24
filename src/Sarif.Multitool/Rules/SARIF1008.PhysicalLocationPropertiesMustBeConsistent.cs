// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class PhysicalLocationPropertiesMustBeConsistent : SarifValidationSkimmerBase
    {
        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_FullDescription_Text
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override string Id => RuleId.PhysicalLocationPropertiesMustBeConsistent;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Error_ContextRegionRequiresRegion_Text),
            nameof(RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Error_ContextRegionMustBeProperSupersetOfRegion_Text)
        };

        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            if (physicalLocation.ContextRegion == null)
            {
                return;
            }

            if (physicalLocation.Region == null)
            {
                LogResult(physicalLocationPointer, nameof(RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Error_ContextRegionRequiresRegion_Text));
                return;
            }

            if (!physicalLocation.ContextRegion.IsProperSupersetOf(physicalLocation.Region))
            {
                LogResult(physicalLocationPointer, nameof(RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Error_ContextRegionMustBeProperSupersetOfRegion_Text));
            }
        }
    }
}

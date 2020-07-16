// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ContextRegionRequiresRegion : SarifValidationSkimmerBase
    {
        public ContextRegionRequiresRegion() : base(
            RuleId.ContextRegionRequiresRegion,
            RuleResources.SARIF1016_ContextRegionRequiresRegion,
            FailureLevel.Error,
            new string[] { nameof(RuleResources.SARIF1016_Default) }
            )
        { }

        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            if (physicalLocation.ContextRegion != null && physicalLocation.Region == null)
            {
                LogResult(physicalLocationPointer, nameof(RuleResources.SARIF1016_Default));
            }
        }
    }
}

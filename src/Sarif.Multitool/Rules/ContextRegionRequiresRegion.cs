// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ContextRegionRequiresRegion : SarifValidationSkimmerBase
    {
        private readonly Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1016_ContextRegionRequiresRegion
        };

        public override Message FullDescription => _fullDescription;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF1016
        /// </summary>
        public override string Id => RuleId.ContextRegionRequiresRegion;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1016_Default)
        };

        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            if (physicalLocation.ContextRegion != null && physicalLocation.Region == null)
            {
                LogResult(physicalLocationPointer, nameof(RuleResources.SARIF1016_Default));
            }
        }
    }
}

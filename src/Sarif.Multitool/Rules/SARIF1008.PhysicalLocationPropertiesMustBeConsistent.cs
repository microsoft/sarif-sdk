// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class PhysicalLocationPropertiesMustBeConsistent : SarifValidationSkimmerBase
    {
        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override string Id => RuleId.PhysicalLocationPropertiesMustBeConsistent;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1008_Default)
        };

        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            if (physicalLocation.ContextRegion != null && physicalLocation.Region == null)
            {
                LogResult(physicalLocationPointer, nameof(RuleResources.SARIF1008_Default));
            }
        }
    }
}

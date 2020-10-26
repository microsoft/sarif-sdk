// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class PhysicalLocationPropertiesMustBeConsistent : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF1008
        /// </summary>
        public override string Id => RuleId.PhysicalLocationPropertiesMustBeConsistent;

        /// <summary>
        /// Ensure consistency among the properties of a 'physicalLocation' object.
        ///
        /// A SARIF 'physicalLocation' object has two related properties 'region' and 'contextRegion'.
        /// If 'contextRegion' is present, then 'region' must also be present, and 'contextRegion'
        /// must be a "proper superset" of 'region'. That is, 'contextRegion' must completely contain
        /// 'region', and it must be larger than 'region'. To understand why this is so we must
        /// understand the roles of the 'region' and 'contextRegion' properties.
        /// 
        /// 'region' allows both users and tools to distinguish similar results within the same
        /// artifact. If a SARIF viewer has access to the artifact, it can display it, and highlight
        /// the location identified by the analysis tool.If the region has a 'snippet' property,
        /// then even if the viewer doesn't have access to the artifact (which might be the case
        /// for a web-based viewer), it can still display the faulty code.
        /// 
        /// 'contextRegion' provides users with a broader view of the result location. Typically,
        /// it consists of a range starting a few lines before 'region' and ending a few lines after.
        /// Again, if a SARIF viewer has access to the artifact, it can display it, and highlight
        /// the context region (perhaps in a lighter shade than the region itself). This isn't
        /// terribly useful since the user can already see the whole file, with the 'region'
        /// already highlighted. But if 'contextRegion' has a 'snippet' property, then even a
        /// viewer without access to the artifact can display a few lines of code surrounding
        /// the actual result, which is helpful to users.
        /// 
        /// If the validator reports that 'contextRegion' is not a proper superset of 'region',
        /// then it's possible that the tool reversed 'region' and 'contextRegion'. If 'region'
        /// and 'contextRegion' are identical, the tool should simply omit 'contextRegion'.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Error_ContextRegionRequiresRegion_Text),
            nameof(RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Error_ContextRegionMustBeProperSupersetOfRegion_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        protected override void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            if (physicalLocation.ContextRegion == null)
            {
                return;
            }

            if (physicalLocation.Region == null)
            {
                // {0}: This 'physicalLocation' object contains a 'contextRegion' property, but it
                // does not contain a 'region' property. This is invalid because the purpose of
                // 'contextRegion' is to provide a viewing context around the 'region' which is the
                // location of the result. If a tool associates only one region with a result, it
                // must populate 'region', not 'contextRegion'.
                LogResult(
                    physicalLocationPointer,
                    nameof(RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Error_ContextRegionRequiresRegion_Text));
                return;
            }

            if (!physicalLocation.ContextRegion.IsProperSupersetOf(physicalLocation.Region))
            {
                // {0}: This 'physicalLocation' object contains both a 'region' and a 'contextRegion'
                // property, but 'contextRegion' is not a proper superset of 'region'. This is invalid
                // because the purpose of 'contextRegion' is to provide a viewing context around the
                // 'region' which is the location of the result. It's possible that the tool reversed
                // 'region' and 'contextRegion'. If 'region' and 'contextRegion' are identical, the
                // tool must omit 'contextRegion'.
                LogResult(
                    physicalLocationPointer,
                    nameof(RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Error_ContextRegionMustBeProperSupersetOfRegion_Text));
            }
        }
    }
}

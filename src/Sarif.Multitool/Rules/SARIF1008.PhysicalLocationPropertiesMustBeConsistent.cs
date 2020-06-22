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

            if (!IsRegionProperSuperset(physicalLocation.ContextRegion, physicalLocation.Region))
            {
                LogResult(physicalLocationPointer, nameof(RuleResources.SARIF1008_PhysicalLocationPropertiesMustBeConsistent_Error_ContextRegionMustBeProperSupersetOfRegion_Text));
            }
        }

        private static bool IsRegionProperSuperset(Region superRegion, Region subRegion)
        {
            if (IsLineColumnBasedTextRegion(superRegion) &&
                IsLineColumnBasedTextRegion(subRegion) &&
                !IsLineColumnBasedTextRegionProperSuperset(superRegion, subRegion))
            {
                return false;
            }

            if (IsOffsetBasedTextRegion(superRegion) &&
                IsOffsetBasedTextRegion(subRegion) &&
                !IsOffsetBasedTextRegionProperSupetSet(superRegion, subRegion))
            {

                return false;
            }

            if (IsBinaryRegion(superRegion) &&
                IsBinaryRegion(subRegion) &&
                !IsBinaryRegionProperSuperset(superRegion, subRegion))
            {
                return false;
            }

            // if we reach here, the region and context region have been expressed as different property sets,
            // and it is not possible to judge validity without looking at the actual content.
            // It is a potential false negative.
            return true;

        }

        private static bool IsBinaryRegionProperSuperset(Region superRegion, Region subRegion)
        {
            if (superRegion.ByteOffset > subRegion.ByteOffset)
            {
                return false;
            }

            if (superRegion.ByteOffset == subRegion.ByteOffset && superRegion.ByteLength <= subRegion.ByteLength)
            {
                return false;
            }

            return true;
        }

        private static bool IsLineColumnBasedTextRegionProperSuperset(Region superRegion, Region subRegion)
        {
            if (superRegion.StartLine > subRegion.StartLine || superRegion.EndLine < subRegion.EndLine)
            {
                return false;
            }

            if (superRegion.StartLine == subRegion.StartLine && superRegion.StartColumn < subRegion.StartColumn)
            {
                return false;
            }

            if (superRegion.EndLine == subRegion.EndLine && superRegion.EndColumn > subRegion.EndColumn)
            {
                return false;
            }

            if (superRegion.StartLine == subRegion.StartLine &&
                superRegion.EndLine == subRegion.EndLine &&
                superRegion.StartColumn == subRegion.StartColumn &&
                superRegion.EndColumn == subRegion.EndColumn)
            {
                return false;
            }

            return true;
        }

        private static bool IsOffsetBasedTextRegionProperSupetSet(Region superRegion, Region subRegion)
        {
            if (superRegion.CharOffset > subRegion.CharOffset)
            {
                return false;
            }

            if (superRegion.CharOffset == subRegion.CharOffset && superRegion.CharLength <= subRegion.CharLength)
            {
                return false;
            }
            return true;
        }

        private static bool IsLineColumnBasedTextRegion(Region region)
        {
            return region.StartLine >= 1;
        }

        private static bool IsOffsetBasedTextRegion(Region region)
        {
            return region.CharOffset > 0;
        }

        private static bool IsBinaryRegion(Region region)
        {
            return region.ByteOffset >= 0;
        }
    }
}

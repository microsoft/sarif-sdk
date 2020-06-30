// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideContextRegion : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2011
        /// </summary>
        public override string Id => RuleId.ProvideContextRegion;

        /// <summary>
        /// Placeholder
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2011_ProvideContextRegion_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2011_ProvideContextRegion_Note_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Note;

        protected override void Analyze(Result result, string resultPointer)
        {
            if (result.Locations != null)
            {
                string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);
                for (int i = 0; i < result.Locations.Count; i++)
                {
                    AnalyzeLocation(result.Locations[i], locationsPointer.AtIndex(i));
                }
            }
        }

        private void AnalyzeLocation(Location location, string locationPointer)
        {
            if (location.PhysicalLocation?.Region != null)
            {
                string physicalLocation = locationPointer.AtProperty(SarifPropertyName.PhysicalLocation);
                if (location.PhysicalLocation?.ContextRegion == null)
                {
                    // {0}: Placeholder
                    LogResult(
                        physicalLocation,
                        nameof(RuleResources.SARIF2011_ProvideContextRegion_Note_Default_Text));
                }
            }
        }
    }
}

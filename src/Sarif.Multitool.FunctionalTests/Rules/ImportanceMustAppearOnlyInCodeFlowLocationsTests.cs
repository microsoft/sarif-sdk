// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ImportanceMustAppearOnlyInCodeFlowLocationsTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(ImportanceMustAppearOnlyInCodeFlowLocations_ImportanceAppearsInCodeFlow))]
        public void ImportanceMustAppearOnlyInCodeFlowLocations_ImportanceAppearsInCodeFlow()
        {
            Verify(new ImportanceMustAppearOnlyInCodeFlowLocations(), "ImportanceAppearsInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(ImportanceMustAppearOnlyInCodeFlowLocations_ImportanceAppearsInRelatedLocation))]
        public void ImportanceMustAppearOnlyInCodeFlowLocations_ImportanceAppearsInRelatedLocation()
        {
            Verify(new ImportanceMustAppearOnlyInCodeFlowLocations(), "ImportanceAppearsInRelatedLocation.sarif");
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class StepMustAppearOnlyInCodeFlowLocationsTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(StepMustAppearOnlyInCodeFlowLocations_StepAppearsInCodeFlow))]
        public void StepMustAppearOnlyInCodeFlowLocations_StepAppearsInCodeFlow()
        {
            Verify(new StepMustAppearOnlyInCodeFlowLocations(), "StepAppearsInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(StepMustAppearOnlyInCodeFlowLocations_StepAppearsInRelatedLocation))]
        public void StepMustAppearOnlyInCodeFlowLocations_StepAppearsInRelatedLocation()
        {
            Verify(new StepMustAppearOnlyInCodeFlowLocations(), "StepAppearsInRelatedLocation.sarif");
        }
    }
}

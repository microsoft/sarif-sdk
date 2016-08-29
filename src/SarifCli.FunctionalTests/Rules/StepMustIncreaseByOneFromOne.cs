// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class StepMustIncreaseByOneFromOneTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(StepMustIncreaseByOneFromOne_ValidSteps))]
        public void StepMustIncreaseByOneFromOne_ValidSteps()
        {
            Verify(new StepMustIncreaseByOneFromOne(), "ValidSteps.sarif");
        }

        [Fact(DisplayName = nameof(StepMustIncreaseByOneFromOne_StepNotPresentOnAllLocations))]
        public void StepMustIncreaseByOneFromOne_StepNotPresentOnAllLocations()
        {
            Verify(new StepMustIncreaseByOneFromOne(), "StepNotPresentOnAllLocations.sarif");
        }
    }
}

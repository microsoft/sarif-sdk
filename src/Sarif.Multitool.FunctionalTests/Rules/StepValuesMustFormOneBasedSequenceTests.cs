// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class StepValuesMustFormOneBasedSequenceTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(StepValuesMustFormOneBasedSequence_ValidSteps))]
        public void StepValuesMustFormOneBasedSequence_ValidSteps()
        {
            Verify(new StepValuesMustFormOneBasedSequence(), "ValidSteps.sarif");
        }

        [Fact(DisplayName = nameof(StepValuesMustFormOneBasedSequence_NoStepsPresent))]
        public void StepValuesMustFormOneBasedSequence_NoStepsPresent()
        {
            Verify(new StepValuesMustFormOneBasedSequence(), "NoStepsPresent.sarif");
        }

        [Fact(DisplayName = nameof(StepValuesMustFormOneBasedSequence_StepNotPresentOnAllLocations))]
        public void StepValuesMustFormOneBasedSequence_StepNotPresentOnAllLocations()
        {
            Verify(new StepValuesMustFormOneBasedSequence(), "StepNotPresentOnAllLocations.sarif");
        }

        [Fact(DisplayName = nameof(StepValuesMustFormOneBasedSequence_InvalidStepValues))]
        public void StepValuesMustFormOneBasedSequence_InvalidStepValues()
        {
            Verify(new StepValuesMustFormOneBasedSequence(), "InvalidStepValues.sarif");
        }
    }
}

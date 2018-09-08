// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class StepValuesMustFormOneBasedSequenceTests : SkimmerTestsBase<StepValuesMustFormOneBasedSequence>
    {
        [Fact(DisplayName = nameof(StepValuesMustFormOneBasedSequence_ValidSteps))]
        public void StepValuesMustFormOneBasedSequence_ValidSteps()
        {
            Verify("ValidSteps.sarif");
        }

        [Fact(DisplayName = nameof(StepValuesMustFormOneBasedSequence_NoStepsPresent))]
        public void StepValuesMustFormOneBasedSequence_NoStepsPresent()
        {
            Verify("NoStepsPresent.sarif");
        }

        [Fact(DisplayName = nameof(StepValuesMustFormOneBasedSequence_StepNotPresentOnAllLocations))]
        public void StepValuesMustFormOneBasedSequence_StepNotPresentOnAllLocations()
        {
            Verify("StepNotPresentOnAllLocations.sarif");
        }

        [Fact(DisplayName = nameof(StepValuesMustFormOneBasedSequence_InvalidStepValues))]
        public void StepValuesMustFormOneBasedSequence_InvalidStepValues()
        {
            Verify("InvalidStepValues.sarif");
        }

        [Fact(DisplayName = nameof(StepValuesMustFormOneBasedSequence_MultipleThreadFlows))]
        public void StepValuesMustFormOneBasedSequence_MultipleThreadFlows()
        {
            Verify("MultipleThreadFlows.sarif");
        }
    }
}

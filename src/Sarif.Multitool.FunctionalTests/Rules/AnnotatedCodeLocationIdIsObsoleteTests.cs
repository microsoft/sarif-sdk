// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AnnotatedCodeLocationIdIsObsoleteTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationDoesNotHaveId))]
        public void AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationDoesNotHaveId()
        {
            Verify(new AnnotatedCodeLocationIdIsObsolete(), "AnnotatedCodeLocationDoesNotHaveId.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationHasIdInCodeFlow))]
        public void AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationHasIdInCodeFlow()
        {
            Verify(new AnnotatedCodeLocationIdIsObsolete(), "AnnotatedCodeLocationHasIdInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationHasIdInRelatedLocation))]
        public void AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationHasIdInRelatedLocation()
        {
            Verify(new AnnotatedCodeLocationIdIsObsolete(), "AnnotatedCodeLocationHasIdInRelatedLocation.sarif");
        }
    }
}

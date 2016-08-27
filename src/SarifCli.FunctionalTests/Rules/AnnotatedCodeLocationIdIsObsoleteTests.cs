// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    class AnnotatedCodeLocationIdIsObsoleteTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationDoesNotHaveId))]
        public void AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationDoesNotHaveId()
        {
            Verify(new AnnotatedCodeLocationIdIsObsolete(), "AnnotatedCodeLocationDoesNotHaveId.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationInCodeFlowHasId))]
        public void AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationInCodeFlowHasId()
        {
            Verify(new AnnotatedCodeLocationIdIsObsolete(), "AnnotatedCodeLocationInCodeFlowHasId.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationInRelatedLocationHasId))]
        public void AnnotatedCodeLocationIdIsObsolete_AnnotatedCodeLocationInRelatedLocationHasId()
        {
            Verify(new AnnotatedCodeLocationIdIsObsolete(), "AnnotatedCodeLocationInRelatedLocationHasId.sarif");
        }
    }
}

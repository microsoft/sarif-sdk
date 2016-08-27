// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    public class AnnotatedCodeLocationEssentialIsObsoleteTests : SkimmerTestsBase
    {
        [Fact(DisplayName = nameof(AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationDoesNotHaveEssential))]
        public void AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationDoesNotHaveEssential()
        {
            Verify(new AnnotatedCodeLocationEssentialIsObsolete(), "AnnotatedCodeLocationDoesNotHaveId.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationInCodeFlowHasEssential))]
        public void AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationInCodeFlowHasEssential()
        {
            Verify(new AnnotatedCodeLocationEssentialIsObsolete(), "AnnotatedCodeLocationInCodeFlowHasEssential.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationInRelatedLocationHasEssential))]
        public void AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationInRelatedLocationHasEssential()
        {
            Verify(new AnnotatedCodeLocationEssentialIsObsolete(), "AnnotatedCodeLocationInRelatedLocationHasEssential.sarif");
        }
    }
}

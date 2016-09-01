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
            Verify(new AnnotatedCodeLocationEssentialIsObsolete(), "AnnotatedCodeLocationDoesNotHaveEssential.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationHasEssentialInCodeFlow))]
        public void AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationHasEssentialInCodeFlow()
        {
            Verify(new AnnotatedCodeLocationEssentialIsObsolete(), "AnnotatedCodeLocationHasEssentialInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationHasDefaultEssentialInCodeFlow))]
        public void AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationHasDefaultEssentialInCodeFlow()
        {
            Verify(new AnnotatedCodeLocationEssentialIsObsolete(), "AnnotatedCodeLocationHasDefaultEssentialInCodeFlow.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationHasEssentialInRelatedLocation))]
        public void AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationHasEssentialInRelatedLocation()
        {
            Verify(new AnnotatedCodeLocationEssentialIsObsolete(), "AnnotatedCodeLocationHasEssentialInRelatedLocation.sarif");
        }

        [Fact(DisplayName = nameof(AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationHasDefaultEssentialInRelatedLocation))]
        public void AnnotatedCodeLocationEssentialIsObsolete_AnnotatedCodeLocationHasDefaultEssentialInRelatedLocation()
        {
            Verify(new AnnotatedCodeLocationEssentialIsObsolete(), "AnnotatedCodeLocationHasDefaultEssentialInRelatedLocation.sarif");
        }
    }
}

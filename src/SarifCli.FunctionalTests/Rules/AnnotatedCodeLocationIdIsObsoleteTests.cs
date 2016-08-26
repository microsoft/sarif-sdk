// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Cli.Rules;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Cli.FunctionalTests.Rules
{
    class AnnotatedCodeLocationIdIsObsoleteTests : SkimmerTestsBase
    {
        [Fact]
        public void AnnotatedCodeLocationIdIsObsolete_NoDiagnostic_AnnotatedCodeLocationDoesNotHaveId()
        {
            Verify(new AnnotatedCodeLocationIdIsObsolete(), "AnnotatedCodeLocationDoesNotHaveId.sarif");
        }

        [Fact]
        public void AnnotatedCodeLocationIdIsObsolete_Diagnostic_AnnotatedCodeLocationInCodeFlowHasId()
        {
            Verify(new AnnotatedCodeLocationIdIsObsolete(), "AnnotatedCodeLocationInCodeFlowHasId.sarif");
        }

        [Fact]
        public void AnnotatedCodeLocationIdIsObsolete_Diagnostic_AnnotatedCodeLocationInRelatedLocationHasId()
        {
            Verify(new AnnotatedCodeLocationIdIsObsolete(), "AnnotatedCodeLocationInRelatedLocationHasId.sarif");
        }
    }
}

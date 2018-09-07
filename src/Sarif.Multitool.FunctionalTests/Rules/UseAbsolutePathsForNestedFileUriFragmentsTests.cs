// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class UseAbsolutePathsForNestedFileUriFragmentsTests : SkimmerTestsBase<UseAbsolutePathsForNestedFileUriFragments>
    {
        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentsAreAbsolute))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentsAreAbsolute()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentsAreAbsolute.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInFileLocationUri))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInFileLocationUri()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInFileLocationUri.sarif");
        }

        [Fact(DisplayName = nameof(UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInFilePropertyName))]
        public void UseAbsolutePathsForNestedFileUriFragments_NestedFileUriFragmentIsRelativeInFilePropertyName()
        {
            Verify(new UseAbsolutePathsForNestedFileUriFragments(), "NestedFileUriFragmentIsRelativeInFilePropertyName.sarif");
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling
{
    public class FilingTargetFactoryTests
    {
        [Fact]
        public void CreateFilingTarget_RequiresAUri()
        {
            Action action = () => FilingTargetFactory.CreateFilingTarget(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateFilingTarget_RequiresRecognizedUriPattern()
        {
            const string ProjectUriString = "https://www.example.com/myOrg/myProject";

            Action action = () => FilingTargetFactory.CreateFilingTarget(ProjectUriString);

            action.Should().Throw<ArgumentException>().WithMessage($"*{ProjectUriString}*");
        }

        [Fact]
        public void CreateFilingTarget_DoesNotAllowExtraPathSegments()
        {
            const string ProjectUriString = "https://github.com/myOrg/myProject/issues";

            Action action = () => FilingTargetFactory.CreateFilingTarget(ProjectUriString);

            action.Should().Throw<ArgumentException>().WithMessage($"*{ProjectUriString}*");
        }

        [Fact]
        public void CreateFilingTarget_CreatesGitHubFilingTarget()
        {
            const string ProjectUriString = "https://github.com/myOrg/myProject";

            FilingTarget filingTarget = FilingTargetFactory.CreateFilingTarget(ProjectUriString);

            filingTarget.Should().BeOfType<GitHubFilingTarget>();
        }

        [Fact]
        public void CreateFilingTarget_CreatesAzureDevOpsFilingTarget()
        {
            const string ProjectUriString = "https://dev.azure.com/myOrg/myProject";

            FilingTarget filingTarget = FilingTargetFactory.CreateFilingTarget(ProjectUriString);

            filingTarget.Should().BeOfType<AzureDevOpsFilingTarget>();
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.WorkItemFiling
{
    public class FilingTargetFactoryTests
    {       
        [Fact]
        public void CreateFilingTarget_ThrowsIfUriIsNull
            ()
        {
            Action action = () => FilingClientFactory.CreateFilingTarget<TestWorkItemData>(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateFilingTarget_ThrowsIfUriPatternIsNotRecognized()
        {
            const string ProjectUriString = "https://www.example.com/myOrg/myProject";

            Action action = () => FilingClientFactory.CreateFilingTarget<TestWorkItemData>(ProjectUriString);

            action.Should().Throw<ArgumentException>().WithMessage($"*{ProjectUriString}*");
        }

        [Fact]
        public void CreateFilingTarget_ThrowsIfUriIncludesAdditionalPathSegments()
        {
            const string ProjectUriString = "https://github.com/myOrg/myProject/issues";

            Action action = () => FilingClientFactory.CreateFilingTarget<TestWorkItemData>(ProjectUriString);

            action.Should().Throw<ArgumentException>().WithMessage($"*{ProjectUriString}*");
        }

        [Fact]
        public void CreateFilingTarget_CreatesGitHubFilingTarget()
        {
            const string ProjectUriString = "https://github.com/myOrg/myProject";

            var filingTarget = FilingClientFactory.CreateFilingTarget<TestWorkItemData>(ProjectUriString);

            filingTarget.Should().BeOfType<GitHubClientWrapper<TestWorkItemData>>();
            string.IsNullOrEmpty(filingTarget.ProjectOrRepository).Should().BeFalse();
            string.IsNullOrEmpty(filingTarget.AccountOrOrganization).Should().BeFalse();
        }

        [Fact]
        public void CreateFilingTarget_CreatesAzureDevOpsFilingTarget()
        {
            const string ProjectUriString = "https://dev.azure.com/myOrg/myProject";

            var filingTarget = FilingClientFactory.CreateFilingTarget<TestWorkItemData>(ProjectUriString);

            filingTarget.Should().BeOfType<AzureDevOpsClientWrapper<TestWorkItemData>>();
            string.IsNullOrEmpty(filingTarget.ProjectOrRepository).Should().BeFalse();
            string.IsNullOrEmpty(filingTarget.AccountOrOrganization).Should().BeFalse();
        }

        [Fact]
        public void CreateFilingTarget_CreatesLegacyAzureDevOpsFilingTarget()
        {
            const string ProjectUriString = "https://myorg.visualstudio.com/myProject";

            var filingTarget = FilingClientFactory.CreateFilingTarget<TestWorkItemData>(ProjectUriString);

            filingTarget.ProjectOrRepository.Should().NotBeNull();
            string.IsNullOrEmpty(filingTarget.ProjectOrRepository).Should().BeFalse();
            string.IsNullOrEmpty(filingTarget.AccountOrOrganization).Should().BeFalse();
        }
    }
}

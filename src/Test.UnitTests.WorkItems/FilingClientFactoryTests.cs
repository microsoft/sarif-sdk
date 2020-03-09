// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.WorkItems
{
    public class FilingClientFactoryTests
    {
        [Fact]
        public void CreateFilingTarget_ThrowsIfUriIsNull
            ()
        {
            Action action = () => FilingClientFactory.Create(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateFilingTarget_ThrowsIfUriPatternIsNotRecognized()
        {
            Uri projectUri = new Uri("https://www.example.com/myOrg/myProject");

            Action action = () => FilingClientFactory.Create(projectUri);

            action.Should().Throw<ArgumentException>().WithMessage($"*{projectUri}*");
        }

        [Fact]
        public void CreateFilingTarget_ThrowsIfUriIncludesAdditionalPathSegments()
        {
            Uri projectUri = new Uri("https://github.com/myOrg/myProject/issues");

            Action action = () => FilingClientFactory.Create(projectUri);

            action.Should().Throw<ArgumentException>().WithMessage($"*{projectUri}*");
        }

        [Fact]
        public void CreateFilingTarget_CreatesGitHubFilingTarget()
        {
            Uri projectUri = new Uri("https://github.com/myOrg/myProject");

            FilingClient filingTarget = FilingClientFactory.Create(projectUri);

            filingTarget.Should().BeOfType<GitHubFilingClient>();
            string.IsNullOrEmpty(filingTarget.ProjectOrRepository).Should().BeFalse();
            string.IsNullOrEmpty(filingTarget.AccountOrOrganization).Should().BeFalse();
        }

        [Fact]
        public void CreateFilingTarget_CreatesAzureDevOpsFilingTarget()
        {
            Uri projectUri = new Uri("https://dev.azure.com/myOrg/myProject");

            FilingClient filingTarget = FilingClientFactory.Create(projectUri);

            filingTarget.Should().BeOfType<AzureDevOpsFilingClient>();
            string.IsNullOrEmpty(filingTarget.ProjectOrRepository).Should().BeFalse();
            string.IsNullOrEmpty(filingTarget.AccountOrOrganization).Should().BeFalse();
        }

        [Fact]
        public void CreateFilingTarget_CreatesLegacyAzureDevOpsFilingTarget()
        {
            Uri projectUri = new Uri("https://myorg.visualstudio.com/myProject");

            FilingClient filingTarget = FilingClientFactory.Create(projectUri);

            filingTarget.ProjectOrRepository.Should().NotBeNull();
            string.IsNullOrEmpty(filingTarget.ProjectOrRepository).Should().BeFalse();
            string.IsNullOrEmpty(filingTarget.AccountOrOrganization).Should().BeFalse();
        }
    }
}

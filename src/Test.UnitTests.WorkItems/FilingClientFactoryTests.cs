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
            var hostUri = new Uri("https://www.example.com/myOrg/myProject");

            Action action = () => FilingClientFactory.Create(hostUri);

            action.Should().Throw<ArgumentException>().WithMessage($"*{hostUri}*");
        }

        [Fact]
        public void CreateFilingTarget_ThrowsIfUriIncludesAdditionalPathSegments()
        {
            var hostUri = new Uri("https://github.com/myOrg/myProject/issues");

            Action action = () => FilingClientFactory.Create(hostUri);

            action.Should().Throw<ArgumentException>().WithMessage($"*{hostUri}*");
        }

        [Fact]
        public void CreateFilingTarget_CreatesGitHubFilingTarget()
        {
            var hostUri = new Uri("https://github.com/myOrg/myProject");

            FilingClient filingTarget = FilingClientFactory.Create(hostUri);

            filingTarget.Should().BeOfType<GitHubFilingClient>();
            string.IsNullOrEmpty(filingTarget.ProjectOrRepository).Should().BeFalse();
            string.IsNullOrEmpty(filingTarget.AccountOrOrganization).Should().BeFalse();
        }

        [Fact]
        public void CreateFilingTarget_CreatesAzureDevOpsFilingTarget()
        {
            var hostUri = new Uri("https://dev.azure.com/myOrg/myProject");

            FilingClient filingTarget = FilingClientFactory.Create(hostUri);

            filingTarget.Should().BeOfType<AzureDevOpsFilingClient>();
            string.IsNullOrEmpty(filingTarget.ProjectOrRepository).Should().BeFalse();
            string.IsNullOrEmpty(filingTarget.AccountOrOrganization).Should().BeFalse();
        }

        [Fact]
        public void CreateFilingTarget_CreatesLegacyAzureDevOpsFilingTarget()
        {
            var hostUri = new Uri("https://myorg.visualstudio.com/myProject");

            FilingClient filingTarget = FilingClientFactory.Create(hostUri);

            filingTarget.ProjectOrRepository.Should().NotBeNull();
            string.IsNullOrEmpty(filingTarget.ProjectOrRepository).Should().BeFalse();
            string.IsNullOrEmpty(filingTarget.AccountOrOrganization).Should().BeFalse();
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.GitHub;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.WorkItemFiling.GitHub
{
    public class GitHubTargetTests
    {
        [Fact]
        public void GitHubFilingTarget_RequiresGitHubClient()
        {
            Action action = () => new GitHubFilingTarget(gitHubClient: null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GitHubFilingTarget_CanBeCreated()
        {
            Action action = () => new GitHubFilingTarget(gitHubClient: CreateMockGitHubClient());

            action.Should().NotThrow();
        }

        private static IGitHubClient CreateMockGitHubClient()
        {
            var mockGitHubClient = new Mock<IGitHubClient>();
            return mockGitHubClient.Object;
        }
    }
}
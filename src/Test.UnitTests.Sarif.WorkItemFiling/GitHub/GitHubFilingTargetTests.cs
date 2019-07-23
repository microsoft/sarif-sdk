// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.GitHub;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling.Grouping;
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

        [Fact]
        public async Task GitHubFilingTarget_IsNotYetImplemented()
        {
            var filingTarget = new GitHubFilingTarget(gitHubClient: CreateMockGitHubClient());

            Func<Task> action = async () => await filingTarget.FileWorkItems(new List<ResultGroup>());

            await action.ShouldThrowAsync<NotImplementedException>();
        }

        private static IGitHubClient CreateMockGitHubClient()
        {
            var mockGitHubClient = new Mock<IGitHubClient>();
            return mockGitHubClient.Object;
        }
    }
}
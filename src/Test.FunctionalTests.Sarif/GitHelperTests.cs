// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class GitHelperTests
    {
        [Fact]
        public void GetVersionControlDetails_ReturnsExpectedInformation()
        {
            var gitHelper = new GitHelper();

            // This test assumes that the directory in which the tests are run lies under the local
            // repo root, for example, <repo-root>\bld\bin\AnyCPU_Release\Test.FunctionalTests.Sarif\netcoreapp2.1.
            string localRepoRoot = gitHelper.GetRepositoryRoot(Environment.CurrentDirectory);

            VersionControlDetails versionControlDetails =
                gitHelper.GetVersionControlDetails(Environment.CurrentDirectory, crawlParentDirectories: true);

            // We don't check for "microsoft/sarif-sdk" so that the test will pass in forks of the
            // original repo.
            versionControlDetails.RepositoryUri.OriginalString
                .Should().StartWith("https://github.com/")
                .And.EndWith("/sarif-sdk");

            versionControlDetails.MappedTo.Uri.OriginalString.Should().Be(localRepoRoot);
            versionControlDetails.MappedTo.UriBaseId.Should().BeNull();
        }
    }
}

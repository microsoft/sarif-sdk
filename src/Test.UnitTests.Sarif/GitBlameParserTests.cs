// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class GitBlameParserTests
    {
        private const string GitBlameText =
            "cf5373aa871706d62dd998fa333219ae9ede03d9 1 1 2" +
                "\nauthor Cat1" +
                "\nauthor-mail <cat1@contoso.com>" +
                "\nauthor-time 1619541913" +
                "\nauthor-tz -0700" +
                "\ncommitter Cat2" +
                "\ncommitter-mail <cat2@contoso.com>" +
            "\ncf5373aa871706d62dd998fa333219ae9ede03e8 1 1 3" +
                "\nauthor Cat3" +
                "\nauthor-mail <cat3@contoso.com>" +
                "\nauthor-time 1619541913" +
                "\nauthor-tz -0700" +
                "\ncommitter Cat4" +
                "\ncommitter-mail <cat4@contoso.com>";

        private readonly List<IBlameHunk> ExpectedBlameHunks = new List<IBlameHunk>
        {
            new BlameHunk("Cat1", "cat1@contoso.com", "cf5373aa871706d62dd998fa333219ae9ede03d9", 2, 1),
            new BlameHunk("Cat3", "cat3@contoso.com", "cf5373aa871706d62dd998fa333219ae9ede03e8", 3, 1),
        };

        [Fact]
        public void ParseBlameInfo_Success()
        {
            // Act
            IEnumerable<IBlameHunk> actualBlameHunks = SarifTransformerUtilities.ParseBlameInformation(GitBlameText);

            // Assert
            actualBlameHunks.Should().BeEquivalentTo(ExpectedBlameHunks);
        }
    }
}

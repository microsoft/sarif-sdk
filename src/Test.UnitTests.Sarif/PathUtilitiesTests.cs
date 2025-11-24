// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class PathUtilitiesTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("file.txt", ".txt")]
        [InlineData("file.tar.gz", ".gz")]
        [InlineData("file", "")]
        [InlineData("file.", "")]
        [InlineData(".file", "")]
        [InlineData("path/file.txt", ".txt")]
        [InlineData("path\\file.txt", ".txt")]
        [InlineData("C:\\path\\file.txt", ".txt")]
        [InlineData("/path/file.txt", ".txt")]
        [InlineData("http://example.com/some<character>test/bad\"characters\"path.txt", ".txt")]
        [InlineData("http://example.com/file|with|pipes.doc", ".doc")]
        [InlineData("path/file<test>.xml", ".xml")]
        public void GetExtension_HandlesVariousPaths(string path, string expected)
        {
            string actual = PathUtilities.GetExtension(path);
            actual.Should().Be(expected);
        }

        [Fact]
        public void GetExtension_DoesNotThrowOnIllegalCharacters()
        {
            // This is the key test - Path.GetExtension would throw ArgumentException on .NET Framework 4.8
            // but our implementation should handle it gracefully
            string pathWithIllegalChars = "http://example.com/some<character>test/bad\"characters\"path.txt";
            
            string extension = PathUtilities.GetExtension(pathWithIllegalChars);
            
            extension.Should().Be(".txt");
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class FileSpecifierTestsFixture : IDisposable
    {
        public FileSpecifierTestsFixture()
        {
            string rootDirectory = Guid.NewGuid().ToString();
            RootDirectory = Path.Combine(Path.GetTempPath(), rootDirectory);

            Directory.CreateDirectory(RootDirectory);

            for (int i = 0; i < 5; i++)
            {
                string childDirectory = Path.Combine(RootDirectory, "TestDirectory" + i.ToString());

                Directory.CreateDirectory(childDirectory);

                for (int j = 0; j < 5; j++)
                {
                    string file = Path.Combine(childDirectory, "TestFile" + j.ToString() + ".txt");
                    File.WriteAllText(file, "12345");
                }
            }
        }

        public string RootDirectory { get; set; }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, true);
            }
        }
    }

    public class FileSpecifierTests : IClassFixture<FileSpecifierTestsFixture>
    {
        private readonly FileSpecifierTestsFixture _fixture;

        public FileSpecifierTests(FileSpecifierTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(@"Sarif.dll", 1)]
        [InlineData(@".\Sarif.dll", 1)]
        [InlineData(@".\Sarif.dll*", 1)]
        [InlineData(@"Sarif.dll*", 1)]

        public void ResolveFiles(string input, int expectedCount)
        {
            // This test provides basic verification that simple patterns
            // matching one or more files in the current directory succeed
            var specifier = new FileSpecifier(input);
            specifier.Files.Count.Should().Be(expectedCount);

            specifier = new FileSpecifier(Path.Combine(Environment.CurrentDirectory, input));
            specifier.Files.Count.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData(@"TestFile1.txt", false, 0)]
        [InlineData(@"TestFile1.txt", true, 5)]
        [InlineData(@"DoesNotExist.dll", true, 0)]
        [InlineData(@".\TestDirectory2\TestFile3.txt", true, 1)]
        [InlineData(@".\TestDirectory2\TestFile3.txt", false, 1)]
        [InlineData(@"*File*", true, 25)]
        public void ResolveDirectoriesAndFiles(string input, bool recurse, int expectedCount)
        {
            string currentWorkingDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = _fixture.RootDirectory;

                // This test provides basic verification that simple patterns
                // matching one or more files in the current directory succeed
                var specifier = new FileSpecifier(input, recurse: recurse);
                specifier.Files.Count.Should().Be(expectedCount);

                specifier = new FileSpecifier(Path.Combine(_fixture.RootDirectory, input), recurse: recurse);
                specifier.Files.Count.Should().Be(expectedCount);

                Environment.CurrentDirectory = currentWorkingDirectory;
                specifier = new FileSpecifier(Path.Combine(_fixture.RootDirectory, input), recurse: recurse);
                specifier.Files.Count.Should().Be(expectedCount);
            }
            finally
            {
                Environment.CurrentDirectory = currentWorkingDirectory;
            }
        }
    }
}

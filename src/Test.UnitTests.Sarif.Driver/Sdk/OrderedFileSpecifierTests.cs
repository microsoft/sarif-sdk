// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class OrderedFileSpecifierTestsFixture : IDisposable
    {
        /// <summary>
        /// Fixture that creates a temporary root directory with a set of subdirectories and files.
        /// The structure includes 5 subdirectories (TestDirectory0...TestDirectory4), each containing 5 text files.
        /// This setup simulates a realistic file system structure for testing file enumeration logic.
        /// </summary>
        public OrderedFileSpecifierTestsFixture()
        {
            TempPath = Path.GetTempPath();
            RootDirectoryRelativePath = Guid.NewGuid().ToString();
            RootDirectory = Path.Combine(TempPath, RootDirectoryRelativePath);

            Directory.CreateDirectory(RootDirectory);

            for (int i = 0; i < 5; i++)
            {
                string childDirectory = Path.Combine(RootDirectory, "OrderedFileSpecifierTestDirectory" + i.ToString());

                Directory.CreateDirectory(childDirectory);

                for (int j = 0; j < 5; j++)
                {
                    string file = Path.Combine(childDirectory, "TestFile" + j.ToString() + ".txt");
                    File.WriteAllText(file, "12345");
                }
            }
        }

        public string RootDirectory { get; set; }

        public string RootDirectoryRelativePath { get; set; }

        public string TempPath { get; set; }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, true);
            }
        }
    }

    [Collection("TestsUsingCurrentDirectory")]
    public class OrderedFileSpecifierTests : IClassFixture<OrderedFileSpecifierTestsFixture>
    {
        private readonly OrderedFileSpecifierTestsFixture _fixture;

        public OrderedFileSpecifierTests(OrderedFileSpecifierTestsFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// This test creates a wide range of high-level command-line argument patterns to verify that the enumerated artifacts 
        /// are generated correctly when using either absolute or relative paths from the OrderedFileSpecifierFixture.
        /// </summary>
        [Fact]
        public void ResolveFileAndDirectoryPaths()
        {
            var testCases = new List<(string, int)>
            {
                // Absolute directory paths.
                (_fixture.RootDirectory, 25),
                (_fixture.RootDirectory + "/", 25),
                (_fixture.RootDirectory + "/*", 25),
                (_fixture.RootDirectory + "/*.txt", 25),
                (_fixture.RootDirectory + "/Test*.txt", 25),
                (Path.Combine(_fixture.RootDirectory, "OrderedFileSpecifierTestDirectory0"), 5),
                (Path.Combine(_fixture.RootDirectory, "OrderedFileSpecifierTestDirectory0/*.txt"), 5),
                (Path.Combine(_fixture.RootDirectory, "OrderedFileSpecifierTestDirectory0/Test*.txt"), 5),

                // Relative directory paths.
                (_fixture.RootDirectoryRelativePath, 25),
                (_fixture.RootDirectoryRelativePath + "/", 25),
                (_fixture.RootDirectoryRelativePath + "/*", 25),
                (_fixture.RootDirectoryRelativePath + "/*.txt", 25),
                (_fixture.RootDirectoryRelativePath + "/Test*.txt", 25),
                ("./" + _fixture.RootDirectoryRelativePath, 25),
                ("./" + _fixture.RootDirectoryRelativePath + "/", 25),
                (Path.Combine(_fixture.RootDirectoryRelativePath, "OrderedFileSpecifierTestDirectory0"), 5),
                (Path.Combine(_fixture.RootDirectoryRelativePath, "OrderedFileSpecifierTestDirectory0/*.txt"), 5),
                (Path.Combine(_fixture.RootDirectoryRelativePath, "OrderedFileSpecifierTestDirectory0/Test*.txt"), 5),

                 // Absolute file paths.
                (_fixture.RootDirectory + "/OrderedFileSpecifierTestDirectory0/" + "TestFile0.txt", 1),
                (_fixture.RootDirectory + "/OrderedFileSpecifierTestDirectory1/" + "TestFile1.txt", 1),

                 // Relative file paths.
                (_fixture.RootDirectoryRelativePath + "/OrderedFileSpecifierTestDirectory0/" + "TestFile0.txt", 1),
                ("./" + _fixture.RootDirectoryRelativePath + "/OrderedFileSpecifierTestDirectory0/" + "TestFile0.txt", 1),

                // Manually tested for symbolic links.
            };

            var testCasesWindowsOnly = new List<(string, int)>
            {
                (_fixture.RootDirectory + "\\", 25),
                (_fixture.RootDirectory + "\\*", 25),
                (_fixture.RootDirectoryRelativePath + "\\", 25),
                (_fixture.RootDirectoryRelativePath + "\\*", 25),
                (".\\" + _fixture.RootDirectoryRelativePath, 25),
                (".\\" + _fixture.RootDirectoryRelativePath + "\\", 25),
                (_fixture.RootDirectory + "\\OrderedFileSpecifierTestDirectory0\\" + "TestFile0.txt", 1),
                (_fixture.RootDirectoryRelativePath + "\\OrderedFileSpecifierTestDirectory0\\" + "TestFile0.txt", 1),
                ("./" + _fixture.RootDirectoryRelativePath + "\\OrderedFileSpecifierTestDirectory0\\" + "TestFile0.txt", 1)
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                testCases.AddRange(testCasesWindowsOnly);
            }

            var sb = new StringBuilder();

            foreach ((string, int) testCase in testCases)
            {
                string currentWorkingDirectory = Environment.CurrentDirectory;
                try
                {
                    // Set current directory to the parent of _fixture.RootDirectory, that is, the assembly directory
                    // to avoid non-deterministic resolution of relative paths when the current working directory changes.
                    Environment.CurrentDirectory = _fixture.TempPath;

                    var specifier = new OrderedFileSpecifier(testCase.Item1, recurse: true);
                    int artifactCount = specifier.Artifacts.Count();

                    if (!Equals(artifactCount, testCase.Item2))
                    {
                        sb.AppendFormat("Incorrect count of artifacts enumerated for specifier {0}. Expected '{1}' but saw '{2}'.", testCase.Item1, testCase.Item2, artifactCount).AppendLine();
                    }
                }
                finally
                {
                    Environment.CurrentDirectory = currentWorkingDirectory;
                }
            }

            sb.Length.Should().Be(0, because: "all artifacts should be enumerated but the following cases failed." + Environment.NewLine + sb.ToString());
        }
    }
}

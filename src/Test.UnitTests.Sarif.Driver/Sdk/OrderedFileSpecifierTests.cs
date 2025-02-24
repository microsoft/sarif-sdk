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
            RootDirectoryRelativePath = Guid.NewGuid().ToString();
            RootDirectory = Path.Combine(GetThisTestAssemblyFilePath(), RootDirectoryRelativePath);

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
        private static string GetThisTestAssemblyFilePath()
        {
            string filePath = typeof(MultithreadedAnalyzeCommandBaseTests).Assembly.Location;
            return Path.GetDirectoryName(filePath);
        }

        public string RootDirectory { get; set; }

        public string RootDirectoryRelativePath { get; set; }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, true);
            }
        }
    }

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
                (Path.Combine(_fixture.RootDirectory, "TestDirectory0"), 5),

                // Relative directory paths.
                (_fixture.RootDirectoryRelativePath, 25),
                (_fixture.RootDirectoryRelativePath + "/", 25),
                (_fixture.RootDirectoryRelativePath + "/*", 25),
                ("./" + _fixture.RootDirectoryRelativePath, 25),
                ("./" + _fixture.RootDirectoryRelativePath + "/", 25),
                (Path.Combine(_fixture.RootDirectoryRelativePath, "TestDirectory0"), 5),

                // Absolute file paths.
                (_fixture.RootDirectory + "/TestDirectory0/" + "TestFile0.txt", 1),
                (_fixture.RootDirectory + "/TestDirectory1/" + "TestFile1.txt", 1),

                // Relative file paths.
                (_fixture.RootDirectoryRelativePath + "/TestDirectory0/" + "TestFile0.txt", 1),
                ("./" + _fixture.RootDirectoryRelativePath + "/TestDirectory0/" + "TestFile0.txt", 1),
            };

            var testCasesWindowsOnly = new List<(string, int)>
            {
                (_fixture.RootDirectory + "\\", 25),
                (_fixture.RootDirectory + "\\*", 25),
                (_fixture.RootDirectoryRelativePath + "\\", 25),
                (_fixture.RootDirectoryRelativePath + "\\*", 25),
                (".\\" + _fixture.RootDirectoryRelativePath, 25),
                (".\\" + _fixture.RootDirectoryRelativePath + "\\", 25),
                (_fixture.RootDirectory + "\\TestDirectory0\\" + "TestFile0.txt", 1),
                (_fixture.RootDirectoryRelativePath + "\\TestDirectory0\\" + "TestFile0.txt", 1),
                ("./" + _fixture.RootDirectoryRelativePath + "\\TestDirectory0\\" + "TestFile0.txt", 1)
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                testCases.AddRange(testCasesWindowsOnly);
            }

            var sb = new StringBuilder();

            foreach ((string, int) testCase in testCases)
            {
                var specifier = new OrderedFileSpecifier(testCase.Item1, recurse: true);
                int artifactCount = specifier.Artifacts.Count();

                if (!Equals(artifactCount, testCase.Item2))
                {
                    sb.AppendFormat("Incorrect count of artifacts enumerated for specifier {0}. Expected '{1}' but saw '{2}'.", testCase.Item1, testCase.Item2, artifactCount).AppendLine();
                }
            }

            sb.Length.Should().Be(0, because: "all artifacts should be enumerated but the following cases failed." + Environment.NewLine + sb.ToString());
        }

        /// <summary>
        /// This test ensures that symbolic links (both for files and directories) correctly resolve
        /// to their target locations and that the expected number of artifacts are enumerated.
        /// </summary>
        [Fact]
        public void ResolveSymbolicLinks()
        {
            string tempPath = Path.GetTempPath();
            var symbolicTestCases = new List<(string, string, int)>
            {
                (Path.Combine(tempPath, "TestDirectorySymbolic0"), "/d", 5),
                (Path.Combine(tempPath, "TestDirectorySymbolic1"), "/j", 5),
                (Path.Combine(tempPath, "TestFileSymbolic.txt"), "", 1)
            };

            string folderTarget = Path.Combine(_fixture.RootDirectory, "TestDirectory1");
            string fileTarget = _fixture.RootDirectory + "/TestDirectory1/" + "TestFile1.txt";

            var sb = new StringBuilder();

            foreach ((string, string, int) testCase in symbolicTestCases)
            {
                try
                {
                    string target = testCase.Item2.Length == 0 ? fileTarget : folderTarget;
                    CreateSymbolicLink(testCase.Item1, target, testCase.Item2);
                    var specifier = new OrderedFileSpecifier(testCase.Item1, recurse: true);
                    int artifactCount = specifier.Artifacts.Count();

                    if (!Equals(artifactCount, testCase.Item3))
                    {
                        sb.AppendFormat("Incorrect count of artifacts enumerated for specifier {0}. Expected '{1}' but saw '{2}'.", testCase.Item1, testCase.Item3, artifactCount).AppendLine();
                    }
                }
                finally
                {
                    if (Directory.Exists(testCase.Item1))
                    {
                        Directory.Delete(testCase.Item1, true);
                    }
                    if (File.Exists(testCase.Item1))
                    {
                        File.Delete(testCase.Item1);
                    }
                }
            }

            sb.Length.Should().Be(0, because: "all artifacts should be enumerated but the following cases failed." + Environment.NewLine + sb.ToString());
        }

        private void CreateSymbolicLink(string linkPath, string targetPath, string targetType)
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c mklink {targetType} \"{linkPath}\" \"{targetPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = System.Diagnostics.Process.Start(processStartInfo))
            {
                process.WaitForExit();
            }
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{

    public class FileSystemTests
    {
        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void FileSystem_IsSymbolicLinkTestCases()
        {
            var mockFileSystem = new FileSystem();

            mockFileSystem.IsSymbolicLink(Environment.SystemDirectory).Should().BeFalse();
            mockFileSystem.IsSymbolicLink(Path.Combine(Environment.SystemDirectory, "notepad.exe")).Should().BeFalse();

            string tempFolder = Path.GetTempPath();
            string folderTarget = Path.Combine(tempFolder, "symbolicLinkFolderTarget");
            string folderSource = Path.Combine(tempFolder, "symbolicLinkFolderSource");
            string fileTarget = Path.Combine(tempFolder, "symbolicLinkFileTarget.txt");
            string fileSource = Path.Combine(tempFolder, "symbolicLinkFileSource.txt");

            CleanupDirectoryOrFile(new string[] { folderTarget, folderSource, fileTarget, fileSource });

            Directory.CreateDirectory(folderTarget);
            File.WriteAllText(fileTarget, "This is the target file.");

            CreateSymbolicLink(folderSource, folderTarget, isDirectory: true);
            CreateSymbolicLink(fileSource, fileTarget, isDirectory: false);

            mockFileSystem.IsSymbolicLink(folderSource).Should().BeTrue();
            mockFileSystem.IsSymbolicLink(folderTarget).Should().BeFalse();
            mockFileSystem.IsSymbolicLink(fileSource).Should().BeTrue();
            mockFileSystem.IsSymbolicLink(fileTarget).Should().BeFalse();

            CleanupDirectoryOrFile(new string[] { folderTarget, folderSource, fileTarget, fileSource });
        }

        private void CreateSymbolicLink(string linkPath, string targetPath, bool isDirectory)
        {
            string targetType = isDirectory ? "/D" : "";
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

        private void CleanupDirectoryOrFile(string[] paths)
        {
            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}

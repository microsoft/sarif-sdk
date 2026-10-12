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

            bool createdFolderSymlink = TryCreateSymbolicLink(folderSource, folderTarget, isDirectory: true);
            bool createdFileSymlink = TryCreateSymbolicLink(fileSource, fileTarget, isDirectory: false);

            if (!createdFolderSymlink || !createdFileSymlink)
            {
                // Symbolic links can be unavailable on some Windows hosts (for example, without
                // developer/admin privileges). Skip the assertions in that case instead of failing
                // the suite for a platform capability issue.
                CleanupDirectoryOrFile(new string[] { folderTarget, folderSource, fileTarget, fileSource });
                return;
            }

            mockFileSystem.IsSymbolicLink(folderSource).Should().BeTrue();
            mockFileSystem.IsSymbolicLink(folderTarget).Should().BeFalse();
            mockFileSystem.IsSymbolicLink(fileSource).Should().BeTrue();
            mockFileSystem.IsSymbolicLink(fileTarget).Should().BeFalse();

            CleanupDirectoryOrFile(new string[] { folderTarget, folderSource, fileTarget, fileSource });
        }

        private bool TryCreateSymbolicLink(string linkPath, string targetPath, bool isDirectory)
        {
            try
            {
                if (isDirectory)
                {
                    Directory.CreateSymbolicLink(linkPath, targetPath);
                }
                else
                {
                    File.CreateSymbolicLink(linkPath, targetPath);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
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

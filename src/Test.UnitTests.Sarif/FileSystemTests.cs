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

        [Fact]
        public void PathGetExtension_ShouldExtractExtension()
        {
            FileSystem.Instance.PathGetExtension("test.txt").Should().Be(".txt");
            FileSystem.Instance.PathGetExtension("C:\\test.abcde").Should().Be(".abcde");
            FileSystem.Instance.PathGetExtension("C:\\some_dir\\test.yft").Should().Be(".yft");
            FileSystem.Instance.PathGetExtension("http://example.com/some.file.ext").Should().Be(".ext");
            FileSystem.Instance.PathGetExtension("some.other.dir/extensionless").Should().Be(string.Empty);
            FileSystem.Instance.PathGetExtension("yet<another>dir\\with|pipes|file.a").Should().Be(".a");
            FileSystem.Instance.PathGetExtension("yet<another>dir\\with|pipes|file.a.").Should().Be(string.Empty);
            FileSystem.Instance.PathGetExtension("yet<another>dir\\with.pipes|file").Should().Be(".pipes|file");
            FileSystem.Instance.PathGetExtension("yet<another>dir\\with.pipes.file").Should().Be(".file");
            FileSystem.Instance.PathGetExtension("yet.another.dir\\with.pipes|file").Should().Be(".pipes|file");
            FileSystem.Instance.PathGetExtension("yet.another.dir\\with|pipes|file").Should().Be(string.Empty);

            const string libTest1 = "call_library_version.txt";
            const string libTest2 = "C:\\test.library_version";
            const string libTest3 = "library_version\\test.file";
            const string libTest4 = "libarary.version\\test.file";
            const string libTest5 = "library.version\\test";

            FileSystem.Instance.PathGetExtension(libTest1).Should().Be(Path.GetExtension(libTest1));
            FileSystem.Instance.PathGetExtension(libTest2).Should().Be(Path.GetExtension(libTest2));
            FileSystem.Instance.PathGetExtension(libTest3).Should().Be(Path.GetExtension(libTest3));
            FileSystem.Instance.PathGetExtension(libTest4).Should().Be(Path.GetExtension(libTest4));
            FileSystem.Instance.PathGetExtension(libTest5).Should().Be(Path.GetExtension(libTest5));
        }
    }
}

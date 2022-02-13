// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Moq;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // WARNING: Because the merge command can accept zero, one, or many input files, it isn't well
    // suited to the FileDiffingUnitTests base class, which assumes you have exactly one input
    // file. We are using this base class because it has the infrastructure to write the "actual"
    // and "expected" output files to disk if a test fails, and to provide you with a "diff"
    // command line that you can use to compare the actual to the expected file contents.
    //
    // You will note that the override of ConstructTestOutputFromInputResource hard-codes the mock
    // file system to return 0 files when GetFilesInDirectory is called. That is fine for the only
    // existing test, which assumes there are no input files. But on the day we write a test that
    // requires one or more input files, we'll have to address this. We will probably have to
    // factor the output file writing and comparison infrastructure out of FileDiffingUnitTests.
    // We might be able to push it into a lower base class.
    public class MergeCommandTests : FileDiffingUnitTests, IClassFixture<MergeCommandTests.MergeCommandTestsFixture>
    {
        public class MergeCommandTestsFixture : DeletesOutputsDirectoryOnClassInitializationFixture { }

        protected override string TestLogResourceNameRoot => "Microsoft.CodeAnalysis.Sarif.Multitool.TestData." + TypeUnderTest;

        public MergeCommandTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1592")]
        public void MergeCommand_WhenThereAreNoInputFiles_ProducesEmptyRunsArray()
        {
            RunTest("NoInputFiles.sarif");
        }

        [Fact]
        public void MergeCommand_WhenThereAreDuplicatedResults_ProducesNonDuplicatedResults()
        {
            RunTest("DuplicatedResults.sarif");
        }

        [Fact]
        public void MergeCommand_WhenPassNoFolderOnlyFile_ProducesCorrectResults()
        {
            RunTest("FileNameOnlyWithoutPath.sarif");
        }

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            string InputFolderPath = Directory.GetCurrentDirectory();
            string targetFileSpecifier =
                !inputResourceName.EndsWith("FileNameOnlyWithoutPath.sarif", StringComparison.Ordinal)
                ? Path.Combine(InputFolderPath, inputResourceName)
                : inputResourceName;
            string outputFileName = Guid.NewGuid().ToString() + SarifConstants.SarifFileExtension;
            string outputFilePath = Path.Combine(OutputFolderPath, outputFileName);

            var mockFileSystem = new Mock<IFileSystem>();
            PrepareFileSystemMock(inputResourceName, InputFolderPath, outputFilePath, mockFileSystem);

            IFileSystem fileSystem = mockFileSystem.Object;

            var options = new MergeOptions
            {
                PrettyPrint = true,
                OutputDirectoryPath = OutputFolderPath,
                TargetFileSpecifiers = new[] { targetFileSpecifier },
                OutputFileName = outputFileName,
                Force = true,
            };

            var mergeCommand = new MergeCommand(fileSystem);

            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(0);

            return File.ReadAllText(outputFilePath);
        }

        private void PrepareFileSystemMock(string inputResourceName, string inputFolderPath, string outputFilePath, Mock<IFileSystem> mockFileSystem)
        {
            if (inputResourceName.EndsWith("NoInputFiles.sarif", StringComparison.Ordinal))
            {
                // We mock the file system to fake out the read operations.
                mockFileSystem.Setup(x => x.FileExists(outputFilePath)).Returns(false);
                mockFileSystem.Setup(x => x.DirectoryExists(inputFolderPath)).Returns(true);
                mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(inputFolderPath, inputResourceName, SearchOption.TopDirectoryOnly)).Returns(new string[0]); // <= The hard-coded return value in question.

                // But we really do want to create the output file, so tell the mock to execute the actual write operations.
                mockFileSystem.Setup(x => x.DirectoryCreateDirectory(OutputFolderPath)).Returns((string path) => { return Directory.CreateDirectory(path); });
                mockFileSystem.Setup(x => x.FileCreate(outputFilePath)).Returns((string path) => { return File.Create(path); });

                return;
            }

            if (inputResourceName.EndsWith("DuplicatedResults.sarif", StringComparison.Ordinal) ||
                inputResourceName.EndsWith("FileNameOnlyWithoutPath.sarif", StringComparison.Ordinal))
            {
                // We mock the file system to fake out the read operations.
                mockFileSystem.Setup(x => x.FileExists(outputFilePath)).Returns(true);
                mockFileSystem.Setup(x => x.DirectoryExists(inputFolderPath)).Returns(true);
                mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(inputFolderPath, inputResourceName, SearchOption.TopDirectoryOnly)).Returns(new string[] { inputResourceName });
                mockFileSystem.Setup(x => x.FileReadAllText(inputResourceName)).Returns(GetResourceText(inputResourceName));

                // But we really do want to create the output file, so tell the mock to execute the actual write operations.
                mockFileSystem.Setup(x => x.DirectoryCreateDirectory(OutputFolderPath)).Returns((string path) => { return Directory.CreateDirectory(path); });
                mockFileSystem.Setup(x => x.FileCreate(outputFilePath)).Returns((string path) => { return File.Create(path); });

                return;
            }
        }
    }
}

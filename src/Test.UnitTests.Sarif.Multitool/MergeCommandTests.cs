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

        protected override string TestLogResourceNameRoot => "Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Multitool.TestData." + TypeUnderTest;

        public MergeCommandTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1592")]
        public void MergeCommand_WhenThereAreNoInputFiles_ProducesEmptyRunsArray()
        {
            RunTest("NoInputFiles.sarif");
        }

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            const string InputFolderPath = @"C:\input";
            string targetFileSpecifier = Path.Combine(InputFolderPath, inputResourceName);

            string outputFileName = Guid.NewGuid().ToString() + SarifConstants.SarifFileExtension;
            string outputFilePath = Path.Combine(OutputFolderPath, outputFileName);

            var mockFileSystem = new Mock<IFileSystem>();

            // We mock the file system to fake out the read operations.
            mockFileSystem.Setup(x => x.FileExists(outputFilePath)).Returns(false);
            mockFileSystem.Setup(x => x.DirectoryExists(InputFolderPath)).Returns(true);
            mockFileSystem.Setup(x => x.GetFilesInDirectory(InputFolderPath, inputResourceName)).Returns(new string[0]); // <= The hard-coded return value in question.

            // But we really do want to create the output file, so tell the mock to execute the actual write operations.
            mockFileSystem.Setup(x => x.CreateDirectory(OutputFolderPath)).Returns((string path) => { return Directory.CreateDirectory(path); });
            mockFileSystem.Setup(x => x.Create(outputFilePath)).Returns((string path) => { return File.Create(path); });

            IFileSystem fileSystem = mockFileSystem.Object;

            var options = new MergeOptions
            {
                PrettyPrint = true,
                OutputFolderPath = OutputFolderPath,
                TargetFileSpecifiers = new[] { targetFileSpecifier },
                OutputFileName = outputFileName
            };

            var mergeCommand = new MergeCommand(fileSystem);

            int returnCode = mergeCommand.Run(options);
            returnCode.Should().Be(0);

            return File.ReadAllText(outputFilePath);
        }
    }
}

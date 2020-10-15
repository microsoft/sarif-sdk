// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class RewriteCommandTests
    {
        [Fact]
        public void RewriteCommand_WhenOutputFormatOptionsAreInconsistent_Fails()
        {
            const string InputFilePath = "AnyFile.sarif";

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(_ => _.FileExists(InputFilePath)).Returns(true);

            var options = new RewriteOptions
            {
                InputFilePath = InputFilePath,
                Inline = true,
                PrettyPrint = true,
                Minify = true
            };

            int returnCode = new RewriteCommand(mockFileSystem.Object).Run(options);

            returnCode.Should().Be(1);

            mockFileSystem.Verify(_ => _.FileExists(InputFilePath), Times.Once);
            mockFileSystem.VerifyNoOtherCalls();
        }
    }
}

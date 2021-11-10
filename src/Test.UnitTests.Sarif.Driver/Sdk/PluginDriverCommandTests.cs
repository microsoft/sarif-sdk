// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Driver;

using Moq;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Driver.Sdk
{
    public class PluginDriverCommandTests
    {
        [Fact]
        public void PluginDriverCommand_ValidateInvocationPropertiesToLog_ShouldValidateParameters()
        {
            var sb = new StringBuilder();
            var mockContext = new Mock<IAnalysisContext>();
            var mockLogger = new Mock<IAnalysisLogger>();

            mockContext.SetupGet(context => context.Logger).Returns(mockLogger.Object);
            bool current = PluginDriverCommand<string>.ValidateInvocationPropertiesToLog(mockContext.Object,
                                                                                         null);
            current.Should().BeTrue();

            current = PluginDriverCommand<string>.ValidateInvocationPropertiesToLog(mockContext.Object,
                                                                                    Array.Empty<string>());
            current.Should().BeTrue();

            current = PluginDriverCommand<string>.ValidateInvocationPropertiesToLog(mockContext.Object,
                                                                                    new List<string> { "Account" });
            current.Should().BeTrue();

            current = PluginDriverCommand<string>.ValidateInvocationPropertiesToLog(mockContext.Object,
                                                                                    new List<string> { "test" });
            current.Should().BeFalse();
        }

        [Fact]
        public async Task PluginDriverCommand_ShouldThrowExitApplicationExceptionIfUnhandledExceptionOccurs()
        {
            string postUri = "https://github.com/microsoft/sarif-sdk";
            string outputFilePath = string.Empty;
            var mockFileSystem = new Mock<IFileSystem>();

            Exception exception = await Record.ExceptionAsync(async () =>
            {
                mockFileSystem
                    .Setup(f => f.FileExists(It.IsAny<string>()))
                    .Throws(new NullReferenceException());

                await PluginDriverCommand<AnalyzeOptionsBase>.PostLogFile(postUri,
                                                                          outputFilePath,
                                                                          mockFileSystem.Object,
                                                                          httpClient: null);
            });
            exception.Should().BeOfType(typeof(ExitApplicationException<ExitReason>));
        }

    }
}

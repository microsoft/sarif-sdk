// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class PluginDriverCommandTests
    {
        [Fact]
        public void PluginDriverCommand_ValidateInvocationPropertiesToLog_ShouldReturnTrueIfPropertiesAreNullOrEmpty()
        {
            var mockContext = new Mock<IAnalysisContext>();
            var mockLogger = new Mock<IAnalysisLogger>();

            mockContext.SetupGet(context => context.Logger).Returns(mockLogger.Object);
            bool current = PluginDriverCommand<string>.ValidateInvocationPropertiesToLog(mockContext.Object);
            current.Should().BeTrue();


            mockContext.Object.InvocationPropertiesToLog = new StringSet(Array.Empty<string>());

            current = PluginDriverCommand<string>.ValidateInvocationPropertiesToLog(mockContext.Object);
            current.Should().BeTrue();
        }

        [Fact]
        public void PluginDriverCommand_ValidateInvocationPropertiesToLog_ShouldReturnTrueIfPropertyIsValid()
        {
            var mockContext = new Mock<IAnalysisContext>();
            var mockLogger = new Mock<IAnalysisLogger>();

            mockContext.SetupGet(context => context.Logger).Returns(mockLogger.Object);
            mockContext.Object.InvocationPropertiesToLog = new StringSet(new[] { "Account" });

            bool current = PluginDriverCommand<string>.ValidateInvocationPropertiesToLog(mockContext.Object);
            current.Should().BeTrue();
        }

        [Fact]
        public void PluginDriverCommand_ValidateInvocationPropertiesToLog_ShouldReturnFalseIfPropertyIsInvalid()
        {
            var mockLogger = new Mock<IAnalysisLogger>();

            var context = new TestAnalysisContext
            {
                Logger = mockLogger.Object,
                InvocationPropertiesToLog = new StringSet(new[] { "invalidname" })
            };

            bool current = PluginDriverCommand<string>.ValidateInvocationPropertiesToLog(context);
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

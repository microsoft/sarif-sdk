// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;

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
        public void PluginDriverCommand_NoExceptionIfLogFileDoesNotExist()
        {
            string postUri = "https://github.com/microsoft/sarif-sdk";
            string outputFilePath = $"{Guid.NewGuid()}.txt";
            using var httpClient = new HttpClientWrapper();

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem
                .Setup(f => f.FileExists(It.IsAny<string>()))
                .Returns(false);

            var context = new TestAnalysisContext
            {
                PostUri = postUri,
                OutputFilePath = outputFilePath,
                FileSystem = mockFileSystem.Object
            };

            PluginDriverCommand<AnalyzeOptionsBase>.SarifPost(context, httpClient);
        }

        [Fact]
        public void PluginDriverCommand_NoExceptionIfOutputPathIsNull()
        {
            string postUri = "https://github.com/microsoft/sarif-sdk";
            string outputFilePath = $"{Guid.NewGuid()}.txt";
            using var httpClient = new HttpClientWrapper();

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem
                .Setup(f => f.FileExists(It.IsAny<string>()))
                .Returns(true);

            var context = new TestAnalysisContext
            {
                PostUri = postUri,
                OutputFilePath = null,
                FileSystem = mockFileSystem.Object
            };

            PluginDriverCommand<AnalyzeOptionsBase>.SarifPost(context, httpClient);
        }

        [Fact]
        public void PluginDriverCommand_NoExceptionIfPostUriNotPresent()
        {
            string outputFilePath = $"{Guid.NewGuid()}.txt";
            using var httpClient = new HttpClientWrapper();

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem
                .Setup(f => f.FileExists(It.IsAny<string>()))
                .Returns(true);

            var context = new TestAnalysisContext
            {
                PostUri = null,
                OutputFilePath = outputFilePath,
                FileSystem = mockFileSystem.Object
            };

            PluginDriverCommand<AnalyzeOptionsBase>.SarifPost(context, httpClient);
        }

        [Fact]
        public void PluginDriverCommand_ThrowsExitApplicationExceptionOnUnhandledException()
        {
            string postUri = "https://github.com/microsoft/sarif-sdk";
            string outputFilePath = $"{Guid.NewGuid()}.txt";

            var mockHttpClient = new Mock<HttpClientWrapper>();

            mockHttpClient
                .Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Throws(new InvalidOperationException());

            var context = new TestAnalysisContext
            {
                PostUri = postUri,
                OutputFilePath = outputFilePath,
            };

            Exception exception = Record.Exception(() =>
                    PluginDriverCommand<AnalyzeOptionsBase>.SarifPost(context,
                                                                      mockHttpClient.Object));

            exception.Should().BeOfType(typeof(ArgumentException));
        }
    }
}

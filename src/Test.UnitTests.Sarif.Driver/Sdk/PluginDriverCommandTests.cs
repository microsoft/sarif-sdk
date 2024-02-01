// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

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
        public void PluginDriverCommand_PostFileTests()
        {
            using var assertionScope = new AssertionScope();

            string postUri = "https://github.com/microsoft/sarif-sdk";
            string outputFilePath = $"{Guid.NewGuid()}.txt";

            var mockHttpClient = new Mock<HttpClientWrapper>();

            mockHttpClient
                .Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Throws(new InvalidOperationException());

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem
                .Setup(f => f.FileExists(It.IsAny<string>()))
                .Returns(true);

            var stream = (MemoryStream)CreateSarifLogStreamWithToolExecutionNotifications(FailureLevel.Error);
            mockFileSystem
                .Setup(f => f.FileReadAllBytes(It.IsAny<string>()))
                .Returns(stream.ToArray());

            var context = new TestAnalysisContext
            {
                PostUri = postUri,
                OutputFilePath = outputFilePath,
                FileSystem = mockFileSystem.Object
            };

            var logger = new TestMessageLogger();
            context.Logger = logger;

            // Case when there are unhandled exceptions.
            Exception exception = Record.Exception(() =>
                    PluginDriverCommand<AnalyzeOptionsBase>.SarifPost(context,
                                                                      mockHttpClient.Object));
            exception.Should().BeOfType(typeof(ExitApplicationException<ExitReason>));
            ((ExitApplicationException<ExitReason>)exception).ExitReason.Should().Be(ExitReason.ExceptionPostingLogFile);
            context.RuntimeErrors.Should().Be(RuntimeConditions.ExceptionPostingLogFile);
            context.RuntimeExceptions.Should().Contain(ex => ex is InvalidOperationException);

            // Case when there are no unhandled exceptions, but the server returns status code other than OK.
            context.RuntimeErrors = RuntimeConditions.None;
            context.RuntimeExceptions = null;
            mockHttpClient
                .Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(HttpMockHelper.CreateInternalServerErrorResponse()));
            exception = Record.Exception(() =>
                    PluginDriverCommand<AnalyzeOptionsBase>.SarifPost(context,
                                                                      mockHttpClient.Object));
            exception.Should().BeNull();
            context.RuntimeErrors.Should().Be(RuntimeConditions.ExceptionPostingLogFile);
            context.RuntimeExceptions.Should().BeNull();

            // Case when there are no unhandled exceptions, and the server returns status code OK.
            context.RuntimeErrors = RuntimeConditions.None;
            context.RuntimeExceptions = null;
            mockHttpClient
                .Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(HttpMockHelper.CreateOKResponse()));
            exception = Record.Exception(() =>
                    PluginDriverCommand<AnalyzeOptionsBase>.SarifPost(context,
                                                                      mockHttpClient.Object));
            exception.Should().BeNull();
            context.RuntimeErrors.Should().Be(RuntimeConditions.None);
            context.RuntimeExceptions.Should().BeNull();
        }

        private Stream CreateSarifLogStreamWithToolExecutionNotifications(FailureLevel level)
        {
            var memoryStream = new MemoryStream();
            var sarifLog = new SarifLog();
            var run = new Run();
            run.Invocations = new Invocation[] { new Invocation()
            { ToolExecutionNotifications = new Notification[] { new Notification() { Level = level } } } };
            sarifLog.Runs = new Run[] { run };
            sarifLog.Save(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}

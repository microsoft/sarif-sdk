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
            var testCases = new[]
            {
                new
                {
                    Title = "Null 'properties' should return true",
                    Properties = (IEnumerable<string>)null,
                    Expected = true
                },
                new
                {
                    Title = "Empty 'properties' should return true",
                    Properties = (IEnumerable<string>)Array.Empty<string>(),
                    Expected = true
                },
                new
                {
                    Title = "'Properties' with valid value should return true",
                    Properties = (IEnumerable<string>)new List<string> { "Account" },
                    Expected = true
                },
                new
                {
                    Title = "'Properties' with invalid value should return false",
                    Properties = (IEnumerable<string>)new List<string> { "test" },
                    Expected = false
                },
            };

            var sb = new StringBuilder();
            var mockContext = new Mock<IAnalysisContext>();
            var mockLogger = new Mock<IAnalysisLogger>();

            mockContext.SetupGet(context => context.Logger).Returns(mockLogger.Object);

            foreach (var testCase in testCases)
            {
                bool current = PluginDriverCommand<string>.ValidateInvocationPropertiesToLog(mockContext.Object,
                                                                                             testCase.Properties);

                if (current != testCase.Expected)
                {
                    sb.AppendLine($"The test '{testCase.Title}' was expecting '{testCase.Expected}' but found '{current}'.");
                }
            }

            sb.Length.Should().Be(0, sb.ToString());
        }

        [Fact]
        public async Task PluginDriverCommand_ProcessPostLogFile()
        {
            string postUri = string.Empty;
            string outputFilePath = string.Empty;
            var mockFileSystem = new Mock<IFileSystem>();

            // Nothing should happen, since driverOptions is not an 'AnalyzeOptionsBase' object.
            await PluginDriverCommand<string>.PostLogFile(postUri,
                                                          outputFilePath,
                                                          mockFileSystem.Object,
                                                          httpClient: null);

            // Generating some random exception and verifying if we are throwing
            // a new ExitApplicationException.
            postUri = "https://github.com/microsoft/sarif-sdk";
            Exception exception = await Record.ExceptionAsync(async () =>
            {
                await PluginDriverCommand<AnalyzeOptionsBase>.PostLogFile(postUri,
                                                                          outputFilePath,
                                                                          fileSystem: null,
                                                                          httpClient: null);
            });
            exception.Should().BeOfType(typeof(ExitApplicationException<ExitReason>));

            var sarifLog = new SarifLog();
            var httpMock = new HttpMockHelper();
            var memoryStream = new MemoryStream();
            sarifLog.Save(memoryStream);
            mockFileSystem
                .Setup(f => f.FileOpenRead(It.IsAny<string>()))
                .Returns(memoryStream);

            // If not OK, we should expect a new ExitApplicationException
            httpMock.Mock(
                new HttpRequestMessage(HttpMethod.Post, postUri) { Content = new StreamContent(memoryStream) },
                HttpMockHelper.BadRequestResponse);

            exception = await Record.ExceptionAsync(async () =>
            {
                await PluginDriverCommand<AnalyzeOptionsBase>.PostLogFile(postUri,
                                                                          outputFilePath,
                                                                          mockFileSystem.Object,
                                                                          new HttpClient(httpMock));
            });
            exception.Should().BeOfType(typeof(ExitApplicationException<ExitReason>));
            httpMock.Clear();

            // Valid request and valid response.
            outputFilePath = "SomeFile.txt";
            httpMock.Mock(
                new HttpRequestMessage(HttpMethod.Post, postUri) { Content = new StreamContent(memoryStream) },
                HttpMockHelper.OKResponse);
            await PluginDriverCommand<AnalyzeOptionsBase>.PostLogFile(postUri,
                                                                      outputFilePath,
                                                                      mockFileSystem.Object,
                                                                      new HttpClient(httpMock));
            httpMock.Clear();
        }

        [Fact]
        public async Task PluginDriverCommand_ProcessPostLogStream()
        {
            string postUri = string.Empty;
            MemoryStream memoryStream = null;
            var mockFileSystem = new Mock<IFileSystem>();

            // Nothing should happen, since driverOptions is not an 'AnalyzeOptionsBase' object.
            await PluginDriverCommand<string>.PostLogStream(postUri,
                                                            memoryStream,
                                                            httpClient: null);

            // Generating some random exception and verifying if we are throwing
            // a new ExitApplicationException.
            memoryStream = new MemoryStream();
            postUri = "https://github.com/microsoft/sarif-sdk";
            Exception exception = await Record.ExceptionAsync(async () =>
            {
                await PluginDriverCommand<AnalyzeOptionsBase>.PostLogStream(postUri,
                                                                            memoryStream,
                                                                            httpClient: null);
            });
            exception.Should().BeOfType(typeof(ExitApplicationException<ExitReason>));

            var sarifLog = new SarifLog();
            memoryStream = new MemoryStream();
            var httpMock = new HttpMockHelper();
            sarifLog.Save(memoryStream);
            mockFileSystem
                .Setup(f => f.FileOpenRead(It.IsAny<string>()))
                .Returns(memoryStream);

            // If not OK, we should expect a new ExitApplicationException
            httpMock.Mock(
                new HttpRequestMessage(HttpMethod.Post, postUri) { Content = new StreamContent(memoryStream) },
                HttpMockHelper.BadRequestResponse);

            exception = await Record.ExceptionAsync(async () =>
            {
                await PluginDriverCommand<AnalyzeOptionsBase>.PostLogStream(postUri,
                                                                            memoryStream,
                                                                            new HttpClient(httpMock));
            });
            exception.Should().BeOfType(typeof(ExitApplicationException<ExitReason>));
            httpMock.Clear();

            // Valid request and valid response.
            memoryStream = new MemoryStream();
            httpMock.Mock(
                new HttpRequestMessage(HttpMethod.Post, postUri) { Content = new StreamContent(memoryStream) },
                HttpMockHelper.OKResponse);
            await PluginDriverCommand<AnalyzeOptionsBase>.PostLogStream(postUri,
                                                                        memoryStream,
                                                                        new HttpClient(httpMock));
            httpMock.Clear();
        }
    }
}

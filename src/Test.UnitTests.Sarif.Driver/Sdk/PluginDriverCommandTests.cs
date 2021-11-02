// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

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
        public void PluginDriverCommand_ProcessPostUri()
        {
            var mockFileSystem = new Mock<IFileSystem>();

            // Nothing should happen, since driverOptions is not an 'AnalyzeOptionsBase' object.
            PluginDriverCommand<string>.ProcessPostUri(string.Empty,
                                                       mockFileSystem.Object,
                                                       httpClient: null);

            // Nothing should happen, since driverOptions does not have a valid 'PostUri'.
            var options = new TestAnalyzeOptions();
            PluginDriverCommand<AnalyzeOptionsBase>.ProcessPostUri(options,
                                                                   mockFileSystem.Object,
                                                                   httpClient: null);

            // Generating some random exception and verifying if we are throwing
            // a new ExitApplicationException.
            options.PostUri = "https://github.com/microsoft/sarif-sdk";
            Exception exception = Record.Exception(() =>
            {
                PluginDriverCommand<AnalyzeOptionsBase>.ProcessPostUri(options,
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
                new HttpRequestMessage(HttpMethod.Post, options.PostUri) { Content = new StreamContent(memoryStream) },
                HttpMockHelper.BadRequestResponse);

            exception = Record.Exception(() =>
            {
                PluginDriverCommand<AnalyzeOptionsBase>.ProcessPostUri(options,
                                                                   mockFileSystem.Object,
                                                                   new HttpClient(httpMock));
            });
            exception.Should().BeOfType(typeof(ExitApplicationException<ExitReason>));
            httpMock.Clear();

            // Valid request and valid response.
            httpMock.Mock(
                new HttpRequestMessage(HttpMethod.Post, options.PostUri) { Content = new StreamContent(memoryStream) },
                HttpMockHelper.OKResponse);
            PluginDriverCommand<AnalyzeOptionsBase>.ProcessPostUri(options,
                                                                   mockFileSystem.Object,
                                                                   new HttpClient(httpMock));
            httpMock.Clear();
        }
    }
}

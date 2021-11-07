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

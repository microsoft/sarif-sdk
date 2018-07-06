// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class DriverExtensionMethodsTests
    {
        [Fact]
        public void ConvertAnalyzeOptionsToLoggingOptions()
        {
            LoggingOptions loggingOptions;
            var analyzeOptions = new TestAnalyzeOptions()
            {
                ComputeFileHashes = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.ComputeFileHashes);

            analyzeOptions = new TestAnalyzeOptions()
            {
                LogEnvironment = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.PersistEnvironment);

            analyzeOptions = new TestAnalyzeOptions()
            {
                PersistTextFileContents = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.PersistTextFileContents);

            analyzeOptions = new TestAnalyzeOptions()
            {
                Verbose = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.Verbose);
        }
    }
}

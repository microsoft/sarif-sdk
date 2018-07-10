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

            TestAnalyzeOptions analyzeOptions = new TestAnalyzeOptions()
            {
                Verbose = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.Verbose);

             analyzeOptions = new TestAnalyzeOptions()
            {
                PrettyPrint = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.PrettyPrint);

            analyzeOptions = new TestAnalyzeOptions()
            {
                Force = true
            };

            loggingOptions = analyzeOptions.ConvertToLoggingOptions();
            loggingOptions.Should().Be(LoggingOptions.OverwriteExistingOutputFile);
        }
    }
}

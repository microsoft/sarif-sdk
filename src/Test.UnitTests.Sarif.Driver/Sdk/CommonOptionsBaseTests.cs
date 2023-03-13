// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Test.UnitTests.Sarif.Driver.Sdk
{
    public class CommonOptionsBaseTests
    {
        [Fact]
        public void CommonOptionsBase_ProducesExpectedOutputFileOptions()
        {
            FilePersistenceOptions loggingOptions;

            // Any case in which PrettyPrint is not specified should default to PrettyPrint.
            TestAnalyzeOptions analyzeOptions = new TestAnalyzeOptions()
            {
                Quiet = true
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(FilePersistenceOptions.PrettyPrint);
            analyzeOptions.ForceOverwrite.Should().Be(false);
            analyzeOptions.PrettyPrint.Should().Be(true);
            analyzeOptions.Optimize.Should().Be(false);
            analyzeOptions.Minify.Should().Be(false);
            analyzeOptions.Inline.Should().Be(false);

            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Minify }
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(FilePersistenceOptions.Minify);
            analyzeOptions.ForceOverwrite.Should().Be(false);
            analyzeOptions.PrettyPrint.Should().Be(false);
            analyzeOptions.Optimize.Should().Be(false);
            analyzeOptions.Inline.Should().Be(false);
            analyzeOptions.Minify.Should().Be(true);

            // If both Minify and PrettyPrint are set, we prefer PrettyPrint, with no exception raised.
            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Minify, FilePersistenceOptions.PrettyPrint },
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(FilePersistenceOptions.PrettyPrint);
            analyzeOptions.ForceOverwrite.Should().Be(false);
            analyzeOptions.PrettyPrint.Should().Be(true);
            analyzeOptions.Optimize.Should().Be(false);
            analyzeOptions.Minify.Should().Be(false);
            analyzeOptions.Inline.Should().Be(false);

            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(
                FilePersistenceOptions.ForceOverwrite |
                FilePersistenceOptions.PrettyPrint);
            analyzeOptions.ForceOverwrite.Should().Be(true);
            analyzeOptions.PrettyPrint.Should().Be(true);
            analyzeOptions.Optimize.Should().Be(false);
            analyzeOptions.Minify.Should().Be(false);
            analyzeOptions.Inline.Should().Be(false);

            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Optimize },
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(
                FilePersistenceOptions.Optimize |
                FilePersistenceOptions.PrettyPrint);
            analyzeOptions.ForceOverwrite.Should().Be(false);
            analyzeOptions.PrettyPrint.Should().Be(true);
            analyzeOptions.Optimize.Should().Be(true);
            analyzeOptions.Minify.Should().Be(false);
            analyzeOptions.Inline.Should().Be(false);

            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Inline, FilePersistenceOptions.Minify },
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(
                FilePersistenceOptions.Inline |
                FilePersistenceOptions.Minify);
            analyzeOptions.ForceOverwrite.Should().Be(false);
            analyzeOptions.PrettyPrint.Should().Be(false);
            analyzeOptions.Optimize.Should().Be(false);
            analyzeOptions.Minify.Should().Be(true);
            analyzeOptions.Inline.Should().Be(true);
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Driver;

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

            var command = new TestMultithreadedAnalyzeCommand();
            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();

            // We need logging options to evaluate as empty when not set,
            // so that we always know when a user has or has not explicitly
            // set this option. This allows is to be overridden in disk
            // configuration.
            loggingOptions.Should().Be(FilePersistenceOptions.None);

            TestAnalysisContext context = null;
            command.InitializeContextFromOptions(analyzeOptions, ref context);

            context.ForceOverwrite.Should().Be(false);
            context.PrettyPrint.Should().Be(true);
            context.Optimize.Should().Be(false);
            context.Minify.Should().Be(false);
            context.Inline.Should().Be(false);

            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Minify }
            };
            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(FilePersistenceOptions.Minify);

            context = null;
            command.InitializeContextFromOptions(analyzeOptions, ref context);

            context.ForceOverwrite.Should().Be(false);
            context.PrettyPrint.Should().Be(false);
            context.Optimize.Should().Be(false);
            context.Inline.Should().Be(false);
            context.Minify.Should().Be(true);

            // If both Minify and PrettyPrint are set, we prefer PrettyPrint, with no exception raised.
            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Minify, FilePersistenceOptions.PrettyPrint },
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(FilePersistenceOptions.PrettyPrint);

            context = null;
            command.InitializeContextFromOptions(analyzeOptions, ref context);

            context.ForceOverwrite.Should().Be(false);
            context.PrettyPrint.Should().Be(true);
            context.Optimize.Should().Be(false);
            context.Minify.Should().Be(false);
            context.Inline.Should().Be(false);

            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(
                FilePersistenceOptions.ForceOverwrite |
                FilePersistenceOptions.PrettyPrint);

            context = null;
            command.InitializeContextFromOptions(analyzeOptions, ref context);

            context.ForceOverwrite.Should().Be(true);
            context.PrettyPrint.Should().Be(true);
            context.Optimize.Should().Be(false);
            context.Minify.Should().Be(false);
            context.Inline.Should().Be(false);

            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Optimize },
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(
                FilePersistenceOptions.Optimize |
                FilePersistenceOptions.PrettyPrint);

            context = null;
            command.InitializeContextFromOptions(analyzeOptions, ref context);

            context.ForceOverwrite.Should().Be(false);
            context.PrettyPrint.Should().Be(true);
            context.Optimize.Should().Be(true);
            context.Minify.Should().Be(false);
            context.Inline.Should().Be(false);

            analyzeOptions = new TestAnalyzeOptions()
            {
                OutputFileOptions = new[] { FilePersistenceOptions.Inline, FilePersistenceOptions.Minify },
            };

            loggingOptions = analyzeOptions.OutputFileOptions.ToFlags();
            loggingOptions.Should().Be(
                FilePersistenceOptions.Inline |
                FilePersistenceOptions.Minify);

            context = null;
            command.InitializeContextFromOptions(analyzeOptions, ref context);

            context.ForceOverwrite.Should().Be(false);
            context.PrettyPrint.Should().Be(false);
            context.Optimize.Should().Be(false);
            context.Minify.Should().Be(true);
            context.Inline.Should().Be(true);
        }
    }
}

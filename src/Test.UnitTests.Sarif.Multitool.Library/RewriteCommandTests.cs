// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Multitool.Library
{
    public class RewriteCommandTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(RewriteCommandTests));

        [Fact]
        public void Run_WhenOutputFormatOptionsAreInconsistent_Fails()
        {
            string sampleFilePath = "minimal.sarif";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText($"RewriteCommand.{sampleFilePath}"));

            var options = new RewriteOptions
            {
                InputFilePath = sampleFilePath,
                Inline = true,
                PrettyPrint = true,
                Minify = true
            };

            int returnCode = new RewriteCommand().Run(options);

            returnCode.Should().Be(1);
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class RewriteCommandTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(RewriteCommandTests));

        [Fact]
        public void RewriteCommand_WhenOutputFormatOptionsAreInconsistent_Fails()
        {
            const string SampleFilePath = "minimal.sarif";
            File.WriteAllText(SampleFilePath, Extractor.GetResourceText($"RewriteCommand.{SampleFilePath}"));

            var options = new RewriteOptions
            {
                InputFilePath = SampleFilePath,
                Inline = true,
                PrettyPrint = true,
                Minify = true
            };

            int returnCode = new RewriteCommand().Run(options);

            returnCode.Should().Be(1);
        }
    }
}

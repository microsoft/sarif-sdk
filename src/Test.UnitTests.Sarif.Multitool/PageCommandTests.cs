// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;
using Newtonsoft.Json;
using Xunit;

namespace Sarif.Multitool.UnitTests
{
    public class PageCommandTests
    {
        private static ResourceExtractor Extractor = new ResourceExtractor(typeof(PageCommandTests));

        [Fact]
        public void PageCommand_Basics()
        {
            string sampleFilePath = "elfie-arriba.sarif";
            string pagedSamplePath = "elfie-arriba.paged.sarif";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText(@"PageCommand.elfie-arriba.sarif"));

            //RunAndCompare(new PageOptions() { Index = 1, Count = 2, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 0, Count = 5, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 3, Count = 0, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 3, Count = 2, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });

            string minifiedPath = "elfie-arriba.min.sarif";
            IFileSystem fileSystem = new FileSystem();
            SarifLog log = PageCommand.ReadSarifFile<SarifLog>(fileSystem, sampleFilePath);
            PageCommand.WriteSarifFile(fileSystem, log, minifiedPath, Formatting.None);

            RunAndCompare(new PageOptions() { Index = 1, Count = 2, InputFilePath = minifiedPath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 0, Count = 5, InputFilePath = minifiedPath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 3, Count = 0, InputFilePath = minifiedPath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 3, Count = 2, InputFilePath = minifiedPath, OutputFilePath = pagedSamplePath });
        }

        private static void RunAndCompare(PageOptions options)
        {
            IFileSystem fileSystem = new FileSystem();
            string actualPath = Path.ChangeExtension(options.OutputFilePath, "act.json");
            string expectPath = Path.ChangeExtension(options.OutputFilePath, "exp.json");

            // Run the normal Page command
            PageCommand command = new PageCommand(fileSystem, 0.1);
            command.Run(options);

            // Rewrite indented
            SarifLog actual = PageCommand.ReadSarifFile<SarifLog>(fileSystem, options.OutputFilePath);
            PageCommand.WriteSarifFile(fileSystem, actual, actualPath, Formatting.Indented);

            // Run "Page via OM"
            SarifLog expected = command.PageViaOm(options);
            PageCommand.WriteSarifFile(fileSystem, expected, expectPath, Formatting.Indented);

            string actualText = File.ReadAllText(actualPath);
            string expectedText = File.ReadAllText(expectPath);
            string diffCommand = $"windiff \"{Path.GetFullPath(expectPath)}\" \"{Path.GetFullPath(actualPath)}\"";

            Assert.True(actualText == expectedText, $"Sarif Page result ({options.Index}, {options.Count}) didn't match.\r\nSee: {diffCommand}");
            //Assert.Equal(expectedText, actualText);
        }
    }
}

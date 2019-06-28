// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Multitool;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Multitool
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

            // Normal file, valid subsets
            RunAndCompare(new PageOptions() { Index = 1, Count = 2, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 0, Count = 5, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 3, Count = 0, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 3, Count = 2, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });

            string minifiedPath = "elfie-arriba.min.sarif";
            IFileSystem fileSystem = new FileSystem();
            SarifLog log = PageCommand.ReadSarifFile<SarifLog>(fileSystem, sampleFilePath);
            PageCommand.WriteSarifFile(fileSystem, log, minifiedPath, Formatting.None);

            // Minified file, valid subsets
            RunAndCompare(new PageOptions() { Index = 1, Count = 2, InputFilePath = minifiedPath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 0, Count = 5, InputFilePath = minifiedPath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 3, Count = 0, InputFilePath = minifiedPath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { Index = 3, Count = 2, InputFilePath = minifiedPath, OutputFilePath = pagedSamplePath });
        }

        [Fact]
        public void PageCommand_MapTooSmallFallback()
        {
            string sampleFilePath = "elfie-arriba.sarif";
            string pagedSamplePath = "elfie-arriba.paged.sarif";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText(@"PageCommand.elfie-arriba.sarif"));

            // File too small for map / results / ArrayStarts
            RunAndCompare(new PageOptions() { TargetMapSizeRatio = 0.009, Index = 1, Count = 2, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { TargetMapSizeRatio = 0.014, Index = 1, Count = 2, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { TargetMapSizeRatio = 0.016, Index = 1, Count = 2, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
            RunAndCompare(new PageOptions() { TargetMapSizeRatio = 0.050, Index = 1, Count = 2, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath });
        }

        [Fact]
        public void PageCommand_Validation()
        {
            string sampleFilePath = "elfie-arriba.sarif";
            string pagedSamplePath = "elfie-arriba.paged.sarif";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText(@"PageCommand.elfie-arriba.sarif"));

            // Index < 0
            Assert.Throws<ArgumentOutOfRangeException>(() => RunAndCompare(new PageOptions() { Index = -1, Count = 1, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath }));

            // Index >= Count
            Assert.Throws<ArgumentOutOfRangeException>(() => RunAndCompare(new PageOptions() { Index = 5, Count = 1, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath }));

            // Index + Count > Count
            Assert.Throws<ArgumentOutOfRangeException>(() => RunAndCompare(new PageOptions() { Index = 0, Count = 6, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath }));

            // RunIndex < 0
            Assert.Throws<ArgumentOutOfRangeException>(() => RunAndCompare(new PageOptions() { RunIndex = -1, Index = 1, Count = 1, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath }));

            // RunIndex >= RunCount
            Assert.Throws<ArgumentOutOfRangeException>(() => RunAndCompare(new PageOptions() { RunIndex = 1, Index = 1, Count = 1, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath }));

            // No input file
            Assert.Throws<FileNotFoundException>(() => RunAndCompare(new PageOptions() { InputFilePath = "NotExists.sarif", OutputFilePath = "NotExists.out.sarif" }));
        }

        private static void RunAndCompare(PageOptions options)
        {
            IFileSystem fileSystem = new FileSystem();
            string actualPath = Path.ChangeExtension(options.OutputFilePath, "act.json");
            string expectPath = Path.ChangeExtension(options.OutputFilePath, "exp.json");

            File.Delete(Path.ChangeExtension(options.InputFilePath, ".map.json"));
            File.Delete(options.OutputFilePath);

            // Reset default target size ratio so that a map is built for file
            if (options.TargetMapSizeRatio == 0.01)
            {
                options.TargetMapSizeRatio = 0.10;
            }

            // Run the normal Page command
            PageCommand command = new PageCommand(fileSystem);
            command.RunWithoutCatch(options);

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

        //[Fact]
        //public void PageCommand_Huge()
        //{
        //    HugeFileCompare(@"<huge file path>", 1000000, 100);
        //}

        // Use for manual verification of files over 2GB and other real life examples which can't be checked in.
        private static void HugeFileCompare(string sourceFilePath, int index, int count)
        {
            string expectPath = Path.ChangeExtension(sourceFilePath, ".Paged.Expect.sarif");
            string actualPath = Path.ChangeExtension(sourceFilePath, ".Paged.Actual.sarif");
            string actualUnindentedPath = Path.ChangeExtension(sourceFilePath, ".Paged.Actual.Unformatted.sarif");

            // Page with the Command
            PageCommand command = new PageCommand();
            command.RunWithoutCatch(new PageOptions() { InputFilePath = sourceFilePath, OutputFilePath = actualUnindentedPath, Index = index, Count = count });

            // Indent the Paged output
            Indent(actualUnindentedPath, actualPath);

            // Page with JsonTextReader/Writer
            PageManual(sourceFilePath, expectPath, index, count);

            // Compare files
            string actualText = File.ReadAllText(actualPath);
            string expectedText = File.ReadAllText(expectPath);
            string diffCommand = $"windiff \"{Path.GetFullPath(expectPath)}\" \"{Path.GetFullPath(actualPath)}\"";

            Assert.True(actualText == expectedText, $"Sarif Page result ({index}, {count}) didn't match.\r\nSee: {diffCommand}");
            //Assert.Equal(expectedText, actualText);
        }

        private static void Indent(string sourceFilePath, string outputPath)
        {
            using (JsonTextReader reader = new JsonTextReader(new StreamReader(sourceFilePath)))
            using (JsonTextWriter writer = new JsonTextWriter(File.CreateText(outputPath)))
            {
                writer.Formatting = Newtonsoft.Json.Formatting.Indented;
                reader.Read();
                writer.WriteToken(reader, true);
            }
        }

        private static void PageManual(string sourceFilePath, string outputPath, int index, int count)
        {
            using (JsonTextReader reader = new JsonTextReader(new StreamReader(sourceFilePath)))
            using (JsonTextWriter writer = new JsonTextWriter(File.CreateText(outputPath)))
            {
                writer.Formatting = Newtonsoft.Json.Formatting.Indented;

                // Copy file up to "results" array
                while (reader.Read())
                {
                    writer.WriteToken(reader, false);
                    if (reader.TokenType == JsonToken.PropertyName && "results" == reader.Value.ToString()) { break; }
                }

                // StartArray
                reader.Read();
                writer.WriteToken(reader, false);

                // Copy results only between [index, index + count)
                int currentIndex = 0;
                while (reader.TokenType != JsonToken.EndArray)
                {
                    reader.Read();

                    if (currentIndex >= index && currentIndex < (index + count))
                    {
                        writer.WriteToken(reader, true);
                    }
                    else
                    {
                        reader.Skip();
                    }

                    currentIndex++;
                }

                // EndArray
                writer.WriteToken(reader, false);

                // Copy after results array 
                while (reader.Read())
                {
                    writer.WriteToken(reader, false);
                }
            }
        }
    }
}

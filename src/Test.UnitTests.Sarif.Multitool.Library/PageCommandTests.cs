// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class PageCommandTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(PageCommandTests));

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

        // Some option validations (for example, "--index must be non-negative") can be performed
        // without reading the input SARIF file. We refer to these as "static" validations. When a
        // a static validation fails, we can simply print an error message and exit. By not
        // throwing an exception in these cases, we save the user from having to dig the error
        // message out of an exception dump.
        //
        // On the other hand, other option validations (for example, "--run-index must be less than
        // the number of runs in the input SARIF file") can only be performed once we've started
        // reading the input file. We refer to these as "dynamic" validations. When a dynamic
        // validation fails, we have to throw an exception.
        //
        // We have separate unit tests for the static and dynamic validations, since they are set
        // up and detected differently.
        internal struct StaticValidationTestCase
        {
            public string Title;
            public PageOptions Options;
            public bool InputFileExists;
            public bool OutputFileExists;
            public bool ExpectedReturnValue;
        }

        private const string InputFilePath = "example.sarif";
        private const string OutputFilePath = "example.paged.sarif";

        private static readonly StaticValidationTestCase[] s_validationTestCases = new StaticValidationTestCase[]
        {
            new StaticValidationTestCase
            {
                Title = "Valid options",
                Options = new PageOptions { RunIndex = 0, Index = 0, Count = 0, InputFilePath = InputFilePath, OutputFilePath = OutputFilePath },
                InputFileExists = true,
                OutputFileExists = false,
                ExpectedReturnValue = true
            },

            new StaticValidationTestCase
            {
                Title = "RunIndex < 0",
                Options = new PageOptions { RunIndex = -1, Index = 0, Count = 0, InputFilePath = InputFilePath, OutputFilePath = OutputFilePath },
                InputFileExists = true,
                OutputFileExists = false,
                ExpectedReturnValue = false
            },

            new StaticValidationTestCase
            {
                Title = "Index < 0",
                Options = new PageOptions { RunIndex = 0, Index = -1, Count = 0, InputFilePath = InputFilePath, OutputFilePath = OutputFilePath },
                InputFileExists = true,
                OutputFileExists = false,
                ExpectedReturnValue = false
            },

            new StaticValidationTestCase
            {
                Title = "Count < 0",
                Options = new PageOptions { RunIndex = 0, Index = 0, Count = -1, InputFilePath = InputFilePath, OutputFilePath = OutputFilePath },
                InputFileExists = true,
                OutputFileExists = false,
                ExpectedReturnValue = false
            },

            new StaticValidationTestCase
            {
                Title = "Nonexistent input file",
                Options = new PageOptions { RunIndex = 0, Index = 0, Count = 0, InputFilePath = InputFilePath, OutputFilePath = OutputFilePath },
                InputFileExists = false,
                OutputFileExists = false,
                ExpectedReturnValue = false
            },

            new StaticValidationTestCase
            {
                Title = "Existing output file without force",
                Options = new PageOptions { RunIndex = 0, Index = 0, Count = 0, InputFilePath = InputFilePath, OutputFilePath = OutputFilePath },
                InputFileExists = true,
                OutputFileExists = true,
                ExpectedReturnValue = false
            },

            new StaticValidationTestCase
            {
                Title = "Existing output file with force",
                Options = new PageOptions { RunIndex = 0, Index = 0, Count = 0, InputFilePath = InputFilePath, OutputFilePath = OutputFilePath, Force = true },
                InputFileExists = true,
                OutputFileExists = true,
                ExpectedReturnValue = true
            }
        };

        [Fact]
        [Trait(TestTraits.Bug, "https://github.com/microsoft/sarif-sdk/issues/1629")]
        public void PageCommand_StaticOptionValidation()
        {
            var failedTestCases = new List<string>();

            foreach (StaticValidationTestCase testCase in s_validationTestCases)
            {
                var mockFileSystem = new Mock<IFileSystem>();
                mockFileSystem.Setup(x => x.FileExists(InputFilePath)).Returns(testCase.InputFileExists);
                mockFileSystem.Setup(x => x.FileExists(OutputFilePath)).Returns(testCase.OutputFileExists);
                IFileSystem fileSystem = mockFileSystem.Object;

                var command = new PageCommand();
                if (command.ValidateOptions(testCase.Options, fileSystem) != testCase.ExpectedReturnValue)
                {
                    failedTestCases.Add(testCase.Title);
                }
            }

            failedTestCases.Should().BeEmpty();
        }

        [Fact]
        public void PageCommand_DynamicOptionValidation()
        {
            string sampleFilePath = "elfie-arriba.sarif";
            string pagedSamplePath = "elfie-arriba.paged.sarif";
            File.WriteAllText(sampleFilePath, Extractor.GetResourceText(@"PageCommand.elfie-arriba.sarif"));

            // Index >= Count
            Assert.Throws<ArgumentOutOfRangeException>(() => RunAndCompare(new PageOptions() { Index = 5, Count = 1, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath }));

            // RunIndex >= RunCount
            Assert.Throws<ArgumentOutOfRangeException>(() => RunAndCompare(new PageOptions() { RunIndex = 1, Index = 1, Count = 1, InputFilePath = sampleFilePath, OutputFilePath = pagedSamplePath }));
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

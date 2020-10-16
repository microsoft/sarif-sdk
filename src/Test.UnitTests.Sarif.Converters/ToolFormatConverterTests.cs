// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ToolFormatConverterTests
    {
        private readonly ToolFormatConverter _converter = new ToolFormatConverter();

        [Fact]
        public void ToolFormatConverter_ConvertToStandardFormat_NullInputFile()
        {
            string file = Path.GetTempFileName();
            Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, null, file);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ToolFormatConverter_ConvertToStandardFormat_NullOutputFile()
        {
            string file = Path.GetTempFileName();
            Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, file, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ToolFormatConverter_ConvertToStandardFormat_NullInputStream()
        {
            var output = new ResultLogObjectWriter();
            Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, null, output);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ToolFormatConverter_ConvertToStandardFormat_NullOutputStream()
        {
            using (var stream = new MemoryStream())
            {
                Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, stream, null);

                action.Should().Throw<ArgumentNullException>();
            }
        }

        [Fact]
        public void ToolFormatConverter_ConvertToStandardFormat_UnknownToolFormat()
        {
            var output = new ResultLogObjectWriter();
            using (var input = new MemoryStream())
            {
                Action action = () => _converter.ConvertToStandardFormat("UnknownTool", input, output);

                action.Should().Throw<ArgumentException>();
            }
        }

        [Fact]
        public void ToolFormatConverter_ConvertToStandardFormat_InputDoesNotExist()
        {
            string file = this.GetType().Assembly.Location;
            string doesNotExist = Guid.NewGuid().ToString();
            Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, doesNotExist, file, LoggingOptions.OverwriteExistingOutputFile);

            action.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void ToolFormatConverter_TruncatesOutputFileInOverwriteMode()
        {
            // Using CPPCheck because its empty file format is the simplest
            string emptyCppCheckLog = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<results version=""2"">
    <cppcheck version=""1.66""/>
    <errors />
</results>";

            using (var tempDir = new TempDirectory())
            {
                string inputFileName = tempDir.Write("input.xml", emptyCppCheckLog);
                string expectedOutputFileName = tempDir.Combine("output_expected.xml");
                _converter.ConvertToStandardFormat(ToolFormat.CppCheck, inputFileName, expectedOutputFileName);

                string expectedOutput = File.ReadAllText(expectedOutputFileName, Encoding.UTF8);
                string actualOutputFileName = tempDir.Write("output_actual.xml", new string('a', expectedOutput.Length + 4096));
                _converter.ConvertToStandardFormat(ToolFormat.CppCheck, inputFileName, actualOutputFileName, LoggingOptions.OverwriteExistingOutputFile);
                string actualOutput = File.ReadAllText(actualOutputFileName, Encoding.UTF8);

                actualOutput.Should().Be(expectedOutput);
            }
        }

        [Fact]
        public void ToolFormatConverter_FailsIfPluginAssemblyDoesNotExist()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "TestTool";
                const string PluginAssemblyPath = "NoSuchAssembly.dll";

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    PluginAssemblyPath);

                action.Should().Throw<ArgumentException>()
                    .Where(ex => ex.Message.Contains(PluginAssemblyPath));
            }
        }

        [Fact]
        public void ToolFormatConverter_FailsIfPluginAssemblyCannotBeLoaded()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "TestTool";
                string PluginAssemblyPath = GetCurrentAssemblyPath();

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                // Unlike all the other tests, which default-construct the converter, this test
                // provides the converter with a delegate which simulates the failure to load the
                // plugin assembly.
                const string Message = "Something went dreadfully wrong.";
                AssemblyLoadFileDelegate assemblyLoadFileDelegate = path => throw new BadImageFormatException(Message);

                var converter = new ToolFormatConverter(assemblyLoadFileDelegate);

                Action action = () => converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    PluginAssemblyPath);

                // The attempt to convert the file should throw the same exception.
                action.Should().Throw<BadImageFormatException>().WithMessage(Message);
            }
        }

        [Fact]
        public void ToolFormatConverter_FailsIfConverterTypeIsNotPresentInPluginAssembly()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "NoSuchTool";
                string pluginAssemblyPath = GetCurrentAssemblyPath();

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    pluginAssemblyPath);

                action.Should().Throw<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [Fact]
        public void ToolFormatConverter_FailsIfConverterTypeIsAmbiguousInPluginAssembly()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "AmbiguousTool";
                string pluginAssemblyPath = GetCurrentAssemblyPath();

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    pluginAssemblyPath);

                action.Should().Throw<ArgumentException>()
                    .Where(ex =>
                        ex.Message.Contains(pluginAssemblyPath)
                        && ex.Message.Contains(ToolName));
            }
        }

        [Fact]
        public void ToolFormatConverter_FailsIfConverterTypeIsNonPublic()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "NonPublicTool";
                string pluginAssemblyPath = GetCurrentAssemblyPath();

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    pluginAssemblyPath);

                action.Should().Throw<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [Fact]
        public void ToolFormatConverter_FailsIfConverterTypeIsAbstract()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "AbstractTool";
                string pluginAssemblyPath = GetCurrentAssemblyPath();

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    pluginAssemblyPath);

                action.Should().Throw<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [Fact]
        public void ToolFormatConverter_FailsIfConverterTypeDoesNotHaveCorrectBaseClass()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "IncorrectlyDerivedTool";
                string pluginAssemblyPath = GetCurrentAssemblyPath();

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    pluginAssemblyPath);

                action.Should().Throw<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [Fact]
        public void ToolFormatConverter_FailsIfConverterTypeDoesNotHaveDefaultConstructor()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "NoDefaultConstructorTool";
                string pluginAssemblyPath = GetCurrentAssemblyPath();

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    pluginAssemblyPath);

                action.Should().Throw<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [Fact]
        public void ToolFormatConverter_FindsConverterInPluginAssembly()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "TestTool";
                string pluginAssemblyPath = GetCurrentAssemblyPath();

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    pluginAssemblyPath);

                action.Should().NotThrow();
            }
        }

        [Fact]
        public void ToolFormatConverter_FindsBuiltInConverterEvenIfPluginIsSpecified()
        {
            using (var tempDir = new TempDirectory())
            {
                string pluginAssemblyPath = GetCurrentAssemblyPath();

                // A minimal valid AndroidStudio output file.
                string inputFilePath = tempDir.Write(
                    "input.txt",
                    @"<?xml version=""1.0"" encoding=""UTF-8""?><problems></problems>");

                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolFormat.AndroidStudio,
                    inputFilePath,
                    outputFilePath,
                    LoggingOptions.None,
                    OptionallyEmittedData.None,
                    pluginAssemblyPath);

                action.Should().NotThrow();
            }
        }

        [Fact]
        public void ToolFormatConverter_BuildsChainOfResponsibility()
        {
            const string PluginAssemblyPath = "Plugin.dll";

            ConverterFactory factory = new ToolFormatConverter().CreateConverterFactory(PluginAssemblyPath);

            factory.Should().BeOfType<PluginConverterFactory>();
            var pluginFactory = factory as PluginConverterFactory;
            pluginFactory.pluginAssemblyPath.Should().Be(PluginAssemblyPath);

            factory = factory.Next;
            factory.Should().BeOfType<BuiltInConverterFactory>();

            factory = factory.Next;
            factory.Should().BeNull();
        }

        private static string GetCurrentAssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uriBuilder = new UriBuilder(new Uri(codeBase));
            string path = Uri.UnescapeDataString(uriBuilder.Path);

            // The returned path has forward slashes, since it comes from a URI.
            // Calling Path.GetDirectoryName changes them to backslashes.
            string fileName = Path.GetFileName(path);
            string directory = Path.GetDirectoryName(path);
            return Path.Combine(directory, fileName);
        }
    }

    namespace TestConverters
    {
        [ExcludeFromCodeCoverage]
        public class TestToolConverter : ToolFileConverterBase
        {
            public override string ToolName => nameof(TestToolConverter);

            public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
            {
            }
        }
        [ExcludeFromCodeCoverage]
        public class AmbiguousToolConverter : ToolFileConverterBase
        {
            public override string ToolName => nameof(AmbiguousToolConverter);

            public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
            {
            }
        }

        [ExcludeFromCodeCoverage]
        internal class NonPublicToolConverter : ToolFileConverterBase
        {
            public override string ToolName => nameof(NonPublicToolConverter);

            public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
            {
            }
        }

        [ExcludeFromCodeCoverage]
        public abstract class AbstractToolConverter : ToolFileConverterBase
        {
            public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
            {
            }
        }

        [ExcludeFromCodeCoverage]
        public class IncorrectlyDerivedToolConverter
        {
        }

        [ExcludeFromCodeCoverage]
        public class NoDefaultConstructorToolConverter : ToolFileConverterBase
        {
            private readonly string name;

            public NoDefaultConstructorToolConverter(string name)
            {
                this.name = name;
            }

            public override string ToolName => nameof(NoDefaultConstructorToolConverter);

            public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
            {
            }
        }
    }

    namespace MoreTestConverters
    {
        [ExcludeFromCodeCoverage]
        public class AmbiguousToolConverter : ToolFileConverterBase
        {
            public override string ToolName => nameof(AmbiguousToolConverter);

            public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
            {
            }
        }
    }
}

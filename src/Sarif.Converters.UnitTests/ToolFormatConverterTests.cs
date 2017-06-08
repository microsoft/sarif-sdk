// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Converters;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class ToolFormatConverterTests
    {
        private readonly ToolFormatConverter _converter = new ToolFormatConverter();

        [TestMethod]
        public void ToolFormatConverter_ConvertToStandardFormat_DirectorySpecifiedAsDestination()
        {
            string input = Path.GetTempFileName();
            string directory = Environment.CurrentDirectory;
            Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, input, directory);

            action.ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void ToolFormatConverter_ConvertToStandardFormat_NullInputFile()
        {
            string file = Path.GetTempFileName();
            Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, null, file);

            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void ToolFormatConverter_ConvertToStandardFormat_NullOutputFile()
        {
            string file = Path.GetTempFileName();
            Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, file, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void ToolFormatConverter_ConvertToStandardFormat_NullInputStream()
        {
            var output = new ResultLogObjectWriter();
            Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, null, output);

            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void ToolFormatConverter_ConvertToStandardFormat_NullOutputStream()
        {
            using (var stream = new MemoryStream())
            {
                Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, stream, null);

                action.ShouldThrow<ArgumentNullException>();
            }
        }

        [TestMethod]
        public void ToolFormatConverter_ConvertToStandardFormat_UnknownToolFormat()
        {
            var output = new ResultLogObjectWriter();
            using (var input = new MemoryStream())
            {
                Action action = () => _converter.ConvertToStandardFormat("UnknownTool", input, output);

                action.ShouldThrow<ArgumentException>();
            }
        }

        [TestMethod]
        public void ToolFormatConverter_ConvertToStandardFormat_InputDoesNotExist()
        {
            string file = this.GetType().Assembly.Location;
            string doesNotExist = Guid.NewGuid().ToString();
            Action action = () =>_converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, doesNotExist, file, ToolFormatConversionOptions.OverwriteExistingOutputFile);

            action.ShouldThrow<FileNotFoundException>();
        }

        [TestMethod]
        public void ToolFormatConverter_ConvertToStandardFormat_OutputExistsAndOverwriteNotSpecified()
        {
            string exists = this.GetType().Assembly.Location;
            Action action = () => _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, exists, exists);

            action.ShouldThrow<InvalidOperationException>();
        }

        [TestMethod]
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
                var inputFileName = tempDir.Write("input.xml", emptyCppCheckLog);
                var expectedOutputFileName = tempDir.Combine("output_expected.xml");
                _converter.ConvertToStandardFormat(ToolFormat.CppCheck, inputFileName, expectedOutputFileName);

                string expectedOutput = File.ReadAllText(expectedOutputFileName, Encoding.UTF8);
                var actualOutputFileName = tempDir.Write("output_actual.xml", new string('a', expectedOutput.Length + 4096));
                _converter.ConvertToStandardFormat(ToolFormat.CppCheck, inputFileName, actualOutputFileName, ToolFormatConversionOptions.OverwriteExistingOutputFile);
                string actualOutput = File.ReadAllText(actualOutputFileName, Encoding.UTF8);
                Assert.AreEqual(expectedOutput, actualOutput);
            }
        }

        [TestMethod]
        public void ToolFormatConverter_FailsIfPluginAssemblyDoesNotExist()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "TestTool";
                var pluginAssemblyPaths = new string[] { "NoSuchAssembly.dll" };

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    ToolFormatConversionOptions.None,
                    pluginAssemblyPaths);

                action.ShouldThrow<ArgumentException>()
                    .Where(ex => ex.Message.Contains(pluginAssemblyPaths[0]));
            }
        }

        [TestMethod]
        public void ToolFormatConverter_FailsIfConverterTypeIsNotPresentInPluginAssembly()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "NoSuchTool";
                var pluginAssemblyPaths = new string[] { GetCurrentAssemblyPath() };

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    ToolFormatConversionOptions.None,
                    pluginAssemblyPaths);

                action.ShouldThrow<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [TestMethod]
        public void ToolFormatConverter_FailsIfConverterTypeIsAmbiguousInPluginAssembly()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "AmbiguousTool";
                var pluginAssemblyPaths = new string[] { GetCurrentAssemblyPath() };

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    ToolFormatConversionOptions.None,
                    pluginAssemblyPaths);

                action.ShouldThrow<ArgumentException>()
                    .Where(ex =>
                        ex.Message.Contains(pluginAssemblyPaths[0])
                        && ex.Message.Contains(ToolName));
            }
        }

        [TestMethod]
        public void ToolFormatConverter_FailsIfConverterTypeIsNonPublic()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "NonPublicTool";
                var pluginAssemblyPaths = new string[] { GetCurrentAssemblyPath() };

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    ToolFormatConversionOptions.None,
                    pluginAssemblyPaths);

                action.ShouldThrow<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [TestMethod]
        public void ToolFormatConverter_FailsIfConverterTypeIsAbstract()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "AbstractTool";
                var pluginAssemblyPaths = new[] { GetCurrentAssemblyPath() };

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    ToolFormatConversionOptions.None,
                    pluginAssemblyPaths);

                action.ShouldThrow<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [TestMethod]
        public void ToolFormatConverter_FailsIfConverterTypeDoesNotHaveCorrectBaseClass()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "IncorrectlyDerivedTool";
                var pluginAssemblyPaths = new string[] { GetCurrentAssemblyPath() };

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    ToolFormatConversionOptions.None,
                    pluginAssemblyPaths);

                action.ShouldThrow<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [TestMethod]
        public void ToolFormatConverter_FailsIfConverterTypeDoesNotHaveDefaultConstructor()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "NoDefaultConstructorTool";
                var pluginAssemblyPaths = new string[] { GetCurrentAssemblyPath() };

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    ToolFormatConversionOptions.None,
                    pluginAssemblyPaths);

                action.ShouldThrow<ArgumentException>()
                    .Where(ex => ex.Message.Contains(ToolName));
            }
        }

        [TestMethod]
        public void ToolFormatConverter_FindsConverterInPluginAssembly()
        {
            using (var tempDir = new TempDirectory())
            {
                const string ToolName = "TestTool";
                var pluginAssemblyPaths = new string[] { GetCurrentAssemblyPath() };

                string inputFilePath = tempDir.Write("input.txt", string.Empty);
                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolName,
                    inputFilePath,
                    outputFilePath,
                    ToolFormatConversionOptions.None,
                    pluginAssemblyPaths);

                action.ShouldNotThrow();
            }
        }

        [TestMethod]
        public void ToolFormatConverter_FindsBuiltInConverterEvenIfPluginIsSpecified()
        {
            using (var tempDir = new TempDirectory())
            {
                var pluginAssemblyPaths = new string[] { GetCurrentAssemblyPath() };

                // A minimal valid AndroidStudio output file.
                string inputFilePath = tempDir.Write(
                    "input.txt",
                    @"<?xml version=""1.0"" encoding=""UTF-8""?><problems></problems>");

                string outputFilePath = tempDir.Combine("output.txt");

                Action action = () => _converter.ConvertToStandardFormat(
                    ToolFormat.AndroidStudio,
                    inputFilePath,
                    outputFilePath,
                    ToolFormatConversionOptions.None,
                    pluginAssemblyPaths);

                action.ShouldNotThrow();
            }
        }

        [TestMethod]
        public void ToolFormatConverter_BuildsChainOfResponsibilityForMultiplePlugins()
        {
            var pluginAssemblyPaths = new[] { "One", "Two", "Three" };

            ConverterFactory factory = ToolFormatConverter.CreateConverterFactory(pluginAssemblyPaths);

            Assert.IsInstanceOfType(factory, typeof(PluginConverterFactory));
            var pluginFactory = factory as PluginConverterFactory;
            Assert.AreEqual("One", pluginFactory.pluginAssemblyPath);

            factory = factory.Next;
            Assert.IsInstanceOfType(factory, typeof(PluginConverterFactory));
            pluginFactory = factory as PluginConverterFactory;
            Assert.AreEqual("Two", pluginFactory.pluginAssemblyPath);

            factory = factory.Next;
            Assert.IsInstanceOfType(factory, typeof(PluginConverterFactory));
            pluginFactory = factory as PluginConverterFactory;
            Assert.AreEqual("Three", pluginFactory.pluginAssemblyPath);

            factory = factory.Next;
            Assert.IsInstanceOfType(factory, typeof(BuiltInConverterFactory));

            factory = factory.Next;
            Assert.IsNull(factory);
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
            public override void Convert(Stream input, IResultLogWriter output)
            {
            }
        }
        [ExcludeFromCodeCoverage]
        public class AmbiguousToolConverter : ToolFileConverterBase
        {
            public override void Convert(Stream input, IResultLogWriter output)
            {
            }
        }

        [ExcludeFromCodeCoverage]
        internal class NonPublicToolConverter : ToolFileConverterBase
        {
            public override void Convert(Stream input, IResultLogWriter output)
            {
            }
        }

        [ExcludeFromCodeCoverage]
        public abstract class AbstractToolConverter : ToolFileConverterBase
        {
            public override void Convert(Stream input, IResultLogWriter output)
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

            public override void Convert(Stream input, IResultLogWriter output)
            {
            }
        }
    }

    namespace MoreTestConverters
    {
        [ExcludeFromCodeCoverage]
        public class AmbiguousToolConverter : ToolFileConverterBase
        {
            public override void Convert(Stream input, IResultLogWriter output)
            {
            }
        }
    }
}

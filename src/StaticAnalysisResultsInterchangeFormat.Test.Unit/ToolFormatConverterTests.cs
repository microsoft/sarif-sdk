// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Writers;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat
{
    [TestClass]
    public class ToolFormatConverterTests
    {
        private readonly ToolFormatConverter _converter = new ToolFormatConverter();

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ToolFormatConverter_ConvertToStandardFormat_DirectorySpecifiedAsDestination()
        {
            string input = Path.GetTempFileName();
            string directory = Environment.CurrentDirectory;
            _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, input, directory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolFormatConverter_ConvertToStandardFormat_NullInputFile()
        {
            string file = Path.GetTempFileName();
            _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, null, file);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolFormatConverter_ConvertToStandardFormat_NullOutputFile()
        {
            string file = Path.GetTempFileName();
            _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, file, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolFormatConverter_ConvertToStandardFormat_NullInputStream()
        {
            var output = new ResultLogObjectWriter();
            _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, null, output);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolFormatConverter_ConvertToStandardFormat_NullOutputStream()
        {
            using (var stream = new MemoryStream())
            {
                _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, stream, null);
            }
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ToolFormatConverter_ConvertToStandardFormat_UnknownToolFormat()
        {
            var output = new ResultLogObjectWriter();
            using (var input = new MemoryStream())
            {
                _converter.ConvertToStandardFormat(0, input, output);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ToolFormatConverter_ConvertToStandardFormat_InputDoesNotExist()
        {
            string file = this.GetType().Assembly.Location;
            string doesNotExist = Guid.NewGuid().ToString();
            _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, doesNotExist, file, ToolFormatConversionOptions.OverwriteExistingOutputFile);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ToolFormatConverter_ConvertToStandardFormat_OutputExistsAndOverwriteNotSpecified()
        {
            string exists = this.GetType().Assembly.Location;
            _converter.ConvertToStandardFormat(ToolFormat.AndroidStudio, exists, exists);
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
    }
}

// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System;
using System.Globalization;
using System.IO;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    // The functional tests for the PREfast converter must be handled differently from
    // the functional tests for all the other tools. The reason is that the
    // PREfastXmlSarifConverter (which produces the PREfast test files) is written in
    // C++, so it does not use Newtonsoft.Json for serialization. As a result, it
    // serializes properties in a different order, and with slightly different
    // formatting, than the other converters (which do use Newtonsoft.Json).
    //
    // Now, the functional tests work by deserializing a SARIF file into an object model,
    // re-serializing it using Newtonsoft.Json, and comparing the two files. Obviously if
    // the serialization and deserialization code doesn't match, the test will fail.
    //
    // So we introduce a "test mode" flag into the functional tests.For all tools except
    // PREfast, we compare the serialized files.For PREfast, we compare the object models
    // obtained by deserializing those files.
    internal enum TestMode
    {
        CompareFileContents,
        CompareObjectModels
    }

    public class SarifConverterTests
    {
        public const string TestDirectory = "ConverterTestData";

        [Fact]
        public void AndroidStudioConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.AndroidStudio, TestMode.CompareFileContents);
        }

        [Fact]
        public void ClangAnalyzerConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.ClangAnalyzer, TestMode.CompareFileContents);
        }

        [Fact]
        public void CppCheckConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.CppCheck, TestMode.CompareFileContents);
        }

        [Fact]
        public void FortifyConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.Fortify, TestMode.CompareFileContents);
        }

        [Fact]
        public void FxCopConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.FxCop, TestMode.CompareFileContents);
        }

        [Fact]
        public void SemmleConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.SemmleQL, "*.csv", TestMode.CompareFileContents);
        }

        [Fact]
        public void StaticDriverVerifierConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.StaticDriverVerifier, "*.tt", TestMode.CompareFileContents);
        }

        [Fact]
        public void PREfastConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.PREfast, TestMode.CompareObjectModels);
        }

        private readonly ToolFormatConverter converter = new ToolFormatConverter();

        private void BatchRunConverter(string tool, TestMode testMode)
        {
            BatchRunConverter(tool, "*.xml", testMode);
        }

        private void BatchRunConverter(string tool, string inputFilter, TestMode testMode)
        {
            var sb = new StringBuilder();

            string testDirectory = SarifConverterTests.TestDirectory + "\\" + tool;
            string[] testFiles = Directory.GetFiles(testDirectory, inputFilter);

            foreach (string file in testFiles)
            {
                RunConverter(sb, tool, file, testMode);
            }

            sb.Length.Should().Be(0, FormatFailureReason(sb, tool));
        }

        private static string FormatFailureReason(StringBuilder sb, string toolName)
        {
            sb.Insert(0, "the converted tool file should have matched the supplied SARIF file. ");

            string rebaselineMessage = "If the actual output is expected, generate new baselines for {0} by executing `UpdateBaselines.ps1 {0}` from a developer command prompt, or `UpdateBaselines.ps1` to update baselines for all tools.";
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, rebaselineMessage, toolName));
            return sb.ToString();
        }

        private void RunConverter(StringBuilder sb, string toolFormat, string inputFileName, TestMode testMode)
        {
            string expectedFileName = inputFileName + ".sarif";
            string generatedFileName = inputFileName + ".actual.sarif";

            try
            {
                this.converter.ConvertToStandardFormat(toolFormat, inputFileName, generatedFileName, LoggingOptions.OverwriteExistingOutputFile | LoggingOptions.PrettyPrint);
            }
            catch (Exception ex)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "The converter {0} threw an exception for input \"{1}\".", toolFormat, inputFileName));
                sb.AppendLine(ex.ToString());
                return;
            }

            string expectedSarif = File.ReadAllText(expectedFileName);
            string actualSarif = File.ReadAllText(generatedFileName);

            if (expectedSarif == actualSarif)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    ContractResolver = SarifContractResolver.Instance,
                    Formatting = Formatting.Indented
                };

                // Make sure we can successfully deserialize what was just generated
                SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(actualSarif, settings);

                actualSarif = JsonConvert.SerializeObject(actualLog, settings);

                bool success;
                switch (testMode)
                {
                    case TestMode.CompareFileContents:
                        success = expectedSarif == actualSarif;
                        break;

                    case TestMode.CompareObjectModels:
                        SarifLog expectedLog = JsonConvert.DeserializeObject<SarifLog>(expectedSarif, settings);
                        success = SarifLogEqualityComparer.Instance.Equals(expectedLog, actualLog);
                        break;

                    default:
                        throw new ArgumentException($"Invalid test mode: {testMode}", nameof(testMode));
                }

                if (success)
                {
                    return;
                }
                else
                {
                    File.WriteAllText(generatedFileName, actualSarif);
                }
            }

            string errorMessage = "The output of the {0} converter did not match for input {1}.";
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, errorMessage, toolFormat, inputFileName));
            sb.AppendLine("Check differences with:");
            sb.AppendLine(GenerateDiffCommand(expectedFileName, generatedFileName));
        }

        private string GenerateDiffCommand(string expected, string actual)
        {
            expected = Path.GetFullPath(expected);
            actual = Path.GetFullPath(actual);

            string beyondCompare = TryFindBeyondCompare();
            if (beyondCompare != null)
            {
                return String.Format(CultureInfo.InvariantCulture, "\"{0}\" \"{1}\" \"{2}\" /title1=Expected /title2=Actual", beyondCompare, expected, actual);
            }

            return String.Format(CultureInfo.InvariantCulture, "tfsodd \"{0}\" \"{1}\"", expected, actual);
        }

        private static string TryFindBeyondCompare()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            for (int idx = 4; idx >= 3; --idx)
            {
                string beyondComparePath = String.Format(CultureInfo.InvariantCulture, "{0}\\Beyond Compare {1}\\BComp.exe", programFiles, idx);
                if (File.Exists(beyondComparePath))
                {
                    return beyondComparePath;
                }
            }

            string beyondCompare2Path = programFiles + "\\Beyond Compare 2\\BC2.exe";
            if (File.Exists(beyondCompare2Path))
            {
                return beyondCompare2Path;
            }

            return null;
        }
    }
}

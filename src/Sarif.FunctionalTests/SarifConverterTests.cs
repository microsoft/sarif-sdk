/********************************************************
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
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class SarifConverterTests
    {
        public const string TestDirectory = "ConverterTestData";

        [Fact]
        public void AndroidStudioConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.AndroidStudio);
        }

        [Fact]
        public void ClangAnalyzerConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.ClangAnalyzer);
        }

        [Fact]
        public void CppCheckConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.CppCheck);
        }

        [Fact]
        public void FortifyConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.Fortify);
        }

        [Fact]
        public void FxCopConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.FxCop);
        }

        [Fact]
        public void SemmleConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.SemmleQL, "*.csv");
        }

        [Fact]
        public void StaticDriverVerifierConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.StaticDriverVerifier, "*.tt");
        }

        [Fact]
        public void TSLint_EndToEnd()
        {
            BatchRunConverter(ToolFormat.TSLint, "*.json");
        }

        [Fact]
        public void PREfastConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.PREfast);
        }

        [Fact]
        public void ValidateDeserializationAndPrettifiedSarifStringComparisionLogic()
        {
            string sarifComparatorFileFolder = "TestSarifComparator/Prettified";            
            string testDirectory = Path.Combine(TestDirectory, sarifComparatorFileFolder);
            string generatedSarif = File.ReadAllText(Directory.GetFiles(testDirectory, "generated.sarif")[0]);
            string expectedSarif = File.ReadAllText(Directory.GetFiles(testDirectory, "expected.sarif")[0]);
            CanGeneratedSarifBeDeserializedToExpectedSarif(generatedSarif, expectedSarif).Should().Be(true);
        }

        [Fact]
        public void ValidateDeserializationAndNotPrettifiedSarifStringComparisionLogic()
        {
            string sarifComparatorFileFolder = "TestSarifComparator/NotPrettified";
            string testDirectory = Path.Combine(TestDirectory, sarifComparatorFileFolder);
            string generatedSarif = File.ReadAllText(Directory.GetFiles(testDirectory, "generated.sarif")[0]);
            string expectedSarif = File.ReadAllText(Directory.GetFiles(testDirectory, "expected.sarif")[0]);
            CanGeneratedSarifBeDeserializedToExpectedSarif(generatedSarif, expectedSarif).Should().Be(true);
        }

        [Fact]
        public void ValidateDeserializationAndJsonStringComparisionLogicFail()
        {
            //CanGeneratedSarifBeDeserializedToExpectedSarif(generatedSarif, expectedSarif).Should().Be(true);
        }    

        private readonly ToolFormatConverter converter = new ToolFormatConverter();

        private void BatchRunConverter(string tool, string inputFilter = "*.xml")
        {
            var sb = new StringBuilder();

            string testDirectory = Path.Combine(TestDirectory, tool);
            string[] testFiles = Directory.GetFiles(testDirectory, inputFilter);

            foreach (string file in testFiles)
            {
                RunConverter(sb, tool, file);
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

        private bool CanGeneratedSarifBeDeserializedToExpectedSarif(string generatedSarif, string expectedSarif)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = Formatting.Indented
            };
            
            // Make sure we can successfully deserialize what was just generated
            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(generatedSarif, settings);

            generatedSarif = JsonConvert.SerializeObject(actualLog, settings);

            JToken generatedToken = JToken.Parse(generatedSarif);
            JToken expectedToken = JToken.Parse(expectedSarif);

            return JToken.DeepEquals(generatedToken, expectedToken);
        }

        private void RunConverter(StringBuilder sb, string toolFormat, string inputFileName)
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
            
            if (CanGeneratedSarifBeDeserializedToExpectedSarif(actualSarif, expectedSarif))
            {
                return;
            }
            else
            {
                File.WriteAllText(generatedFileName, actualSarif);
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

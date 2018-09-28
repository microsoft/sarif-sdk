// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;


namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class SarifConverterTests : FileDiffingTests
    {
        public const string TestDirectory = @"v2\ConverterTestData";

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
            BatchRunConverter(ToolFormat.FxCop, enrichConvertedSarif: true);
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
        public void PyLintConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.Pylint, "*.json");
        }

        private readonly ToolFormatConverter converter = new ToolFormatConverter();

        private void BatchRunConverter(string tool, string inputFilter = "*.xml", bool enrichConvertedSarif = false)
        {
            var sb = new StringBuilder();

            string testDirectory = Path.Combine(TestDirectory, tool);
            string[] testFiles = Directory.GetFiles(testDirectory, inputFilter);

            foreach (string file in testFiles)
            {
                string actualFilePath = RunConverter(sb, tool, file);

                if (enrichConvertedSarif && File.Exists(actualFilePath))
                {
                    EnrichSarifLog(actualFilePath);
                }
            }

            sb.Length.Should().Be(0, FormatFailureReason(sb, tool));
        }

        private static void EnrichSarifLog(string actualFilePath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            string logText = File.ReadAllText(actualFilePath);

            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(logText, settings);

            OptionallyEmittedData dataToInsert =
                OptionallyEmittedData.Hashes |
                OptionallyEmittedData.RegionSnippets |
                OptionallyEmittedData.ContextRegionSnippets |
                OptionallyEmittedData.ComprehensiveRegionProperties;

            SarifLog reformattedLog = new InsertOptionalDataVisitor(dataToInsert).VisitSarifLog(actualLog);

            File.WriteAllText(actualFilePath, JsonConvert.SerializeObject(reformattedLog, settings));
        }

        private static string FormatFailureReason(StringBuilder sb, string toolName)
        {
            sb.Insert(0, "the converted tool file should have matched the supplied SARIF file. ");

            string rebaselineMessage = "If the actual output is expected, generate new baselines for {0} by executing `UpdateBaselines.ps1 {0}` from a developer command prompt, or `UpdateBaselines.ps1` to update baselines for all tools.";
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, rebaselineMessage, toolName));
            return sb.ToString();
        }

        private string RunConverter(StringBuilder sb, string toolFormat, string inputFileName)
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
                return generatedFileName;
            }

            string expectedSarif = File.ReadAllText(expectedFileName);
            string actualSarif = File.ReadAllText(generatedFileName);

            if (!AreEquivalentSarifLogs(actualSarif, expectedSarif))
            {
                File.WriteAllText(generatedFileName, actualSarif);

                string errorMessage = "The output of the {0} converter did not match for input {1}.";
                sb.AppendLine(string.Format(CultureInfo.CurrentCulture, errorMessage, toolFormat, inputFileName));
                sb.AppendLine("Check differences with:");
                sb.AppendLine(GenerateDiffCommand(expectedFileName, generatedFileName));
            }
            return generatedFileName;
        }
    }
}

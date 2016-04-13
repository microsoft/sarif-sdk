// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Validation;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifValidatorTests
    {
        public const string TestDirectory = "SarifConverterTestData";
        public const string JsonSchemaFile = "Sarif.schema.json";

        private readonly string _jsonSchemaFilePath;

        public SarifValidatorTests()
        {
            _jsonSchemaFilePath = Path.Combine(Environment.CurrentDirectory, JsonSchemaFile);
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Sample_tool_output_files_conform_to_SARIF_schema(string inputFile)
        {
            IEnumerable<JsonError> errors = Validator.ValidateFile(inputFile, JsonSchemaFile);

            // Test errors.Count(), rather than errors.Should().BeEmpty, because the latter
            // produces a less clear error message: it calls ToString on each member of
            // errors, and appends it to the string returned by FailureReason. Since
            // FailureReason already displayed the error messages in VisualStudio format,
            // there is no reason to append this additional, less well formatted information.
            errors.Count().Should().Be(0, FailureReason(errors, inputFile));
        }

        private static IEnumerable<object[]> s_testCases;

        private static string[] InvalidFiles = new string[]
        {
        };

        public static IEnumerable<object[]> TestCases
        {
            get
            {
                if (s_testCases == null)
                {
                    var sarifFiles = Directory.GetFiles(TestDirectory, "*.sarif", SearchOption.AllDirectories);

                    // The converter functional tests produce output files in the test directory
                    // with the filename extension ".actual.sarif". Don't include those in this test.
                    var actualSarifFiles = Directory.GetFiles(TestDirectory, "*.actual.sarif", SearchOption.AllDirectories);

                    s_testCases = sarifFiles.Except(actualSarifFiles)
                        .Except(InvalidFiles.Select(f => Path.Combine(TestDirectory, f)))
                        .Select(file => new object[] { file });
                }

                return s_testCases;
            }
        }

        private string FailureReason(IEnumerable<JsonError> errors, string inputFile)
        {
            var sb = new StringBuilder("file should be valid, but the following errors were found:\n");

            string inputPath = MakeFullPath(inputFile);
            string outputPath = MakeOutputPath(inputPath);

            using (var logBuilder = new ResultLogBuilder(
                inputPath,
                _jsonSchemaFilePath,
                outputPath,
                new FileSystem()))
            {
                IEnumerable<string> messages = logBuilder.BuildLog(errors);
                foreach (var message in messages)
                {
                    sb.AppendLine(message);
                }
            }

            return sb.ToString();
        }

        private static string MakeFullPath(string inputFile)
        {
            return Path.Combine(Environment.CurrentDirectory, inputFile);
        }

        private string MakeOutputPath(string inputPath)
        {
            // Give the output files an extension of .json rather than .sarif so that
            // a subsequent test run (which again will validate all .sarif files in
            // the test file directory) does not attempt to validate the output files.
            return Path.Combine(
                Path.GetDirectoryName(inputPath),
                Path.GetFileNameWithoutExtension(inputPath)) + ".validation.sarif.json";
        }
    }
}

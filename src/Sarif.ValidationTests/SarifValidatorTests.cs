// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Sarif;
using Microsoft.Json.Schema.Validation;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifValidatorTests
    {
        public const string DirectProducerTestDataDirectory = "DirectProducerTestData";
        public const string ConverterTestDataDirectory = "ConverterTestData";
        public const string SpectExamplesDirectory = "SpecExamples";
        public const string JsonSchemaFile = "Sarif.schema.json";

        private readonly string _jsonSchemaFilePath;
        private readonly JsonSchema _schema;

        public SarifValidatorTests()
        {
            _jsonSchemaFilePath = Path.Combine(Environment.CurrentDirectory, JsonSchemaFile);
            string schemaText = File.ReadAllText(JsonSchemaFile);
            _schema = SchemaReader.ReadSchema(schemaText, JsonSchemaFile);
        }

        [Theory]
        [MemberData(nameof(DirectProducerTestCases))]
        public void DirectProducerValidation(string inputFile)
        {
            string instanceText = File.ReadAllText(inputFile);
            var validator = new Validator(_schema);

            Result[] errors = validator.Validate(instanceText, inputFile);

            // Test errors.Count(), rather than errors.Should().BeEmpty, because the latter
            // produces a less clear error message: it calls ToString on each member of
            // errors, and appends it to the string returned by FailureReason. Since
            // FailureReason already displayed the error messages in VisualStudio format,
            // there is no reason to append this additional, less well formatted information.
            errors.Count().Should().Be(0, FailureReason(errors));
        }

        [Theory]
        [MemberData(nameof(ConverterTestCases))]
        public void ConverterValidation(string inputFile)
        {
            string instanceText = File.ReadAllText(inputFile);
            var validator = new Validator(_schema);

            Result[] errors = validator.Validate(instanceText, inputFile);

            // Test errors.Count(), rather than errors.Should().BeEmpty, because the latter
            // produces a less clear error message: it calls ToString on each member of
            // errors, and appends it to the string returned by FailureReason. Since
            // FailureReason already displayed the error messages in VisualStudio format,
            // there is no reason to append this additional, less well formatted information.
            errors.Count().Should().Be(0, FailureReason(errors));
        }

        [Theory]
        [MemberData(nameof(SpecExampleTestCases))]
        public void SpecExampleValidation(string inputFile)
        {
            string instanceText = File.ReadAllText(inputFile);
            var validator = new Validator(_schema);

            Result[] errors = validator.Validate(instanceText, inputFile);

            // Test errors.Count(), rather than errors.Should().BeEmpty, because the latter
            // produces a less clear error message: it calls ToString on each member of
            // errors, and appends it to the string returned by FailureReason. Since
            // FailureReason already displayed the error messages in VisualStudio format,
            // there is no reason to append this additional, less well formatted information.
            errors.Count().Should().Be(0, FailureReason(errors));
        }

        private static IEnumerable<object[]> s_converterTestCases;
        private static IEnumerable<object[]> s_directProducerTestCases;
        private static IEnumerable<object[]> s_exampleTestCases;

        private static string[] InvalidFiles = new string[]
        {
        };

        public static IEnumerable<object[]> DirectProducerTestCases
        {
            get
            {
                if (s_directProducerTestCases == null)
                {
                    var sarifFiles = Directory.GetFiles(DirectProducerTestDataDirectory, "*.sarif", SearchOption.AllDirectories);

                    s_directProducerTestCases = sarifFiles
                        .Except(InvalidFiles.Select(f => Path.Combine(DirectProducerTestDataDirectory, f)))
                        .Select(file => new object[] { file.ToLowerInvariant() });
                }

                return s_directProducerTestCases;
            }
        }

        public static IEnumerable<object[]> SpecExampleTestCases
        {
            get
            {
                if (s_exampleTestCases == null)
                {
                    var sarifFiles = Directory.GetFiles(SpectExamplesDirectory, "*.sarif", SearchOption.AllDirectories);

                    s_exampleTestCases = sarifFiles
                        .Except(InvalidFiles.Select(f => Path.Combine(SpectExamplesDirectory, f)))
                        .Select(file => new object[] { file.ToLowerInvariant() });
                }

                return s_exampleTestCases;
            }
        }

        public static IEnumerable<object[]> ConverterTestCases
        {
            get
            {
                if (s_converterTestCases == null)
                {
                    var sarifFiles = Directory.GetFiles(ConverterTestDataDirectory, "*.sarif", SearchOption.AllDirectories);

                    // The converter functional tests produce output files in the test directory
                    // with the filename extension ".actual.sarif". Don't include those in this test.
                    var actualSarifFiles = Directory.GetFiles(ConverterTestDataDirectory, "*.actual.sarif", SearchOption.AllDirectories);

                    s_converterTestCases = sarifFiles.Except(actualSarifFiles)
                        .Except(InvalidFiles.Select(f => Path.Combine(ConverterTestDataDirectory, f)))
                        .Select(file => new object[] { file.ToLowerInvariant() });
                }

                return s_converterTestCases;
            }
        }

        private string FailureReason(Result[] errors)
        {
            var sb = new StringBuilder("file should be valid, but the following errors were found:\n");

            foreach (var error in errors)
            {
                sb.AppendLine(error.FormatForVisualStudio(RuleFactory.GetRuleFromRuleId(error.RuleId)));
            }

            return sb.ToString();
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Validation;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifValidatorTests
    {
        public const string JsonSchemaFile = "Sarif.schema.json";

        private readonly string _jsonSchemaFilePath;
        private readonly JsonSchema _schema;

        public SarifValidatorTests()
        {
            _jsonSchemaFilePath = Path.Combine(Environment.CurrentDirectory, JsonSchemaFile);
            string schemaText = File.ReadAllText(JsonSchemaFile);
            _schema = SchemaReader.ReadSchema(schemaText, JsonSchemaFile);
        }

        [Fact]
        public void ValidatesAllTestFiles()
        {
            var validator = new Validator(_schema);
            var sb = new StringBuilder();

            foreach (string inputFile in TestCases)
            {
                string instanceText = File.ReadAllText(inputFile);
                Result[] errors = validator.Validate(instanceText, inputFile);

                // Test errors.Count(), rather than errors.Should().BeEmpty, because the latter
                // produces a less clear error message: it calls ToString on each member of
                // errors, and appends it to the string returned by FailureReason. Since
                // FailureReason already displayed the error messages in VisualStudio format,
                // there is no reason to append this additional, less well formatted information.

                if (errors.Length > 0)
                {
                    sb.AppendLine(FailureReason(errors));
                }
            }

            sb.Length.Should().Be(0, sb.ToString());
        }

        private static readonly string[] s_testFileDirectories = new string[]
        {
            @"v2\ConverterTestData",
            @"v2\SpecExamples"
        };

        private static IEnumerable<string> s_testCases;

        private static string[] InvalidFiles = new string[]
        {
        };

        public static IEnumerable<string> TestCases
        {
            get
            {
                if (s_testCases == null)
                {
                    List<string> sarifFiles = s_testFileDirectories.Aggregate(
                        new List<string>(),
                        (allFiles, dir) =>
                        {
                            allFiles.AddRange(Directory.GetFiles(dir, "*.sarif", SearchOption.AllDirectories)

                                // The converter functional tests produce output files in the test directory
                                // with the filename extension ".actual.sarif". Don't include those in this test.
                                .Where(file => !file.EndsWith(".actual.sarif", StringComparison.OrdinalIgnoreCase))

                                // Leave a loophole in case we have to temporarily include a file that doesn't
                                // conform to the current spec.
                                .Except(InvalidFiles));

                            return allFiles;
                        });

                    s_testCases = sarifFiles;
                }

                return s_testCases;
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

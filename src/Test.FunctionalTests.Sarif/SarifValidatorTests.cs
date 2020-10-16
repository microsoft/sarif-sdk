// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifValidatorTests
    {
        public const string JsonSchemaFile = "sarif-" + VersionConstants.SchemaVersionAsPublishedToSchemaStoreOrg + ".json";

        private readonly string _jsonSchemaFilePath;
        private readonly JsonSchema _schema;

        public SarifValidatorTests()
        {
            _jsonSchemaFilePath = Path.Combine(Environment.CurrentDirectory, JsonSchemaFile);
            string schemaText = File.ReadAllText(JsonSchemaFile);
            _schema = SchemaReader.ReadSchema(schemaText, JsonSchemaFile);
        }

        [Fact]
        public void ValidateAllTheThings()
        {
            // First, we start with builders that only populate required properties that are backed by primitives.
            IDictionary<Type, DefaultObjectPopulatingVisitor.PrimitiveValueBuilder> propertyValueBuilders =
                DefaultObjectPopulatingVisitor.GetBuildersForAllPrimitives();

            Func<SarifLog, SarifLog> callback =
                (sarifLog) =>
                {
                    var visitor = new OverridePrimitiveArraysPopulatingVisitor();
                    return visitor.VisitSarifLog(sarifLog);
                };

            ValidateDefaultDocument(propertyValueBuilders, callback);
        }

        [Fact]
        public void ValidatesUriConversion()
        {
            Func<SarifLog, SarifLog> callback =
                (sarifLog) =>
                {
                    var visitor = new OverridePrimitiveArraysPopulatingVisitor();
                    return visitor.VisitSarifLog(sarifLog);
                };

            // First, we start with builders that only populate required properties that are backed by primitives.
            IDictionary<Type, DefaultObjectPopulatingVisitor.PrimitiveValueBuilder> propertyValueBuilders =
                DefaultObjectPopulatingVisitor.GetBuildersForRequiredPrimitives();

            // This test injects a URI into every URI-based property in the format that is a file path with a space.
            // This URI won't be properly percent-encoded unless our UriConverter was invoked during seriallization.
            // This test therefore ensures that all URI's in the format are properly associated with that converter.            
            propertyValueBuilders[typeof(Uri)] = (isRequired) => { return new Uri(@"c:\path with a space\file.txt"); };

            ValidateDefaultDocument(propertyValueBuilders, postPopulationCallback: callback);
        }

        [Fact]
        public void DefaultValuesDoNotSerialize()
        {
            Func<SarifLog, SarifLog> callback =
                (sarifLog) =>
                {
                    var visitor = new OverridePrimitiveArraysPopulatingVisitor();
                    return visitor.VisitSarifLog(sarifLog);
                };

            ValidateDefaultDocument(
                propertyValueBuilders: DefaultObjectPopulatingVisitor.GetBuildersForRequiredPrimitives(),
                postPopulationCallback: callback);
        }

        public class OverridePrimitiveArraysPopulatingVisitor : SarifRewritingVisitor
        {
            // TODO: https://github.com/Microsoft/sarif-sdk/issues/1425
            // SarifValidatorTests improvement: add generic logic for populating primitive lists.

            public override ReportingDescriptorRelationship VisitReportingDescriptorRelationship(ReportingDescriptorRelationship node)
            {
                node.Kinds[0] = "relevant";
                return base.VisitReportingDescriptorRelationship(node);
            }

            public override Run VisitRun(Run node)
            {
                node.RedactionTokens[0] = SarifConstants.RedactedMarker;
                return base.VisitRun(node);
            }

            public override LocationRelationship VisitLocationRelationship(LocationRelationship node)
            {
                node.Kinds[0] = "includes";
                return base.VisitLocationRelationship(node);
            }

            public override ReportingDescriptor VisitReportingDescriptor(ReportingDescriptor node)
            {
                node.DeprecatedGuids[0] = "36D2C336-7730-425A-9A94-43593A2A651C";
                return base.VisitReportingDescriptor(node);
            }
        }

        private void ValidateDefaultDocument(IDictionary<Type, DefaultObjectPopulatingVisitor.PrimitiveValueBuilder> propertyValueBuilders, Func<SarifLog, SarifLog> postPopulationCallback = null)
        {
            var visitor = new DefaultObjectPopulatingVisitor(_schema, propertyValueBuilders);

            var sarifLog = new SarifLog();

            visitor.Visit(sarifLog);

            sarifLog.Version = SarifVersion.Current;

            sarifLog.InlineExternalProperties[0].Version = SarifVersion.Current;

            if (postPopulationCallback != null)
            {
                sarifLog = postPopulationCallback(sarifLog);
            }

            string toValidate = JsonConvert.SerializeObject(sarifLog, Formatting.Indented);

            var validator = new Validator(_schema);

            // Guid here simply creates a verifiably non-existent file name. This name is only
            // used in validation reporting, there's no code that attempts to access the file.
            Result[] errors = validator.Validate(toValidate, Guid.NewGuid().ToString() + SarifConstants.SarifFileExtension);

            var sb = new StringBuilder();

            if (errors.Length > 0)
            {
                sb.AppendLine(FailureReason(errors));
            }

            sb.Length.Should().Be(0, sb.ToString());

            SarifLog clonedLog = sarifLog.DeepClone();
            clonedLog.ValueEquals(sarifLog).Should().BeTrue();

            JObject jObjectSarifLog = JObject.Parse(toValidate);

            // The following validations ensure NotYetAutoGenerated fields work as expected.

            // see .src/sarif/NotYetAutoGenerated/ReportingConfiguration.cs (Rank property)
            // see .src/sarif/NotYetAutoGenerated/Result.cs (Rank property)
            bool isRankSerialized = RecursivePropertySearch(jObjectSarifLog, "rank");
            isRankSerialized.Should().BeFalse();

            // TODO: why does this line of code provoke a stack overflow exception?
            // clonedLog.Should().BeEquivalentTo(sarifLog);
        }

        [Fact]
        public void ValidatesAllTestFiles()
        {
            var validator = new Validator(_schema);
            var sb = new StringBuilder();

            foreach (string inputFile in TestCases)
            {
                string instanceText = File.ReadAllText(inputFile);

                PrereleaseCompatibilityTransformer.UpdateToCurrentVersion(instanceText, formatting: Formatting.None, out instanceText);

                Result[] errors = validator.Validate(instanceText, inputFile);

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
            @"v2\SpecExamples",
            @"v2\ObsoleteFormats",
            @"..\..\Test.UnitTests.Sarif\netcoreapp2.1\TestData"
        };

        private static IEnumerable<string> s_testCases;

        private readonly static string[] InvalidFiles = new string[]
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

            foreach (Result error in errors)
            {
                sb.AppendLine(error.FormatForVisualStudio(RuleFactory.GetRuleFromRuleId(error.RuleId)));
            }

            return sb.ToString();
        }

        private static bool RecursivePropertySearch(JObject parentObject, string searchKey)
        {
            var properties = new List<JProperty>(parentObject.Properties());
            foreach (JProperty property in properties)
            {
                // We won't process property bags
                if (property.Name.Equals("properties"))
                {
                    continue;
                }

                if (property.Name.Equals(searchKey))
                {
                    return true;
                }

                if (property.Value is JArray jArray)
                {
                    foreach (JToken item in jArray)
                    {
                        if (item is JObject) // This is to ensure we do not process the items if the array of primitive types.
                        {
                            if (RecursivePropertySearch(item as JObject, searchKey))
                            {
                                return true;
                            }
                        }
                    }
                }

                if (property.Value is JObject jObject)
                {
                    if (RecursivePropertySearch(jObject, searchKey))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

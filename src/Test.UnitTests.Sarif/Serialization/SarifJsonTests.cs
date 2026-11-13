// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Serialization;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifJsonTests
    {
        [Fact]
        public void SarifJson_RoundTripsSarifLog()
        {
            var log = new SarifLog
            {
                Version = SarifVersion.Current,
                SchemaUri = new Uri(SarifUtilities.SarifSchemaUri),
                Runs = new[]
                {
                    new Run
                    {
                        Tool = new Tool
                        {
                            Driver = new ToolComponent { Name = "Test" }
                        }
                    }
                }
            };

            string json = SarifJson.Serialize(log);
            SarifLog roundTrippedLog = SarifJson.Deserialize<SarifLog>(json);

            roundTrippedLog.Should().BeEquivalentTo(log);
        }

        [Fact]
        public void SarifJson_DeserializesLeadingLineComment()
        {
            AssertDeserializesCommentedJson("// comment\n{\"Number\":42}");
        }

        [Fact]
        public void SarifJson_DeserializesLeadingBlockComment()
        {
            AssertDeserializesCommentedJson("/* comment */{\"Number\":42}");
        }

        [Fact]
        public void SarifJson_DeserializesMixedCommentsAndUnicodeWhitespace()
        {
            AssertDeserializesCommentedJson(
                "\u00a0\u2003 /* first */\r\n// second\r\n\t/* third */\u2003" +
                "{/* member */'Number':42,}\u00a0/* trailing */ // last");
        }

        [Fact]
        public void SarifJson_PreservesCommentOnlyInputBehavior()
        {
            MappingContainer value = SarifJson.Deserialize<MappingContainer>(
                "// comment\n /* comment */");

            value.Should().BeNull();
        }

        [Fact]
        public void SarifJson_RejectsMalformedJsonAfterLeadingComment()
        {
            SarifJsonReaderException error = AssertRejectsCommentedJson(
                "// comment\n{\"Number\": }");

            error.Path.Should().Be("Number");
            error.LineNumber.Should().Be(2);
            error.LinePosition.Should().BeGreaterThan(0);
        }

        [Fact]
        public void SarifJson_RejectsAdditionalJsonContentAfterLeadingComment()
        {
            AssertRejectsCommentedJson(
                "/* leading */{\"Number\":42}/* between */{\"Number\":1}");
        }

        [Fact]
        public void SarifJson_RejectsUnterminatedLeadingComment()
        {
            AssertRejectsCommentedJson("/* unterminated");
        }

        [Fact]
        public void SarifJson_MapsMalformedJsonToReaderException()
        {
            Action action = () => SarifJson.Deserialize<SarifLog>("{");

            action.Should().Throw<SarifJsonReaderException>()
                .Which.InnerException.Should().NotBeNull();
        }

        [Fact]
        public void SarifJson_RejectsAdditionalJsonContentAsReaderException()
        {
            Action action = () => SarifJson.Deserialize<MappingContainer>("{}{}");

            action.Should().Throw<SarifJsonReaderException>();
        }

        [Fact]
        public void SarifJson_PreservesIso8601PropertyBagStrings()
        {
            const string timestamp = "2026-11-09T12:34:56.7890123Z";
            string json =
                "{\"version\":\"2.1.0\",\"runs\":[{\"tool\":{\"driver\":{\"name\":\"Test\"}},\"results\":[{\"message\":{\"text\":\"Test\"},\"properties\":{\"timestamp\":\"" +
                timestamp +
                "\"}}]}]}";

            SarifLog log = SarifJson.Deserialize<SarifLog>(json);

            log.Runs[0].Results[0].GetProperty("timestamp").Should().Be(timestamp);
        }

        [Fact]
        public void SarifJson_PreservesHighPrecisionDecimalValues()
        {
            const decimal expected = 0.1234567890123456789012345678m;

            decimal actual = SarifJson.Deserialize<decimal>(
                "0.1234567890123456789012345678");

            actual.Should().Be(expected);
        }

        [Fact]
        public void SarifJson_MapsUnmappableJsonToSerializationException()
        {
            Action action = () => SarifJson.Deserialize<MappingContainer>("{\"Number\":\"not a number\"}");

            action.Should().Throw<SarifJsonSerializationException>()
                .Which.InnerException.Should().NotBeNull();
        }

        [Fact]
        public void SarifJson_MapsUnmappableJsonAfterLeadingCommentToSerializationException()
        {
            Action action = () => SarifJson.Deserialize<MappingContainer>(
                "// comment\n{\"Number\":\"not a number\"}");

            SarifJsonSerializationException error = action.Should()
                .Throw<SarifJsonSerializationException>().Which;

            error.Path.Should().Be("Number");
            error.InnerException.Should().NotBeNull();
        }

        [Fact]
        public void SarifJson_HandledErrorCallbackContinuesDeserialization()
        {
            AssertHandledErrorCallbackContinuesDeserialization("{\"Number\":\"not a number\"}");
        }

        [Fact]
        public void SarifJson_HandledErrorCallbackContinuesAfterLeadingComment()
        {
            AssertHandledErrorCallbackContinuesDeserialization(
                "// comment\n{\"Number\":\"not a number\"}");
        }

        [Fact]
        public void SarifJson_AppliesSerializerNeutralOptions()
        {
            var value = new OptionsContainer { Html = "<p>", Optional = null };
            var options = new SarifJsonOptions
            {
                EscapeHtml = true,
                IgnoreNullValues = true,
                WriteIndented = true
            };

            string json = SarifJson.Serialize(value, options);

            json.Should().Contain(Environment.NewLine);
            json.Should().Contain(@"\u003c");
            json.Should().NotContain("Optional");
        }

        [Fact]
        public void SarifJson_EscapesHtmlInSarifPropertyBagValues()
        {
            var result = new Result
            {
                Message = new Message { Text = "Test" }
            };
            result.SetProperty("payload", new
            {
                Value = "</script>",
                Number = 0.1234567890123456789012345678m
            });

            string json = SarifJson.Serialize(
                result,
                new SarifJsonOptions { EscapeHtml = true });

            json.Should().NotContain("</script>");
            json.Should().Contain(@"\u003c/script\u003e");
            json.Should().Contain("0.1234567890123456789012345678");
        }

        [Fact]
        public void SarifJson_PreservesSarifUriTextForConsumerTypes()
        {
            var value = new UriContainer
            {
                Uri = new Uri("file:///C:/dir1/dir2/../file.c", UriKind.Absolute)
            };

            string json = SarifJson.Serialize(value);
            UriContainer roundTrippedValue = SarifJson.Deserialize<UriContainer>(json);

            json.Should().Contain("dir2/../file.c");
            roundTrippedValue.Uri.OriginalString.Should().Be(value.Uri.OriginalString);
        }

        [Fact]
        public void SarifJson_TextWriterOverloadLeavesWriterOpen()
        {
            var writer = new StringWriter();

            SarifJson.Serialize(writer, new OptionsContainer { Html = "value" });

            writer.Write("still open");
            writer.ToString().Should().EndWith("still open");
        }

        [Fact]
        public void SarifJson_RoundTripsFile()
        {
            string filePath = Path.GetTempFileName();

            try
            {
                var expected = new MappingContainer { Number = 42 };

                SarifJson.SerializeToFile(filePath, expected);
                MappingContainer actual = SarifJson.DeserializeFromFile<MappingContainer>(filePath);

                actual.Should().BeEquivalentTo(expected);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public void SarifJson_UpdatesPrereleaseLogsWithoutSerializerTypes()
        {
            const string prereleaseLog =
                "{\"version\":\"2.1.0\",\"$schema\":\"http://json.schemastore.org/sarif-2.1.0-rtm.0\",\"runs\":[]}";

            SarifLog log = SarifJson.UpdateToCurrentVersion(
                prereleaseLog,
                writeIndented: true,
                out string updatedLog);

            log.Version.Should().Be(SarifVersion.Current);
            updatedLog.Should().Contain(Environment.NewLine);
        }

        [Fact]
        public void SerializerNeutralTypesDoNotExposeNewtonsoftJson()
        {
            Type[] neutralTypes =
            {
                typeof(SarifJson),
                typeof(SarifJsonOptions),
                typeof(SarifJsonErrorEventArgs),
                typeof(SarifJsonException),
                typeof(SarifJsonReaderException),
                typeof(SarifJsonSerializationException),
                typeof(ResultLogWriter)
            };

            foreach (Type type in neutralTypes)
            {
                foreach (System.Reflection.MemberInfo member in type.GetMembers())
                {
                    member.ToString().Should().NotContain(
                        "Newtonsoft.Json",
                        $"{type.FullName}.{member.Name} is part of the serializer-neutral API");
                }
            }
        }

        private static void AssertHandledErrorCallbackContinuesDeserialization(string json)
        {
            SarifJsonErrorEventArgs observedError = null;
            var options = new SarifJsonOptions
            {
                Error = error =>
                {
                    observedError = error;
                    error.Handled = true;
                }
            };

            MappingContainer value = SarifJson.Deserialize<MappingContainer>(
                json,
                options);

            value.Should().NotBeNull();
            value.Number.Should().Be(0);
            observedError.Should().NotBeNull();
            observedError.Path.Should().Be("Number");
        }

        private static void AssertDeserializesCommentedJson(string json)
        {
            MappingContainer expected = Newtonsoft.Json.JsonConvert.DeserializeObject<MappingContainer>(json);
            MappingContainer actual = SarifJson.Deserialize<MappingContainer>(json);

            actual.Should().BeEquivalentTo(expected);
            actual.Number.Should().Be(42);
        }

        private static SarifJsonReaderException AssertRejectsCommentedJson(string json)
        {
            int callbackCount = 0;
            var options = new SarifJsonOptions
            {
                Error = error =>
                {
                    callbackCount++;
                    error.Handled = true;
                }
            };
            Action action = () => SarifJson.Deserialize<MappingContainer>(json, options);

            SarifJsonReaderException exception = action.Should().Throw<SarifJsonReaderException>().Which;

            exception.InnerException.Should().NotBeNull();
            callbackCount.Should().Be(0);
            return exception;
        }

        private sealed class UriContainer
        {
            public Uri Uri { get; set; }
        }

        private sealed class OptionsContainer
        {
            public string Html { get; set; }

            public string Optional { get; set; }
        }

        private sealed class MappingContainer
        {
            public int Number { get; set; }
        }
    }
}

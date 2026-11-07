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
        public void SarifJson_MapsUnmappableJsonToSerializationException()
        {
            Action action = () => SarifJson.Deserialize<MappingContainer>("{\"Number\":\"not a number\"}");

            action.Should().Throw<SarifJsonSerializationException>()
                .Which.InnerException.Should().NotBeNull();
        }

        [Fact]
        public void SarifJson_HandledErrorCallbackContinuesDeserialization()
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
                "{\"Number\":\"not a number\"}",
                options);

            value.Should().NotBeNull();
            value.Number.Should().Be(0);
            observedError.Should().NotBeNull();
            observedError.Path.Should().Be("Number");
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

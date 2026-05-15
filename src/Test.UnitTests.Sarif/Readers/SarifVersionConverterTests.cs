// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Readers;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Readers
{
    /// <summary>
    /// Tests covering the strict-throw behavior added in SDK-F. Before the fix,
    /// <see cref="SarifVersionConverter.ReadJson"/> performed a direct
    /// <c>(string)reader.Value</c> cast, so non-string SARIF <c>version</c> tokens (a
    /// SARIF §3.13.2 violation) escaped as cryptic <see cref="InvalidCastException"/>
    /// ("Unable to cast Double to String") instead of a SARIF-domain diagnostic.
    /// </summary>
    public class SarifVersionConverterTests
    {
        private static SarifVersion Deserialize(string json)
        {
            using var stringReader = new StringReader(json);
            using var jsonReader = new JsonTextReader(stringReader);
            return (SarifVersion)SarifVersionConverter.Instance.ReadJson(
                jsonReader, typeof(SarifVersion), existingValue: null, serializer: new JsonSerializer());
        }

        [Fact]
        public void ReadJson_AcceptsStableSarifVersionString()
        {
            // Sanity: the happy path still produces SarifVersion.Current.
            using var stringReader = new StringReader($"\"{VersionConstants.StableSarifVersion}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // advance to the string token

            object actual = SarifVersionConverter.Instance.ReadJson(
                jsonReader, typeof(SarifVersion), existingValue: null, serializer: new JsonSerializer());

            actual.Should().Be(SarifVersion.Current);
        }

        [Fact]
        public void ReadJson_ThrowsTypedError_ForNumericVersion()
        {
            using var stringReader = new StringReader("2.1");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // advance to the float token

            Action act = () => SarifVersionConverter.Instance.ReadJson(
                jsonReader, typeof(SarifVersion), existingValue: null, serializer: new JsonSerializer());

            act.Should().Throw<JsonSerializationException>()
                .WithMessage("*must be a string per §3.13.2*Float*");
        }

        [Fact]
        public void ReadJson_ThrowsTypedError_ForIntegerVersion()
        {
            using var stringReader = new StringReader("2");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();

            Action act = () => SarifVersionConverter.Instance.ReadJson(
                jsonReader, typeof(SarifVersion), existingValue: null, serializer: new JsonSerializer());

            act.Should().Throw<JsonSerializationException>()
                .WithMessage("*Integer*");
        }

        [Fact]
        public void ReadJson_ThrowsTypedError_ForBooleanVersion()
        {
            using var stringReader = new StringReader("true");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();

            Action act = () => SarifVersionConverter.Instance.ReadJson(
                jsonReader, typeof(SarifVersion), existingValue: null, serializer: new JsonSerializer());

            act.Should().Throw<JsonSerializationException>()
                .WithMessage("*Boolean*");
        }

        [Fact]
        public void ReadJson_ThrowsTypedError_ForNullVersion()
        {
            using var stringReader = new StringReader("null");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();

            Action act = () => SarifVersionConverter.Instance.ReadJson(
                jsonReader, typeof(SarifVersion), existingValue: null, serializer: new JsonSerializer());

            act.Should().Throw<JsonSerializationException>()
                .WithMessage("*Null*");
        }

        [Fact]
        public void ReadJson_ExceptionMessageCitesSpecSection()
        {
            using var stringReader = new StringReader("2.1");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read();

            Action act = () => SarifVersionConverter.Instance.ReadJson(
                jsonReader, typeof(SarifVersion), existingValue: null, serializer: new JsonSerializer());

            act.Should().Throw<JsonSerializationException>()
                .Which.Message.Should().Contain("§3.13.2");
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Readers.SystemTextJson
{
    /// <summary>
    /// System.Text.Json equivalents of the Newtonsoft custom converters under
    /// <c>src/Sarif/Readers/</c>. Each is a faithful behavioral port; the
    /// Newtonsoft originals stay in place so the deferred-load path continues
    /// to work during the v6 transition (issue #3038).
    /// </summary>
    /// <remarks>
    /// Registered globally on <see cref="SarifJson.Options"/> rather than via
    /// per-property <c>[JsonConverter]</c> attributes, mirroring how the
    /// Newtonsoft <see cref="SarifContractResolver"/> binds converters by type.
    /// </remarks>

    // -----------------------------------------------------------------------
    // Uri — preserves OriginalString round-trip; promotes a bare local path
    // ("C:\test\file.c") to its file:// form on write. Port of UriConverter.
    // -----------------------------------------------------------------------
    internal sealed class StjUriConverter : JsonConverter<Uri>
    {
        public override Uri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Null
                ? null
                : new Uri(reader.GetString(), UriKind.RelativeOrAbsolute);

        public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
        {
            if (value == null) { writer.WriteNullValue(); return; }

            bool useAbsoluteUri =
                value.IsAbsoluteUri &&
                value.Scheme.Equals(UriUtilities.FileScheme, StringComparison.Ordinal) &&
                !value.OriginalString.StartsWith(UriUtilities.FileScheme.WithColon(), StringComparison.Ordinal);

            writer.WriteStringValue(useAbsoluteUri ? value.AbsoluteUri : value.OriginalString);
        }
    }

    // -----------------------------------------------------------------------
    // DateTime — SARIF millisecond-precision UTC ISO-8601. Port of
    // DateTimeConverter (which routes through SarifUtilities's format string).
    // -----------------------------------------------------------------------
    internal sealed class StjDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => DateTime.Parse(
                reader.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(
                SarifUtilities.SarifDateTimeFormatMillisecondsPrecision,
                CultureInfo.InvariantCulture));
    }

    // -----------------------------------------------------------------------
    // Version — System.Version <-> "1.2.3.4" string. Port of VersionConverter.
    // -----------------------------------------------------------------------
    internal sealed class StjVersionConverter : JsonConverter<Version>
    {
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Null ? null : new Version(reader.GetString());

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }

    // -----------------------------------------------------------------------
    // SarifVersion — string <-> SarifVersion enum via the existing extension
    // helpers. Port of SarifVersionConverter.
    // -----------------------------------------------------------------------
    internal sealed class StjSarifVersionConverter : JsonConverter<SarifVersion>
    {
        public override SarifVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetString().ConvertToSarifVersion();

        public override void Write(Utf8JsonWriter writer, SarifVersion value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ConvertToText());
    }

    // -----------------------------------------------------------------------
    // Plain enums — camelCase JSON <-> PascalCase enum, with the existing
    // two-letter-abbreviation rule. Port of EnumConverter as a converter
    // factory so it binds to every non-[Flags] enum in the model without an
    // exhaustive whitelist.
    // -----------------------------------------------------------------------
    internal sealed class StjSarifEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsEnum
            && !Attribute.IsDefined(typeToConvert, typeof(FlagsAttribute))
            && typeToConvert != typeof(SarifVersion); // handled by StjSarifVersionConverter

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter)Activator.CreateInstance(
                typeof(StjSarifEnumConverter<>).MakeGenericType(typeToConvert));
    }

    internal sealed class StjSarifEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => (T)Enum.Parse(typeof(T), EnumConverter.ConvertToPascalCase(reader.GetString()));

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => writer.WriteStringValue(EnumConverter.ConvertToCamelCase(value.ToString()));
    }

    // -----------------------------------------------------------------------
    // [Flags] enums — JSON array of camelCase strings <-> bitwise-OR'd enum.
    // Port of FlagsEnumConverter as a converter factory.
    // -----------------------------------------------------------------------
    internal sealed class StjFlagsEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsEnum && Attribute.IsDefined(typeToConvert, typeof(FlagsAttribute));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter)Activator.CreateInstance(
                typeof(StjFlagsEnumConverter<>).MakeGenericType(typeToConvert));
    }

    internal sealed class StjFlagsEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int result = 0;
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected JSON array for [Flags] enum.");
            }

            while (reader.Read() && reader.TokenType == JsonTokenType.String)
            {
                string name = EnumConverter.ConvertToPascalCase(reader.GetString());
                result |= (int)(object)Enum.Parse(typeof(T), name);
            }

            return (T)(object)result;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (string token in value.ToString().Split(','))
            {
                writer.WriteStringValue(EnumConverter.ConvertToCamelCase(token.Trim()));
            }
            writer.WriteEndArray();
        }
    }

    // -----------------------------------------------------------------------
    // BigInteger — Location.Id is System.Numerics.BigInteger (SARIF §3.28.2
    // permits arbitrarily large location ids). Newtonsoft writes it as a raw
    // JSON number; STJ has no built-in converter and would otherwise serialize
    // the struct's public properties. WriteRawValue emits the number without
    // quoting; on read, the token text is parsed back via BigInteger.Parse.
    // -----------------------------------------------------------------------
    internal sealed class StjBigIntegerConverter : JsonConverter<BigInteger>
    {
        public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException("Expected JSON number for BigInteger.");
            }

            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            return BigInteger.Parse(doc.RootElement.GetRawText(), CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
            => writer.WriteRawValue(value.ToString(CultureInfo.InvariantCulture), skipInputValidation: true);
    }

    // -----------------------------------------------------------------------
    // Property bags — the "harder" case from #3038. SerializedPropertyInfo
    // already stores the value as raw JSON text (so live JSON objects don't
    // anchor a parent reference and bloat memory). STJ has the primitives we
    // need: JsonDocument.ParseValue captures arbitrary JSON without binding
    // to a CLR type, JsonElement.GetRawText() yields the raw text, and
    // Utf8JsonWriter.WriteRawValue() emits it back. Port of
    // PropertyBagConverter + SerializedPropertyInfoConverter.
    // -----------------------------------------------------------------------
    internal sealed class StjSerializedPropertyInfoConverter : JsonConverter<SerializedPropertyInfo>
    {
        public override SerializedPropertyInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new SerializedPropertyInfo(null, isString: false);
            }

            bool isString = reader.TokenType == JsonTokenType.String;
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            return new SerializedPropertyInfo(doc.RootElement.GetRawText(), isString);
        }

        public override void Write(Utf8JsonWriter writer, SerializedPropertyInfo value, JsonSerializerOptions options)
        {
            if (value?.SerializedValue == null) { writer.WriteNullValue(); return; }
            writer.WriteRawValue(value.SerializedValue, skipInputValidation: false);
        }
    }

    internal sealed class StjPropertyBagConverter : JsonConverter<IDictionary<string, SerializedPropertyInfo>>
    {
        private static readonly StjSerializedPropertyInfoConverter Inner = new();

        public override bool CanConvert(Type typeToConvert)
            => typeof(IDictionary<string, SerializedPropertyInfo>).IsAssignableFrom(typeToConvert);

        public override IDictionary<string, SerializedPropertyInfo> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) { return null; }
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected JSON object for property bag.");
            }

            var result = new Dictionary<string, SerializedPropertyInfo>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                string name = reader.GetString();
                reader.Read();
                result[name] = Inner.Read(ref reader, typeof(SerializedPropertyInfo), options);
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, SerializedPropertyInfo> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (KeyValuePair<string, SerializedPropertyInfo> kv in value)
            {
                writer.WritePropertyName(kv.Key);
                Inner.Write(writer, kv.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}

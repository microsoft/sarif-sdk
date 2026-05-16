// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class SarifVersionConverter : JsonConverter
    {
        public static readonly SarifVersionConverter Instance = new SarifVersionConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SarifVersion);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            // SARIF §3.13.2: sarifLog.version is a string-valued property whose value MUST be one of the
            // recognized SARIF version texts. Producers that emit version as a number, bool, or null are
            // non-conformant; throw a clean SARIF-domain diagnostic rather than letting a raw
            // InvalidCastException ("Unable to cast Double to String") escape the SDK.
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException(
                    $"SARIF '$schema'/'version' property must be a string per §3.13.2; encountered token type '{reader.TokenType}'" +
                    (reader.Value != null ? $" with value '{reader.Value}'" : "") +
                    $" at line {(reader as IJsonLineInfo)?.LineNumber ?? 0}, position {(reader as IJsonLineInfo)?.LinePosition ?? 0}.");
            }

            string sarifVersionText = (string)reader.Value;
            return sarifVersionText.ConvertToSarifVersion();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            string sarifVersionText = ((SarifVersion)value).ConvertToText();
            writer.WriteRawValue(@"""" + sarifVersionText + @"""");
        }
    }
}

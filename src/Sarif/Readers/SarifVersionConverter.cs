// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class SarifVersionConverter : JsonToSarifVersion
    {
        public static readonly SarifVersionConverter Instance = new SarifVersionConverter();
    }

    public class JsonToSarifVersion : JsonConverter
    {
        public static SarifVersion Read<TRoot>(JsonReader reader, TRoot root)
        {
            return Read(reader);
        }

        public static SarifVersion Read(JsonReader reader)
        {
            string sarifVersionText = (string)reader.Value;
            return sarifVersionText.ConvertToSarifVersion();
        }

        public static void Write(JsonWriter writer, SarifVersion value)
        {
            string sarifVersionText = value.ConvertToText();
            writer.WriteRawValue(@"""" + sarifVersionText + @"""");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SarifVersion);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Read(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Write(writer, (SarifVersion)value);
        }
    }
}

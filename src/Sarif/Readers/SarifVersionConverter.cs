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

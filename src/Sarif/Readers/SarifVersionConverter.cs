// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Microsoft.CodeAnalysis.Sarif.Sdk;
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
            switch ((string)reader.Value)
            {
                case "0.4": return SarifVersion.ZeroDotFour;
            }

            return SarifVersion.Unknown;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch ((SarifVersion)value)
            {
                case SarifVersion.ZeroDotFour: { writer.WriteRawValue(@"""0.4"""); return; }
            }
            writer.WriteRawValue(@"""unknown"""); 
        }
    }
}

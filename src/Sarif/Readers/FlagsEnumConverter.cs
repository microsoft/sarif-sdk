// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class FlagsEnumConverter : JsonConverter
    {
        public static readonly FlagsEnumConverter Instance = new FlagsEnumConverter();

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            SuppressionStates result = SuppressionStates.None;

            // Read start of array
            reader.Read();

            while (reader.TokenType == JsonToken.String)
            {
                string enumName = EnumConverter.ConvertToPascalCase((string)reader.Value);
                result |= (SuppressionStates)Enum.Parse(typeof(SuppressionStates), enumName);
                reader.Read();
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is SuppressionStates))
            {
                writer.WriteValue(value);
                return;
            }

            string flagsEnumValue = value.ToString();

            string[] tokens = flagsEnumValue.Split(',');


            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = EnumConverter.ConvertToCamelCase(tokens[i].Trim());
            }

            writer.WriteRawValue("[\"" + String.Join("\",\"", tokens) + "\"]");
        }
    }
}

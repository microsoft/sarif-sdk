// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

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
            int result = 0;

            // What's happening in this code? We express [Flags] enums in JSON as arrays of
            // strings. On deserialization, we walk the array, locate each string, 
            // and convert it to its equivalent enum value. Because we don't have a strong
            // sense of the destination type, we simply treat the enum values as numbers
            // and OR them together. This number will eventually be unboxed and assigned
            // to the target enum property.

            // Read start of array
            reader.Read();

            while (reader.TokenType == JsonToken.String)
            {
                string enumName = EnumConverter.ConvertToPascalCase((string)reader.Value);
                result |= (int)Enum.Parse(objectType, enumName);
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

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class EnumConverter : JsonConverter
    {
        public static readonly EnumConverter Instance = new EnumConverter();

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = (string)reader.Value;
            return Enum.Parse(objectType, ConvertToPascalCase(value));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {            
            string resultLevelString = value.ToString();

            resultLevelString = ConvertToCamelCase(resultLevelString);

            writer.WriteRawValue("\"" + resultLevelString + "\"");
        }

        internal static string ConvertToCamelCase(string upperCaseName)
        {
            if (upperCaseName.Length == 1)
            {
                return upperCaseName.ToLowerInvariant();
            }

            int prefixCount = IsPrefixedWithTwoLetterWord(upperCaseName) ? 2 : 1;

            return upperCaseName.Substring(0, prefixCount).ToLowerInvariant() + upperCaseName.Substring(prefixCount);
        }

        internal static string ConvertToPascalCase(string camelCaseName)
        {
            if (camelCaseName.Length == 1)
            {
                return camelCaseName.ToUpperInvariant();
            }

            int prefixCount = IsPrefixedWithTwoLetterWord(camelCaseName) ? 2 : 1;

            return camelCaseName.Substring(0, prefixCount).ToUpperInvariant() + camelCaseName.Substring(prefixCount);
        }

        private static bool IsPrefixedWithTwoLetterWord(string name)
        {
            if (name.Length < 2)
            {
                return false;
            }

            bool isPrefixedWithTwoLetterWord = Char.IsUpper(name[0]) == Char.IsUpper(name[1]);

            if (name.Length == 2)
            {
                return isPrefixedWithTwoLetterWord;
            }

            return (Char.IsDigit(name[2]) || Char.IsUpper(name[2]));
        }
    }
}

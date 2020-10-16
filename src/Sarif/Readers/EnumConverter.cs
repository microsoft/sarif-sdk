// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class EnumConverter : JsonConverter
    {
        public static readonly EnumConverter Instance = new EnumConverter();
        public static readonly List<string> LegalTwoLetterWordsList = new List<string>() { "in" };

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            string value = (string)reader.Value;
            return Enum.Parse(objectType, ConvertToPascalCase(value));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

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

            int prefixCount = IsPrefixedWithTwoLetterAbbreviation(upperCaseName) ? 2 : 1;

            return upperCaseName.Substring(0, prefixCount).ToLowerInvariant() + upperCaseName.Substring(prefixCount);
        }

        internal static string ConvertToPascalCase(string camelCaseName)
        {
            if (camelCaseName.Length == 1)
            {
                return camelCaseName.ToUpperInvariant();
            }

            int prefixCount = IsPrefixedWithTwoLetterAbbreviation(camelCaseName) ? 2 : 1;

            return camelCaseName.Substring(0, prefixCount).ToUpperInvariant() + camelCaseName.Substring(prefixCount);
        }

        private static bool IsPrefixedWithTwoLetterAbbreviation(string name)
        {
            if (name.Length < 2)
            {
                return false;
            }

            if (LegalTwoLetterWordsList.Contains(name.Substring(0, 2)))
            {
                return false;
            }

            bool isPrefixedWithTwoLetterWord = char.IsUpper(name[0]) == char.IsUpper(name[1]);

            if (name.Length == 2)
            {
                return isPrefixedWithTwoLetterWord;
            }

            return (char.IsDigit(name[2]) || char.IsUpper(name[2]));
        }
    }
}

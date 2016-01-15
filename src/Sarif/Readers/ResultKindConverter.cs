// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.



namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class ResultKindConverter : JsonConverter
    {
        public static readonly ResultKindConverter Instance = new ResultKindConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ResultKind);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = (string)reader.Value;
            return Enum.Parse(typeof(ResultKind), value.Substring(0, 1).ToUpper() + value.Substring(1));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string resultKindString = value.ToString();

            resultKindString = resultKindString.Substring(0, 1).ToLower() + resultKindString.Substring(1);

            writer.WriteRawValue("\"" + resultKindString + "\"");
        }
    }
}

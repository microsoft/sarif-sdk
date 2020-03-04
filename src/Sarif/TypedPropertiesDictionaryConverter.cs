// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class TypedPropertiesDictionaryConverter : JsonConverter
    {
        public TypedPropertiesDictionaryConverter()
        {
            _versionConverter = new VersionConverter();
        }

        private readonly VersionConverter _versionConverter;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StringSet) ||
                   objectType == typeof(IntegerSet) ||
                   objectType == typeof(PropertiesDictionary) ||
                   objectType == typeof(IDictionary);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray ja;

            if (objectType == typeof(StringSet))
            {
                ja = JArray.Load(reader);
                return new StringSet(ja.Values().Select(token => token.ToString()));
            }
            else if (objectType == typeof(IntegerSet))
            {
                ja = JArray.Load(reader);
                return new IntegerSet(ja.Values().Select(token => int.Parse(token.ToString())));
            }
            else if (objectType == typeof(Version))
            {
                return JsonConvert.DeserializeObject<Version>(reader.ReadAsString(), _versionConverter);
            }

            JObject jo = JObject.Load(reader);
            var result = new PropertiesDictionary();

            foreach (JProperty property in jo.Properties())
            {
                string key = property.Name;
                JToken token = property.Value;

                switch (property.Value.Type)
                {
                    case JTokenType.String:
                    {
                        result[key] = token.ToObject<string>(serializer);
                        break;
                    }
                    case JTokenType.Integer:
                    {
                        result[key] = token.ToObject<int>(serializer);
                        break;
                    }
                    case JTokenType.Boolean:
                    {
                        result[key] = token.ToObject<bool>(serializer);
                        break;
                    }
                    case JTokenType.Object:
                    {
                        result[key] = token.ToObject<PropertiesDictionary>(serializer);
                        break;
                    }
                    case JTokenType.Array:
                    {
                        ja = (JArray)property.Value;
                        if (ja.Children().First().Type == JTokenType.Integer)
                        {
                            result[key] = token.ToObject<IntegerSet>(serializer);
                        }
                        else
                        {
                            Debug.Assert(ja.Children().First().Type == JTokenType.String);
                            result[key] = token.ToObject<StringSet>(serializer);
                        }
                        break;
                    }
                }

            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JArray ja;
            if (value is StringSet)
            {
                StringSet hashSet = (StringSet)value;
                ja = new JArray(hashSet.Select(i => new JValue(i)));
                ja.WriteTo(writer);
            }
            else if (value is IntegerSet)
            {
                IntegerSet hashSet = (IntegerSet)value;
                ja = new JArray(hashSet.Select(i => new JValue(i)));
                ja.WriteTo(writer);
            }
            else
            {
                var dictionary = (IDictionary)value;
                writer.WriteStartObject();

                foreach (string key in dictionary.Keys)
                {
                    writer.WritePropertyName(key);

                    object dictionaryValue = dictionary[key];

                    Type t = typeof(object);
                    if (dictionaryValue is IDictionary ||
                        dictionaryValue is IntegerSet ||
                        dictionaryValue is StringSet)
                    {
                        WriteJson(writer, dictionaryValue, serializer);
                    }
                    else if (dictionaryValue is Version)
                    {
                        serializer.Serialize(writer, dictionaryValue.ToString());
                    }
                    else
                    {
                        serializer.Serialize(writer, dictionaryValue);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }

}

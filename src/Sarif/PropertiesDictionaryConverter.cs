// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class PropertiesDictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StringSet) ||
                   objectType == typeof(IntegerSet) ||
                   objectType == typeof(PropertiesDictionary);
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
                return new IntegerSet(ja.Values().Select(token => Int32.Parse(token.ToString())));
            }

            // We do this to forward the reader past the objet
            JObject.Load(reader);

            var result = new PropertiesDictionary();

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jo;
            if (value is StringSet)
            {
                StringSet hashSet = (StringSet)value;
                jo = new JObject(hashSet.Select(s => new JProperty(s, s)));
                jo.WriteTo(writer);
            }
            else if (value is IntegerSet)
            {
                IntegerSet hashSet = (IntegerSet)value;
                jo = new JObject(hashSet.Select(i => new JProperty(i.ToString(), i)));
                jo.WriteTo(writer);
            }
            else
            {
                var dictionary = (PropertiesDictionary)value;
                writer.WriteStartObject();

                foreach (string key in dictionary.Keys)
                {
                    writer.WritePropertyName(key);

                    object dictionaryValue = dictionary[key];

                    if (dictionaryValue is PropertiesDictionary ||
                        dictionaryValue is IntegerSet ||
                        dictionaryValue is StringSet)
                    {
                        WriteJson(writer, dictionaryValue, serializer);
                    }
                    else
                    {
                        writer.WriteValue(dictionaryValue);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }

}

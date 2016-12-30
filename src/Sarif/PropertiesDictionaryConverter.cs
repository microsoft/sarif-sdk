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
            ja = JArray.Load(reader);

            if (objectType == typeof(StringSet))
            {
                return new StringSet(ja.Values().Select(token => token.ToString()));
            }
            else if (objectType == typeof(IntegerSet))
            {
                return new IntegerSet(ja.Values().Select(token => Int32.Parse(token.ToString())));
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jo;
            if (value is StringSet)
            {
                StringSet hashSet = (StringSet)value;
                jo = new JObject(hashSet.Select(s => new JProperty(s, s)));
            }
            else
            {
                IntegerSet hashSet = (IntegerSet)value;
                jo = new JObject(hashSet.Select(i => new JProperty(i.ToString(), i)));
            }
            jo.WriteTo(writer);
        }
    }

}

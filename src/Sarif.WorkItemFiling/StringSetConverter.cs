// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class StringSetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HashSet<string>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            return new HashSet<string>(jObject.Properties().Select(p => p.Name));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            HashSet<string> hashSet = (HashSet<string>)value;
            JObject jObject = new JObject(hashSet.Select(s => new JProperty(s, s)));
            jObject.WriteTo(writer);
        }
    }
}
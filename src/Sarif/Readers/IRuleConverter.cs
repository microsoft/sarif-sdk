// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class IRuleDictionaryConverter : JsonConverter
    {
        public static readonly IRuleDictionaryConverter Instance = new IRuleDictionaryConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, IRule>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IDictionary<string, IRule> incoming = (IDictionary<string, IRule>)value;
            Dictionary<string, Rule> outgoing = new Dictionary<string, Rule>();

            foreach (string key in incoming.Keys)
            {
                IRule iRule = incoming[key];
                Rule rule = iRule as Rule;

                if (rule != null)
                {
                    outgoing[key] = rule;
                    continue;
                }

                rule = new Rule
                {
                    DefaultLevel = iRule.DefaultLevel,
                    FullDescription = iRule.FullDescription,
                    HelpUri = iRule.HelpUri,
                    Id = iRule.Id,
                    MessageFormats = iRule.MessageFormats,
                    Name = iRule.Name,
                    Properties = iRule.Properties,
                    ShortDescription = iRule.ShortDescription,
                    Tags = iRule.Tags
                };

                outgoing[key] = rule;
            }

            serializer.Serialize(writer, outgoing, typeof(Dictionary<string, Rule>));
        }
    }
}

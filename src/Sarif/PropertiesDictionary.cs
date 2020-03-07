// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Serializable]
    [JsonConverter(typeof(TypedPropertiesDictionaryConverter))]
    public class PropertiesDictionary : TypedPropertiesDictionary<object>
    {
        internal const string DEFAULT_POLICY_NAME = "default";

        public PropertiesDictionary() : this(null) { }

        public PropertiesDictionary(PropertiesDictionary initializer) :
            this(initializer, null)
        {
        }

        public PropertiesDictionary(
            PropertiesDictionary initializer,
            IEqualityComparer<string> comparer)
            : base(initializer, comparer)
        {
        }

        protected PropertiesDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Name { get; set; }

        public virtual T GetProperty<T>(PerLanguageOption<T> setting)
        {
            return GetProperty(setting, cacheDefault: true);
        }

        public virtual T GetProperty<T>(PerLanguageOption<T> setting, bool cacheDefault)

        {
            if (setting == null) { throw new ArgumentNullException(nameof(setting)); }

            PropertiesDictionary properties = GetSettingsContainer(setting, cacheDefault);

            T value;
            if (!properties.TryGetProperty(setting.Name, out value) && setting.DefaultValue != null)
            {
                value = setting.DefaultValue();

                if (cacheDefault) { properties[setting.Name] = value; }
            }
            return value;
        }

        public override void SetProperty(IOption setting, object value)
        {
            SetProperty(setting, value, cacheDescription: false);
        }

        public override void SetProperty(IOption setting, object value, bool cacheDescription)
        {
            SetProperty(setting, value, cacheDescription, true);
        }

        public void SetProperty(IOption setting, object value, bool cacheDescription, bool persistToSettingsContainer)

        {
            if (setting == null) { throw new ArgumentNullException(nameof(setting)); }

            PropertiesDictionary properties = persistToSettingsContainer ? GetSettingsContainer(setting, true) : this;

            if (value == null && properties.ContainsKey(setting.Name))
            {
                properties.Remove(setting.Name);
                return;
            }

            if (cacheDescription)
            {
                SettingNameToDescriptionsMap = SettingNameToDescriptionsMap ?? new Dictionary<string, string>();
                SettingNameToDescriptionsMap[setting.Name] = setting.Description;
            }

            properties[setting.Name] = value;
        }

        internal bool TryGetProperty<T>(string key, out T value)
        {
            value = default(T);

            object result;
            if (this.TryGetValue(key, out result))
            {
                if (result is T)
                {
                    value = (T)result;
                    return true;
                }
                else if (result is JToken)
                {
                    value = ((JToken)result).ToObject<T>();
                    return true;
                }
                return TryConvertFromString((string)result, out value);
            }

            return false;
        }

        private PropertiesDictionary GetSettingsContainer(IOption setting, bool cacheDefault)
        {
            PropertiesDictionary properties = this;

            if (string.IsNullOrEmpty(Name))
            {
                object propertiesObject;
                string featureOptionsName = setting.Feature + ".Options";
                if (!TryGetValue(featureOptionsName, out propertiesObject))
                {
                    properties = new PropertiesDictionary();
                    if (cacheDefault) { this[featureOptionsName] = properties; }
                    properties.Name = featureOptionsName;
                }
                else
                {
                    properties = (PropertiesDictionary)propertiesObject;
                }
            }
            return properties;
        }

        private static bool TryConvertFromString<T>(string source, out T destination)
        {
            destination = default(T);
            if (source == null) { return false; }
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            destination = (T)converter.ConvertFrom(source);
            return destination != null;
        }

        public void SaveToJson(string filePath, bool prettyPrint = true)
        {

            Newtonsoft.Json.Formatting formatting = prettyPrint
                ? Newtonsoft.Json.Formatting.Indented
                : Newtonsoft.Json.Formatting.None;

            var settings = new JsonSerializerSettings
            {
                Formatting = formatting
            };

            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, settings));
        }


        public void LoadFromJson(string filePath)
        {
            PropertiesDictionary properties = JsonConvert.DeserializeObject<PropertiesDictionary>(File.ReadAllText(filePath));
            this.Clear();

            foreach (string key in properties.Keys)
            {
                this[key] = properties[key];
            }
        }

        public void SaveToXml(string filePath)
        {
            using (var writer = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                SaveToXml(writer);
            }
        }

        public void SaveToXml(Stream stream)
        {
            var settings = new XmlWriterSettings { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                this.SavePropertiesToXmlStream(writer, settings, null, SettingNameToDescriptionsMap);
            }
        }

        public void LoadFromXml(string filePath)
        {
            using (var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                LoadFromXml(reader);
            }
        }

        public void LoadFromXml(Stream stream)
        {
            this.Clear();

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null
            };

            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                if (reader.IsStartElement(PropertiesDictionaryExtensionMethods.PROPERTIES_ID))
                {
                    bool isEmpty = reader.IsEmptyElement;
                    this.Clear();

                    // Note: we do not recover the property bag id
                    //       as there is no current product use for the value

                    reader.ReadStartElement(PropertiesDictionaryExtensionMethods.PROPERTIES_ID);

                    this.LoadPropertiesFromXmlStream(reader);
                    if (!isEmpty) { reader.ReadEndElement(); }
                }
            }
        }

        // Current consumers of this data expect that child namespaces
        // will always precede parent namespaces, if also included.
        public static readonly ImmutableArray<string> DefaultNamespaces = new List<string>(
            new string[] {
                "Microsoft.CodeAnalysis.Sarif.",
                "Microsoft.CodeAnalysis.",
                "Microsoft."
            }).ToImmutableArray();
    }
}

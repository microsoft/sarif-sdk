﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Serializable]
    public class PropertyBagDictionary : TypedPropertyBagDictionary<object>
    {
        internal const string DEFAULT_POLICY_NAME = "default";

        public PropertyBagDictionary() : this(null) { }

        public PropertyBagDictionary(PropertyBagDictionary initializer) :
            this(initializer, null)
        {
        }

        public PropertyBagDictionary(
            PropertyBagDictionary initializer,
            IEqualityComparer<string> comparer)
            : base(initializer, comparer)
        {
        }

        protected PropertyBagDictionary(SerializationInfo info, StreamingContext context)
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

            PropertyBagDictionary properties = GetSettingsContainer(setting, cacheDefault);

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
            if (setting == null) { throw new ArgumentNullException(nameof(setting)); }

            PropertyBagDictionary properties = GetSettingsContainer(setting, true);

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
                return TryConvertFromString((string)result, out value);
            }

            return false;
        }

        private PropertyBagDictionary GetSettingsContainer(IOption setting, bool cacheDefault)
        {
            PropertyBagDictionary properties = this;

            if (String.IsNullOrEmpty(Name))
            {
                object propertiesObject;
                string featureOptionsName = setting.Feature + ".Options";
                if (!TryGetValue(featureOptionsName, out propertiesObject))
                {
                    properties = new PropertyBagDictionary();
                    if (cacheDefault) { this[featureOptionsName] = properties; }
                    properties.Name = featureOptionsName;
                }
                else
                {
                    properties = (PropertyBagDictionary)propertiesObject;
                }
            }
            return properties;
        }

        private static bool TryConvertFromString<T>(string source, out T destination)
        {
            destination = default(T);
            if (source == null) return false;
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            destination = (T)converter.ConvertFrom(source);
            return destination != null;
        }

        public void SaveTo(string filePath, string id)
        {
            using (var writer = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                SaveTo(writer, id);
        }

        public void SaveTo(Stream stream, string id)
        {
            var settings = new XmlWriterSettings { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                this.SavePropertyBagToStream(writer, settings, id, SettingNameToDescriptionsMap);
            }
        }

        public void LoadFrom(string filePath)
        {
            using (var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                LoadFrom(reader);
        }

        public void LoadFrom(Stream stream)
        {
            var settings = new XmlReaderSettings
            {
                XmlResolver = null
            };

            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                if (reader.IsStartElement(PropertyBagExtensionMethods.PROPERTIES_ID))
                {
                    bool isEmpty = reader.IsEmptyElement;
                    this.Clear();

                    // Note: we do not recover the property bag id
                    //       as there is no current product use for the value

                    reader.ReadStartElement(PropertyBagExtensionMethods.PROPERTIES_ID);

                    this.LoadPropertiesFromXmlStream(reader);
                    if (!isEmpty) reader.ReadEndElement();
                }
            }
        }

        // Current consumers of this data expect that child namespaces
        // will always precede parent namespaces, if also included.
        public static readonly ImmutableArray<string> DefaultNamespaces = new List<string>(
            new string[] {
                "Microsoft.CodeAnalysis.Options.",
                "Microsoft.CodeAnalysis."
            }).ToImmutableArray();
    }
}

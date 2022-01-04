// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IMarker { }

    [Serializable]
    [JsonConverter(typeof(TypedPropertiesDictionaryConverter))]
    public class TypedPropertiesDictionary<T> : ConcurrentDictionary<string, T>, IMarker where T : new()
    {
        public TypedPropertiesDictionary() : this(null, StringComparer.Ordinal)
        {
        }

        public TypedPropertiesDictionary(PropertiesDictionary initializer, IEqualityComparer<string> comparer) : base(comparer)
        {
            if (initializer != null)
            {
                foreach (string key in initializer.Keys)
                {
                    this[key] = (T)initializer[key];
                }
            }
        }

        public void Add(string key, T value)
        {
            ((IDictionary<string, T>)this).Add(key, value);
        }

        public void Remove(string key)
        {
            ((IDictionary<string, T>)this).Remove(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        protected ConcurrentDictionary<string, string> SettingNameToDescriptionsMap { get; set; }

        public virtual T GetProperty(PerLanguageOption<T> setting)
        {
            return GetProperty(setting, cacheDefault: true);
        }

        public virtual T GetProperty(PerLanguageOption<T> setting, bool cacheDefault)
        {
            if (setting == null) { throw new ArgumentNullException(nameof(setting)); }

            T value;
            if (!base.TryGetValue(setting.Name, out value) && setting.DefaultValue != null)
            {
                value = setting.DefaultValue();

                if (cacheDefault) { this[setting.Name] = value; }
            }
            return value;
        }

        public virtual void SetProperty(IOption setting, T value)
        {
            SetProperty(setting, value, cacheDescription: false);
        }

        public virtual void SetProperty(IOption setting, T value, bool cacheDescription)
        {
            if (setting == null) { throw new ArgumentNullException(nameof(setting)); }

            if (value == null && this.ContainsKey(setting.Name))
            {
                this.TryRemove(setting.Name, out T Value);
                return;
            }

            if (cacheDescription)
            {
                SettingNameToDescriptionsMap ??= new ConcurrentDictionary<string, string>();
                SettingNameToDescriptionsMap[setting.Name] = setting.Description;
            }

            this[setting.Name] = value;
        }
    }
}

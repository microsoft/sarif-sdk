// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Base class for objects that can hold properties of arbitrary types.
    /// </summary>
    public abstract class PropertyBagHolderVersionOne : IPropertyBagHolderVersionOne
    {
        private const string NullValue = "null";

        protected PropertyBagHolderVersionOne()
        {
            Tags = new TagsCollectionVersionOne(this);
        }

        [JsonIgnore]
        public IList<string> PropertyNames
        {
            get
            {
                return Properties != null ? Properties.Keys.ToList() : new List<string>();
            }
        }

        /// <summary>
        /// Key/value pairs that provide additional information about the run.
        /// </summary>
        [JsonConverter(typeof(PropertyBagConverter))]
        [JsonProperty("properties", DefaultValueHandling = DefaultValueHandling.Ignore)]
        internal virtual IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        public bool TryGetProperty(string propertyName, out string value)
        {
            if (Properties != null && Properties.Keys.Contains(propertyName))
            {
                value = GetProperty(propertyName);
                return true;
            }

            value = null;
            return false;
        }

        public string GetProperty(string propertyName)
        {
            if (!PropertyNames.Contains(propertyName))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.PropertyDoesNotExist,
                        propertyName));
            }

            if (!Properties[propertyName].IsString)
            {
                throw new InvalidOperationException(SdkResources.CallGenericGetProperty);
            }

            string value = Properties[propertyName].SerializedValue;

            // Remove the quotes around the serialized value ("x" => x) -- unless it's null.
            return value.Equals(NullValue, StringComparison.Ordinal)
                ? null
                : value.Substring(1, value.Length - 2);
        }

        public bool TryGetProperty<T>(string propertyName, out T value)
        {
            if (Properties != null && Properties.Keys.Contains(propertyName))
            {
                value = GetProperty<T>(propertyName);
                return true;
            }

            value = default(T);
            return false;
        }

        public T GetProperty<T>(string propertyName)
        {
            if (!PropertyNames.Contains(propertyName))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.PropertyDoesNotExist,
                        propertyName));
            }

            return JsonConvert.DeserializeObject<T>(Properties[propertyName].SerializedValue);
        }

        public void SetProperty<T>(string propertyName, T value)
        {
            if (Properties == null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>();
            }

            bool isString = typeof(T) == typeof(string);

            string serializedValue;
            if (value == null)
            {
                serializedValue = NullValue;
            }
            else
            {
                serializedValue = isString
                    ? JsonConvert.ToString(value)
                    : JsonConvert.SerializeObject(value);
            }

            Properties[propertyName] = new SerializedPropertyInfo(serializedValue, isString);
        }

        public void SetPropertiesFrom(IPropertyBagHolderVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            // We need the concrete class because the IPropertyBagHolderVersionOne interface
            // doesn't expose the raw Properties array.
            PropertyBagHolderVersionOne otherHolder = other as PropertyBagHolderVersionOne;
            Debug.Assert(otherHolder != null);

            Properties = other.PropertyNames.Count > 0 ? new Dictionary<string, SerializedPropertyInfo>() : null;

            foreach (string propertyName in other.PropertyNames)
            {
                SerializedPropertyInfo otherInfo = otherHolder.Properties[propertyName];
                Properties[propertyName] = new SerializedPropertyInfo(otherInfo.SerializedValue, otherInfo.IsString);
            }
        }

        public void RemoveProperty(string propertyName)
        {
            if (Properties != null)
            {
                Properties.Remove(propertyName);
            }
        }

        [JsonIgnore]
        public TagsCollectionVersionOne Tags { get; }
    }
}

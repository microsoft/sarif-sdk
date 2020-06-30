// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Base class for objects that can hold properties of arbitrary types.
    /// </summary>
    public class PropertyBagHolder : IPropertyBagHolder
    {
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
            value = null;
            SerializedPropertyInfo propValue = null;

            if (Properties?.TryGetValue(propertyName, out propValue) ?? false)
            {
                if (propValue == null) { return true; }

                if (!propValue.IsString)
                {
                    throw new InvalidOperationException(SdkResources.CallGenericGetProperty);
                }

                // Remove the quotes around the serialized value ("x" => x).
                value = propValue.SerializedValue.Substring(1, propValue.SerializedValue.Length - 2);
                return true;
            }

            return false;
        }

        public string GetProperty(string propertyName)
        {
            string value;

            if (!TryGetProperty(propertyName, out value))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.PropertyDoesNotExist,
                        propertyName));
            }

            return value;
        }

        public bool TryGetProperty<T>(string propertyName, out T value)
        {
            value = default;

            SerializedPropertyInfo propValue = null;
            if (Properties?.TryGetValue(propertyName, out propValue) ?? false)
            {
                if (propValue == null)
                {
                    if (typeof(T).IsValueType)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                SdkResources.PropertyOfValueTypeCannotBeNull,
                                propertyName,
                                typeof(T).FullName));
                    }

                    // This will return null for reference types. Could not set null here b/c T could be a value type as well.
                    value = default;
                }
                else
                {
                    value = JsonConvert.DeserializeObject<T>(propValue.SerializedValue);
                }

                return true;
            }

            return false;
        }

        public T GetProperty<T>(string propertyName)
        {
            T value = default;

            if (!TryGetProperty<T>(propertyName, out value))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.PropertyDoesNotExist,
                        propertyName));
            }

            return value;
        }

        public bool TryGetSerializedPropertyValue(string propertyName, out string serializedValue)
        {
            SerializedPropertyInfo propValue = null;
            if (Properties?.TryGetValue(propertyName, out propValue) ?? false)
            {
                serializedValue = null;
                return false;
            }

            serializedValue = propValue?.SerializedValue;
            return true;
        }

        public string GetSerializedPropertyValue(string propertyName)
        {
            if (!TryGetSerializedPropertyValue(propertyName, out string serializedValue))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.PropertyDoesNotExist,
                        propertyName));
            }

            return serializedValue;
        }

        private static readonly JsonSerializerSettings s_settingsWithComprehensiveV2ContractResolver = new JsonSerializerSettings
        {
            ContractResolver = new SarifContractResolver(),
            Formatting = Formatting.None
        };

        public void SetProperty<T>(string propertyName, T value)
        {
            Properties = Properties ?? new Dictionary<string, SerializedPropertyInfo>();

            bool isString = typeof(T) == typeof(string);

            if (value == null)
            {
                // This is consistent with what the PropertyBagConverter does when it encounters
                // a null-valued property. Whether we create a property bag dictionary entry
                // by deserializing a null from the log file or by calling SetProperty("aProp", null),
                // the internal representation is the same: a null value in the Properties
                // dictionary.
                Properties[propertyName] = null;
            }
            else
            {
                string serializedValue;

                if (isString)
                {
                    serializedValue = JsonConvert.ToString(value);
                }
                else
                {
                    // Use the appropriate serializer settings
                    JsonSerializerSettings settings = null;

                    if (propertyName.StartsWith("sarifv1/"))
                    {
                        settings = SarifTransformerUtilities.JsonSettingsV1Compact;
                    }
                    else if (propertyName.StartsWith("sarifv2/"))
                    {
                        settings = s_settingsWithComprehensiveV2ContractResolver;
                    }

                    serializedValue = JsonConvert.SerializeObject(value, settings);
                }

                Properties[propertyName] = new SerializedPropertyInfo(serializedValue, isString);
            }
        }

        public void SetPropertiesFrom(IPropertyBagHolder other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            // We need the concrete class because the IPropertyBagHolder interface
            // doesn't expose the raw Properties array.
            PropertyBagHolder otherHolder = other as PropertyBagHolder;
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
            Properties?.Remove(propertyName);
        }

        [JsonIgnore]
        public TagsCollection Tags => new TagsCollection(this);

        public static bool PropertyBagHasAtLeastOneNonNullValue(IDictionary<string, SerializedPropertyInfo> properties)
        {
            return properties != null && properties.Any();
        }
    }
}

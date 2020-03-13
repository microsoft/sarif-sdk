// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    /// <summary>
    ///  TrustMap determines how much to trust different Result attributes for matching.
    ///  
    ///  Trust is the product of how often values match and how unique the values were.
    ///  An attribute which always has the same value will have very low trust.
    ///  An attribute which never matches will have very low trust.
    /// </summary>
    internal class TrustMap
    {
        public const float DefaultTrust = 0.1f;
        public bool WasMatched { get; private set; }
        private readonly Dictionary<TrustKey, TrustValue> _map;

        public TrustMap()
        {
            _map = new Dictionary<TrustKey, TrustValue>();
        }

        /// <summary>
        ///  Return the matching trust of a given property.
        /// </summary>
        /// <param name="propertySetName">Set name (Base, PartialFingerprints, Properties, etc)</param>
        /// <param name="propertyName">Property Name (valueFingerprint/v1)</param>
        /// <returns>Value between zero and one indicating how much to trust this property for matching</returns>
        public float Trust(string propertySetName, string propertyName)
        {
            if (_map.TryGetValue(new TrustKey(propertySetName, propertyName), out TrustValue value))
            {
                // No trust for constant values - more than one occurrence but only one value
                if (value.UseCount > 1 && value.UniqueValues.Count == 1) { return 0; }

                // Unique values but no matches defaults to low trust; no matches can occur with useful attributes

                if (WasMatched)
                {
                    // Simplification of (Match / Unique) * (Unique / Use)
                    // The product of how many values match and how many unique values there were
                    return Math.Max(DefaultTrust, ((float)value.MatchCount) / (float)(value.UseCount));
                }
                else
                {
                    return Math.Max(DefaultTrust, ((float)value.UniqueValues.Count) / (float)(value.UseCount));
                }
            }

            // Unknown
            return DefaultTrust;
        }

        /// <summary>
        ///  Add a Result attribute to the map.
        /// </summary>
        /// <param name="component">WhatComponent for a Result attribute used in baselining</param>
        public void Add(WhatComponent component)
        {
            TrustKey key = new TrustKey(component);

            TrustValue value = null;
            if (!_map.TryGetValue(key, out value))
            {
                value = new TrustValue();
                _map[key] = value;
            }

            value.Add(component);
        }

        /// <summary>
        ///  Compare two TrustMaps to determine how many of the found values per property matched,
        ///  to incorporate that into trust scoring.
        /// </summary>
        /// <param name="otherRunMap">TrustMap for the other Run we're baselining against</param>
        public void CountMatchesWith(TrustMap otherRunMap)
        {
            foreach (TrustKey key in _map.Keys)
            {
                int matchCount = 0;

                TrustValue ourValue = _map[key];
                if (otherRunMap._map.TryGetValue(key, out TrustValue theirValue))
                {
                    foreach (string value in ourValue.UniqueValues)
                    {
                        if (theirValue.UniqueValues.Contains(value))
                        {
                            matchCount++;
                        }
                    }

                    theirValue.MatchCount = matchCount;
                }

                ourValue.MatchCount = matchCount;
            }

            this.WasMatched = true;
            otherRunMap.WasMatched = true;
        }

        private class TrustValue
        {
            public int UseCount { get; set; }
            public int MatchCount { get; set; }
            public HashSet<string> UniqueValues { get; set; }

            public TrustValue()
            {
                UniqueValues = new HashSet<string>();
            }

            public void Add(WhatComponent component)
            {
                this.UseCount++;
                this.UniqueValues.Add(component.PropertyValue);
            }
        }

        private struct TrustKey
        {
            public string PropertySet { get; set; }
            public string PropertyName { get; set; }

            public TrustKey(WhatComponent component) : this(component.PropertySet, component.PropertyName)
            { }

            public TrustKey(string propertySet, string propertyName)
            {
                this.PropertySet = propertySet;
                this.PropertyName = propertyName;
            }

            public bool Equals(TrustKey other)
            {
                return string.Equals(this.PropertySet, other.PropertySet)
                    && string.Equals(this.PropertyName, other.PropertyName);
            }

            public override bool Equals(object obj)
            {
                if (obj is TrustKey)
                {
                    return Equals((TrustKey)obj);
                }

                return false;
            }

            public override int GetHashCode()
            {
                int hashCode = 17;

                unchecked
                {
                    if (this.PropertySet != null)
                    {
                        hashCode = hashCode * 31 + this.PropertySet.GetHashCode();
                    }

                    if (this.PropertyName != null)
                    {
                        hashCode = hashCode * 31 + this.PropertyName.GetHashCode();
                    }
                }

                return hashCode;
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    internal class WhatComponent : IEquatable<WhatComponent>
    {
        public string Category { get; }
        public string PropertySet { get; }
        public string PropertyName { get; }
        public string PropertyValue { get; }

        public WhatComponent(string category, string propertySet, string propertyName, string propertyValue)
        {
            this.Category = category;
            this.PropertySet = propertySet;
            this.PropertyName = propertyName;
            this.PropertyValue = propertyValue;
        }

        public override string ToString()
        {
            return $"{Category} | {PropertySet} | {PropertyName} | {PropertyValue}";
        }

        public bool Equals(WhatComponent other)
        {
            if (other == null) { return false; }

            return string.Equals(this.PropertyValue, other.PropertyValue)
                && string.Equals(this.PropertyName, other.PropertyName)
                && string.Equals(this.PropertySet, other.PropertySet)
                && string.Equals(this.Category, other.Category);
        }

        public override bool Equals(object obj)
        {
            return Equals(this as WhatComponent);
        }

        public override int GetHashCode()
        {
            int hashCode = 17;

            unchecked
            {
                if (this.Category != null)
                {
                    hashCode = hashCode * 31 + this.Category.GetHashCode();
                }

                if (this.PropertySet != null)
                {
                    hashCode = hashCode * 31 + this.PropertySet.GetHashCode();
                }

                if (this.PropertyName != null)
                {
                    hashCode = hashCode * 31 + this.PropertyName.GetHashCode();
                }

                if (this.PropertyValue != null)
                {
                    hashCode = hashCode * 31 + this.PropertyValue.GetHashCode();
                }
            }

            return hashCode;
        }
    }
}

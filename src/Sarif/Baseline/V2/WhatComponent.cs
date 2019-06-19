using System;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    internal class WhatComponent : IEquatable<WhatComponent>
    {
        public string Category { get; set; }
        public string PropertySet { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }

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
            int hashCode = 0;

            hashCode *= 13;
            hashCode = this.Category?.GetHashCode() ?? 0;

            hashCode *= 13;
            hashCode = this.PropertySet?.GetHashCode() ?? 0;

            hashCode *= 13;
            hashCode = this.PropertyName?.GetHashCode() ?? 0;

            hashCode *= 13;
            hashCode = this.PropertyValue?.GetHashCode() ?? 0;

            return hashCode;
        }
    }
}

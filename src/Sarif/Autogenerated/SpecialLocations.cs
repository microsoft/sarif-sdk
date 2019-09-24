// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines locations of special significance to SARIF consumers.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class SpecialLocations : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<SpecialLocations> ValueComparer => SpecialLocationsEqualityComparer.Instance;

        public bool ValueEquals(SpecialLocations other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.SpecialLocations;
            }
        }

        /// <summary>
        /// Provides a suggestion to SARIF consumers to display file paths relative to the specified location.
        /// </summary>
        [DataMember(Name = "displayBase", IsRequired = false, EmitDefaultValue = false)]
        public virtual ArtifactLocation DisplayBase { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the special locations.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialLocations" /> class.
        /// </summary>
        public SpecialLocations()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialLocations" /> class from the supplied values.
        /// </summary>
        /// <param name="displayBase">
        /// An initialization value for the <see cref="P:DisplayBase" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public SpecialLocations(ArtifactLocation displayBase, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(displayBase, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialLocations" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public SpecialLocations(SpecialLocations other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.DisplayBase, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual SpecialLocations DeepClone()
        {
            return (SpecialLocations)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new SpecialLocations(this);
        }

        protected virtual void Init(ArtifactLocation displayBase, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (displayBase != null)
            {
                DisplayBase = new ArtifactLocation(displayBase);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
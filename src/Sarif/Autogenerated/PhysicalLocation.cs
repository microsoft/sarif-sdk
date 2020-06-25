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
    /// A physical location relevant to a result. Specifies a reference to a programming artifact together with a range of bytes or characters within that artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class PhysicalLocation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<PhysicalLocation> ValueComparer => PhysicalLocationEqualityComparer.Instance;

        public bool ValueEquals(PhysicalLocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.PhysicalLocation;
            }
        }

        /// <summary>
        /// The address of the location.
        /// </summary>
        [DataMember(Name = "address", IsRequired = false, EmitDefaultValue = false)]
        public virtual Address Address { get; set; }

        /// <summary>
        /// The location of the artifact.
        /// </summary>
        [DataMember(Name = "artifactLocation", IsRequired = false, EmitDefaultValue = false)]
        public virtual ArtifactLocation ArtifactLocation { get; set; }

        /// <summary>
        /// Specifies a portion of the artifact.
        /// </summary>
        [DataMember(Name = "region", IsRequired = false, EmitDefaultValue = false)]
        public virtual Region Region { get; set; }

        /// <summary>
        /// Specifies a portion of the artifact that encloses the region. Allows a viewer to display additional context around the region.
        /// </summary>
        [DataMember(Name = "contextRegion", IsRequired = false, EmitDefaultValue = false)]
        public virtual Region ContextRegion { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the physical location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocation" /> class.
        /// </summary>
        public PhysicalLocation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocation" /> class from the supplied values.
        /// </summary>
        /// <param name="address">
        /// An initialization value for the <see cref="P:Address" /> property.
        /// </param>
        /// <param name="artifactLocation">
        /// An initialization value for the <see cref="P:ArtifactLocation" /> property.
        /// </param>
        /// <param name="region">
        /// An initialization value for the <see cref="P:Region" /> property.
        /// </param>
        /// <param name="contextRegion">
        /// An initialization value for the <see cref="P:ContextRegion" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public PhysicalLocation(Address address, ArtifactLocation artifactLocation, Region region, Region contextRegion, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(address, artifactLocation, region, contextRegion, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public PhysicalLocation(PhysicalLocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Address, other.ArtifactLocation, other.Region, other.ContextRegion, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual PhysicalLocation DeepClone()
        {
            return (PhysicalLocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new PhysicalLocation(this);
        }

        protected virtual void Init(Address address, ArtifactLocation artifactLocation, Region region, Region contextRegion, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (address != null)
            {
                Address = new Address(address);
            }

            if (artifactLocation != null)
            {
                ArtifactLocation = new ArtifactLocation(artifactLocation);
            }

            if (region != null)
            {
                Region = new Region(region);
            }

            if (contextRegion != null)
            {
                ContextRegion = new Region(contextRegion);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
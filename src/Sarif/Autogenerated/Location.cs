// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A location within a programming artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class Location : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Location> ValueComparer => LocationEqualityComparer.Instance;

        public bool ValueEquals(Location other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Location;
            }
        }

        /// <summary>
        /// Identifies the artifact and region.
        /// </summary>
        [DataMember(Name = "physicalLocation", IsRequired = false, EmitDefaultValue = false)]
        public PhysicalLocation PhysicalLocation { get; set; }

        /// <summary>
        /// The human-readable fully qualified name of the logical location. If run.logicalLocations is present, this value matches a property name within that object, from which further information about the logical location can be obtained.
        /// </summary>
        [DataMember(Name = "fullyQualifiedLogicalName", IsRequired = false, EmitDefaultValue = false)]
        public string FullyQualifiedLogicalName { get; set; }

        /// <summary>
        /// The index within the logical locations array of the logical location associated with the result.
        /// </summary>
        [DataMember(Name = "logicalLocationIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int LogicalLocationIndex { get; set; }

        /// <summary>
        /// A message relevant to the location.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public Message Message { get; set; }

        /// <summary>
        /// A set of regions relevant to the location.
        /// </summary>
        [DataMember(Name = "annotations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<Region> Annotations { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location" /> class.
        /// </summary>
        public Location()
        {
            LogicalLocationIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location" /> class from the supplied values.
        /// </summary>
        /// <param name="physicalLocation">
        /// An initialization value for the <see cref="P:PhysicalLocation" /> property.
        /// </param>
        /// <param name="fullyQualifiedLogicalName">
        /// An initialization value for the <see cref="P:FullyQualifiedLogicalName" /> property.
        /// </param>
        /// <param name="logicalLocationIndex">
        /// An initialization value for the <see cref="P:LogicalLocationIndex" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P:Message" /> property.
        /// </param>
        /// <param name="annotations">
        /// An initialization value for the <see cref="P:Annotations" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Location(PhysicalLocation physicalLocation, string fullyQualifiedLogicalName, int logicalLocationIndex, Message message, IEnumerable<Region> annotations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(physicalLocation, fullyQualifiedLogicalName, logicalLocationIndex, message, annotations, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Location(Location other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.PhysicalLocation, other.FullyQualifiedLogicalName, other.LogicalLocationIndex, other.Message, other.Annotations, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Location DeepClone()
        {
            return (Location)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Location(this);
        }

        private void Init(PhysicalLocation physicalLocation, string fullyQualifiedLogicalName, int logicalLocationIndex, Message message, IEnumerable<Region> annotations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (physicalLocation != null)
            {
                PhysicalLocation = new PhysicalLocation(physicalLocation);
            }

            FullyQualifiedLogicalName = fullyQualifiedLogicalName;
            LogicalLocationIndex = logicalLocationIndex;
            if (message != null)
            {
                Message = new Message(message);
            }

            if (annotations != null)
            {
                var destination_0 = new List<Region>();
                foreach (var value_0 in annotations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Region(value_0));
                    }
                }

                Annotations = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
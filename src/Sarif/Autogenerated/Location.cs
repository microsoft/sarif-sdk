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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Location : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Location> ValueComparer => LocationEqualityComparer.Instance;

        public bool ValueEquals(Location other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Location;
            }
        }

        /// <summary>
        /// Value that distinguishes this location from all other locations within a single result object.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int Id { get; set; }

        /// <summary>
        /// Identifies the artifact and region.
        /// </summary>
        [DataMember(Name = "physicalLocation", IsRequired = false, EmitDefaultValue = false)]
        public virtual PhysicalLocation PhysicalLocation { get; set; }

        /// <summary>
        /// The logical locations associated with the result.
        /// </summary>
        [DataMember(Name = "logicalLocations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<LogicalLocation> LogicalLocations { get; set; }

        /// <summary>
        /// A message relevant to the location.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Message { get; set; }

        /// <summary>
        /// A set of regions relevant to the location.
        /// </summary>
        [DataMember(Name = "annotations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<Region> Annotations { get; set; }

        /// <summary>
        /// An array of objects that describe relationships between this location and others.
        /// </summary>
        [DataMember(Name = "relationships", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<LocationRelationship> Relationships { get; set; }

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
            Id = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="physicalLocation">
        /// An initialization value for the <see cref="P:PhysicalLocation" /> property.
        /// </param>
        /// <param name="logicalLocations">
        /// An initialization value for the <see cref="P:LogicalLocations" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P:Message" /> property.
        /// </param>
        /// <param name="annotations">
        /// An initialization value for the <see cref="P:Annotations" /> property.
        /// </param>
        /// <param name="relationships">
        /// An initialization value for the <see cref="P:Relationships" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Location(int id, PhysicalLocation physicalLocation, IEnumerable<LogicalLocation> logicalLocations, Message message, IEnumerable<Region> annotations, IEnumerable<LocationRelationship> relationships, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, physicalLocation, logicalLocations, message, annotations, relationships, properties);
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

            Init(other.Id, other.PhysicalLocation, other.LogicalLocations, other.Message, other.Annotations, other.Relationships, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Location DeepClone()
        {
            return (Location)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Location(this);
        }

        protected virtual void Init(int id, PhysicalLocation physicalLocation, IEnumerable<LogicalLocation> logicalLocations, Message message, IEnumerable<Region> annotations, IEnumerable<LocationRelationship> relationships, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            if (physicalLocation != null)
            {
                PhysicalLocation = new PhysicalLocation(physicalLocation);
            }

            if (logicalLocations != null)
            {
                var destination_0 = new List<LogicalLocation>();
                foreach (var value_0 in logicalLocations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new LogicalLocation(value_0));
                    }
                }

                LogicalLocations = destination_0;
            }

            if (message != null)
            {
                Message = new Message(message);
            }

            if (annotations != null)
            {
                var destination_1 = new List<Region>();
                foreach (var value_1 in annotations)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new Region(value_1));
                    }
                }

                Annotations = destination_1;
            }

            if (relationships != null)
            {
                var destination_2 = new List<LocationRelationship>();
                foreach (var value_2 in relationships)
                {
                    if (value_2 == null)
                    {
                        destination_2.Add(null);
                    }
                    else
                    {
                        destination_2.Add(new LocationRelationship(value_2));
                    }
                }

                Relationships = destination_2;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
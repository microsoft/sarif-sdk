// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Information about the relation of one location to another.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class LocationRelationship : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<LocationRelationship> ValueComparer => LocationRelationshipEqualityComparer.Instance;

        public bool ValueEquals(LocationRelationship other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.LocationRelationship;
            }
        }

        /// <summary>
        /// A reference to the related location.
        /// </summary>
        [DataMember(Name = "target", IsRequired = true)]
        public virtual int Target { get; set; }

        /// <summary>
        /// A set of distinct strings that categorize the relationship. Well-known kinds include 'includes', 'isIncludedBy' and 'relevant'.
        /// </summary>
        [DataMember(Name = "kinds", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<string> Kinds { get; set; }

        /// <summary>
        /// A description of the location relationship.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Description { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the location relationship.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationRelationship" /> class.
        /// </summary>
        public LocationRelationship()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationRelationship" /> class from the supplied values.
        /// </summary>
        /// <param name="target">
        /// An initialization value for the <see cref="P:Target" /> property.
        /// </param>
        /// <param name="kinds">
        /// An initialization value for the <see cref="P:Kinds" /> property.
        /// </param>
        /// <param name="description">
        /// An initialization value for the <see cref="P:Description" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public LocationRelationship(int target, IEnumerable<string> kinds, Message description, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(target, kinds, description, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationRelationship" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public LocationRelationship(LocationRelationship other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Target, other.Kinds, other.Description, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual LocationRelationship DeepClone()
        {
            return (LocationRelationship)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new LocationRelationship(this);
        }

        protected virtual void Init(int target, IEnumerable<string> kinds, Message description, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Target = target;
            if (kinds != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in kinds)
                {
                    destination_0.Add(value_0);
                }

                Kinds = destination_0;
            }

            if (description != null)
            {
                Description = new Message(description);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
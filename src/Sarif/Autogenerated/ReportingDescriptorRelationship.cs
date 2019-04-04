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
    /// Information about the relation of one reporting descriptor to another.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class ReportingDescriptorRelationship : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ReportingDescriptorRelationship> ValueComparer => ReportingDescriptorRelationshipEqualityComparer.Instance;

        public bool ValueEquals(ReportingDescriptorRelationship other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ReportingDescriptorRelationship;
            }
        }

        /// <summary>
        /// A reference to the related reporting descriptor.
        /// </summary>
        [DataMember(Name = "descriptor", IsRequired = true)]
        public ReportingDescriptorReference Descriptor { get; set; }

        /// <summary>
        /// A set of distinct strings that categorize the relationshship. Well-known kinds include canPrecede, canFollow, canPrecedeOrFollow, willPrecede, willFollow, superset, subset, equal, disjoint, relevant, and incomparable.
        /// </summary>
        [DataMember(Name = "kinds", IsRequired = true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<string> Kinds { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the reporting descriptor reference.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorRelationship" /> class.
        /// </summary>
        public ReportingDescriptorRelationship()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorRelationship" /> class from the supplied values.
        /// </summary>
        /// <param name="descriptor">
        /// An initialization value for the <see cref="P:Descriptor" /> property.
        /// </param>
        /// <param name="kinds">
        /// An initialization value for the <see cref="P:Kinds" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ReportingDescriptorRelationship(ReportingDescriptorReference descriptor, IEnumerable<string> kinds, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(descriptor, kinds, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorRelationship" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ReportingDescriptorRelationship(ReportingDescriptorRelationship other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Descriptor, other.Kinds, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ReportingDescriptorRelationship DeepClone()
        {
            return (ReportingDescriptorRelationship)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ReportingDescriptorRelationship(this);
        }

        private void Init(ReportingDescriptorReference descriptor, IEnumerable<string> kinds, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (descriptor != null)
            {
                Descriptor = new ReportingDescriptorReference(descriptor);
            }

            if (kinds != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in kinds)
                {
                    destination_0.Add(value_0);
                }

                Kinds = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
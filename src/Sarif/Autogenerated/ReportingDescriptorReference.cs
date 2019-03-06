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
    /// Information about how to locate a relevant reporting descriptor.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class ReportingDescriptorReference : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ReportingDescriptorReference> ValueComparer => ReportingDescriptorReferenceEqualityComparer.Instance;

        public bool ValueEquals(ReportingDescriptorReference other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ReportingDescriptorReference;
            }
        }

        /// <summary>
        /// A notification identifier.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// A JSON pointer used to retrieve a reporting descriptor from an array within a tool component.
        /// </summary>
        [DataMember(Name = "pointer", IsRequired = false, EmitDefaultValue = false)]
        public string Pointer { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the reporting descriptor reference.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorReference" /> class.
        /// </summary>
        public ReportingDescriptorReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorReference" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="pointer">
        /// An initialization value for the <see cref="P:Pointer" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ReportingDescriptorReference(string id, string pointer, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, pointer, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorReference" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ReportingDescriptorReference(ReportingDescriptorReference other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.Pointer, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ReportingDescriptorReference DeepClone()
        {
            return (ReportingDescriptorReference)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ReportingDescriptorReference(this);
        }

        private void Init(string id, string pointer, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            Pointer = pointer;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
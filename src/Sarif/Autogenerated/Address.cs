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
    /// The effective address of a reported issue.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class Address : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Address> ValueComparer => AddressEqualityComparer.Instance;

        public bool ValueEquals(Address other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Address;
            }
        }

        /// <summary>
        /// A base address rendered as a hexadecimal string.
        /// </summary>
        [DataMember(Name = "baseAddress", IsRequired = false, EmitDefaultValue = false)]
        public int BaseAddress { get; set; }

        /// <summary>
        /// An open-ended string that identifies the address kind. 'section' and 'segment' are well-known values.
        /// </summary>
        [DataMember(Name = "kind", IsRequired = false, EmitDefaultValue = false)]
        public string Kind { get; set; }

        /// <summary>
        /// A name that is associated with the address, e.g., '.text'.
        /// </summary>
        [DataMember(Name = "name", IsRequired = false, EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// A human-readable fully qualified name that is associated with the address.
        /// </summary>
        [DataMember(Name = "fullyQualifiedName", IsRequired = false, EmitDefaultValue = false)]
        public string FullyQualifiedName { get; set; }

        /// <summary>
        /// an offset from the base address, if present, rendered as a hexadecimal string.
        /// </summary>
        [DataMember(Name = "offset", IsRequired = false, EmitDefaultValue = false)]
        public int Offset { get; set; }

        /// <summary>
        /// An index into run.addresses used to retrieve a cached instance to represent the address.
        /// </summary>
        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        public int Index { get; set; }

        /// <summary>
        /// An index into run.addresses to retrieve a parent address. The parent can provide a base address (from which the current offset value is relevant) and other details.
        /// </summary>
        [DataMember(Name = "parentIndex", IsRequired = false, EmitDefaultValue = false)]
        public int ParentIndex { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the address.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Address" /> class.
        /// </summary>
        public Address()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Address" /> class from the supplied values.
        /// </summary>
        /// <param name="baseAddress">
        /// An initialization value for the <see cref="P:BaseAddress" /> property.
        /// </param>
        /// <param name="kind">
        /// An initialization value for the <see cref="P:Kind" /> property.
        /// </param>
        /// <param name="name">
        /// An initialization value for the <see cref="P:Name" /> property.
        /// </param>
        /// <param name="fullyQualifiedName">
        /// An initialization value for the <see cref="P:FullyQualifiedName" /> property.
        /// </param>
        /// <param name="offset">
        /// An initialization value for the <see cref="P:Offset" /> property.
        /// </param>
        /// <param name="index">
        /// An initialization value for the <see cref="P:Index" /> property.
        /// </param>
        /// <param name="parentIndex">
        /// An initialization value for the <see cref="P:ParentIndex" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Address(int baseAddress, string kind, string name, string fullyQualifiedName, int offset, int index, int parentIndex, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(baseAddress, kind, name, fullyQualifiedName, offset, index, parentIndex, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Address" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Address(Address other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.BaseAddress, other.Kind, other.Name, other.FullyQualifiedName, other.Offset, other.Index, other.ParentIndex, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Address DeepClone()
        {
            return (Address)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Address(this);
        }

        private void Init(int baseAddress, string kind, string name, string fullyQualifiedName, int offset, int index, int parentIndex, IDictionary<string, SerializedPropertyInfo> properties)
        {
            BaseAddress = baseAddress;
            Kind = kind;
            Name = name;
            FullyQualifiedName = fullyQualifiedName;
            Offset = offset;
            Index = index;
            ParentIndex = parentIndex;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Sarif.Readers;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A code annotation that consists of single physical location and associated message, used to express code flows through a method, or other locations that are related to a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.32.0.0")]
    public partial class AnnotatedCodeLocation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<AnnotatedCodeLocation> ValueComparer => AnnotatedCodeLocationEqualityComparer.Instance;

        public bool ValueEquals(AnnotatedCodeLocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.AnnotatedCodeLocation;
            }
        }

        /// <summary>
        /// An identifier for the location, unique within the scope of the code flow within which it occurs.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// A code location to which this annotation refers.
        /// </summary>
        [DataMember(Name = "physicalLocation", IsRequired = true)]
        public PhysicalLocation PhysicalLocation { get; set; }

        /// <summary>
        /// The name of the module that contains the code that is executing.
        /// </summary>
        [DataMember(Name = "module", IsRequired = false, EmitDefaultValue = false)]
        public string Module { get; set; }

        /// <summary>
        /// A message relevant to this annotation.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// A descriptive identifier that categorizes the annotation.
        /// </summary>
        [DataMember(Name = "kind", IsRequired = false, EmitDefaultValue = false)]
        public AnnotatedCodeLocationKind Kind { get; set; }

        /// <summary>
        /// True if this location is essential to understanding the code flow in which it occurs.
        /// </summary>
        [DataMember(Name = "essential", IsRequired = false, EmitDefaultValue = false)]
        public bool Essential { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the code location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocation" /> class.
        /// </summary>
        public AnnotatedCodeLocation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocation" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="physicalLocation">
        /// An initialization value for the <see cref="P: PhysicalLocation" /> property.
        /// </param>
        /// <param name="module">
        /// An initialization value for the <see cref="P: Module" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="kind">
        /// An initialization value for the <see cref="P: Kind" /> property.
        /// </param>
        /// <param name="essential">
        /// An initialization value for the <see cref="P: Essential" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public AnnotatedCodeLocation(string id, PhysicalLocation physicalLocation, string module, string message, AnnotatedCodeLocationKind kind, bool essential, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, physicalLocation, module, message, kind, essential, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public AnnotatedCodeLocation(AnnotatedCodeLocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.PhysicalLocation, other.Module, other.Message, other.Kind, other.Essential, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public AnnotatedCodeLocation DeepClone()
        {
            return (AnnotatedCodeLocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new AnnotatedCodeLocation(this);
        }

        private void Init(string id, PhysicalLocation physicalLocation, string module, string message, AnnotatedCodeLocationKind kind, bool essential, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            if (physicalLocation != null)
            {
                PhysicalLocation = new PhysicalLocation(physicalLocation);
            }

            Module = module;
            Message = message;
            Kind = kind;
            Essential = essential;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
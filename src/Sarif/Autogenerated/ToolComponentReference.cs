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
    /// Identifies a particular toolComponent object, either the driver or an extension.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ToolComponentReference : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ToolComponentReference> ValueComparer => ToolComponentReferenceEqualityComparer.Instance;

        public bool ValueEquals(ToolComponentReference other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ToolComponentReference;
            }
        }

        /// <summary>
        /// The 'name' property of the referenced toolComponent.
        /// </summary>
        [DataMember(Name = "name", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Name { get; set; }

        /// <summary>
        /// An index into the referenced toolComponent in tool.extensions.
        /// </summary>
        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int Index { get; set; }

        /// <summary>
        /// The 'guid' property of the referenced toolComponent.
        /// </summary>
        [DataMember(Name = "guid", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Guid { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the toolComponentReference.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponentReference" /> class.
        /// </summary>
        public ToolComponentReference()
        {
            Index = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponentReference" /> class from the supplied values.
        /// </summary>
        /// <param name="name">
        /// An initialization value for the <see cref="P:Name" /> property.
        /// </param>
        /// <param name="index">
        /// An initialization value for the <see cref="P:Index" /> property.
        /// </param>
        /// <param name="guid">
        /// An initialization value for the <see cref="P:Guid" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ToolComponentReference(string name, int index, string guid, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(name, index, guid, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolComponentReference" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ToolComponentReference(ToolComponentReference other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Name, other.Index, other.Guid, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ToolComponentReference DeepClone()
        {
            return (ToolComponentReference)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ToolComponentReference(this);
        }

        protected virtual void Init(string name, int index, string guid, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Name = name;
            Index = index;
            Guid = guid;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
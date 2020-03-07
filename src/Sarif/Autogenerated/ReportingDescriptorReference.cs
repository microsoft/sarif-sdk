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
    /// Information about how to locate a relevant reporting descriptor.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ReportingDescriptorReference : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ReportingDescriptorReference> ValueComparer => ReportingDescriptorReferenceEqualityComparer.Instance;

        public bool ValueEquals(ReportingDescriptorReference other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ReportingDescriptorReference;
            }
        }

        /// <summary>
        /// The id of the descriptor.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Id { get; set; }

        /// <summary>
        /// The index into an array of descriptors in toolComponent.ruleDescriptors, toolComponent.notificationDescriptors, or toolComponent.taxonomyDescriptors, depending on context.
        /// </summary>
        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int Index { get; set; }

        /// <summary>
        /// A guid that uniquely identifies the descriptor.
        /// </summary>
        [DataMember(Name = "guid", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Guid { get; set; }

        /// <summary>
        /// A reference used to locate the toolComponent associated with the descriptor.
        /// </summary>
        [DataMember(Name = "toolComponent", IsRequired = false, EmitDefaultValue = false)]
        public virtual ToolComponentReference ToolComponent { get; set; }

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
            Index = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDescriptorReference" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="index">
        /// An initialization value for the <see cref="P:Index" /> property.
        /// </param>
        /// <param name="guid">
        /// An initialization value for the <see cref="P:Guid" /> property.
        /// </param>
        /// <param name="toolComponent">
        /// An initialization value for the <see cref="P:ToolComponent" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ReportingDescriptorReference(string id, int index, string guid, ToolComponentReference toolComponent, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, index, guid, toolComponent, properties);
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

            Init(other.Id, other.Index, other.Guid, other.ToolComponent, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ReportingDescriptorReference DeepClone()
        {
            return (ReportingDescriptorReference)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ReportingDescriptorReference(this);
        }

        protected virtual void Init(string id, int index, string guid, ToolComponentReference toolComponent, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            Index = index;
            Guid = guid;
            if (toolComponent != null)
            {
                ToolComponent = new ToolComponentReference(toolComponent);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
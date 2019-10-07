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
    /// Represents a directed edge in a graph.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Edge : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Edge> ValueComparer => EdgeEqualityComparer.Instance;

        public bool ValueEquals(Edge other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Edge;
            }
        }

        /// <summary>
        /// A string that uniquely identifies the edge within its graph.
        /// </summary>
        [DataMember(Name = "id", IsRequired = true)]
        public virtual string Id { get; set; }

        /// <summary>
        /// A short description of the edge.
        /// </summary>
        [DataMember(Name = "label", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Label { get; set; }

        /// <summary>
        /// Identifies the source node (the node at which the edge starts).
        /// </summary>
        [DataMember(Name = "sourceNodeId", IsRequired = true)]
        public virtual string SourceNodeId { get; set; }

        /// <summary>
        /// Identifies the target node (the node at which the edge ends).
        /// </summary>
        [DataMember(Name = "targetNodeId", IsRequired = true)]
        public virtual string TargetNodeId { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the edge.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge" /> class.
        /// </summary>
        public Edge()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="label">
        /// An initialization value for the <see cref="P:Label" /> property.
        /// </param>
        /// <param name="sourceNodeId">
        /// An initialization value for the <see cref="P:SourceNodeId" /> property.
        /// </param>
        /// <param name="targetNodeId">
        /// An initialization value for the <see cref="P:TargetNodeId" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Edge(string id, Message label, string sourceNodeId, string targetNodeId, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, label, sourceNodeId, targetNodeId, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Edge(Edge other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.Label, other.SourceNodeId, other.TargetNodeId, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Edge DeepClone()
        {
            return (Edge)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Edge(this);
        }

        protected virtual void Init(string id, Message label, string sourceNodeId, string targetNodeId, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            if (label != null)
            {
                Label = new Message(label);
            }

            SourceNodeId = sourceNodeId;
            TargetNodeId = targetNodeId;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
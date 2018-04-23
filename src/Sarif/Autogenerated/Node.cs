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
    /// Represents a node in a graph.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class Node : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Node> ValueComparer => NodeEqualityComparer.Instance;

        public bool ValueEquals(Node other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Node;
            }
        }

        /// <summary>
        /// A string that uniquely identifies the node within its graph.
        /// </summary>
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; }

        /// <summary>
        /// A short description of the node.
        /// </summary>
        [DataMember(Name = "label", IsRequired = false, EmitDefaultValue = false)]
        public Message Label { get; set; }

        /// <summary>
        /// A code location associated with the node.
        /// </summary>
        [DataMember(Name = "location", IsRequired = false, EmitDefaultValue = false)]
        public Location Location { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the node.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node" /> class.
        /// </summary>
        public Node()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="label">
        /// An initialization value for the <see cref="P: Label" /> property.
        /// </param>
        /// <param name="location">
        /// An initialization value for the <see cref="P: Location" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public Node(string id, Message label, Location location, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, label, location, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Node(Node other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.Label, other.Location, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Node DeepClone()
        {
            return (Node)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Node(this);
        }

        private void Init(string id, Message label, Location location, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            if (label != null)
            {
                Label = new Message(label);
            }

            if (location != null)
            {
                Location = new Location(location);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
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
    /// A network of nodes and directed edges that describes some aspect of the structure of the code (for example, a call graph).
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Graph : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Graph> ValueComparer => GraphEqualityComparer.Instance;

        public bool ValueEquals(Graph other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Graph;
            }
        }

        /// <summary>
        /// A description of the graph.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Description { get; set; }

        /// <summary>
        /// An array of node objects representing the nodes of the graph.
        /// </summary>
        [DataMember(Name = "nodes", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<Node> Nodes { get; set; }

        /// <summary>
        /// An array of edge objects representing the edges of the graph.
        /// </summary>
        [DataMember(Name = "edges", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<Edge> Edges { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the graph.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph" /> class.
        /// </summary>
        public Graph()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph" /> class from the supplied values.
        /// </summary>
        /// <param name="description">
        /// An initialization value for the <see cref="P:Description" /> property.
        /// </param>
        /// <param name="nodes">
        /// An initialization value for the <see cref="P:Nodes" /> property.
        /// </param>
        /// <param name="edges">
        /// An initialization value for the <see cref="P:Edges" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Graph(Message description, IEnumerable<Node> nodes, IEnumerable<Edge> edges, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(description, nodes, edges, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Graph(Graph other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Description, other.Nodes, other.Edges, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Graph DeepClone()
        {
            return (Graph)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Graph(this);
        }

        protected virtual void Init(Message description, IEnumerable<Node> nodes, IEnumerable<Edge> edges, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (description != null)
            {
                Description = new Message(description);
            }

            if (nodes != null)
            {
                var destination_0 = new List<Node>();
                foreach (var value_0 in nodes)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Node(value_0));
                    }
                }

                Nodes = destination_0;
            }

            if (edges != null)
            {
                var destination_1 = new List<Edge>();
                foreach (var value_1 in edges)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new Edge(value_1));
                    }
                }

                Edges = destination_1;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
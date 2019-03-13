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
    /// Represents a path through a graph.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class GraphTraversal : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<GraphTraversal> ValueComparer => GraphTraversalEqualityComparer.Instance;

        public bool ValueEquals(GraphTraversal other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.GraphTraversal;
            }
        }

        /// <summary>
        /// The index within the run.graphs to be associated with the result.
        /// </summary>
        [DataMember(Name = "graphIndex", IsRequired = false, EmitDefaultValue = false)]
        public int GraphIndex { get; set; }

        /// <summary>
        /// A string that uniquely identifies that graph being traversed.
        /// </summary>
        [DataMember(Name = "graphId", IsRequired = true)]
        public string GraphId { get; set; }

        /// <summary>
        /// A description of this graph traversal.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public Message Description { get; set; }

        /// <summary>
        /// Values of relevant expressions at the start of the graph traversal.
        /// </summary>
        [DataMember(Name = "initialState", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> InitialState { get; set; }

        /// <summary>
        /// The sequences of edges traversed by this graph traversal.
        /// </summary>
        [DataMember(Name = "edgeTraversals", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IList<EdgeTraversal> EdgeTraversals { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the graph traversal.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTraversal" /> class.
        /// </summary>
        public GraphTraversal()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTraversal" /> class from the supplied values.
        /// </summary>
        /// <param name="graphIndex">
        /// An initialization value for the <see cref="P:GraphIndex" /> property.
        /// </param>
        /// <param name="graphId">
        /// An initialization value for the <see cref="P:GraphId" /> property.
        /// </param>
        /// <param name="description">
        /// An initialization value for the <see cref="P:Description" /> property.
        /// </param>
        /// <param name="initialState">
        /// An initialization value for the <see cref="P:InitialState" /> property.
        /// </param>
        /// <param name="edgeTraversals">
        /// An initialization value for the <see cref="P:EdgeTraversals" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public GraphTraversal(int graphIndex, string graphId, Message description, IDictionary<string, string> initialState, IEnumerable<EdgeTraversal> edgeTraversals, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(graphIndex, graphId, description, initialState, edgeTraversals, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTraversal" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public GraphTraversal(GraphTraversal other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.GraphIndex, other.GraphId, other.Description, other.InitialState, other.EdgeTraversals, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public GraphTraversal DeepClone()
        {
            return (GraphTraversal)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new GraphTraversal(this);
        }

        private void Init(int graphIndex, string graphId, Message description, IDictionary<string, string> initialState, IEnumerable<EdgeTraversal> edgeTraversals, IDictionary<string, SerializedPropertyInfo> properties)
        {
            GraphIndex = graphIndex;
            GraphId = graphId;
            if (description != null)
            {
                Description = new Message(description);
            }

            if (initialState != null)
            {
                InitialState = new Dictionary<string, string>(initialState);
            }

            if (edgeTraversals != null)
            {
                var destination_0 = new List<EdgeTraversal>();
                foreach (var value_0 in edgeTraversals)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new EdgeTraversal(value_0));
                    }
                }

                EdgeTraversals = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
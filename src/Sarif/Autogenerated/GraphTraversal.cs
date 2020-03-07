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
    /// Represents a path through a graph.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class GraphTraversal : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<GraphTraversal> ValueComparer => GraphTraversalEqualityComparer.Instance;

        public bool ValueEquals(GraphTraversal other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.GraphTraversal;
            }
        }

        /// <summary>
        /// The index within the run.graphs to be associated with the result.
        /// </summary>
        [DataMember(Name = "runGraphIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int RunGraphIndex { get; set; }

        /// <summary>
        /// The index within the result.graphs to be associated with the result.
        /// </summary>
        [DataMember(Name = "resultGraphIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int ResultGraphIndex { get; set; }

        /// <summary>
        /// A description of this graph traversal.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Description { get; set; }

        /// <summary>
        /// Values of relevant expressions at the start of the graph traversal that may change during graph traversal.
        /// </summary>
        [DataMember(Name = "initialState", IsRequired = false, EmitDefaultValue = false)]
        public virtual IDictionary<string, MultiformatMessageString> InitialState { get; set; }

        /// <summary>
        /// Values of relevant expressions at the start of the graph traversal that remain constant for the graph traversal.
        /// </summary>
        [DataMember(Name = "immutableState", IsRequired = false, EmitDefaultValue = false)]
        public virtual object ImmutableState { get; set; }

        /// <summary>
        /// The sequences of edges traversed by this graph traversal.
        /// </summary>
        [DataMember(Name = "edgeTraversals", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<EdgeTraversal> EdgeTraversals { get; set; }

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
            RunGraphIndex = -1;
            ResultGraphIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTraversal" /> class from the supplied values.
        /// </summary>
        /// <param name="runGraphIndex">
        /// An initialization value for the <see cref="P:RunGraphIndex" /> property.
        /// </param>
        /// <param name="resultGraphIndex">
        /// An initialization value for the <see cref="P:ResultGraphIndex" /> property.
        /// </param>
        /// <param name="description">
        /// An initialization value for the <see cref="P:Description" /> property.
        /// </param>
        /// <param name="initialState">
        /// An initialization value for the <see cref="P:InitialState" /> property.
        /// </param>
        /// <param name="immutableState">
        /// An initialization value for the <see cref="P:ImmutableState" /> property.
        /// </param>
        /// <param name="edgeTraversals">
        /// An initialization value for the <see cref="P:EdgeTraversals" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public GraphTraversal(int runGraphIndex, int resultGraphIndex, Message description, IDictionary<string, MultiformatMessageString> initialState, object immutableState, IEnumerable<EdgeTraversal> edgeTraversals, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(runGraphIndex, resultGraphIndex, description, initialState, immutableState, edgeTraversals, properties);
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

            Init(other.RunGraphIndex, other.ResultGraphIndex, other.Description, other.InitialState, other.ImmutableState, other.EdgeTraversals, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual GraphTraversal DeepClone()
        {
            return (GraphTraversal)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new GraphTraversal(this);
        }

        protected virtual void Init(int runGraphIndex, int resultGraphIndex, Message description, IDictionary<string, MultiformatMessageString> initialState, object immutableState, IEnumerable<EdgeTraversal> edgeTraversals, IDictionary<string, SerializedPropertyInfo> properties)
        {
            RunGraphIndex = runGraphIndex;
            ResultGraphIndex = resultGraphIndex;
            if (description != null)
            {
                Description = new Message(description);
            }

            if (initialState != null)
            {
                InitialState = new Dictionary<string, MultiformatMessageString>();
                foreach (var value_0 in initialState)
                {
                    InitialState.Add(value_0.Key, new MultiformatMessageString(value_0.Value));
                }
            }

            ImmutableState = immutableState;
            if (edgeTraversals != null)
            {
                var destination_0 = new List<EdgeTraversal>();
                foreach (var value_1 in edgeTraversals)
                {
                    if (value_1 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new EdgeTraversal(value_1));
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
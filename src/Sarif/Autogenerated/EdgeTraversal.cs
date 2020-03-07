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
    /// Represents the traversal of a single edge during a graph traversal.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class EdgeTraversal : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<EdgeTraversal> ValueComparer => EdgeTraversalEqualityComparer.Instance;

        public bool ValueEquals(EdgeTraversal other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.EdgeTraversal;
            }
        }

        /// <summary>
        /// Identifies the edge being traversed.
        /// </summary>
        [DataMember(Name = "edgeId", IsRequired = true)]
        public virtual string EdgeId { get; set; }

        /// <summary>
        /// A message to display to the user as the edge is traversed.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Message { get; set; }

        /// <summary>
        /// The values of relevant expressions after the edge has been traversed.
        /// </summary>
        [DataMember(Name = "finalState", IsRequired = false, EmitDefaultValue = false)]
        public virtual IDictionary<string, MultiformatMessageString> FinalState { get; set; }

        /// <summary>
        /// The number of edge traversals necessary to return from a nested graph.
        /// </summary>
        [DataMember(Name = "stepOverEdgeCount", IsRequired = false, EmitDefaultValue = false)]
        public virtual int StepOverEdgeCount { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the edge traversal.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeTraversal" /> class.
        /// </summary>
        public EdgeTraversal()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeTraversal" /> class from the supplied values.
        /// </summary>
        /// <param name="edgeId">
        /// An initialization value for the <see cref="P:EdgeId" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P:Message" /> property.
        /// </param>
        /// <param name="finalState">
        /// An initialization value for the <see cref="P:FinalState" /> property.
        /// </param>
        /// <param name="stepOverEdgeCount">
        /// An initialization value for the <see cref="P:StepOverEdgeCount" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public EdgeTraversal(string edgeId, Message message, IDictionary<string, MultiformatMessageString> finalState, int stepOverEdgeCount, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(edgeId, message, finalState, stepOverEdgeCount, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeTraversal" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public EdgeTraversal(EdgeTraversal other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.EdgeId, other.Message, other.FinalState, other.StepOverEdgeCount, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual EdgeTraversal DeepClone()
        {
            return (EdgeTraversal)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new EdgeTraversal(this);
        }

        protected virtual void Init(string edgeId, Message message, IDictionary<string, MultiformatMessageString> finalState, int stepOverEdgeCount, IDictionary<string, SerializedPropertyInfo> properties)
        {
            EdgeId = edgeId;
            if (message != null)
            {
                Message = new Message(message);
            }

            if (finalState != null)
            {
                FinalState = new Dictionary<string, MultiformatMessageString>();
                foreach (var value_0 in finalState)
                {
                    FinalState.Add(value_0.Key, new MultiformatMessageString(value_0.Value));
                }
            }

            StepOverEdgeCount = stepOverEdgeCount;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
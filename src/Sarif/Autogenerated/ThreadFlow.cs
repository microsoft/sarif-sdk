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
    /// Describes a sequence of code locations that specify a path through a single thread of execution such as an operating system or fiber.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ThreadFlow : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ThreadFlow> ValueComparer => ThreadFlowEqualityComparer.Instance;

        public bool ValueEquals(ThreadFlow other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ThreadFlow;
            }
        }

        /// <summary>
        /// An string that uniquely identifies the threadFlow within the codeFlow in which it occurs.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Id { get; set; }

        /// <summary>
        /// A message relevant to the thread flow.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Message { get; set; }

        /// <summary>
        /// Values of relevant expressions at the start of the thread flow that may change during thread flow execution.
        /// </summary>
        [DataMember(Name = "initialState", IsRequired = false, EmitDefaultValue = false)]
        public virtual object InitialState { get; set; }

        /// <summary>
        /// Values of relevant expressions at the start of the thread flow that remain constant.
        /// </summary>
        [DataMember(Name = "immutableState", IsRequired = false, EmitDefaultValue = false)]
        public virtual object ImmutableState { get; set; }

        /// <summary>
        /// A temporally ordered array of 'threadFlowLocation' objects, each of which describes a location visited by the tool while producing the result.
        /// </summary>
        [DataMember(Name = "locations", IsRequired = true)]
        public virtual IList<ThreadFlowLocation> Locations { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the thread flow.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadFlow" /> class.
        /// </summary>
        public ThreadFlow()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadFlow" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P:Message" /> property.
        /// </param>
        /// <param name="initialState">
        /// An initialization value for the <see cref="P:InitialState" /> property.
        /// </param>
        /// <param name="immutableState">
        /// An initialization value for the <see cref="P:ImmutableState" /> property.
        /// </param>
        /// <param name="locations">
        /// An initialization value for the <see cref="P:Locations" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ThreadFlow(string id, Message message, object initialState, object immutableState, IEnumerable<ThreadFlowLocation> locations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, message, initialState, immutableState, locations, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadFlow" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ThreadFlow(ThreadFlow other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.Message, other.InitialState, other.ImmutableState, other.Locations, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ThreadFlow DeepClone()
        {
            return (ThreadFlow)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ThreadFlow(this);
        }

        protected virtual void Init(string id, Message message, object initialState, object immutableState, IEnumerable<ThreadFlowLocation> locations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            if (message != null)
            {
                Message = new Message(message);
            }

            InitialState = initialState;
            ImmutableState = immutableState;
            if (locations != null)
            {
                var destination_0 = new List<ThreadFlowLocation>();
                foreach (var value_0 in locations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ThreadFlowLocation(value_0));
                    }
                }

                Locations = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
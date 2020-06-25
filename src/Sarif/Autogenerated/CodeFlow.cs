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
    /// A set of threadFlows which together describe a pattern of code execution relevant to detecting a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class CodeFlow : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<CodeFlow> ValueComparer => CodeFlowEqualityComparer.Instance;

        public bool ValueEquals(CodeFlow other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.CodeFlow;
            }
        }

        /// <summary>
        /// A message relevant to the code flow.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Message { get; set; }

        /// <summary>
        /// An array of one or more unique threadFlow objects, each of which describes the progress of a program through a thread of execution.
        /// </summary>
        [DataMember(Name = "threadFlows", IsRequired = true)]
        public virtual IList<ThreadFlow> ThreadFlows { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the code flow.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlow" /> class.
        /// </summary>
        public CodeFlow()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlow" /> class from the supplied values.
        /// </summary>
        /// <param name="message">
        /// An initialization value for the <see cref="P:Message" /> property.
        /// </param>
        /// <param name="threadFlows">
        /// An initialization value for the <see cref="P:ThreadFlows" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public CodeFlow(Message message, IEnumerable<ThreadFlow> threadFlows, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(message, threadFlows, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlow" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public CodeFlow(CodeFlow other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Message, other.ThreadFlows, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual CodeFlow DeepClone()
        {
            return (CodeFlow)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new CodeFlow(this);
        }

        protected virtual void Init(Message message, IEnumerable<ThreadFlow> threadFlows, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (message != null)
            {
                Message = new Message(message);
            }

            if (threadFlows != null)
            {
                var destination_0 = new List<ThreadFlow>();
                foreach (var value_0 in threadFlows)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ThreadFlow(value_0));
                    }
                }

                ThreadFlows = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
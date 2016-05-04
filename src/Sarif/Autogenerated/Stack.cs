// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A call stack that is relevant to a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class Stack : ISarifNode
    {
        public static IEqualityComparer<Stack> ValueComparer => StackEqualityComparer.Instance;

        public bool ValueEquals(Stack other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Stack;
            }
        }

        /// <summary>
        /// A message relevant to this call stack.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// An array of stack frames that represent a sequence of calls, rendered in reverse chronological order, that comprise the call stack.
        /// </summary>
        [DataMember(Name = "frames", IsRequired = true)]
        public IList<StackFrame> Frames { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the stack.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A unique set of strings that provide additional information about the stack.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack" /> class.
        /// </summary>
        public Stack()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack" /> class from the supplied values.
        /// </summary>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="frames">
        /// An initialization value for the <see cref="P: Frames" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public Stack(string message, IEnumerable<StackFrame> frames, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(message, frames, properties, tags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Stack(Stack other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Message, other.Frames, other.Properties, other.Tags);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Stack DeepClone()
        {
            return (Stack)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Stack(this);
        }

        private void Init(string message, IEnumerable<StackFrame> frames, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Message = message;
            if (frames != null)
            {
                var destination_0 = new List<StackFrame>();
                foreach (var value_0 in frames)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new StackFrame(value_0));
                    }
                }

                Frames = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, string>(properties);
            }

            if (tags != null)
            {
                var destination_1 = new List<string>();
                foreach (var value_1 in tags)
                {
                    destination_1.Add(value_1);
                }

                Tags = destination_1;
            }
        }
    }
}
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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.14.0.0")]
    public partial class Stack : ISarifNode, IEquatable<Stack>
    {
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
        /// Key/value pairs that provide additional details about the stack.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A unique set of strings that provide additional information for the stack.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public ISet<string> Tags { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as Stack);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (Message != null)
                {
                    result = (result * 31) + Message.GetHashCode();
                }

                if (Frames != null)
                {
                    foreach (var value_0 in Frames)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }

                if (Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_1 in Properties)
                    {
                        xor_0 ^= value_1.Key.GetHashCode();
                        if (value_1.Value != null)
                        {
                            xor_0 ^= value_1.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (Tags != null)
                {
                    foreach (var value_2 in Tags)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(Stack other)
        {
            if (other == null)
            {
                return false;
            }

            if (Message != other.Message)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Frames, other.Frames))
            {
                if (Frames == null || other.Frames == null)
                {
                    return false;
                }

                if (Frames.Count != other.Frames.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < Frames.Count; ++index_0)
                {
                    if (!Object.Equals(Frames[index_0], other.Frames[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Properties, other.Properties))
            {
                if (Properties == null || other.Properties == null || Properties.Count != other.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in Properties)
                {
                    string value_1;
                    if (!other.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Tags, other.Tags))
            {
                if (Tags == null || other.Tags == null)
                {
                    return false;
                }

                if (!Tags.SetEquals(other.Tags))
                {
                    return false;
                }
            }

            return true;
        }

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
        public Stack(string message, IEnumerable<StackFrame> frames, IDictionary<string, string> properties, ISet<string> tags)
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

        private void Init(string message, IEnumerable<StackFrame> frames, IDictionary<string, string> properties, ISet<string> tags)
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
                var destination_1 = new HashSet<string>();
                foreach (var value_1 in tags)
                {
                    destination_1.Add(value_1);
                }

                Tags = destination_1;
            }
        }
    }
}
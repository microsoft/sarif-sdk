// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// A call stack that is relevant to a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class StackVersionOne : PropertyBagHolder, ISarifNodeVersionOne
    {
        public static IEqualityComparer<StackVersionOne> ValueComparer => StackVersionOneEqualityComparer.Instance;

        public bool ValueEquals(StackVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.StackVersionOne;
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
        public IList<StackFrameVersionOne> Frames { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the stack.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackVersionOne" /> class.
        /// </summary>
        public StackVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackVersionOne" /> class from the supplied values.
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
        public StackVersionOne(string message, IEnumerable<StackFrameVersionOne> frames, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(message, frames, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public StackVersionOne(StackVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Message, other.Frames, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public StackVersionOne DeepClone()
        {
            return (StackVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new StackVersionOne(this);
        }

        private void Init(string message, IEnumerable<StackFrameVersionOne> frames, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Message = message;
            if (frames != null)
            {
                var destination_0 = new List<StackFrameVersionOne>();
                foreach (var value_0 in frames)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new StackFrameVersionOne(value_0));
                    }
                }

                Frames = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
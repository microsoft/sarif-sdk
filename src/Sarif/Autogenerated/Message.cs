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
    /// Encapsulates a message intended to be read by the end user.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Message : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Message> ValueComparer => MessageEqualityComparer.Instance;

        public bool ValueEquals(Message other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Message;
            }
        }

        /// <summary>
        /// A plain text message string.
        /// </summary>
        [DataMember(Name = "text", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Text { get; set; }

        /// <summary>
        /// A Markdown message string.
        /// </summary>
        [DataMember(Name = "markdown", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Markdown { get; set; }

        /// <summary>
        /// The identifier for this message.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Id { get; set; }

        /// <summary>
        /// An array of strings to substitute into the message string.
        /// </summary>
        [DataMember(Name = "arguments", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<string> Arguments { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the message.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class.
        /// </summary>
        public Message()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class from the supplied values.
        /// </summary>
        /// <param name="text">
        /// An initialization value for the <see cref="P:Text" /> property.
        /// </param>
        /// <param name="markdown">
        /// An initialization value for the <see cref="P:Markdown" /> property.
        /// </param>
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="arguments">
        /// An initialization value for the <see cref="P:Arguments" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Message(string text, string markdown, string id, IEnumerable<string> arguments, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(text, markdown, id, arguments, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Message(Message other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Text, other.Markdown, other.Id, other.Arguments, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Message DeepClone()
        {
            return (Message)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Message(this);
        }

        protected virtual void Init(string text, string markdown, string id, IEnumerable<string> arguments, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Text = text;
            Markdown = markdown;
            Id = id;
            if (arguments != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in arguments)
                {
                    destination_0.Add(value_0);
                }

                Arguments = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Encapsulates a message intended to be read by the end user.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.55.0.0")]
    public partial class Message : ISarifNode
    {
        public static IEqualityComparer<Message> ValueComparer => MessageEqualityComparer.Instance;

        public bool ValueEquals(Message other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
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
        public string Text { get; set; }

        /// <summary>
        /// The resource id for a plain text message string.
        /// </summary>
        [DataMember(Name = "messageId", IsRequired = false, EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// A rich text message string.
        /// </summary>
        [DataMember(Name = "richText", IsRequired = false, EmitDefaultValue = false)]
        public string RichText { get; set; }

        /// <summary>
        /// The resource id for a rich text message string.
        /// </summary>
        [DataMember(Name = "richMessageId", IsRequired = false, EmitDefaultValue = false)]
        public string RichMessageId { get; set; }

        /// <summary>
        /// An array of strings to substitute into the message string.
        /// </summary>
        [DataMember(Name = "arguments", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Arguments { get; set; }

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
        /// An initialization value for the <see cref="P: Text" /> property.
        /// </param>
        /// <param name="messageId">
        /// An initialization value for the <see cref="P: MessageId" /> property.
        /// </param>
        /// <param name="richText">
        /// An initialization value for the <see cref="P: RichText" /> property.
        /// </param>
        /// <param name="richMessageId">
        /// An initialization value for the <see cref="P: RichMessageId" /> property.
        /// </param>
        /// <param name="arguments">
        /// An initialization value for the <see cref="P: Arguments" /> property.
        /// </param>
        public Message(string text, string messageId, string richText, string richMessageId, IEnumerable<string> arguments)
        {
            Init(text, messageId, richText, richMessageId, arguments);
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

            Init(other.Text, other.MessageId, other.RichText, other.RichMessageId, other.Arguments);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Message DeepClone()
        {
            return (Message)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Message(this);
        }

        private void Init(string text, string messageId, string richText, string richMessageId, IEnumerable<string> arguments)
        {
            Text = text;
            MessageId = messageId;
            RichText = richText;
            RichMessageId = richMessageId;
            if (arguments != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in arguments)
                {
                    destination_0.Add(value_0);
                }

                Arguments = destination_0;
            }
        }
    }
}
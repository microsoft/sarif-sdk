// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Contains information that can be used to construct a message that describes a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class TemplatedMessage : ISarifNode
    {
        public static IEqualityComparer<TemplatedMessage> ValueComparer => TemplatedMessageEqualityComparer.Instance;

        public bool ValueEquals(TemplatedMessage other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.TemplatedMessage;
            }
        }

        /// <summary>
        /// A string that identifies the message template used to construct the message that describes this result. The value of templateId must correspond to one of the names in the set of name/value pairs contained in the 'messageTemplates' property of the rule object whose 'id' property matches the 'ruleId' property of this result.
        /// </summary>
        [DataMember(Name = "templateId", IsRequired = true)]
        public string TemplateId { get; set; }

        /// <summary>
        /// An array of strings that will be used, in combination with a message template, to construct a result message.
        /// </summary>
        [DataMember(Name = "arguments", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Arguments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatedMessage" /> class.
        /// </summary>
        public TemplatedMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatedMessage" /> class from the supplied values.
        /// </summary>
        /// <param name="templateId">
        /// An initialization value for the <see cref="P: TemplateId" /> property.
        /// </param>
        /// <param name="arguments">
        /// An initialization value for the <see cref="P: Arguments" /> property.
        /// </param>
        public TemplatedMessage(string templateId, IEnumerable<string> arguments)
        {
            Init(templateId, arguments);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatedMessage" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public TemplatedMessage(TemplatedMessage other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.TemplateId, other.Arguments);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public TemplatedMessage DeepClone()
        {
            return (TemplatedMessage)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new TemplatedMessage(this);
        }

        private void Init(string templateId, IEnumerable<string> arguments)
        {
            TemplateId = templateId;
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
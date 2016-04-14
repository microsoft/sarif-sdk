// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Contains information that can be used to construct a formatted message that describes a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.10.0.0")]
    public partial class FormattedMessage : ISarifNode, IEquatable<FormattedMessage>
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.FormattedMessage;
            }
        }

        /// <summary>
        /// A string that identifies the message format used to format the message that describes this result. The value of formatId must correspond to one of the names in the set of name/value pairs contained in the 'messageFormats' property of the rule object whose 'id' property matches the 'ruleId' property of this result.
        /// </summary>
        [DataMember(Name = "formatId", IsRequired = true)]
        public string FormatId { get; set; }

        /// <summary>
        /// An array of strings that will be used, in combination with a message format, to construct a result message.
        /// </summary>
        [DataMember(Name = "arguments", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Arguments { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as FormattedMessage);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (FormatId != null)
                {
                    result = (result * 31) + FormatId.GetHashCode();
                }

                if (Arguments != null)
                {
                    foreach (var value_0 in Arguments)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(FormattedMessage other)
        {
            if (other == null)
            {
                return false;
            }

            if (FormatId != other.FormatId)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Arguments, other.Arguments))
            {
                if (Arguments == null || other.Arguments == null)
                {
                    return false;
                }

                if (Arguments.Count != other.Arguments.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < Arguments.Count; ++index_0)
                {
                    if (Arguments[index_0] != other.Arguments[index_0])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedMessage" /> class.
        /// </summary>
        public FormattedMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedMessage" /> class from the supplied values.
        /// </summary>
        /// <param name="formatId">
        /// An initialization value for the <see cref="P: FormatId" /> property.
        /// </param>
        /// <param name="arguments">
        /// An initialization value for the <see cref="P: Arguments" /> property.
        /// </param>
        public FormattedMessage(string formatId, IEnumerable<string> arguments)
        {
            Init(formatId, arguments);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedMessage" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public FormattedMessage(FormattedMessage other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.FormatId, other.Arguments);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public FormattedMessage DeepClone()
        {
            return (FormattedMessage)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new FormattedMessage(this);
        }

        private void Init(string formatId, IEnumerable<string> arguments)
        {
            FormatId = formatId;
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
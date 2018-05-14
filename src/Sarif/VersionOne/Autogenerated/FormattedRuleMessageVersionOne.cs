// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Contains information that can be used to construct a formatted message that describes a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class FormattedRuleMessageVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<FormattedRuleMessageVersionOne> ValueComparer => FormattedRuleMessageVersionOneEqualityComparer.Instance;

        public bool ValueEquals(FormattedRuleMessageVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.FormattedRuleMessageVersionOne;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedRuleMessageVersionOne" /> class.
        /// </summary>
        public FormattedRuleMessageVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedRuleMessageVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="formatId">
        /// An initialization value for the <see cref="P: FormatId" /> property.
        /// </param>
        /// <param name="arguments">
        /// An initialization value for the <see cref="P: Arguments" /> property.
        /// </param>
        public FormattedRuleMessageVersionOne(string formatId, IEnumerable<string> arguments)
        {
            Init(formatId, arguments);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedRuleMessageVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public FormattedRuleMessageVersionOne(FormattedRuleMessageVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.FormatId, other.Arguments);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public FormattedRuleMessageVersionOne DeepClone()
        {
            return (FormattedRuleMessageVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new FormattedRuleMessageVersionOne(this);
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
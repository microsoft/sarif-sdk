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
    /// A message string or message format string rendered in multiple formats.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class MultiformatMessageString : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<MultiformatMessageString> ValueComparer => MultiformatMessageStringEqualityComparer.Instance;

        public bool ValueEquals(MultiformatMessageString other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.MultiformatMessageString;
            }
        }

        /// <summary>
        /// A plain text message string or format string.
        /// </summary>
        [DataMember(Name = "text", IsRequired = true)]
        public virtual string Text { get; set; }

        /// <summary>
        /// A Markdown message string or format string.
        /// </summary>
        [DataMember(Name = "markdown", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Markdown { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the message.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiformatMessageString" /> class.
        /// </summary>
        public MultiformatMessageString()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiformatMessageString" /> class from the supplied values.
        /// </summary>
        /// <param name="text">
        /// An initialization value for the <see cref="P:Text" /> property.
        /// </param>
        /// <param name="markdown">
        /// An initialization value for the <see cref="P:Markdown" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public MultiformatMessageString(string text, string markdown, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(text, markdown, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiformatMessageString" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public MultiformatMessageString(MultiformatMessageString other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Text, other.Markdown, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual MultiformatMessageString DeepClone()
        {
            return (MultiformatMessageString)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new MultiformatMessageString(this);
        }

        protected virtual void Init(string text, string markdown, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Text = text;
            Markdown = markdown;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
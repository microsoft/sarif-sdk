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
    /// Represents content from an external file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class ArtifactContent : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ArtifactContent> ValueComparer => ArtifactContentEqualityComparer.Instance;

        public bool ValueEquals(ArtifactContent other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ArtifactContent;
            }
        }

        /// <summary>
        /// UTF-8-encoded content from a text file.
        /// </summary>
        [DataMember(Name = "text", IsRequired = false, EmitDefaultValue = false)]
        public string Text { get; set; }

        /// <summary>
        /// MIME Base64-encoded content from a binary file, or from a text file in its original encoding.
        /// </summary>
        [DataMember(Name = "binary", IsRequired = false, EmitDefaultValue = false)]
        public string Binary { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the external file.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactContent" /> class.
        /// </summary>
        public ArtifactContent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactContent" /> class from the supplied values.
        /// </summary>
        /// <param name="text">
        /// An initialization value for the <see cref="P:Text" /> property.
        /// </param>
        /// <param name="binary">
        /// An initialization value for the <see cref="P:Binary" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ArtifactContent(string text, string binary, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(text, binary, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactContent" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ArtifactContent(ArtifactContent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Text, other.Binary, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ArtifactContent DeepClone()
        {
            return (ArtifactContent)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ArtifactContent(this);
        }

        private void Init(string text, string binary, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Text = text;
            Binary = binary;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
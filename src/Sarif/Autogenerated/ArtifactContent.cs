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
    /// Represents the contents of an artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ArtifactContent : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ArtifactContent> ValueComparer => ArtifactContentEqualityComparer.Instance;

        public bool ValueEquals(ArtifactContent other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ArtifactContent;
            }
        }

        /// <summary>
        /// UTF-8-encoded content from a text artifact.
        /// </summary>
        [DataMember(Name = "text", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Text { get; set; }

        /// <summary>
        /// MIME Base64-encoded content from a binary artifact, or from a text artifact in its original encoding.
        /// </summary>
        [DataMember(Name = "binary", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Binary { get; set; }

        /// <summary>
        /// An alternate rendered representation of the artifact (e.g., a decompiled representation of a binary region).
        /// </summary>
        [DataMember(Name = "rendered", IsRequired = false, EmitDefaultValue = false)]
        public virtual MultiformatMessageString Rendered { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the artifact content.
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
        /// <param name="rendered">
        /// An initialization value for the <see cref="P:Rendered" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ArtifactContent(string text, string binary, MultiformatMessageString rendered, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(text, binary, rendered, properties);
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

            Init(other.Text, other.Binary, other.Rendered, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ArtifactContent DeepClone()
        {
            return (ArtifactContent)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ArtifactContent(this);
        }

        protected virtual void Init(string text, string binary, MultiformatMessageString rendered, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Text = text;
            Binary = binary;
            if (rendered != null)
            {
                Rendered = new MultiformatMessageString(rendered);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
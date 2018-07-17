// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Represents content from an external file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.55.0.0")]
    public partial class FileContent : ISarifNode
    {
        public static IEqualityComparer<FileContent> ValueComparer => FileContentEqualityComparer.Instance;

        public bool ValueEquals(FileContent other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.FileContent;
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
        /// Initializes a new instance of the <see cref="FileContent" /> class.
        /// </summary>
        public FileContent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContent" /> class from the supplied values.
        /// </summary>
        /// <param name="text">
        /// An initialization value for the <see cref="P: Text" /> property.
        /// </param>
        /// <param name="binary">
        /// An initialization value for the <see cref="P: Binary" /> property.
        /// </param>
        public FileContent(string text, string binary)
        {
            Init(text, binary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContent" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public FileContent(FileContent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Text, other.Binary);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public FileContent DeepClone()
        {
            return (FileContent)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new FileContent(this);
        }

        private void Init(string text, string binary)
        {
            Text = text;
            Binary = binary;
        }
    }
}
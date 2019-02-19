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
    /// A region within a file where a result was detected.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class Region : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Region> ValueComparer => RegionEqualityComparer.Instance;

        public bool ValueEquals(Region other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Region;
            }
        }

        /// <summary>
        /// The line number of the first character in the region.
        /// </summary>
        [DataMember(Name = "startLine", IsRequired = false, EmitDefaultValue = false)]
        public int StartLine { get; set; }

        /// <summary>
        /// The column number of the first character in the region.
        /// </summary>
        [DataMember(Name = "startColumn", IsRequired = false, EmitDefaultValue = false)]
        public int StartColumn { get; set; }

        /// <summary>
        /// The line number of the last character in the region.
        /// </summary>
        [DataMember(Name = "endLine", IsRequired = false, EmitDefaultValue = false)]
        public int EndLine { get; set; }

        /// <summary>
        /// The column number of the character following the end of the region.
        /// </summary>
        [DataMember(Name = "endColumn", IsRequired = false, EmitDefaultValue = false)]
        public int EndColumn { get; set; }

        /// <summary>
        /// The zero-based offset from the beginning of the file of the first character in the region.
        /// </summary>
        [DataMember(Name = "charOffset", IsRequired = false, EmitDefaultValue = false)]
        public int CharOffset { get; set; }

        /// <summary>
        /// The length of the region in characters.
        /// </summary>
        [DataMember(Name = "charLength", IsRequired = false, EmitDefaultValue = false)]
        public int CharLength { get; set; }

        /// <summary>
        /// The zero-based offset from the beginning of the file of the first byte in the region.
        /// </summary>
        [DataMember(Name = "byteOffset", IsRequired = false, EmitDefaultValue = false)]
        public int ByteOffset { get; set; }

        /// <summary>
        /// The length of the region in bytes.
        /// </summary>
        [DataMember(Name = "byteLength", IsRequired = false, EmitDefaultValue = false)]
        public int ByteLength { get; set; }

        /// <summary>
        /// The portion of the file contents within the specified region.
        /// </summary>
        [DataMember(Name = "snippet", IsRequired = false, EmitDefaultValue = false)]
        public ArtifactContent Snippet { get; set; }

        /// <summary>
        /// A message relevant to the region.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public Message Message { get; set; }

        /// <summary>
        /// Specifies the source language, if any, of the portion of the file specified by the region object.
        /// </summary>
        [DataMember(Name = "sourceLanguage", IsRequired = false, EmitDefaultValue = false)]
        public string SourceLanguage { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the region.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Region" /> class.
        /// </summary>
        public Region()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Region" /> class from the supplied values.
        /// </summary>
        /// <param name="startLine">
        /// An initialization value for the <see cref="P:StartLine" /> property.
        /// </param>
        /// <param name="startColumn">
        /// An initialization value for the <see cref="P:StartColumn" /> property.
        /// </param>
        /// <param name="endLine">
        /// An initialization value for the <see cref="P:EndLine" /> property.
        /// </param>
        /// <param name="endColumn">
        /// An initialization value for the <see cref="P:EndColumn" /> property.
        /// </param>
        /// <param name="charOffset">
        /// An initialization value for the <see cref="P:CharOffset" /> property.
        /// </param>
        /// <param name="charLength">
        /// An initialization value for the <see cref="P:CharLength" /> property.
        /// </param>
        /// <param name="byteOffset">
        /// An initialization value for the <see cref="P:ByteOffset" /> property.
        /// </param>
        /// <param name="byteLength">
        /// An initialization value for the <see cref="P:ByteLength" /> property.
        /// </param>
        /// <param name="snippet">
        /// An initialization value for the <see cref="P:Snippet" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P:Message" /> property.
        /// </param>
        /// <param name="sourceLanguage">
        /// An initialization value for the <see cref="P:SourceLanguage" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Region(int startLine, int startColumn, int endLine, int endColumn, int charOffset, int charLength, int byteOffset, int byteLength, ArtifactContent snippet, Message message, string sourceLanguage, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(startLine, startColumn, endLine, endColumn, charOffset, charLength, byteOffset, byteLength, snippet, message, sourceLanguage, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Region" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Region(Region other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.StartLine, other.StartColumn, other.EndLine, other.EndColumn, other.CharOffset, other.CharLength, other.ByteOffset, other.ByteLength, other.Snippet, other.Message, other.SourceLanguage, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Region DeepClone()
        {
            return (Region)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Region(this);
        }

        private void Init(int startLine, int startColumn, int endLine, int endColumn, int charOffset, int charLength, int byteOffset, int byteLength, ArtifactContent snippet, Message message, string sourceLanguage, IDictionary<string, SerializedPropertyInfo> properties)
        {
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            CharOffset = charOffset;
            CharLength = charLength;
            ByteOffset = byteOffset;
            ByteLength = byteLength;
            if (snippet != null)
            {
                Snippet = new ArtifactContent(snippet);
            }

            if (message != null)
            {
                Message = new Message(message);
            }

            SourceLanguage = sourceLanguage;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
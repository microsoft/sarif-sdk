// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A single artifact. In some cases, this artifact might be nested within another artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Artifact : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Artifact> ValueComparer => ArtifactEqualityComparer.Instance;

        public bool ValueEquals(Artifact other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Artifact;
            }
        }

        /// <summary>
        /// A short description of the artifact.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Description { get; set; }

        /// <summary>
        /// The location of the artifact.
        /// </summary>
        [DataMember(Name = "location", IsRequired = false, EmitDefaultValue = false)]
        public virtual ArtifactLocation Location { get; set; }

        /// <summary>
        /// Identifies the index of the immediate parent of the artifact, if this artifact is nested.
        /// </summary>
        [DataMember(Name = "parentIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int ParentIndex { get; set; }

        /// <summary>
        /// The offset in bytes of the artifact within its containing artifact.
        /// </summary>
        [DataMember(Name = "offset", IsRequired = false, EmitDefaultValue = false)]
        public virtual int Offset { get; set; }

        /// <summary>
        /// The length of the artifact in bytes.
        /// </summary>
        [DataMember(Name = "length", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int Length { get; set; }

        /// <summary>
        /// The role or roles played by the artifact in the analysis.
        /// </summary>
        [DataMember(Name = "roles", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [JsonConverter(typeof(FlagsEnumConverter))]
        public virtual ArtifactRoles Roles { get; set; }

        /// <summary>
        /// The MIME type (RFC 2045) of the artifact.
        /// </summary>
        [DataMember(Name = "mimeType", IsRequired = false, EmitDefaultValue = false)]
        public virtual string MimeType { get; set; }

        /// <summary>
        /// The contents of the artifact.
        /// </summary>
        [DataMember(Name = "contents", IsRequired = false, EmitDefaultValue = false)]
        public virtual ArtifactContent Contents { get; set; }

        /// <summary>
        /// Specifies the encoding for an artifact object that refers to a text file.
        /// </summary>
        [DataMember(Name = "encoding", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Encoding { get; set; }

        /// <summary>
        /// Specifies the source language for any artifact object that refers to a text file that contains source code.
        /// </summary>
        [DataMember(Name = "sourceLanguage", IsRequired = false, EmitDefaultValue = false)]
        public virtual string SourceLanguage { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is the name of a hash function and each of whose values is the hashed value of the artifact produced by the specified hash function.
        /// </summary>
        [DataMember(Name = "hashes", IsRequired = false, EmitDefaultValue = false)]
        public virtual IDictionary<string, string> Hashes { get; set; }

        /// <summary>
        /// The Coordinated Universal Time (UTC) date and time at which the artifact was most recently modified. See "Date/time properties" in the SARIF spec for the required format.
        /// </summary>
        [DataMember(Name = "lastModifiedTimeUtc", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.DateTimeConverter))]
        public virtual DateTime LastModifiedTimeUtc { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the artifact.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Artifact" /> class.
        /// </summary>
        public Artifact()
        {
            ParentIndex = -1;
            Length = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Artifact" /> class from the supplied values.
        /// </summary>
        /// <param name="description">
        /// An initialization value for the <see cref="P:Description" /> property.
        /// </param>
        /// <param name="location">
        /// An initialization value for the <see cref="P:Location" /> property.
        /// </param>
        /// <param name="parentIndex">
        /// An initialization value for the <see cref="P:ParentIndex" /> property.
        /// </param>
        /// <param name="offset">
        /// An initialization value for the <see cref="P:Offset" /> property.
        /// </param>
        /// <param name="length">
        /// An initialization value for the <see cref="P:Length" /> property.
        /// </param>
        /// <param name="roles">
        /// An initialization value for the <see cref="P:Roles" /> property.
        /// </param>
        /// <param name="mimeType">
        /// An initialization value for the <see cref="P:MimeType" /> property.
        /// </param>
        /// <param name="contents">
        /// An initialization value for the <see cref="P:Contents" /> property.
        /// </param>
        /// <param name="encoding">
        /// An initialization value for the <see cref="P:Encoding" /> property.
        /// </param>
        /// <param name="sourceLanguage">
        /// An initialization value for the <see cref="P:SourceLanguage" /> property.
        /// </param>
        /// <param name="hashes">
        /// An initialization value for the <see cref="P:Hashes" /> property.
        /// </param>
        /// <param name="lastModifiedTimeUtc">
        /// An initialization value for the <see cref="P:LastModifiedTimeUtc" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Artifact(Message description, ArtifactLocation location, int parentIndex, int offset, int length, ArtifactRoles roles, string mimeType, ArtifactContent contents, string encoding, string sourceLanguage, IDictionary<string, string> hashes, DateTime lastModifiedTimeUtc, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(description, location, parentIndex, offset, length, roles, mimeType, contents, encoding, sourceLanguage, hashes, lastModifiedTimeUtc, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Artifact" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Artifact(Artifact other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Description, other.Location, other.ParentIndex, other.Offset, other.Length, other.Roles, other.MimeType, other.Contents, other.Encoding, other.SourceLanguage, other.Hashes, other.LastModifiedTimeUtc, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Artifact DeepClone()
        {
            return (Artifact)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Artifact(this);
        }

        protected virtual void Init(Message description, ArtifactLocation location, int parentIndex, int offset, int length, ArtifactRoles roles, string mimeType, ArtifactContent contents, string encoding, string sourceLanguage, IDictionary<string, string> hashes, DateTime lastModifiedTimeUtc, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (description != null)
            {
                Description = new Message(description);
            }

            if (location != null)
            {
                Location = new ArtifactLocation(location);
            }

            ParentIndex = parentIndex;
            Offset = offset;
            Length = length;
            Roles = roles;
            MimeType = mimeType;
            if (contents != null)
            {
                Contents = new ArtifactContent(contents);
            }

            Encoding = encoding;
            SourceLanguage = sourceLanguage;
            if (hashes != null)
            {
                Hashes = new Dictionary<string, string>(hashes);
            }

            LastModifiedTimeUtc = lastModifiedTimeUtc;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
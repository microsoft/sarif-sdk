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
    /// A single file. In some cases, this file might be nested within another file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial class FileData : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<FileData> ValueComparer => FileDataEqualityComparer.Instance;

        public bool ValueEquals(FileData other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.FileData;
            }
        }

        /// <summary>
        /// The location of the file.
        /// </summary>
        [DataMember(Name = "fileLocation", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation FileLocation { get; set; }

        /// <summary>
        /// Identifies the index of the immediate parent of the file, if this file is nested.
        /// </summary>
        [DataMember(Name = "parentIndex", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int ParentIndex { get; set; }

        /// <summary>
        /// The offset in bytes of the file within its containing file.
        /// </summary>
        [DataMember(Name = "offset", IsRequired = false, EmitDefaultValue = false)]
        public int Offset { get; set; }

        /// <summary>
        /// The length of the file in bytes.
        /// </summary>
        [DataMember(Name = "length", IsRequired = false, EmitDefaultValue = false)]
        public int Length { get; set; }

        /// <summary>
        /// The role or roles played by the file in the analysis.
        /// </summary>
        [DataMember(Name = "roles", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [JsonConverter(typeof(FlagsEnumConverter))]
        public FileRoles Roles { get; set; }

        /// <summary>
        /// The MIME type (RFC 2045) of the file.
        /// </summary>
        [DataMember(Name = "mimeType", IsRequired = false, EmitDefaultValue = false)]
        public string MimeType { get; set; }

        /// <summary>
        /// The contents of the file.
        /// </summary>
        [DataMember(Name = "contents", IsRequired = false, EmitDefaultValue = false)]
        public FileContent Contents { get; set; }

        /// <summary>
        /// Specifies the encoding for a file object that refers to a text file.
        /// </summary>
        [DataMember(Name = "encoding", IsRequired = false, EmitDefaultValue = false)]
        public string Encoding { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys is the name of a hash function and each of whose values is the hashed value of the file produced by the specified hash function.
        /// </summary>
        [DataMember(Name = "hashes", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Hashes { get; set; }

        /// <summary>
        /// The Coordinated Universal Time (UTC) date and time at which the file was most recently modified. See "Date/time properties" in the SARIF spec for the required format.
        /// </summary>
        [DataMember(Name = "lastModifiedTimeUtc", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.DateTimeConverter))]
        public DateTime LastModifiedTimeUtc { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the file.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileData" /> class.
        /// </summary>
        public FileData()
        {
            ParentIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileData" /> class from the supplied values.
        /// </summary>
        /// <param name="fileLocation">
        /// An initialization value for the <see cref="P:FileLocation" /> property.
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
        /// <param name="hashes">
        /// An initialization value for the <see cref="P:Hashes" /> property.
        /// </param>
        /// <param name="lastModifiedTimeUtc">
        /// An initialization value for the <see cref="P:LastModifiedTimeUtc" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public FileData(FileLocation fileLocation, int parentIndex, int offset, int length, FileRoles roles, string mimeType, FileContent contents, string encoding, IDictionary<string, string> hashes, DateTime lastModifiedTimeUtc, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(fileLocation, parentIndex, offset, length, roles, mimeType, contents, encoding, hashes, lastModifiedTimeUtc, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileData" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public FileData(FileData other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.FileLocation, other.ParentIndex, other.Offset, other.Length, other.Roles, other.MimeType, other.Contents, other.Encoding, other.Hashes, other.LastModifiedTimeUtc, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public FileData DeepClone()
        {
            return (FileData)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new FileData(this);
        }

        private void Init(FileLocation fileLocation, int parentIndex, int offset, int length, FileRoles roles, string mimeType, FileContent contents, string encoding, IDictionary<string, string> hashes, DateTime lastModifiedTimeUtc, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (fileLocation != null)
            {
                FileLocation = new FileLocation(fileLocation);
            }

            ParentIndex = parentIndex;
            Offset = offset;
            Length = length;
            Roles = roles;
            MimeType = mimeType;
            if (contents != null)
            {
                Contents = new FileContent(contents);
            }

            Encoding = encoding;
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
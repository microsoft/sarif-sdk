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
    /// Provides additional metadata related to translation.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class TranslationMetadata : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<TranslationMetadata> ValueComparer => TranslationMetadataEqualityComparer.Instance;

        public bool ValueEquals(TranslationMetadata other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.TranslationMetadata;
            }
        }

        /// <summary>
        /// The name associated with the translation metadata.
        /// </summary>
        [DataMember(Name = "name", IsRequired = true)]
        public virtual string Name { get; set; }

        /// <summary>
        /// The full name associated with the translation metadata.
        /// </summary>
        [DataMember(Name = "fullName", IsRequired = false, EmitDefaultValue = false)]
        public virtual string FullName { get; set; }

        /// <summary>
        /// A brief description of the translation metadata.
        /// </summary>
        [DataMember(Name = "shortDescription", IsRequired = false, EmitDefaultValue = false)]
        public virtual MultiformatMessageString ShortDescription { get; set; }

        /// <summary>
        /// A comprehensive description of the translation metadata.
        /// </summary>
        [DataMember(Name = "fullDescription", IsRequired = false, EmitDefaultValue = false)]
        public virtual MultiformatMessageString FullDescription { get; set; }

        /// <summary>
        /// The absolute URI from which the translation metadata can be downloaded.
        /// </summary>
        [DataMember(Name = "downloadUri", IsRequired = false, EmitDefaultValue = false)]
        public virtual Uri DownloadUri { get; set; }

        /// <summary>
        /// The absolute URI from which information related to the translation metadata can be downloaded.
        /// </summary>
        [DataMember(Name = "informationUri", IsRequired = false, EmitDefaultValue = false)]
        public virtual Uri InformationUri { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the translation metadata.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMetadata" /> class.
        /// </summary>
        public TranslationMetadata()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMetadata" /> class from the supplied values.
        /// </summary>
        /// <param name="name">
        /// An initialization value for the <see cref="P:Name" /> property.
        /// </param>
        /// <param name="fullName">
        /// An initialization value for the <see cref="P:FullName" /> property.
        /// </param>
        /// <param name="shortDescription">
        /// An initialization value for the <see cref="P:ShortDescription" /> property.
        /// </param>
        /// <param name="fullDescription">
        /// An initialization value for the <see cref="P:FullDescription" /> property.
        /// </param>
        /// <param name="downloadUri">
        /// An initialization value for the <see cref="P:DownloadUri" /> property.
        /// </param>
        /// <param name="informationUri">
        /// An initialization value for the <see cref="P:InformationUri" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public TranslationMetadata(string name, string fullName, MultiformatMessageString shortDescription, MultiformatMessageString fullDescription, Uri downloadUri, Uri informationUri, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(name, fullName, shortDescription, fullDescription, downloadUri, informationUri, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMetadata" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public TranslationMetadata(TranslationMetadata other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Name, other.FullName, other.ShortDescription, other.FullDescription, other.DownloadUri, other.InformationUri, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual TranslationMetadata DeepClone()
        {
            return (TranslationMetadata)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new TranslationMetadata(this);
        }

        protected virtual void Init(string name, string fullName, MultiformatMessageString shortDescription, MultiformatMessageString fullDescription, Uri downloadUri, Uri informationUri, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Name = name;
            FullName = fullName;
            if (shortDescription != null)
            {
                ShortDescription = new MultiformatMessageString(shortDescription);
            }

            if (fullDescription != null)
            {
                FullDescription = new MultiformatMessageString(fullDescription);
            }

            if (downloadUri != null)
            {
                DownloadUri = new Uri(downloadUri.OriginalString, downloadUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            if (informationUri != null)
            {
                InformationUri = new Uri(informationUri.OriginalString, informationUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
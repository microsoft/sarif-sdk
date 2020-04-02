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
    /// Specifies the location of an artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ArtifactLocation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ArtifactLocation> ValueComparer => ArtifactLocationEqualityComparer.Instance;

        public bool ValueEquals(ArtifactLocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ArtifactLocation;
            }
        }

        /// <summary>
        /// A string containing a valid relative or absolute URI.
        /// </summary>
        [DataMember(Name = "uri", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.UriConverter))]
        public virtual Uri Uri { get; set; }

        /// <summary>
        /// A string which indirectly specifies the absolute URI with respect to which a relative URI in the "uri" property is interpreted.
        /// </summary>
        [DataMember(Name = "uriBaseId", IsRequired = false, EmitDefaultValue = false)]
        public virtual string UriBaseId { get; set; }

        /// <summary>
        /// The index within the run artifacts array of the artifact object associated with the artifact location.
        /// </summary>
        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int Index { get; set; }

        /// <summary>
        /// A short description of the artifact location.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Description { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the artifact location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactLocation" /> class.
        /// </summary>
        public ArtifactLocation()
        {
            Index = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactLocation" /> class from the supplied values.
        /// </summary>
        /// <param name="uri">
        /// An initialization value for the <see cref="P:Uri" /> property.
        /// </param>
        /// <param name="uriBaseId">
        /// An initialization value for the <see cref="P:UriBaseId" /> property.
        /// </param>
        /// <param name="index">
        /// An initialization value for the <see cref="P:Index" /> property.
        /// </param>
        /// <param name="description">
        /// An initialization value for the <see cref="P:Description" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ArtifactLocation(Uri uri, string uriBaseId, int index, Message description, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(uri, uriBaseId, index, description, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactLocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ArtifactLocation(ArtifactLocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Uri, other.UriBaseId, other.Index, other.Description, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ArtifactLocation DeepClone()
        {
            return (ArtifactLocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ArtifactLocation(this);
        }

        protected virtual void Init(Uri uri, string uriBaseId, int index, Message description, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            UriBaseId = uriBaseId;
            Index = index;
            if (description != null)
            {
                Description = new Message(description);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Specifies the location of a file.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.56.0.0")]
    public partial class FileLocation : ISarifNode
    {
        public static IEqualityComparer<FileLocation> ValueComparer => FileLocationEqualityComparer.Instance;

        public bool ValueEquals(FileLocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.FileLocation;
            }
        }

        /// <summary>
        /// A string containing a valid relative or absolute URI.
        /// </summary>
        [DataMember(Name = "uri", IsRequired = true)]
        public Uri Uri { get; set; }

        /// <summary>
        /// A string which indirectly specifies the absolute URI with respect to which a relative URI in the "uri" property is interpreted.
        /// </summary>
        [DataMember(Name = "uriBaseId", IsRequired = false, EmitDefaultValue = false)]
        public string UriBaseId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLocation" /> class.
        /// </summary>
        public FileLocation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLocation" /> class from the supplied values.
        /// </summary>
        /// <param name="uri">
        /// An initialization value for the <see cref="P: Uri" /> property.
        /// </param>
        /// <param name="uriBaseId">
        /// An initialization value for the <see cref="P: UriBaseId" /> property.
        /// </param>
        public FileLocation(Uri uri, string uriBaseId)
        {
            Init(uri, uriBaseId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public FileLocation(FileLocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Uri, other.UriBaseId);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public FileLocation DeepClone()
        {
            return (FileLocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new FileLocation(this);
        }

        private void Init(Uri uri, string uriBaseId)
        {
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            UriBaseId = uriBaseId;
        }
    }
}
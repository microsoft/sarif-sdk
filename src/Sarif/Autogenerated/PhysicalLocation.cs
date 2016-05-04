// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// The physical location where a result was detected. Specifies a reference to a programming artifact together with a range of bytes or characters within that artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class PhysicalLocation : ISarifNode
    {
        public static IEqualityComparer<PhysicalLocation> ValueComparer => PhysicalLocationEqualityComparer.Instance;

        public bool ValueEquals(PhysicalLocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.PhysicalLocation;
            }
        }

        /// <summary>
        /// The location of the file as a valid URI.
        /// </summary>
        [DataMember(Name = "uri", IsRequired = true)]
        public Uri Uri { get; set; }

        /// <summary>
        /// The region within the file where the result was detected.
        /// </summary>
        [DataMember(Name = "region", IsRequired = false, EmitDefaultValue = false)]
        public Region Region { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocation" /> class.
        /// </summary>
        public PhysicalLocation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocation" /> class from the supplied values.
        /// </summary>
        /// <param name="uri">
        /// An initialization value for the <see cref="P: Uri" /> property.
        /// </param>
        /// <param name="region">
        /// An initialization value for the <see cref="P: Region" /> property.
        /// </param>
        public PhysicalLocation(Uri uri, Region region)
        {
            Init(uri, region);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public PhysicalLocation(PhysicalLocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Uri, other.Region);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public PhysicalLocation DeepClone()
        {
            return (PhysicalLocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new PhysicalLocation(this);
        }

        private void Init(Uri uri, Region region)
        {
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            if (region != null)
            {
                Region = new Region(region);
            }
        }
    }
}
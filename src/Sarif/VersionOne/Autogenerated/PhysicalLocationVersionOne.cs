// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// A physical location relevant to a result. Specifies a reference to a programming artifact together with a range of bytes or characters within that artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class PhysicalLocationVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<PhysicalLocationVersionOne> ValueComparer => PhysicalLocationVersionOneEqualityComparer.Instance;

        public bool ValueEquals(PhysicalLocationVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.PhysicalLocationVersionOne;
            }
        }

        /// <summary>
        /// The location of the file as a valid URI.
        /// </summary>
        [DataMember(Name = "uri", IsRequired = false, EmitDefaultValue = false)]
        public Uri Uri { get; set; }

        /// <summary>
        /// A string that identifies the conceptual base for the 'uri' property (if it is relative), e.g.,'$(SolutionDir)' or '%SRCROOT%'.
        /// </summary>
        [DataMember(Name = "uriBaseId", IsRequired = false, EmitDefaultValue = false)]
        public string UriBaseId { get; set; }

        /// <summary>
        /// The region within the file where the result was detected.
        /// </summary>
        [DataMember(Name = "region", IsRequired = false, EmitDefaultValue = false)]
        public RegionVersionOne Region { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocationVersionOne" /> class.
        /// </summary>
        public PhysicalLocationVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocationVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="uri">
        /// An initialization value for the <see cref="P: Uri" /> property.
        /// </param>
        /// <param name="uriBaseId">
        /// An initialization value for the <see cref="P: UriBaseId" /> property.
        /// </param>
        /// <param name="region">
        /// An initialization value for the <see cref="P: Region" /> property.
        /// </param>
        public PhysicalLocationVersionOne(Uri uri, string uriBaseId, RegionVersionOne region)
        {
            Init(uri, uriBaseId, region);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocationVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public PhysicalLocationVersionOne(PhysicalLocationVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Uri, other.UriBaseId, other.Region);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public PhysicalLocationVersionOne DeepClone()
        {
            return (PhysicalLocationVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new PhysicalLocationVersionOne(this);
        }

        private void Init(Uri uri, string uriBaseId, RegionVersionOne region)
        {
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            UriBaseId = uriBaseId;
            if (region != null)
            {
                Region = new RegionVersionOne(region);
            }
        }
    }
}
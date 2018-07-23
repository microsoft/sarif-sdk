// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A physical location relevant to a result. Specifies a reference to a programming artifact together with a range of bytes or characters within that artifact.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.56.0.0")]
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
        /// Value that distinguishes this physical location from all other physical locations in this run object.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public int Id { get; set; }

        /// <summary>
        /// The location of the file.
        /// </summary>
        [DataMember(Name = "fileLocation", IsRequired = true)]
        public FileLocation FileLocation { get; set; }

        /// <summary>
        /// The region within the file where the result was detected.
        /// </summary>
        [DataMember(Name = "region", IsRequired = false, EmitDefaultValue = false)]
        public Region Region { get; set; }

        /// <summary>
        /// Specifies a portion of the file that encloses the region. Allows a viewer to display additional context around the region.
        /// </summary>
        [DataMember(Name = "contextRegion", IsRequired = false, EmitDefaultValue = false)]
        public Region ContextRegion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocation" /> class.
        /// </summary>
        public PhysicalLocation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalLocation" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="fileLocation">
        /// An initialization value for the <see cref="P: FileLocation" /> property.
        /// </param>
        /// <param name="region">
        /// An initialization value for the <see cref="P: Region" /> property.
        /// </param>
        /// <param name="contextRegion">
        /// An initialization value for the <see cref="P: ContextRegion" /> property.
        /// </param>
        public PhysicalLocation(int id, FileLocation fileLocation, Region region, Region contextRegion)
        {
            Init(id, fileLocation, region, contextRegion);
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

            Init(other.Id, other.FileLocation, other.Region, other.ContextRegion);
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

        private void Init(int id, FileLocation fileLocation, Region region, Region contextRegion)
        {
            Id = id;
            if (fileLocation != null)
            {
                FileLocation = new FileLocation(fileLocation);
            }

            if (region != null)
            {
                Region = new Region(region);
            }

            if (contextRegion != null)
            {
                ContextRegion = new Region(contextRegion);
            }
        }
    }
}
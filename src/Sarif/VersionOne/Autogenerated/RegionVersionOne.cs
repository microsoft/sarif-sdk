// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// A region within a file where a result was detected.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class RegionVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<RegionVersionOne> ValueComparer => RegionVersionOneEqualityComparer.Instance;

        public bool ValueEquals(RegionVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.RegionVersionOne;
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
        /// The column number of the last character in the region.
        /// </summary>
        [DataMember(Name = "endColumn", IsRequired = false, EmitDefaultValue = false)]
        public int EndColumn { get; set; }

        /// <summary>
        /// The zero-based offset from the beginning of the file of the first byte or character in the region.
        /// </summary>
        [DataMember(Name = "offset", IsRequired = false, EmitDefaultValue = false)]
        public int Offset { get; set; }

        /// <summary>
        /// The length of the region in bytes or characters.
        /// </summary>
        [DataMember(Name = "length", IsRequired = false, EmitDefaultValue = false)]
        public int Length { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionVersionOne" /> class.
        /// </summary>
        public RegionVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="startLine">
        /// An initialization value for the <see cref="P: StartLine" /> property.
        /// </param>
        /// <param name="startColumn">
        /// An initialization value for the <see cref="P: StartColumn" /> property.
        /// </param>
        /// <param name="endLine">
        /// An initialization value for the <see cref="P: EndLine" /> property.
        /// </param>
        /// <param name="endColumn">
        /// An initialization value for the <see cref="P: EndColumn" /> property.
        /// </param>
        /// <param name="offset">
        /// An initialization value for the <see cref="P: Offset" /> property.
        /// </param>
        /// <param name="length">
        /// An initialization value for the <see cref="P: Length" /> property.
        /// </param>
        public RegionVersionOne(int startLine, int startColumn, int endLine, int endColumn, int offset, int length)
        {
            Init(startLine, startColumn, endLine, endColumn, offset, length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public RegionVersionOne(RegionVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.StartLine, other.StartColumn, other.EndLine, other.EndColumn, other.Offset, other.Length);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public RegionVersionOne DeepClone()
        {
            return (RegionVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new RegionVersionOne(this);
        }

        private void Init(int startLine, int startColumn, int endLine, int endColumn, int offset, int length)
        {
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            Offset = offset;
            Length = length;
        }
    }
}
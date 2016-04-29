// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A region within a file where a result was detected.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.19.0.0")]
    public partial class Region : ISarifNode, IEquatable<Region>
    {
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

        public override bool Equals(object other)
        {
            return Equals(other as Region);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                result = (result * 31) + StartLine.GetHashCode();
                result = (result * 31) + StartColumn.GetHashCode();
                result = (result * 31) + EndLine.GetHashCode();
                result = (result * 31) + EndColumn.GetHashCode();
                result = (result * 31) + Offset.GetHashCode();
                result = (result * 31) + Length.GetHashCode();
            }

            return result;
        }

        public bool Equals(Region other)
        {
            if (other == null)
            {
                return false;
            }

            if (StartLine != other.StartLine)
            {
                return false;
            }

            if (StartColumn != other.StartColumn)
            {
                return false;
            }

            if (EndLine != other.EndLine)
            {
                return false;
            }

            if (EndColumn != other.EndColumn)
            {
                return false;
            }

            if (Offset != other.Offset)
            {
                return false;
            }

            if (Length != other.Length)
            {
                return false;
            }

            return true;
        }

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
        public Region(int startLine, int startColumn, int endLine, int endColumn, int offset, int length)
        {
            Init(startLine, startColumn, endLine, endColumn, offset, length);
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

            Init(other.StartLine, other.StartColumn, other.EndLine, other.EndColumn, other.Offset, other.Length);
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
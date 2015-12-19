// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Information about an offset in a block of text.</summary>
    [Serializable]
    public struct OffsetInfo : IEquatable<OffsetInfo>, ISerializable
    {
        private readonly int _columnNumber;
        private readonly int _lineNumber;

        // The Math.Max calls are present because arbitrary bytes can be interpreted as a struct;
        // they maintain the invariants that ColumnNumber >= 0, LineNumber >= 1
        //
        // If profiling indicates that these Max calls are expensive it may make sense to sacrifice
        // some safety by turning them into asserts instead.

        /// <summary>The zero-based index of the column where the offset is located.</summary> 
        public int ColumnNumber { get { return Math.Max(_columnNumber, 0); } }

        /// <summary>The one-based index of the line in the file or string on which the offset is
        /// located.</summary> 
        public int LineNumber { get { return Math.Max(_lineNumber, 1); } }

        /// <summary>Initializes a new instance of the <see cref="OffsetInfo"/> struct.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="columnNumber"/> is
        /// negative or <paramref name="lineNumber"/> is not at least 1.</exception>
        /// <param name="columnNumber">The zero-based index of the column on which an offset is located.</param>
        /// <param name="lineNumber">The one-based index of the line in the file or string.</param>
        public OffsetInfo(int columnNumber, int lineNumber)
        {
            if (columnNumber < 0)
            {
                throw new ArgumentOutOfRangeException("columnNumber", columnNumber, ExceptionStrings.ValueCannotBeNegative);
            }

            if (lineNumber <= 0)
            {
                throw new ArgumentOutOfRangeException("lineNumber", lineNumber, ExceptionStrings.ValueMustBeAtLeastOne);
            }

            _columnNumber = columnNumber;
            _lineNumber = lineNumber;
        }

        /// <summary>Initializes a new instance of the <see cref="OffsetInfo"/> struct.</summary>
        /// <param name="info">The serialization info from which the value shall be deserialized.</param>
        /// <param name="context">The streaming context from which the value shall be deserialized.</param>
        private OffsetInfo(SerializationInfo info, StreamingContext context)
            : this(info.GetInt32("ColumnNumber"), info.GetInt32("lineNumber"))
        { }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public override bool Equals(object obj)
        {
            OffsetInfo? other = obj as OffsetInfo?;
            return other != null
                && this.Equals(other.Value);
        }

        /// <summary>Tests if this <see cref="OffsetInfo"/> is considered equal to another.</summary>
        /// <param name="other">Another object to compare to.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(OffsetInfo other)
        {
            return this.LineNumber == other.LineNumber
                && this.ColumnNumber == other.ColumnNumber;
        }

        /// <summary>Returns a hash code for this object.</summary>
        /// <returns>A hash code for this object.</returns>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.LineNumber);
            hash.Add(this.ColumnNumber);
            return hash.GetHashCode();
        }

        /// <summary>Convert this object into a string representation.</summary>
        /// <returns>A string that represents this object.</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}:{1}", this.LineNumber, this.ColumnNumber);
        }

        /// <summary>Gets object data for serialization.</summary>
        /// <param name="info">The serialization info into which the value shall be serialized.</param>
        /// <param name="context">The streaming context into which the value shall be serialized.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("ColumnNumber", this.ColumnNumber);
            info.AddValue("LineNumber", this.LineNumber);
        }

        /// <summary>Equality operator.</summary>
        /// <param name="lhs">The left hand side.</param>
        /// <param name="rhs">The right hand side.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator ==(OffsetInfo lhs, OffsetInfo rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>Inequality operator.</summary>
        /// <param name="lhs">The left hand side.</param>
        /// <param name="rhs">The right hand side.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator !=(OffsetInfo lhs, OffsetInfo rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}

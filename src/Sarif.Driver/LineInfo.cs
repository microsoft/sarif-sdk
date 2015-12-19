// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>Information about a line of text.</summary>
    public struct LineInfo : IEquatable<LineInfo>
    {
        private readonly int _startOffset;
        private readonly int _lineNumber;

        // The Math.Max calls are present because arbitrary bytes can be interpreted as a struct;
        // they maintain the invariants that ColumnNumber >= 0, LineNumber >= 1
        //
        // If profiling indicates that these Max calls are expensive it may make sense to sacrifice
        // some safety by turning them into asserts instead.

        /// <summary>The zero-based index into a file or string at which a given line starts.</summary> 
        public int StartOffset { get { return Math.Max(_startOffset, 0); } }

        /// <summary>The one-based index of the line in the file or string.</summary> 
        public int LineNumber { get { return Math.Max(_lineNumber, 1); } }

        /// <summary>Initializes a new instance of the <see cref="LineInfo"/> struct.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startOffset"/> is
        /// negative or <paramref name="lineNumber"/> is not at least 1.</exception>
        /// <param name="startOffset">The zero-based index into a file or string at which a given line
        /// starts.</param>
        /// <param name="lineNumber">The one-based index of the line in the file or string.</param>
        public LineInfo(int startOffset, int lineNumber)
        {
            if (startOffset < 0)
            {
                throw new ArgumentOutOfRangeException("startOffset", startOffset, ExceptionStrings.ValueCannotBeNegative);
            }

            if (lineNumber <= 0)
            {
                throw new ArgumentOutOfRangeException("lineNumber", lineNumber, ExceptionStrings.ValueMustBeAtLeastOne);
            }

            _startOffset = startOffset;
            _lineNumber = lineNumber;
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public override bool Equals(object obj)
        {
            LineInfo? other = obj as LineInfo?;
            return other != null
                && this.Equals(other.Value);
        }

        /// <summary>Tests if this <see cref="LineInfo"/> is considered equal to another.</summary>
        /// <param name="other">Another object to compare to.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(LineInfo other)
        {
            return this.LineNumber == other.LineNumber
                && this.StartOffset == other.StartOffset;
        }

        /// <summary>Returns a hash code for this object.</summary>
        /// <returns>A hash code for this object.</returns>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.LineNumber);
            hash.Add(this.StartOffset);
            return hash.GetHashCode();
        }

        /// <summary>Convert this object into a string representation.</summary>
        /// <returns>A string that represents this object.</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "Line {0} starting at {1}", this.LineNumber, this.StartOffset);
        }

        /// <summary>Equality operator.</summary>
        /// <param name="lhs">The left hand side.</param>
        /// <param name="rhs">The right hand side.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator ==(LineInfo lhs, LineInfo rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>Inequality operator.</summary>
        /// <param name="lhs">The left hand side.</param>
        /// <param name="rhs">The right hand side.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator !=(LineInfo lhs, LineInfo rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}

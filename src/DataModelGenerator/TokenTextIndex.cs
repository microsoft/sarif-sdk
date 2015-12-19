// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>A token text that wraps a string and provides facilities used by token parsing.</summary>
    /// <remarks>This class treats strings as the halfopen range [0, Count) to make tokenizing easier
    /// rather than returning -1 sentinel values.</remarks>
    internal class TokenTextIndex : IEquatable<TokenTextIndex>
    {
        private readonly NewLineIndex _newlines;

        /// <summary>A <see cref="TokenTextIndex"/> wrapping <see cref="String.Empty"/>.</summary>
        public static readonly TokenTextIndex EndOfFile = new TokenTextIndex(String.Empty);

        /// <summary>Initializes a new instance of the <see cref="TokenTextIndex"/> class.</summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
        /// <param name="text">The string to tokenize.</param>
        public TokenTextIndex(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            this.Text = text;
            _newlines = new NewLineIndex(text);
        }

        /// <summary>
        /// The text associated with this instance.
        /// </summary>
        public readonly string Text;

        /// <summary>Generates a token pointing to the string wrapped by this instance.</summary>
        /// <param name="begin">The offset of the token.</param>
        /// <param name="end">The end offset of the token.</param>
        /// <param name="kind">The kind of the token.</param>
        /// <returns>A <see cref="T:Token"/> with the supplied parameters denoting the halfopen range [begin,
        /// end).</returns>
        public Token Token(int begin, int end, TokenKind kind)
        {
            return new Token(this, begin, end - begin, kind);
        }

        /// <summary>Generates a token of length 1 pointing to the string wrapped by this instance.</summary>
        /// <param name="offset">The offset of the token.</param>
        /// <param name="kind">The kind of the token.</param>
        /// <returns>A <see cref="T:Token"/> with the supplied parameters denoting the halfopen range [offset,
        /// offset + 1).</returns>
        public Token Token(int offset, TokenKind kind)
        {
            return new Token(this, offset, 1, kind);
        }

        /// <summary>Gets a location for the specified offset.</summary>
        /// <param name="offset">The offset to get location data for.</param>
        /// <returns>An <see cref="OffsetInfo"/> for <paramref name="offset"/>.</returns>
        public OffsetInfo Location(int offset)
        {
            return _newlines.GetOffsetInfoForOffset(offset);
        }

        /// <summary>Gets a substring from the string wrapped by this token factory.</summary>
        /// <param name="offset">The offset from which the substring will start.</param>
        /// <param name="length">The length of the substring to retrieve.</param>
        /// <returns>A string containing the characters in the range [offset, offset + length).</returns>
        public string Substring(int offset, int length)
        {
            return this.Text.Substring(offset, length);
        }

        /// <summary>Tests if this <see cref="TokenTextIndex"/> is considered equal to another.</summary>
        /// <param name="other">The token factory to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(TokenTextIndex other)
        {
            return other != null
                && this.Text == other.Text;
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        /// <seealso cref="M:System.Object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TokenTextIndex);
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance.</returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        public override int GetHashCode()
        {
            return this.Text.GetHashCode();
        }

        /// <summary>Convert this instance into a string representation.</summary>
        /// <returns>A string that represents this instance.</returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        public override string ToString()
        {
            return "TokenFactory: " + this.Text;
        }

        /// <summary>
        /// Tests 2 sub-ranges of this index for equality without allocating additional strings.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when one or more arguments are outside the required range.
        /// </exception>
        /// <param name="offset1">The first text range offset.</param>
        /// <param name="length1">The first text range length.</param>
        /// <param name="offset2">The second text range offset.</param>
        /// <param name="length2">The second text range length.</param>
        /// <returns>
        /// true if the set of characters defined by [<paramref name="offset1"/>,
        /// <paramref name="offset1"/> + <paramref name="length1"/>) is the same range of characters
        /// defined by the range [<paramref name="offset2"/>, <paramref name="offset2"/> +
        /// <paramref name="length2"/>); otherwise, false.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public bool TextEqual(int offset1, int length1, int offset2, int length2)
        {
            int end1 = offset1 + length1;
            int end2 = offset2 + length2;
            if (end1 > this.Text.Length || end2 > this.Text.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            return String.CompareOrdinal(this.Text, offset1, this.Text, offset2, length1) == 0;
        }
    }
}

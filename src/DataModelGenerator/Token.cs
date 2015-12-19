// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>A token in a .g4 file.</summary>
    internal class Token : IEquatable<Token>
    {
        private readonly TokenTextIndex _tokenFactory;

        /// <summary>The offset in the source text where this token is located.</summary>
        public readonly int Offset;
        /// <summary>The length in the source text where this token is located.</summary>
        public readonly int Length;
        /// <summary>The kind of token this is.</summary>
        public readonly TokenKind Kind;

        /// <summary>The empty sentinal token; used where null would typically be used to avoid needing
        /// null checks everywhere.</summary> 
        public static readonly Token Empty = new Token(TokenTextIndex.EndOfFile, 0, 0, TokenKind.Default);

        /// <summary>
        /// Do not call this constructor directly. Initializes a new instance of the
        /// <see cref="Token"/> class. Call <see cref="TokenTextIndex.Token(int, int, TokenKind)"/> instead.
        /// </summary>
        /// <param name="factory">The token text index into which this token forms a pointer to.</param>
        /// <param name="offset">The offset in <paramref name="factory"/> from which this token
        /// starts.</param>
        /// <param name="length">The length of text represented by this token.</param>
        /// <param name="kind">The kind of token this is.</param>  
        internal Token(TokenTextIndex factory, int offset, int length, TokenKind kind)
        {
            string underlyingString = factory.Text;
            Debug.Assert(offset + length <= underlyingString.Length);
            _tokenFactory = factory;
            this.Offset = offset;
            this.Length = length;
            this.Kind = kind;
        }

        /// <summary>Gets the text of this token.</summary>
        /// <returns>The text.</returns>
        public string GetText()
        {
            return _tokenFactory.Substring(this.Offset, this.Length);
        }

        /// <summary>Returns whether the text backing this token is the same text as another token.</summary>
        /// <param name="otherToken">The other token.</param>
        /// <returns>Whether or not the tokens are equal.</returns>
        public bool TextEqual(Token otherToken)
        {
            return _tokenFactory.TextEqual(this.Offset, this.Length, otherToken.Offset, otherToken.Length);
        }

        /// <summary>Gets a substring of this token.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one or more arguments are outside the
        /// required range.</exception>
        /// <param name="offset">The offset in this token to begin taking a substring.</param>
        /// <param name="length">The length of text to get.</param>
        /// <returns>A string containing the characters in the range [<paramref name="offset"/>,
        /// <paramref name="offset"/> + <paramref name="length"/>).</returns>
        public string Substring(int offset, int length)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", offset, Strings.TokenAccessOutOfRange);
            }

            int max = checked(this.Offset + this.Length);
            int selectedMax = checked(offset + length);
            if (selectedMax > max)
            {
                throw new ArgumentOutOfRangeException("length", length, Strings.TokenAccessOutOfRange);
            }

            return _tokenFactory.Substring(this.Offset + offset, length);
        }

        /// <summary>Indexer to returning character objects backing this token.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="idx"/> is outside
        /// the range [0, <see cref="M:Length"/>).</exception>
        /// <param name="idx">The index within the token to retrieve a character.</param>
        /// <returns>The indexed character.</returns>
        public char this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= this.Length)
                {
                    throw new ArgumentOutOfRangeException("idx", idx, Strings.TokenAccessOutOfRange);
                }

                return _tokenFactory.Text[this.Offset + idx];
            }
        }

        /// <summary>Convert this object into a string representation.</summary>
        /// <returns>A string that represents this object.</returns>
        public override string ToString()
        {
            return this.Kind + ": " + this.GetText();
        }

        /// <summary>Tests if this Token is considered equal to another.</summary>
        /// <param name="other">The token to compare to this object.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(Token other)
        {
            return other != null
                && this.Offset == other.Offset
                && this.Length == other.Length
                && this.Kind == other.Kind
                && _tokenFactory == other._tokenFactory;
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Token);
        }

        /// <summary>Returns a hash code for this object.</summary>
        /// <returns>A hash code for this object.</returns>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(_tokenFactory);
            hash.Add(this.Offset);
            hash.Add(this.Length);
            hash.Add((int)this.Kind);
            return hash.GetHashCode();
        }

        /// <summary>Gets the line and column location of this token.</summary>
        /// <returns>The line and column location of this token.</returns>
        public OffsetInfo GetLocation()
        {
            return _tokenFactory.Location(this.Offset);
        }
    }
}
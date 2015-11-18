// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Driver;
using System.Text;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>A symbol or tree node from a parsed G4 file.</summary>
    /// <seealso cref="T:System.IEquatable{Microsoft.CodeAnalysis.DataModelGenerator.GrammarSymbol}"/>
    internal sealed class GrammarSymbol : IEquatable<GrammarSymbol>
    {
        /// <summary>The first token giving rise to this symbol, inclusive.</summary>
        public readonly Token FirstToken;
        /// <summary>The last token giving rise to this symbol, inclusive.</summary>
        public readonly Token LastToken;
        /// <summary>The kind of symbol this is.</summary>
        public readonly SymbolKind Kind;
        /// <summary>Symbols lexically underneath this symbol.</summary>
        public readonly ImmutableArray<GrammarSymbol> Children;
        /// <summary>The annotations attached to this symbol, if any.</summary>
        public readonly ImmutableArray<Annotation> Annotations;
        /// <summary>An empty, sentinel symbol.</summary>
        public static readonly GrammarSymbol Empty = new GrammarSymbol(Token.Empty, SymbolKind.Default);

        /// <summary>Initializes a new instance of the <see cref="GrammarSymbol"/> class.</summary>
        /// <param name="token">The token.</param>
        /// <param name="kind">The kind of symbol this is.</param>
        public GrammarSymbol(Token token, SymbolKind kind)
            : this(token, token, kind)
        { }

        /// <summary>Initializes a new instance of the <see cref="GrammarSymbol"/> class.</summary>
        /// <param name="token">The token.</param>
        /// <param name="kind">The kind of symbol this is.</param>
        /// <param name="annotations">Annotations attached to the symbol.</param>
        public GrammarSymbol(Token token, SymbolKind kind, ImmutableArray<Annotation> annotations)
            : this(token, token, kind, annotations)
        { }

        /// <summary>Initializes a new instance of the <see cref="GrammarSymbol"/> class.</summary>
        /// <param name="firstToken">The first token giving rise to this symbol, inclusive.</param>
        /// <param name="lastToken">The last token giving rise to this symbol, inclusive.</param>
        /// <param name="kind">The kind of symbol this is.</param>
        public GrammarSymbol(
            Token firstToken,
            Token lastToken,
            SymbolKind kind)
            : this(firstToken, lastToken, kind, ImmutableArray<GrammarSymbol>.Empty)
        { }

        /// <summary>Initializes a new instance of the <see cref="GrammarSymbol"/> class.</summary>
        /// <param name="firstToken">The first token giving rise to this symbol, inclusive.</param>
        /// <param name="lastToken">The last token giving rise to this symbol, inclusive.</param>
        /// <param name="kind">The kind of symbol this is.</param>
        /// <param name="children">Symbols lexically underneath this symbol.</param>
        public GrammarSymbol(
            Token firstToken,
            Token lastToken,
            SymbolKind kind,
            ImmutableArray<GrammarSymbol> children)
            : this(firstToken, lastToken, kind, ImmutableArray<Annotation>.Empty, children)
        { }

        /// <summary>Initializes a new instance of the <see cref="GrammarSymbol"/> class.</summary>
        /// <param name="firstToken">The first token giving rise to this symbol, inclusive.</param>
        /// <param name="lastToken">The last token giving rise to this symbol, inclusive.</param>
        /// <param name="kind">The kind of symbol this is.</param>
        /// <param name="annotations">Annotations attached to the symbol.</param>
        /// <param name="children">Symbols lexically underneath this symbol.</param>
        public GrammarSymbol(
            Token firstToken,
            Token lastToken,
            SymbolKind kind,
            ImmutableArray<Annotation> annotations,
            ImmutableArray<GrammarSymbol> children)
        {
            this.FirstToken = firstToken;
            this.LastToken = lastToken;
            this.Kind = kind;
            this.Children = children;
            this.Annotations = annotations;
        }

        /// <summary>Initializes a new instance of the <see cref="GrammarSymbol"/> class.</summary>
        /// <param name="firstToken">The first token giving rise to this symbol, inclusive.</param>
        /// <param name="lastToken">The last token giving rise to this symbol, inclusive.</param>
        /// <param name="kind">The kind of symbol this is.</param>
        /// <param name="children">Symbols lexically underneath this symbol.</param>
        public GrammarSymbol(
            Token firstToken,
            Token lastToken,
            SymbolKind kind,
            params GrammarSymbol[] children)
            : this(firstToken, lastToken, kind, ImmutableArray<Annotation>.Empty, children)
        { }

        /// <summary>Initializes a new instance of the <see cref="GrammarSymbol"/> class.</summary>
        /// <param name="firstToken">The first token giving rise to this symbol, inclusive.</param>
        /// <param name="lastToken">The last token giving rise to this symbol, inclusive.</param>
        /// <param name="kind">The kind of symbol this is.</param>
        /// <param name="annotations">Annotations attached to the symbol.</param>
        /// <param name="children">Symbols lexically underneath this symbol.</param>
        public GrammarSymbol(
            Token firstToken,
            Token lastToken,
            SymbolKind kind,
            ImmutableArray<Annotation> annotations,
            params GrammarSymbol[] children)
            : this(firstToken, lastToken, kind, annotations, children.ToImmutableArray())
        { }

        /// <summary>Convert this instance into a string representation.</summary>
        /// <returns>A string that represents this instance.</returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        public override string ToString()
        {
            var result = new StringBuilder();
            this.ToStringRecursive(result, 0);
            return result.ToString();
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance.</returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.AddRange(this.Children);
            hash.Add(this.Kind);
            hash.Add(this.FirstToken);
            hash.Add(this.LastToken);
            hash.AddRange(this.Annotations);
            return hash.GetHashCode();
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        /// <seealso cref="M:System.Object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as GrammarSymbol);
        }

        /// <summary>Tests if this GrammarSymbol is considered equal to another.</summary>
        /// <param name="other">The grammar symbol to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(GrammarSymbol other)
        {
            return other != null
                && this.Kind == other.Kind
                && this.FirstToken.Equals(other.FirstToken)
                && this.LastToken.Equals(other.LastToken)
                && this.Children.SequenceEqual(other.Children)
                && this.Annotations.SequenceEqual(other.Annotations);
        }

        /// <summary>Gets the location of the start of this symbol.</summary>
        /// <returns>The location of the start of this symbol.</returns>
        public OffsetInfo GetLocation()
        {
            return this.FirstToken.GetLocation();
        }

        /// <summary>Gets logical text of this token, if available.</summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the requested operation is invalid.
        /// </exception>
        /// <returns>
        /// The logical text of this token; e.g. the string value of strings, the identifier of
        /// identifiers, etc.
        /// </returns>
        public string GetLogicalText()
        {
            switch (this.Kind)
            {
                case SymbolKind.ProductionDecl:
                    return this.LastToken.GetText();
                case SymbolKind.Identifier:
                    return this.FirstToken.GetText();
                case SymbolKind.String:
                    return this.FirstToken.Substring(1, this.FirstToken.Length - 2);
                default:
                    throw new InvalidOperationException(Strings.LogicalTextNotAvailable);
            }
        }

        /// <summary>Gets the child symbol at <paramref name="index"/>.</summary>
        /// <param name="index">Zero-based index of the entry to access.</param>
        /// <returns>The child symbol at the indicated index.</returns>
        public GrammarSymbol this[int index]
        {
            get
            {
                return this.Children[index];
            }
        }

        private string ToSingleLevelString(int depth)
        {
            var result = new StringBuilder();
            if (depth != 0)
            {
                result.Append(' ', depth * 2);
            }

            // Add "Symbol" suffix to "string" and "identifier" -- without this
            // it becomes very easy to confuse identifier symbols with identifier tokens.
            string kindStr;
            switch (this.Kind)
            {
                case SymbolKind.Identifier:
                    kindStr = "IdentifierSymbol";
                    break;
                case SymbolKind.String:
                    kindStr = "StringSymbol";
                    break;
                default:
                    kindStr = this.Kind.ToString();
                    break;
            }

            result.Append(kindStr);
            result.Append(": [");
            result.Append(this.FirstToken.ToString());
            if (this.FirstToken != this.LastToken)
            {
                result.Append(", ");
                result.Append(this.LastToken.ToString());
            }

            result.Append("]");
            foreach (Annotation annotation in this.Annotations)
            {
                result.Append(' ');
                result.Append(annotation.ToString());
            }

            return result.ToString();
        }

        private void ToStringRecursive(StringBuilder target, int depth)
        {
            target.Append(this.ToSingleLevelString(depth));
            foreach (GrammarSymbol child in this.Children)
            {
                target.AppendLine();
                child.ToStringRecursive(target, depth + 1);
            }
        }
    }

    /// <summary>Extension methods for working with groups of symbols.</summary>
    internal static class GrammarSymbolExtensions
    {
        /// <summary>
        /// An <see cref="ImmutableArray{GrammarSymbol}.Builder"/> extension method that creates a symbol
        /// enclosing the supplied list of symbols of the specified kind.
        /// </summary>
        /// <param name="children">The children of the new generated symbol.</param>
        /// <param name="kind">The kind of the new generated symbol.</param>
        /// <returns>The new enclosing symbol.</returns>
        public static GrammarSymbol CreateEnclosingSymbol(this ImmutableArray<GrammarSymbol>.Builder children, SymbolKind kind)
        {
            if (children.Count == 0)
            {
                return null;
            }
            else if (children.Count == 1)
            {
                return children[0];
            }
            else
            {
                return new GrammarSymbol(
                    children[0].FirstToken,
                    children[children.Count - 1].LastToken,
                    kind,
                    children.ToImmutable()
                    );
            }
        }
    }
}

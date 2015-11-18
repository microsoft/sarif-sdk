// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Data model metadata declared at the top of a grammar.</summary>
    internal class DataModelMetadata : IEquatable<DataModelMetadata>
    {
        /// <summary>The name of the grammar; typically used as a name prefix for common helper machinery,
        /// such as the common kind interface.</summary> 
        public readonly string Name;

        /// <summary>The namespace into which the data model shall be generated.</summary> 
        public readonly string Namespace;

        /// <summary>If set, generate extra "length" and "offset" parameters in all generated types.</summary>
        public readonly bool GenerateLocations;

        /// <summary>If set, generates Equals and GetHashCode overloads in all generated types.</summary>
        public readonly bool GenerateEquals;

        /// <summary>Initializes a new instance of the <see cref="DataModelMetadata"/> class.</summary>
        /// <param name="name">The name of the grammar; typically used as a name prefix for common
        /// helper machinery, such as the common kind interface.</param>
        /// <param name="nameSpace">The namespace into which the data model shall be generated.</param>
        /// <param name="generateLocations">If set, generate extra "length" and "offset" parameters in
        /// all generated types.</param>
        /// <param name="generateEquals">If set, generates Equals and GetHashCode overloads in all
        /// generated types.</param>
        public DataModelMetadata(string name, string nameSpace, bool generateLocations, bool generateEquals)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
            this.Namespace = nameSpace;
            this.GenerateLocations = generateLocations;
            this.GenerateEquals = generateEquals;
        }

        /// <summary>
        /// Initializes a <see cref="DataModelMetadata"/> instance from the given grammar.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when the supplied <see cref="GrammarSymbol"/> is not a valid grammar.
        /// </exception>
        /// <param name="grammar">
        /// The grammar from which the <see cref="DataModelMetadata"/> shall be generated.
        /// </param>
        /// <returns>
        /// A <see cref="DataModelMetadata"/> containing data from the grammar <paramref name="grammar"/>.
        /// </returns>
        public static DataModelMetadata FromGrammar(GrammarSymbol grammar)
        {
            if (grammar.Kind != SymbolKind.Grammar)
            {
                throw new ArgumentException("FromGrammar requires a grammar.");
            }

            GrammarSymbol decl = grammar.Children[0];
            GrammarSymbol grammarId = decl.Children[0];
            string grammarName = grammarId.GetLogicalText();
            System.Collections.Immutable.ImmutableArray<Annotation> annotations = grammarId.Annotations;
            string nameSpace = annotations.GetAnnotationValue("namespace") ?? grammarName;

            return new DataModelMetadata(grammarName, nameSpace, annotations.HasAnnotation("generateLocations"), !annotations.HasAnnotation("noGenerateEquals"));
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.GenerateEquals ? 1 : 0);
            hash.Add(this.GenerateLocations ? 1 : 0);
            hash.Add(this.Name);
            hash.Add(this.Namespace);
            return hash.GetHashCode();
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DataModelMetadata);
        }

        /// <summary>Tests if this <see cref="DataModelMetadata" /> is considered equal to another.</summary>
        /// <param name="other">The data model metadata to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(DataModelMetadata other)
        {
            return other != null
                && this.GenerateEquals == other.GenerateEquals
                && this.GenerateLocations == other.GenerateLocations
                && this.Name == other.Name
                && this.Namespace == other.Namespace;
        }

        /// <summary>Convert this instance into a string representation.</summary>
        /// <returns>A string that represents this instance.</returns>
        public override string ToString()
        {
            string result = this.Namespace;

            if (String.IsNullOrEmpty(result))
            {
                result = this.Name;
            }
            else
            {
                result += "." + this.Name;
            }

            if (!this.GenerateEquals)
            {
                result += " @noGenerateEquals";
            }

            if (this.GenerateLocations)
            {
                result += " @generateLocations";
            }

            return result;
        }
    }
}

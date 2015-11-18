// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Values that represent kinds of symbols.</summary>
    internal enum SymbolKind
    {
        /// <summary>An invalid symbol; the default <see cref="SymbolKind"/> value.</summary>
        Default,

        /// <summary>A symbol representing an entire grammar.</summary>
        Grammar,

        /// <summary>A symbol representing a grammar declaration, e.g. grammar foo;.</summary>
        GrammarDecl,

        /// <summary>A symbol representing a production e.g. a: other things;.</summary>
        Production,

        /// <summary>A symbol representing a production declaration, e.g. a:.</summary>
        ProductionDecl,

        /// <summary>A symbol representing an alternation, e.g. a | b.</summary>
        Alternation,

        /// <summary>A symbol representing a group, e.g. a b c.</summary>
        Group,

        /// <summary>A symbol representing a zero or more quantifier, e.g. *.</summary>
        ZeroOrMoreQuantifier,

        /// <summary>A symbol representing a one or more quantifier, e.g. +.</summary>
        OneOrMoreQuantifier,

        /// <summary>A symbol representing a zero or one quantifier, e.g. ?.</summary>
        ZeroOrOneQuantifier,

        /// <summary>A symbol representing a string nonterminal, e.g. 'exampleString'.</summary>
        String,

        /// <summary>A symbol representing a identifier nonterminal, e.g. exampleIdentifier.</summary>
        Identifier,

        /// <summary>A symbol representing a character range nonterminal, e.g. 'a'..'z'.</summary>
        CharacterRange
    }
}

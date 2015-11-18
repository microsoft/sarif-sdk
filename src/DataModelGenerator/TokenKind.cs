// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Values that represent kinds of token.</summary>
    public enum TokenKind
    {
        /// <summary>An invalid sentinal token kind.</summary>
        Default,

        /// <summary>A token representing a single quoted string in a g4 file.</summary>
        String,

        /// <summary>A token representing an identifier in a g4 file.</summary>
        Identifier,

        /// <summary>A token representing a pipe character in a g4 file.</summary>
        Pipe,

        /// <summary>A token representing a semicolon character in a g4 file.</summary>
        Semicolon,

        /// <summary>A token representing a colon character in a g4 file.</summary>
        Colon,

        /// <summary>A token representing a star character in a g4 file.</summary>
        Star,

        /// <summary>A token representing a plus character in a g4 file.</summary>
        Plus,

        /// <summary>A token representing a question mark character in a g4 file.</summary>
        Question,

        /// <summary>A token representing a left parenthesis character in a g4 file.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Lparen")]
        Lparen,

        /// <summary>A token representing a right parenthesis character in a g4 file.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rparen")]
        Rparen,

        /// <summary>A token representing a range dots (that is, '..') in a g4 file.</summary>
        Dots,

        /// <summary>A token representing an annotation of the form @annotation in a g4 file.</summary>
        Annotation,

        /// <summary>A token representing an annotation value of the form {annotationValue} in a g4 file.</summary>
        AnnotationValue
    };
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Performs lexical analysis on .g4 files.</summary>
    internal static class Lexer
    {
        /// <summary>Lexes text in the given <see cref="TokenTextIndex"/>.</summary>
        /// <exception cref="G4ParseFailureException">Thrown when the input is not lexically valid.</exception>
        /// <param name="tokenFactory">The token factory from which lexical analysis will be performed.</param>
        /// <returns>An <see cref="ImmutableArray{Token}"/> containing the generated tokens.</returns>
        public static ImmutableArray<Token> Lex(TokenTextIndex tokenFactory)
        {
            return LexImpl(tokenFactory).ToImmutableArray();
        }

        /// <summary>Lexes text in the given text.</summary>
        /// <exception cref="G4ParseFailureException">Thrown when the input is not lexically valid.</exception>
        /// <param name="sourceText">The text on which lexical analysis will be performed.</param>
        /// <returns>An <see cref="ImmutableArray{Token}"/> containing the generated tokens.</returns>
        public static ImmutableArray<Token> Lex(string sourceText)
        {
            return Lex(new TokenTextIndex(sourceText));
        }

        private enum LexerState
        {
            // Default state; skips whitespace.
            SkipWhitespace,
            // Looks for a newline and then returns to `SkipWhitespace`
            SkipSingleLineComment,
            // Parsing a /* */ block for the */, annotations, and annotationValues.
            MultiLineComment,
            // Detected a * in a multiline comment; looking for / to end the multiline comment block
            MultiLineCommentStar,
            // Detected a /, looking for the * to enter mutliline comment mode, or / for single line
            CommentCandidate,
            // Inside an @annotation
            CollectingAnnotation,
            // Inside an {annotationValue}
            CollectingAnnotationValue,
            // Inside an {annotationValue}, looking for someone incorrectly putting */ inside.
            CollectingAnnotationValueStar,
            // Inside a 'quoted string'
            CollectingString,
            // Inside an identifier
            CollectingIdentifier,
            // Seen a ., looking for the second . in a "dots" token
            DotsCandidate
        }

        private static IEnumerable<Token> LexImpl(TokenTextIndex tokenFactory)
        {
            string text = tokenFactory.Text;
            LexerState state = LexerState.SkipWhitespace;
            int tokenStart = 0;
            int multiLineCommentStart = 0;
            int valueLeftBraceDepth = 0;
            for (int idx = 0; idx < text.Length; ++idx)
            {
                char ch = text[idx];
                // Note: The "error detection" cases are later in the switch because we
                // expect them to be visited less often. (and the C# compiler emits the branches
                // in order)
                switch (state)
                {
                    case LexerState.SkipWhitespace:
                        // Putting Default first because we expect most of the time to be skipping
                        // whitespace.
                        tokenStart = idx;
                        switch (ch)
                        {
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                            case '\u2028':
                            case '\u2029':
                                // Skip whitespace
                                break;
                            case '\'':
                                state = LexerState.CollectingString;
                                break;
                            case '/':
                                state = LexerState.CommentCandidate;
                                break;
                            case '|':
                                yield return tokenFactory.Token(idx, TokenKind.Pipe);
                                break;
                            case ':':
                                yield return tokenFactory.Token(idx, TokenKind.Colon);
                                break;
                            case ';':
                                yield return tokenFactory.Token(idx, TokenKind.Semicolon);
                                break;
                            case '.':
                                state = LexerState.DotsCandidate;
                                break;
                            case '(':
                                yield return tokenFactory.Token(idx, TokenKind.Lparen);
                                break;
                            case ')':
                                yield return tokenFactory.Token(idx, TokenKind.Rparen);
                                break;
                            case '*':
                                yield return tokenFactory.Token(idx, TokenKind.Star);
                                break;
                            case '+':
                                yield return tokenFactory.Token(idx, TokenKind.Plus);
                                break;
                            case '?':
                                yield return tokenFactory.Token(idx, TokenKind.Question);
                                break;
                            default:
                                state = LexerState.CollectingIdentifier;
                                break;
                        }
                        break;

                    case LexerState.CollectingString:
                        if (ch == '\'')
                        {
                            yield return tokenFactory.Token(tokenStart, idx + 1, TokenKind.String);
                            state = LexerState.SkipWhitespace;
                        }
                        break;

                    case LexerState.SkipSingleLineComment:
                        switch (ch)
                        {
                            case '\r':
                            case '\n':
                            case '\u2028':
                            case '\u2029':
                                state = LexerState.SkipWhitespace;
                                break;
                        }
                        break;

                    case LexerState.CommentCandidate:
                        switch (ch)
                        {
                            case '/':
                                state = LexerState.SkipSingleLineComment;
                                break;
                            case '*':
                                state = LexerState.MultiLineComment;
                                multiLineCommentStart = idx - 1;
                                break;
                            default:
                                throw new G4ParseFailureException(tokenFactory.Location(idx - 1), Strings.UnrecognizedForwardSlash);
                        }
                        break;

                    case LexerState.MultiLineComment:
                        switch (ch)
                        {
                            case '*':
                                state = LexerState.MultiLineCommentStar;
                                break;
                            case '@':
                                state = LexerState.CollectingAnnotation;
                                tokenStart = idx;
                                break;
                            case '{':
                                state = LexerState.CollectingAnnotationValue;
                                tokenStart = idx;
                                break;
                        }
                        break;

                    case LexerState.MultiLineCommentStar:
                        switch (ch)
                        {
                            case '*':
                                // Do nothing, e.g. in case *****/
                                break;
                            case '@':
                                state = LexerState.CollectingAnnotation;
                                tokenStart = idx;
                                break;
                            case '{':
                                state = LexerState.CollectingAnnotationValue;
                                tokenStart = idx;
                                break;
                            case '/':
                                state = LexerState.SkipWhitespace;
                                break;
                            default:
                                state = LexerState.MultiLineComment;
                                break;
                        }
                        break;

                    case LexerState.CollectingAnnotation:
                        switch (ch)
                        {
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                            case '\u2028':
                            case '\u2029':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Annotation);
                                state = LexerState.MultiLineComment;
                                break;
                            case '*':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Annotation);
                                state = LexerState.MultiLineCommentStar;
                                break;
                            case '{':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Annotation);
                                valueLeftBraceDepth = 0;
                                state = LexerState.CollectingAnnotationValue;
                                tokenStart = idx;
                                break;
                            case '@':
                                throw new G4ParseFailureException(tokenFactory.Location(tokenStart), Strings.UnrecognizedAtInAnnotation);
                        }
                        break;

                    case LexerState.CollectingAnnotationValue:
                        switch (ch)
                        {
                            case '{':
                                valueLeftBraceDepth++;
                                break;
                            case '}':
                                if (valueLeftBraceDepth > 0)
                                {
                                    valueLeftBraceDepth--;
                                }
                                else
                                {
                                    yield return tokenFactory.Token(tokenStart, idx + 1, TokenKind.AnnotationValue);
                                    state = LexerState.MultiLineComment;
                                }
                                break;
                            case '*':
                                state = LexerState.CollectingAnnotationValueStar;
                                break;
                        }
                        break;

                    case LexerState.CollectingIdentifier:
                        switch (ch)
                        {
                            case ' ':
                            case '\t':
                            case '\r':
                            case '\n':
                            case '\u2028':
                            case '\u2029':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                state = LexerState.SkipWhitespace;
                                break;
                            case '\'':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                tokenStart = idx;
                                state = LexerState.CollectingString;
                                break;
                            case '/':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                state = LexerState.CommentCandidate;
                                break;
                            case '|':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                yield return tokenFactory.Token(idx, TokenKind.Pipe);
                                state = LexerState.SkipWhitespace;
                                break;
                            case ':':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                yield return tokenFactory.Token(idx, TokenKind.Colon);
                                state = LexerState.SkipWhitespace;
                                break;
                            case ';':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                yield return tokenFactory.Token(idx, TokenKind.Semicolon);
                                state = LexerState.SkipWhitespace;
                                break;
                            case '.':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                tokenStart = idx;
                                state = LexerState.DotsCandidate;
                                break;
                            case '(':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                yield return tokenFactory.Token(idx, TokenKind.Lparen);
                                state = LexerState.SkipWhitespace;
                                break;
                            case ')':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                yield return tokenFactory.Token(idx, TokenKind.Rparen);
                                state = LexerState.SkipWhitespace;
                                break;
                            case '*':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                yield return tokenFactory.Token(idx, TokenKind.Star);
                                state = LexerState.SkipWhitespace;
                                break;
                            case '+':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                yield return tokenFactory.Token(idx, TokenKind.Plus);
                                state = LexerState.SkipWhitespace;
                                break;
                            case '?':
                                yield return tokenFactory.Token(tokenStart, idx, TokenKind.Identifier);
                                yield return tokenFactory.Token(idx, TokenKind.Question);
                                state = LexerState.SkipWhitespace;
                                break;
                        }
                        break;

                    case LexerState.CollectingAnnotationValueStar:
                        switch (ch)
                        {
                            case '}':
                                yield return tokenFactory.Token(tokenStart, idx + 1, TokenKind.AnnotationValue);
                                state = LexerState.MultiLineComment;
                                break;
                            case '/':
                                throw new G4ParseFailureException(tokenFactory.Location(tokenStart), Strings.UnclosedAnnotation);
                            default:
                                state = LexerState.CollectingAnnotationValue;
                                break;
                        }
                        break;

                    case LexerState.DotsCandidate:
                        switch (ch)
                        {
                            case '.':
                                yield return tokenFactory.Token(tokenStart, idx + 1, TokenKind.Dots);
                                state = LexerState.SkipWhitespace;
                                break;
                            default:
                                throw new G4ParseFailureException(tokenFactory.Location(tokenStart), Strings.SingleDot);
                        }
                        break;
                }
            }

            switch (state)
            {
                case LexerState.CollectingIdentifier:
                    yield return tokenFactory.Token(tokenStart, text.Length, TokenKind.Identifier);
                    break;
                case LexerState.MultiLineComment:
                case LexerState.MultiLineCommentStar:
                case LexerState.CollectingAnnotation:
                case LexerState.CollectingAnnotationValue:
                case LexerState.CollectingAnnotationValueStar:
                    throw new G4ParseFailureException(tokenFactory.Location(multiLineCommentStart), Strings.UnclosedMultiLineComment);
                case LexerState.CommentCandidate:
                    throw new G4ParseFailureException(tokenFactory.Location(text.Length), Strings.UnrecognizedForwardSlash);
                case LexerState.CollectingString:
                    throw new G4ParseFailureException(tokenFactory.Location(tokenStart), Strings.UnclosedString);
                case LexerState.DotsCandidate:
                    throw new G4ParseFailureException(tokenFactory.Location(text.Length), Strings.SingleDot);
                case LexerState.SkipWhitespace:
                case LexerState.SkipSingleLineComment:
                    // OK (do nothing)
                    break;
            }
        }
    }
}

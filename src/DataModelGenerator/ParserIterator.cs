// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>
    /// A helper class for the parser that processes comment annotation blocks and provides lookahead
    /// and current position tracking. Represents the token stream to the parser as if it had the
    /// annotation tokens removed. Similar to <see cref="IEnumerator{Token}"/>.
    /// </summary>
    internal class ParserIterator
    {
        private readonly ImmutableArray<Token> _tokens;

        // Using a flat array for now even though this may need to be searched reasonably
        // often because N is usually something like 3 or 4 tops.
        private readonly ImmutableArray<Annotation>.Builder _annotations;
        // Annotations applied to LookAhead
        private readonly List<Annotation> _nextAnnotations;

        private int _nextPosition;

        /// <summary>Initializes a new instance of the <see cref="ParserIterator"/> class.</summary>
        /// <param name="tokens">The token stream which will be iterated over.</param>
        public ParserIterator(ImmutableArray<Token> tokens)
        {
            _tokens = tokens;
            _annotations = ImmutableArray.CreateBuilder<Annotation>();
            _nextAnnotations = new List<Annotation>();
            // current = null
            // lookAhead = null
            this.Move();
            // current = null
            // lookAhead = firstToken
            this.Move();
            // current = firstToken
            // lookAhead = firstToken + 1
        }

        /// <summary>Gets the current token.</summary>
        /// <value>The current token.</value>
        public Token Current { get; private set; }

        /// <summary>Gets what the next token will be.</summary>
        /// <value>What the next token will be.</value>
        public Token LookAhead { get; private set; }

        /// <summary>
        /// Gets the annotations valid at this point in the token stream, then clears the current
        /// annoations.
        /// </summary>
        /// <returns>The annotations from the current point in the token stream.</returns>
        public ImmutableArray<Annotation> ConsumeAnnotations()
        {
            ImmutableArray<Annotation> result = _annotations.ToImmutable();
            _annotations.Clear();
            return result;
        }

        /// <summary>Clears the annotations.</summary>
        public void DiscardAnnotations()
        {
            _annotations.Clear();
        }

        /// <summary>Moves to the next token.</summary>
        public void Move()
        {
            this.Current = this.LookAhead;
            _annotations.AddRange(_nextAnnotations);
            _nextAnnotations.Clear();
            Token tokenTry = this.GetNextAndMove();
            while (tokenTry.Kind == TokenKind.Annotation)
            {
                // @anno => anno
                string annotationName = tokenTry.Substring(1, tokenTry.Length - 1);
                string annotationContent;
                Token lookAhead = this.GetNextAndMove();
                if (lookAhead.Kind == TokenKind.AnnotationValue)
                {
                    // {value} => value
                    annotationContent = lookAhead.Substring(1, lookAhead.Length - 2);
                    tokenTry = this.GetNextAndMove();
                }
                else
                {
                    annotationContent = null;
                    tokenTry = lookAhead;
                }

                _nextAnnotations.Add(new Annotation(annotationName, annotationContent));
            }

            this.LookAhead = tokenTry;
        }

        /// <summary>
        /// Consumes a token. Calls <see cref="Move"/> and returns the previous value of
        /// <see cref="Current"/>.
        /// </summary>
        /// <returns>The next token for consumption.</returns>
        public Token Consume()
        {
            Token result = this.Current;
            this.Move();
            return result;
        }

        /// <summary>
        /// Consumes a token and asserts that it is of a given kind. Calls <see cref="Move"/> and returns
        /// the previous value of <see cref="Current"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="requiredKind"/> is
        /// <see cref="TokenKind.Default"/>.</exception>
        /// <param name="requiredKind">The required kind. This may not be
        /// <see cref="TokenKind.Default"/>.</param>
        /// <returns>The next token for consumption.</returns>
        public Token Require(TokenKind requiredKind)
        {
            if (requiredKind == TokenKind.Default)
            {
                throw new ArgumentException(Strings.RequiredInvalidToken, "requiredKind");
            }

            Token result = this.Current;
            if (result.Kind != requiredKind)
            {
                throw G4ParseFailureException.UnexpectedToken(this.Current);
            }

            this.Move();
            return result;
        }

        private Token GetNextAndMove()
        {
            int next = _nextPosition;
            if (next == _tokens.Length)
            {
                return Token.Empty;
            }

            _nextPosition = next + 1;
            return _tokens[next];
        }
    }
}

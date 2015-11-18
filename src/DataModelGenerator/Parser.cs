// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>
    /// The Parser class produces a Grammar data structure base on the input grammar file.
    /// </summary>
    internal class Parser
    {
        private readonly ParserIterator _iterator;

        /// <summary>
        /// A utility function that does the tokenize and parse steps together from a string.
        /// </summary>
        /// <param name="grammarText">The text to parse.</param>
        /// <returns>A parsed grammar.</returns>
        public static GrammarSymbol Parse(string grammarText)
        {
            var textIndex = new TokenTextIndex(grammarText);
            ImmutableArray<Token> tokens = Lexer.Lex(textIndex);
            return new Parser(tokens).ParseGrammar();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="tokens">The tokens to be parsed.</param>
        public Parser(ImmutableArray<Token> tokens)
        {
            _iterator = new ParserIterator(tokens);
        }

        /// <summary>Gets a value indicating whether the parser is at the end of the token list.</summary>
        /// <value>true if at end, false if not.</value>
        public bool AtEnd
        {
            get
            {
                return _iterator.Current == Token.Empty;
            }
        }

        /// <summary>Parses a G4 grammar.</summary>
        /// <exception cref="G4ParseFailureException">Thrown when the input is not valid.</exception>
        /// <returns>A GrammarSymbol containing the grammar's contents.</returns>
        public GrammarSymbol ParseGrammar()
        {
            ImmutableArray<GrammarSymbol>.Builder children = ImmutableArray.CreateBuilder<GrammarSymbol>();
            children.Add(this.ParseGrammarDeclaration());
            while (!this.AtEnd)
            {
                children.Add(this.ParseProduction());
            }

            return new GrammarSymbol(
                children[0].FirstToken,
                children[children.Count - 1].LastToken,
                SymbolKind.Grammar,
                children.ToImmutable()
                );
        }

        /// <summary>Parses a G4 grammar declaration.</summary>
        /// <exception cref="G4ParseFailureException">Thrown when the input is not valid.</exception>
        /// <returns>A GrammarSymbol containing the declaration's contents.</returns>
        public GrammarSymbol ParseGrammarDeclaration()
        {
            Token grammar = _iterator.Require(TokenKind.Identifier);
            if (!grammar.GetText().Equals("grammar"))
            {
                throw new G4ParseFailureException(grammar.GetLocation(), Strings.GrammarExpectedHere);
            }

            Token grammarName = _iterator.Require(TokenKind.Identifier);
            ImmutableArray<Annotation> annotations = _iterator.ConsumeAnnotations();
            Token semi = _iterator.Require(TokenKind.Semicolon);
            return new GrammarSymbol(grammar, semi, SymbolKind.GrammarDecl,
                new GrammarSymbol(grammarName, SymbolKind.Identifier, annotations));
        }

        /// <summary>Parses a G4 grammar production.</summary>
        /// <exception cref="G4ParseFailureException">Thrown when the input is not valid.</exception>
        /// <returns>A GrammarSymbol containing the production.</returns>
        public GrammarSymbol ParseProduction()
        {
            Token firstToken = _iterator.Current; // May be != identifierToken if 'fragment'
            Token identifierToken = _iterator.Require(TokenKind.Identifier);
            if (identifierToken.GetText().Equals("fragment"))
            {
                identifierToken = _iterator.Require(TokenKind.Identifier);
            }

            ImmutableArray<Annotation> productionAnnotations = _iterator.ConsumeAnnotations();
            var declaration = new GrammarSymbol(identifierToken, SymbolKind.ProductionDecl, productionAnnotations);
            _iterator.Require(TokenKind.Colon);
            GrammarSymbol alternation = this.ParseAlternation();
            _iterator.DiscardAnnotations();
            Token lastToken = _iterator.Require(TokenKind.Semicolon);

            return new GrammarSymbol(firstToken, lastToken, SymbolKind.Production, ImmutableArray.Create(declaration, alternation));
        }

        /// <summary>Parses a G4 alternation.</summary>
        /// <exception cref="G4ParseFailureException">Thrown when the input is not valid.</exception>
        /// <returns>A GrammarSymbol containing the alternation.</returns>
        public GrammarSymbol ParseAlternation()
        {
            ImmutableArray<GrammarSymbol>.Builder groups = ImmutableArray.CreateBuilder<GrammarSymbol>();
            groups.Add(this.ParseGroup());
            while (_iterator.Current.Kind == TokenKind.Pipe)
            {
                _iterator.Move();
                groups.Add(this.ParseGroup());
            }

            return groups.CreateEnclosingSymbol(SymbolKind.Alternation);
        }

        /// <summary>Parses a G4 group.</summary>
        /// <exception cref="G4ParseFailureException">Thrown when the input is not valid.</exception>
        /// <returns>A GrammarSymbol containing the group.</returns>
        public GrammarSymbol ParseGroup()
        {
            ImmutableArray<GrammarSymbol>.Builder quantifiers = ImmutableArray.CreateBuilder<GrammarSymbol>();

            for (;;)
            {
                Token current = _iterator.Current;
                switch (current.Kind)
                {
                    // Quantifier must start with a nonTerminal
                    case TokenKind.Identifier:
                    case TokenKind.String:
                    // CharacterRange
                    case TokenKind.Lparen:
                        quantifiers.Add(this.ParseQuantifier());
                        break;
                    default:
                        GrammarSymbol result = quantifiers.CreateEnclosingSymbol(SymbolKind.Group);
                        if (result == null)
                        {
                            throw new G4ParseFailureException(current.GetLocation(), Strings.EmptyGroup);
                        }

                        return result;
                }
            }
        }

        /// <summary>Parses a G4 quantifier.</summary>
        /// <exception cref="G4ParseFailureException">Thrown when the input is not valid.</exception>
        /// <returns>A GrammarSymbol containing the quantifier.</returns>
        public GrammarSymbol ParseQuantifier()
        {
            GrammarSymbol nonTerminal = this.ParseNonTerminal();
            Token current = _iterator.Current;
            SymbolKind quantifierKind;
            switch (current.Kind)
            {
                case TokenKind.Star:
                    quantifierKind = SymbolKind.ZeroOrMoreQuantifier;
                    break;
                case TokenKind.Plus:
                    quantifierKind = SymbolKind.OneOrMoreQuantifier;
                    break;
                case TokenKind.Question:
                    quantifierKind = SymbolKind.ZeroOrOneQuantifier;
                    break;
                default:
                    quantifierKind = SymbolKind.Default;
                    break;
            }

            if (quantifierKind != SymbolKind.Default)
            {
                _iterator.Move();
                return new GrammarSymbol(nonTerminal.FirstToken, current, quantifierKind, ImmutableArray.Create(nonTerminal));
            }
            else
            {
                return nonTerminal;
            }
        }

        /// <summary>Parses a G4 non-terminal.</summary>
        /// <exception cref="G4ParseFailureException">Thrown when the input is not valid.</exception>
        /// <returns>A GrammarSymbol containing the non-terminal.</returns>
        public GrammarSymbol ParseNonTerminal()
        {
            ImmutableArray<Annotation> annotations = _iterator.ConsumeAnnotations();
            Token current = _iterator.Consume();
            if (current.Kind == TokenKind.Identifier)
            {
                return new GrammarSymbol(current, SymbolKind.Identifier, annotations);
            }

            if (current.Kind == TokenKind.Lparen)
            {
                GrammarSymbol result = this.ParseAlternation();
                _iterator.Require(TokenKind.Rparen);
                return result;
            }

            if (current.Kind != TokenKind.String)
            {
                throw G4ParseFailureException.UnexpectedToken(current);
            }

            if (_iterator.Current.Kind == TokenKind.Dots)
            {
                CheckForBadCharacterRangeCharacterLength(current);
                _iterator.Move(); // dots
                Token secondString = _iterator.Require(TokenKind.String);
                CheckForBadCharacterRangeCharacterLength(secondString);
                CheckForCorrectlyOrderedCharacterRange(current, secondString);
                return new GrammarSymbol(current, secondString, SymbolKind.CharacterRange);
            }
            else
            {
                return new GrammarSymbol(current, SymbolKind.String);
            }
        }

        private static void CheckForCorrectlyOrderedCharacterRange(Token firstString, Token lastString)
        {
            char first = firstString[1];
            char last = lastString[1];
            if (first > last)
            {
                throw new G4ParseFailureException(firstString.GetLocation(), Strings.FirstCharacterInCharacterRangeMustBeLower);
            }
        }

        private static void CheckForBadCharacterRangeCharacterLength(Token current)
        {
            if (current.Length != 3)
            {
                throw new G4ParseFailureException(current.GetLocation(), Strings.CharacterRangeBadCharacterCount);
            }
        }
    }
}
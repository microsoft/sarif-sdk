// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.SecurityDevelopmentLifecycle.SdlCommon;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public class AntlrParser
    {
        private enum ParserState
        {
            None, 
            ReadyForLhs, 
            ReadyForRhs, 
            LhsRead, 
        }

        public Grammar Parse(string text)
        {
            IEnumerable<Token> tokens = Token.GetTokens(text);
            Contract.Assert(tokens != null);
            return this.Parse(tokens, text);
        }

        private static Stack<KeyValuePair<Group, int>> DecrementStack(
            Stack<KeyValuePair<Group, int>> stack, 
            int sizeDelta)
        {
            if (stack.Count == 0) { return stack; }

            var result = new Stack<KeyValuePair<Group, int>>();
            foreach (KeyValuePair<Group, int> pair in stack.Reverse())
            {
                result.Push(new KeyValuePair<Group, int>(pair.Key, pair.Value - sizeDelta));
            }

            return result;
        }

        private IEnumerable<IGrammarSymbol> CreateRHS(IList<Token> rhsTokens)
        {
            var result = new LinkedList<IGrammarSymbol>();
            var stack = new Stack<KeyValuePair<Group, int>>();
            Alternative alternative = null;
            string annotation = null;

            Dictionary<string,string> annotationDictionary = new Dictionary<string, string>();

            for (int i = 0; i < rhsTokens.Count; i++)
            {
                Token token = rhsTokens[i];

                switch (token.Kind)
                {
                    case Token.TokenKind.Annotation:
                        {
                            annotation = token.Value;
                            string[] split = token.Value.Split(new[] { ':' });
                            if (split.Count() == 2)
                            {
                                annotationDictionary[split[0]] = split[1];
                            }

                            break;
                        }

                    case Token.TokenKind.Identifier:
                        {
                            string nameToUse = annotation != null ? annotation : token.Value;
                            string typeToUse = token.Value;
                            bool isTypeFromGrammar = false;

                            if (annotationDictionary.ContainsKey("name"))
                            {
                                nameToUse = annotationDictionary["name"];
                            }

                            if (annotationDictionary.ContainsKey("type"))
                            {
                                typeToUse = annotationDictionary["type"];
                                isTypeFromGrammar = true;
                            }

                            var nonterminal = new NonTerminal
                                                  {
                                                      Name = nameToUse, 
                                                      GrammarType = token.Value, 
                                                      Type = typeToUse,
                                                      IsTypeFromGrammar = isTypeFromGrammar,
                                                      Annotations = new Dictionary<string, string> (annotationDictionary)
                                                  };
                            annotation = null;
                            annotationDictionary.Clear();

                            result.AddLast(nonterminal);
                            if (stack.Count > 0)
                            {
                                stack.Peek().Key.Symbols.Add(nonterminal);
                            }

                            break;
                        }

                    case Token.TokenKind.String:
                        {
                            var terminal = new Terminal { Value = token.Value };
                            result.AddLast(terminal);
                            if (stack.Count > 0)
                            {
                                stack.Peek().Key.Symbols.Add(terminal);
                            }

                            break;
                        }

                    case Token.TokenKind.Lparen:
                        {
                            var group = new Group();
                            stack.Push(new KeyValuePair<Group, int>(group, i));
                            annotation = null;
                            annotationDictionary.Clear();
                            break;
                        }

                    case Token.TokenKind.Rparen:
                        {
                            // done with the group
                            KeyValuePair<Group, int> groupAndMarker = stack.Pop();
                            Group group = groupAndMarker.Key;
                            int marker = groupAndMarker.Value;

                            Contract.Assert(group.Symbols.Count > 0);
                            for (int j = 0; j < group.Symbols.Count; j++)
                            {
                                result.RemoveLast();
                            }

                            result.AddLast(group);
                            annotation = null;
                            annotationDictionary.Clear();
                            break;
                        }

                    case Token.TokenKind.Plus:
                        {
                            // done with the group
                            IGrammarSymbol prev = result.Last.Value;
                            result.RemoveLast();
                            var plus = new Plus { Symbol = prev };
                            result.AddLast(plus);
                            if (stack.Count > 0)
                            {
                                stack.Peek().Key.Symbols.Add(plus);
                            }

                            annotation = null;
                            annotationDictionary.Clear();
                            break;
                        }

                    case Token.TokenKind.Star:
                        {
                            // done with the group
                            IGrammarSymbol prev = result.Last.Value;
                            result.RemoveLast();
                            var star = new Star { Symbol = prev };
                            result.AddLast(star);
                            if (stack.Count > 0)
                            {
                                stack.Peek().Key.Symbols.Add(star);
                            }

                            annotation = null;
                            annotationDictionary.Clear();
                            break;
                        }

                    case Token.TokenKind.Pipe:
                        {
                            if (alternative == null)
                            {
                                string typeToUse = "string";

                                if (annotationDictionary.ContainsKey("type"))
                                {
                                    typeToUse = annotationDictionary["type"];
                                }

                                alternative = new Alternative { Type = typeToUse };
                            }

                            var group = new Group { Symbols = result.Where(e => !(e is Alternative)).ToList() };
                            if (group.Symbols.Count > 0)
                            {
                                alternative.Symbols.Add(group);
                            }

                            result.Clear();
                            result.AddLast(alternative);
                            Contract.Assert(result.Count == 1);

                            annotation = null;
                            annotationDictionary.Clear();
                            break;
                        }

                    case Token.TokenKind.Arrow:
                        annotation = null;
                        break;
                }
            }

            if (alternative != null)
            {
                var group = new Group { Symbols = result.Where(e => !(e is Alternative)).ToList() };
                if (group.Symbols.Count > 0)
                {
                    alternative.Symbols.Add(group);
                }

                result.Clear();
                result.AddLast(alternative);
            }

            return result;
        }

        private Grammar Parse(IEnumerable<Token> tokens, string text)
        {
            List<Token> list = tokens.ToList();
            NewLineIndex newLines = new NewLineIndex(text);

            Grammar grammar = null;
            ParserState state = ParserState.None;
            Production production = null;
            List<Token> rhsTokens = null;
            string grammarNamespace = "Undefined";

            for (int i = 0; i < list.Count; i++)
            {
                Token token = list[i];
                Token next = (i < list.Count - 1) ? list[i + 1] : null;

                switch (state)
                {
                    case ParserState.None:
                        if (token.Kind == Token.TokenKind.Identifier)
                        {
                            if (token.Value == "grammar")
                            {
                                if (next != null)
                                {
                                    grammar = new Grammar
                                                  {
                                                      Name = next.Value,
                                                      GrammarNamespace = grammarNamespace
                                                  };
                                    i++;
                                }

                                next = (i < list.Count - 1) ? list[i + 1] : null;
                                Contract.Assert(next != null && next.Kind == Token.TokenKind.Semicolon);
                                i++;
                                state = ParserState.ReadyForLhs;
                                production = new Production();
                            }
                        }
                        else if (token.Kind == Token.TokenKind.Annotation)
                        {
                            string[] split = token.Value.Split(new[] { ':' });
                            if (split.Count() == 2)
                            {
                                if (split[0].Equals("namespace"))
                                {
                                    grammarNamespace = split[1];
                                }
                            }
                        }
                        break;
                    case ParserState.ReadyForLhs:
                        Contract.Assert(
                            token.Kind == Token.TokenKind.Identifier, 
                            String.Format(
                                "Expected an identifier at location {0}, found {1}", 
                                newLines.GetOffsetInfoForOffset(token.Offset), 
                                token.Kind));
                        if (token.Value == "fragment")
                        {
                            while (list[i].Kind != Token.TokenKind.Semicolon)
                            {
                                i++;
                            }
                        }
                        else
                        {
                            if (production != null)
                            {
                                production.LHS = new NonTerminal { Name = token.Value, Type = token.Value, };
                                production.Location = newLines.GetOffsetInfoForOffset(token.Offset);
                            }

                            Contract.Assert(
                                next != null && next.Kind == Token.TokenKind.Colon, 
                                String.Format(
                                    "Expected a colon at {0}, found {1}",
                                    newLines.GetOffsetInfoForOffset(token.Offset), 
                                    token));
                            i++;
                            rhsTokens = new List<Token>();
                            state = ParserState.ReadyForRhs;
                        }

                        break;
                    case ParserState.ReadyForRhs:
                        if (token.Kind != Token.TokenKind.Semicolon)
                        {
                            Contract.Assert(rhsTokens != null);
                            rhsTokens.Add(token);
                        }
                        else
                        {
                            if (production != null)
                            {
                                production.RHS = this.CreateRHS(rhsTokens).ToList();
                                if (grammar != null)
                                {
                                    grammar.AddProductionRaw(production);
                                }

                                state = ParserState.ReadyForLhs;
                            }

                            production = new Production();
                        }

                        break;
                }
            }

            return grammar;
        }
    }
}
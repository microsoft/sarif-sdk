// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    internal class RHSBuilder
    {
        private readonly Dictionary<string, PropertyInfo> nameToIdentifier;

        internal RHSBuilder(Production production)
        {
            Contract.Requires(!production.IsAlternatingClassDeclaration());
            this.nameToIdentifier = new Dictionary<string, PropertyInfo>();
            this.Production = production;
            this.LHS = production.LHS;
            this.Symbols = production.RHS;
        }

        public IEnumerable<string> DeclaredClasses
        {
            get
            {
                return this.nameToIdentifier.Keys;
            }
        }

        private IGrammarSymbol LHS { get; set; }

        private Production Production { get; set; }
        private IEnumerable<IGrammarSymbol> Symbols { get; set; }

        public override string ToString()
        {
            return String.Format("RHSBuilder for {0}", String.Join(", ", this.Symbols));
        }

        internal static bool IsConstantAlternative(Alternative alternative)
        {
            Contract.Requires(alternative.Symbols.Count > 0);

            foreach (IGrammarSymbol sym in alternative.Symbols)
            {
                if (!(sym is Terminal)
                    && !(sym is Group && ((Group)sym).Symbols.Count == 1 && ((Group)sym).Symbols.First() is Terminal))
                {
                    return false;
                }
            }

            return true;
        }

        internal SyntaxList<StatementSyntax> CreateSetPropertyStatements()
        {
            this.ToProperties();
            var list = new SyntaxList<StatementSyntax>();
            if (!this.Production.IsAlternatingClassDeclaration())
            {
                foreach (IGrammarSymbol sym in this.Symbols)
                {
                    StatementSyntax statement = this.SetPropertyIfSymbol(sym);
                    if (statement != null)
                    {
                        list = list.Add(statement);
                    }
                }
            }

            return list;
        }

        // GetPropertyNames
        internal IEnumerable<string> GetPropertyNames()
        {
            Contract.Requires(!this.Production.IsAlternatingClassDeclaration());
            List<string> names = new List<string>();

            foreach (IGrammarSymbol sym in this.Symbols)
            {
                var name = sym as NonTerminal;
                if (name != null)
                {
                    names.Add(Common.ToProperCase(name.Name));
                }
            }

            // properties.AddRange(this.nameToIdentifier.Keys);
            return names;
        }

        internal IEnumerable<PropertyInfo> ToProperties()
        {
            Contract.Requires(!this.Production.IsAlternatingClassDeclaration());

            foreach (IGrammarSymbol sym in this.Symbols)
            {
                this.ProcessSymbol(sym);
            }

            // properties.AddRange(this.nameToIdentifier.Keys);
            return this.nameToIdentifier.Values;
        }

        internal IEnumerable<StatementSyntax> ToSerializeProperties()
        {
            this.ToProperties();
            var list = new SyntaxList<StatementSyntax>();
            if (!this.Production.IsAlternatingClassDeclaration())
            {
                foreach (IGrammarSymbol sym in this.Symbols)
                {
                    StatementSyntax statement = this.SerializeSymbol(sym);
                    if (statement != null)
                    {
                        list = list.Add(statement);
                    }
                }
            }

            return list;
        }

        internal IEnumerable<StatementSyntax> ToVisitProperties(bool rewriting)
        {
            this.ToProperties();
            var list = new SyntaxList<StatementSyntax>();
            if (!this.Production.IsAlternatingClassDeclaration())
            {
                foreach (IGrammarSymbol sym in this.Symbols)
                {
                    StatementSyntax statement = this.VisitSymbol(sym, rewriting);
                    if (statement != null)
                    {
                        list = list.Add(statement);
                    }
                }
            }

            return list;
        }

        private static StatementSyntax CreateIfForEach(NonTerminal term, bool rewriting)
        {
            StatementSyntax statement =
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.NotEqualsExpression, 
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, 
                            SyntaxFactory.IdentifierName(@"node"), 
                            SyntaxFactory.IdentifierName(Common.ToProperCase(term.Name))), 
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)), 
                    SyntaxFactory.Block(CreatePropertyInteration(term, rewriting)));
            return statement;
        }

        private static StatementSyntax CreatePropertyInteration(NonTerminal term, bool rewriting)
        {
            StatementSyntax result;

            string typeToUse = term.Type != Common.IdentifierName ? term.Type : "Identifier";

            if (!rewriting)
            {
                result =
                    SyntaxFactory.ForEachStatement(
                        SyntaxFactory.IdentifierName(@"var"), 
                        SyntaxFactory.Identifier(@"item"), 
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, 
                            SyntaxFactory.IdentifierName(@"node"), 
                            SyntaxFactory.IdentifierName(Common.ToProperCase(term.Name)))
                            .WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken)), 
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, 
                                            SyntaxFactory.ThisExpression()
                                                .WithToken(SyntaxFactory.Token(SyntaxKind.ThisKeyword)), 
                                            SyntaxFactory.IdentifierName(@"Visit"))
                                            .WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken)))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"item"))))
                                                .WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                                                .WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken))))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))))
                            .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                            .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken)))
                        .WithForEachKeyword(SyntaxFactory.Token(SyntaxKind.ForEachKeyword))
                        .WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                        .WithInKeyword(SyntaxFactory.Token(SyntaxKind.InKeyword))
                        .WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken));
            }
            else
            {
                result =
                    SyntaxFactory.ForStatement(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.SimpleAssignmentExpression, 
                                        SyntaxFactory.ElementAccessExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                SyntaxFactory.IdentifierName(@"node"), 
                                                SyntaxFactory.IdentifierName(Common.ToProperCase(term.Name))))
                                            .WithArgumentList(
                                                SyntaxFactory.BracketedArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"index"))))), 
                                        SyntaxFactory.CastExpression(
                                            SyntaxFactory.IdentifierName(Common.ToProperCase(typeToUse)), 
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.ThisExpression(), 
                                                    SyntaxFactory.IdentifierName(@"Visit")))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.ElementAccessExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression, 
                                                                        SyntaxFactory.IdentifierName(@"node"), 
                                                                        SyntaxFactory.IdentifierName(
                                                                            Common.ToProperCase(term.Name))))
                                                                    .WithArgumentList(
                                                                        SyntaxFactory.BracketedArgumentList(
                                                                            SyntaxFactory
                                                                                .SingletonSeparatedList<ArgumentSyntax>(
                                                                                    SyntaxFactory.Argument(
                                                                                        SyntaxFactory.IdentifierName(
                                                                                            @"index")))))))))))))))
                        .WithDeclaration(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                                .WithVariables(
                                    SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"index"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression, 
                                                        SyntaxFactory.Literal(
                                                            SyntaxFactory.TriviaList(), 
                                                            @"0", 
                                                            0, 
                                                            SyntaxFactory.TriviaList())))))))
                        .WithCondition(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.LessThanExpression, 
                                SyntaxFactory.IdentifierName(@"index"), 
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression, 
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, 
                                        SyntaxFactory.IdentifierName(@"node"), 
                                        SyntaxFactory.IdentifierName(Common.ToProperCase(term.Name))), 
                                    SyntaxFactory.IdentifierName(@"Count"))))
                        .WithIncrementors(
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.PostfixUnaryExpression(
                                    SyntaxKind.PostIncrementExpression, 
                                    SyntaxFactory.IdentifierName(@"index"))));
            }

            return result;
        }

        private static SyntaxList<StatementSyntax> CreateSerializeChildStatement(NonTerminal star)
        {
            SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();

            var statement = new StatementSyntax[]
                                {
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                SyntaxFactory.IdentifierName(@"sb"), 
                                                SyntaxFactory.IdentifierName(@"Append")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression, 
                                                            SyntaxFactory.Literal(
                                                                SyntaxFactory.TriviaList(), 
                                                                "\"\\\"" + star.Name + "\\\" : \"", 
                                                                "\"\\\"" + star.Name + "\\\" : \"", 
                                                                SyntaxFactory.TriviaList()))))))), 
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                SyntaxFactory.IdentifierName(@"sb"), 
                                                SyntaxFactory.IdentifierName(@"Append")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.InvocationExpression(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                                SyntaxFactory.ThisExpression(), 
                                                                SyntaxFactory.IdentifierName(@"Visit")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression, 
                                                            SyntaxFactory.IdentifierName(@"node"), 
                                                            SyntaxFactory.IdentifierName(Common.ToProperCase(star.Name))))))))))))
                                };
            statements = statements.AddRange(statement);

            return statements;

        }

        private static ExpressionSyntax CreateVisitChildStatement(NonTerminal star, bool rewriting)
        {
            ExpressionSyntax result;

            if (rewriting)
            {
                string typeToUse = star.Type != Common.IdentifierName ? star.Type : "Identifier";
                result = SyntaxFactory.BinaryExpression(
                    SyntaxKind.SimpleAssignmentExpression, 
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression, 
                        SyntaxFactory.IdentifierName(@"node"), 
                        SyntaxFactory.IdentifierName(Common.ToProperCase(star.Name))), 
                    SyntaxFactory.CastExpression(
                        SyntaxFactory.IdentifierName(Common.ToProperCase(typeToUse)), 
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression, 
                                SyntaxFactory.ThisExpression(), 
                                SyntaxFactory.IdentifierName(@"Visit")))
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                SyntaxFactory.IdentifierName(@"node"), 
                                                SyntaxFactory.IdentifierName(Common.ToProperCase(star.Name)))))))));
            }
            else
            {
                result =
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, 
                            SyntaxFactory.ThisExpression(), 
                            SyntaxFactory.IdentifierName(@"Visit")))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, 
                                            SyntaxFactory.IdentifierName(@"node"), 
                                            SyntaxFactory.IdentifierName(Common.ToProperCase(star.Name)))))));
            }

            return result;
        }

        private static StatementSyntax SerializeIfForEach(NonTerminal term)
        {
            StatementSyntax statement =
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.NotEqualsExpression, 
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, 
                            SyntaxFactory.IdentifierName(@"node"), 
                            SyntaxFactory.IdentifierName(Common.ToProperCase(term.Name))), 
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)), 
                    SyntaxFactory.Block(SerializePropertyIteration(term, false)));
            return statement;
        }

        private static StatementSyntax SerializePropertyIteration(NonTerminal term, bool rewriting)
        {
            string methodToCall = (term.Type.Equals(Common.StringName)) ? "WriteLiteralCollection" : 
                               (term.GrammarType.Equals(Common.DictionaryName)) ? "WriteDictionary" :
                               (term.IsTypeFromGrammar) ? "WriteObjectCollection" : "WriteCollection";

            StatementSyntax result =
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, 
                            SyntaxFactory.IdentifierName(@"sb"), 
                            SyntaxFactory.IdentifierName(@"Append")))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                SyntaxFactory.ThisExpression(), 
                                                SyntaxFactory.IdentifierName(methodToCall)))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                            {
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.LiteralExpression(
                                                                        SyntaxKind.StringLiteralExpression, 
                                                                        SyntaxFactory.Literal(
                                                                            SyntaxFactory.TriviaList(), 
                                                                            "\"" + term.Name.ToLower() + "\"", 
                                                                            "\"" + term.Name.ToLower() + "\"", 
                                                                            SyntaxFactory.TriviaList()))), 
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression, 
                                                                        SyntaxFactory.IdentifierName(@"node"), 
                                                                        SyntaxFactory.IdentifierName(
                                                                            Common.ToProperCase(term.Name))))
                                                            }))))))));

            return result;
        }

        private void MakeAlternative(Alternative alternative)
        {
            // support only two cases: alternative of string constants
            // and of plain statements
            if (IsConstantAlternative(alternative))
            {
                PropertyDeclarationSyntax result = this.MakeIdentifierProperty("StringValue", false, "string");
                this.nameToIdentifier.Add("StringValue", new PropertyInfo { Syntax = result, IsList = false, });
            }
            else
            {
                Contract.Assert(
                    false, 
                    "Unexpected alternative on " + this.Production.Location
                    + "Support only two cases: alternative of string constants and of plain statements");
            }
        }

        private TypeSyntax MakeArrayType()
        {
            return
                SyntaxFactory.ArrayType(SyntaxFactory.IdentifierName("Identifier"))
                    .WithRankSpecifiers(
                        SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                            SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression()))));
        }

        private PropertyDeclarationSyntax MakeClassProperty(NonTerminal nonterminal, bool isList)
        {
            Contract.Requires(nonterminal != null);
            Contract.Requires(nonterminal.Type != null);
            Contract.Requires(
                !this.nameToIdentifier.ContainsKey(nonterminal.Name), 
                "Alraedy have a key " + nonterminal.Name + " while translating " + this.ToString());

            PropertyDeclarationSyntax result =
                SyntaxFactory.PropertyDeclaration(
                    isList ? this.MakeListType(nonterminal.Type) : SyntaxFactory.IdentifierName(nonterminal.Type),
                    this.MakeIdentifier(nonterminal.Name))
                    .WithAttributeLists(
                        SyntaxFactory.SingletonList<AttributeListSyntax>(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(@"DataMember"))
                                        .WithArgumentList(
                                            SyntaxFactory.AttributeArgumentList(
                                                SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.TrueLiteralExpression))
                                                                .WithNameEquals(
                                                                    SyntaxFactory.NameEquals(
                                                                        SyntaxFactory.IdentifierName(
                                                                            @"EmitDefaultValue"))),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression,
                                                                    SyntaxFactory.Literal(
                                                                        SyntaxFactory.TriviaList(),
                                                                        "\"" + nonterminal.Name + "\"",
                                                                        "\"" + nonterminal.Name + "\"",
                                                                        SyntaxFactory.TriviaList())))
                                                                .WithNameEquals(
                                                                    SyntaxFactory.NameEquals(
                                                                        SyntaxFactory.IdentifierName(@"Name")))
                                                        })))))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List<AccessorDeclarationSyntax>(
                                new[]
                                    {
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)

                                            // .WithModifiers(
                                            // SyntaxFactory.TokenList(
                                            // SyntaxFactory.Token(
                                            // SyntaxKind.PrivateKeyword)))
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                    })));

            return result;
        }

        private SyntaxToken MakeIdentifier(string name)
        {
            return SyntaxFactory.Identifier(Common.ToProperCase(name));
        }

        private PropertyDeclarationSyntax MakeIdentifierProperty(
            string name, 
            bool isList, 
            string typeName = "Identifier")
        {
            Contract.Requires(name != null);
            Contract.Requires(typeName != null);
            Contract.Requires(typeName != Common.IdentifierName);
            Contract.Requires(
                !this.nameToIdentifier.ContainsKey(name), 
                "Already have a key " + name + " while translating " + this.ToString());

            PropertyDeclarationSyntax result =
                SyntaxFactory.PropertyDeclaration(
                    isList ? this.MakeListType() : SyntaxFactory.IdentifierName(typeName), 
                    this.MakeIdentifier(name))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List<AccessorDeclarationSyntax>(
                                new[]
                                    {
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)), 
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)

                                            // .WithModifiers(
                                            // SyntaxFactory.TokenList(
                                            // SyntaxFactory.Token(
                                            // SyntaxKind.PrivateKeyword)))
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                    })));

            return result;
        }

        private TypeSyntax MakeListType(string className = "Identifier")
        {
            return
                SyntaxFactory.GenericName(SyntaxFactory.Identifier("IList"))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName(className))));
        }

        private PropertyDeclarationSyntax MakeDictionaryProperty(NonTerminal nonterminal)
        {
            string name = nonterminal.Name ?? Common.DictionaryName;
            string type = nonterminal.Type.Equals(Common.DictionaryName) ? "string" : nonterminal.Type;
            Contract.Requires(
                !this.nameToIdentifier.ContainsKey(name),
                "Already have a key " + name + " while translating " + this.ToString());

            PropertyDeclarationSyntax result =
                SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier(Common.GrammarDictionary))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                    new SyntaxNodeOrToken[]
                                        {
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                            SyntaxFactory.IdentifierName(type)

                                        }))),
                    SyntaxFactory.Identifier(name))
                    .WithAttributeLists(
                        SyntaxFactory.SingletonList<AttributeListSyntax>(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(@"DataMember"))
                                        .WithArgumentList(
                                            SyntaxFactory.AttributeArgumentList(
                                                SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.TrueLiteralExpression))
                                                                .WithNameEquals(
                                                                    SyntaxFactory.NameEquals(
                                                                        SyntaxFactory.IdentifierName(
                                                                            @"EmitDefaultValue"))),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression,
                                                                    SyntaxFactory.Literal(
                                                                        SyntaxFactory.TriviaList(),
                                                                        "\"" + nonterminal.Name + "\"",
                                                                        "\"" + nonterminal.Name + "\"",
                                                                        SyntaxFactory.TriviaList())))
                                                                .WithNameEquals(
                                                                    SyntaxFactory.NameEquals(
                                                                        SyntaxFactory.IdentifierName(@"Name")))
                                                        })))))))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List<AccessorDeclarationSyntax>(
                                new AccessorDeclarationSyntax[]
                                    {
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                    })));
            return result;

        }

        private PropertyDeclarationSyntax MakeLiteralProperty(NonTerminal nonterminal, bool isList)
        {
            string name = nonterminal.Name ?? Common.StringName;
            Contract.Requires(
                !this.nameToIdentifier.ContainsKey(name), 
                "Already have a key " + name + " while translating " + this.ToString());

            PredefinedTypeSyntax type = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
            PropertyDeclarationSyntax result =
                SyntaxFactory.PropertyDeclaration(
                    isList ? this.MakeListType("string") : type, 
                    this.MakeIdentifier(name))
                    .WithAttributeLists(
                        SyntaxFactory.SingletonList<AttributeListSyntax>(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(@"DataMember"))
                                        .WithArgumentList(
                                            SyntaxFactory.AttributeArgumentList(
                                                SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.TrueLiteralExpression))
                                                                .WithNameEquals(
                                                                    SyntaxFactory.NameEquals(
                                                                        SyntaxFactory.IdentifierName(
                                                                            @"EmitDefaultValue"))),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression,
                                                                    SyntaxFactory.Literal(
                                                                        SyntaxFactory.TriviaList(),
                                                                        "\"" + nonterminal.Name + "\"",
                                                                        "\"" + nonterminal.Name + "\"",
                                                                        SyntaxFactory.TriviaList())))
                                                                .WithNameEquals(
                                                                    SyntaxFactory.NameEquals(
                                                                        SyntaxFactory.IdentifierName(@"Name")))
                                                        })))))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List<AccessorDeclarationSyntax>(
                                new[]
                                    {
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)), 
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                    })));

            return result;
        }

        private PropertyDeclarationSyntax MakeNumericProperty(string name, bool isList)
        {
            Contract.Requires(name != null);
            Contract.Requires(
                !this.nameToIdentifier.ContainsKey(name), 
                "Already have a key " + name + " while translating " + this.ToString());

            PredefinedTypeSyntax type = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
            PropertyDeclarationSyntax result =
                SyntaxFactory.PropertyDeclaration(
                    isList ? this.MakeListType("double") : type, 
                    this.MakeIdentifier(name))
                    .WithAttributeLists(
                        SyntaxFactory.SingletonList<AttributeListSyntax>(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(@"DataMember"))
                                        .WithArgumentList(
                                            SyntaxFactory.AttributeArgumentList(
                                                SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.TrueLiteralExpression))
                                                                .WithNameEquals(
                                                                    SyntaxFactory.NameEquals(
                                                                        SyntaxFactory.IdentifierName(
                                                                            @"EmitDefaultValue"))),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression,
                                                                    SyntaxFactory.Literal(
                                                                        SyntaxFactory.TriviaList(),
                                                                        "\"" + name + "\"",
                                                                        "\"" + name + "\"",
                                                                        SyntaxFactory.TriviaList())))
                                                                .WithNameEquals(
                                                                    SyntaxFactory.NameEquals(
                                                                        SyntaxFactory.IdentifierName(@"Name")))
                                                        })))))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List<AccessorDeclarationSyntax>(
                                new[]
                                    {
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)), 
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                    })));

            return result;
        }

        private void MakeStructural(StructuralSymbol star)
        {
            if (star.Symbol is NonTerminal)
            {
                string name = (star.Symbol as NonTerminal).Name;
                if (this.nameToIdentifier.ContainsKey(name))
                {
                    PropertyInfo prop = this.nameToIdentifier[name];
                    if (!(prop.Syntax.Type is ArrayTypeSyntax))
                    {
                        this.nameToIdentifier.Remove(name);
                    }
                    else
                    {
                        return;
                    }
                }

                PropertyDeclarationSyntax listProp = this.NonterminalToProperty(star.Symbol as NonTerminal, true);

                this.nameToIdentifier[name] = new PropertyInfo { Syntax = listProp, IsList = true, };
            }
            else if (star.Symbol is Group)
            {
                this.PatchStarProperties(star.Symbol as Group);
            }
            else
            {
                Contract.Assert(false, "Unexpected type " + star.Symbol);
            }
        }

        private PropertyDeclarationSyntax NonterminalToProperty(NonTerminal nonterminal, bool isList)
        {
            Contract.Requires(!Common.IsReserved(nonterminal.Type));

            if (Common.IsIdentifier(nonterminal))
            {
                PropertyDeclarationSyntax result = this.MakeIdentifierProperty(nonterminal.Name, isList);
                Contract.Assert(
                    !this.nameToIdentifier.ContainsKey(nonterminal.Name), 
                    "Didn't expect property by name " + nonterminal.Name + " while processing " + this.ToString());
                this.nameToIdentifier.Add(nonterminal.Name, new PropertyInfo { Syntax = result, IsList = false });
                return result;
            }
            else if (Common.IsLiteral(nonterminal.Type))
            {
                PropertyDeclarationSyntax result = this.MakeLiteralProperty(nonterminal, isList);
                Contract.Assert(
                    !this.nameToIdentifier.ContainsKey(nonterminal.Name), 
                    "Didn't expect property by name " + nonterminal.Name + " while processing " + this.ToString());
                this.nameToIdentifier.Add(nonterminal.Name, new PropertyInfo { Syntax = result, IsList = false });
                return result;
            }
            else if (Common.IsNumeric(nonterminal.Type))
            {
                PropertyDeclarationSyntax result = this.MakeNumericProperty(nonterminal.Name, false);
                Contract.Assert(
                    !this.nameToIdentifier.ContainsKey(nonterminal.Name), 
                    "Didn't expect property by name " + nonterminal.Name + " while processing " + this.ToString());
                this.nameToIdentifier.Add(nonterminal.Name, new PropertyInfo { Syntax = result, IsList = false });
                return result;
            }
            else if (Common.IsDictionary(nonterminal.GrammarType))
            {
                PropertyDeclarationSyntax result = this.MakeDictionaryProperty(nonterminal);
                Contract.Assert(
                    !this.nameToIdentifier.ContainsKey(nonterminal.Name),
                    "Didn't expect property by name " + nonterminal.Name + " while processing " + this.ToString());
                this.nameToIdentifier.Add(nonterminal.Name, new PropertyInfo { Syntax = result, IsList = false });
                return result;

            }
            else
            {
                Contract.Assert(nonterminal.Name != Common.IdentifierName);

                // dealing with a specialized class -- expand the class reference
                PropertyDeclarationSyntax result = this.MakeClassProperty(nonterminal, isList);
                Contract.Assert(
                    !this.nameToIdentifier.ContainsKey(nonterminal.Name), 
                    "Already have key " + nonterminal.Name + " at " + this.Production.Location);
                this.nameToIdentifier.Add(nonterminal.Name, new PropertyInfo { Syntax = result, IsList = false });

                return result;
            }
        }

        private void PatchStarProperties(Group group)
        {
            foreach (IGrammarSymbol sym in group.Symbols)
            {
                if (sym is NonTerminal)
                {
                    var nonterminal = sym as NonTerminal;
                    if (Common.IsIdentifier(nonterminal))
                    {
                        string name = (sym as NonTerminal).Name;
                        Contract.Assert(
                            this.nameToIdentifier.ContainsKey(name), 
                            "Missing declaration for " + sym + " at " + this.Production.Location);
                        PropertyInfo prop = this.nameToIdentifier[name];
                        if (!(prop.Syntax.Type is ArrayTypeSyntax))
                        {
                            this.nameToIdentifier.Remove(name);
                        }

                        PropertyDeclarationSyntax listProp = this.MakeIdentifierProperty(name, true);
                        this.nameToIdentifier[name] = new PropertyInfo { Syntax = listProp, IsList = true, };
                    }
                    else
                    {
                        Contract.Assert(nonterminal.Name != Common.IdentifierName);
                        PropertyDeclarationSyntax prop = this.MakeClassProperty(nonterminal, true);
                        if (!(prop.Type is ArrayTypeSyntax))
                        {
                            this.nameToIdentifier.Remove(nonterminal.Name);
                        }

                        Contract.Assert(
                            !this.nameToIdentifier.ContainsKey(nonterminal.Name), 
                            "Already have key " + nonterminal.Name + " at " + this.Production.Location);
                        this.nameToIdentifier.Add(nonterminal.Name, new PropertyInfo { Syntax = prop, IsList = true, });
                    }
                }
                else if (sym is Terminal)
                {
                    // nothing to do    
                }
                else
                {
                    Contract.Assert(false, "Unsupported structure " + sym);
                }
            }
        }

        private void ProcessSymbol(IGrammarSymbol sym)
        {
            if (sym is NonTerminal)
            {
                this.NonterminalToProperty(sym as NonTerminal, false);
            }
            else if (sym is Star)
            {
                var star = sym as Star;
                this.MakeStructural(star);
            }
            else if (sym is Plus)
            {
                var star = sym as Plus;
                this.MakeStructural(star);
            }
            else if (sym is QuestionMark)
            {
                var question = sym as QuestionMark;
                this.MakeStructural(question);
            }
            else if (sym is Alternative)
            {
                var alternative = sym as Alternative;
                this.MakeAlternative(alternative);
            }
        }

        private StatementSyntax SerializeSymbol(IGrammarSymbol sym)
        {
            StatementSyntax statement = null;

            if (sym is NonTerminal)
            {
                var star = sym as NonTerminal;

                PropertyInfo propertyInfo = this.nameToIdentifier[star.Name];
                if (propertyInfo != null)
                {
                    if (star.Type.Equals(Common.StringName))
                    {
                        statement =
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(@"sb"),
                                        SyntaxFactory.IdentifierName(@"Append")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.BinaryExpression(
                                                        SyntaxKind.AddExpression,
                                                        SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            SyntaxFactory.Literal(
                                                                SyntaxFactory.TriviaList(),
                                                                @""", """,
                                                                @""", """,
                                                                SyntaxFactory.TriviaList())),
                                                        SyntaxFactory.InvocationExpression(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.ThisExpression(),
                                                                SyntaxFactory.IdentifierName(@"WriteProperty")))
                                                            .WithArgumentList(
                                                                SyntaxFactory.ArgumentList(
                                                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                        new SyntaxNodeOrToken[]
                                                                            {
                                                                                SyntaxFactory.Argument(
                                                                                    SyntaxFactory.LiteralExpression(
                                                                                        SyntaxKind
                                                                                    .StringLiteralExpression,
                                                                                        SyntaxFactory.Literal(
                                                                                            SyntaxFactory.TriviaList(),
                                                                                            "\"" + star.Name.ToLower() + "\"",
                                                                                            "\"" + star.Name.ToLower() + "\"",
                                                                                            SyntaxFactory.TriviaList()))),
                                                                                SyntaxFactory.Token(
                                                                                    SyntaxKind.CommaToken),
                                                                                SyntaxFactory.Argument(
                                                                                    SyntaxFactory.MemberAccessExpression
                                                                                    (
                                                                                        SyntaxKind
                                                                                    .SimpleMemberAccessExpression,
                                                                                        SyntaxFactory.IdentifierName(
                                                                                            @"node"),
                                                                                        SyntaxFactory.IdentifierName(
                                                                                            Common.ToProperCase(star.Name))))
                                                                            })))))))));
                    }
                    else if (star.GrammarType.Equals(Common.DictionaryName))
                    {
                        statement = SerializeIfForEach(star);
                    }
                    else if (!propertyInfo.IsList)
                    {
                        statement =
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.NotEqualsExpression, 
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, 
                                        SyntaxFactory.IdentifierName(@"node"), 
                                        SyntaxFactory.IdentifierName(Common.ToProperCase(star.Name))), 
                                    SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)), 
                                SyntaxFactory.Block(
                                    SyntaxFactory.List<StatementSyntax>(CreateSerializeChildStatement(star))));
                    }
                    else
                    {
                        statement = SerializeIfForEach(star);
                    }
                }
            }
            else if (sym is Star)
            {
                var star = sym as Star;
                var term = star.Symbol as NonTerminal;
                if (term != null && term.Type != Common.IdentifierName && term.Type != Common.NumberName)
                {
                    statement = SerializeIfForEach(term);
                }
            }
            else if (sym is Plus)
            {
                var star = sym as Plus;
                var term = star.Symbol as NonTerminal;
                if (term != null && term.Type != Common.IdentifierName && term.Type != Common.NumberName)
                {
                    statement = SerializeIfForEach(term);
                }
            }
            else if (sym is QuestionMark)
            {
                var question = sym as QuestionMark;
            }
            else if (sym is Alternative)
            {
                var alternative = sym as Alternative;
            }

            return statement;
        }

        private StatementSyntax SetPropertyIfSymbol(IGrammarSymbol sym)
        {
            StatementSyntax statement = null;
            var lhs = this.LHS as NonTerminal;
            PropertyInfo propertyInfo = null;
            NonTerminal nonTerminal = sym as NonTerminal;

            if (nonTerminal == null)
            {
                if (sym is Star)
                {
                    var star = sym as Star;
                    if (star.Symbol is NonTerminal)
                    {
                        nonTerminal = star.Symbol as NonTerminal;
                    }
                }
                else if (sym is Plus)
                {
                    var plus = sym as Plus;
                    if (plus.Symbol is NonTerminal)
                    {
                        nonTerminal = plus.Symbol as NonTerminal;
                    }
                }
            }

            propertyInfo = (nonTerminal != null) ? this.nameToIdentifier[nonTerminal.Name] : null;
             
            if (propertyInfo != null)
            {
                string typeToUse = nonTerminal.Type;

                switch (nonTerminal.GrammarType)
                {
                    case Common.BooleanName:
                        typeToUse = "bool";
                        break;
                    case Common.DictionaryName:
                        string realTypeToUse = typeToUse.Equals(Common.DictionaryName) ? "string" : typeToUse;
                        typeToUse = Common.GrammarDictionary  + "<string, " + realTypeToUse + ">";
                        break;
                    case Common.StringName:
                        typeToUse = typeToUse.Equals(Common.StringName) ? "string" : typeToUse;
                        break;
                    case Common.NumberName:
                        typeToUse = "double";
                        break;
                    case Common.IdentifierName:
                        typeToUse = "Identifier";
                        break;
                    default:
                        typeToUse = nonTerminal.Type;
                        break;
                }

                if (!propertyInfo.IsList || nonTerminal.Type == Common.NumberName) // Lists of DIGIT is not really a list 
                {
                    statement =
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.EqualsExpression,
                                SyntaxFactory.IdentifierName(@"name"),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(
                                        SyntaxFactory.TriviaList(),
                                        "\"" + nonTerminal.Name.ToLower() + "\"",
                                        "\"" + nonTerminal.Name.ToLower() + "\"",
                                        SyntaxFactory.TriviaList()))),
                            SyntaxFactory.Block(
                                SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(Common.ToProperCase(nonTerminal.Name))),
                                            SyntaxFactory.CastExpression(
                                                SyntaxFactory.IdentifierName(typeToUse),
                                                SyntaxFactory.IdentifierName(@"value")))))));
                }
                else
                {
                    if (nonTerminal.IsTypeFromGrammar)
                    {
                        statement =
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.EqualsExpression,
                                    SyntaxFactory.IdentifierName(@"name"),
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(
                                            SyntaxFactory.TriviaList(),
                                            "\"" + nonTerminal.Name.ToLower() + "\"",
                                            "\"" + nonTerminal.Name.ToLower() + "\"",
                                            SyntaxFactory.TriviaList()))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxFactory.IdentifierName(Common.ToProperCase(nonTerminal.Name))),
                                                SyntaxFactory.CastExpression(
                                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"IList"))
                                                        .WithTypeArgumentList(
                                                            SyntaxFactory.TypeArgumentList(
                                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                                    SyntaxFactory.IdentifierName(typeToUse)))),
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.IdentifierName(@"Common"),
                                                            SyntaxFactory.IdentifierName(@"SetGenericListData")))
                                                        .WithArgumentList(
                                                            SyntaxFactory.ArgumentList(
                                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                    new SyntaxNodeOrToken[]
                                                                        {
                                                                            SyntaxFactory.Argument(
                                                                                SyntaxFactory.CastExpression(
                                                                                    SyntaxFactory.GenericName(
                                                                                        SyntaxFactory.Identifier(
                                                                                            @"IEnumerable"))
                                                                                .WithTypeArgumentList(
                                                                                    SyntaxFactory.TypeArgumentList(
                                                                                        SyntaxFactory
                                                                                .SingletonSeparatedList<TypeSyntax>(
                                                                                    SyntaxFactory.PredefinedType(
                                                                                        SyntaxFactory.Token(
                                                                                            SyntaxKind.StringKeyword))))),
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"value"))),
                                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                            SyntaxFactory.Argument(
                                                                                SyntaxFactory.TypeOfExpression(
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        typeToUse)))
                                                                        })))))))));

                    }
                    else
                    {
                        statement =
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.EqualsExpression,
                                    SyntaxFactory.IdentifierName(@"name"),
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(
                                            SyntaxFactory.TriviaList(),
                                            "\"" + nonTerminal.Name.ToLower() + "\"",
                                            "\"" + nonTerminal.Name.ToLower() + "\"",
                                            SyntaxFactory.TriviaList()))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxFactory.IdentifierName(Common.ToProperCase(nonTerminal.Name))),
                                                SyntaxFactory.CastExpression(
                                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"IList"))
                                                        .WithTypeArgumentList(
                                                            SyntaxFactory.TypeArgumentList(
                                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                                    SyntaxFactory.IdentifierName(typeToUse)))),
                                                    SyntaxFactory.IdentifierName(@"value")))))));
                    }
                }
            }

            return statement;
        }

        private StatementSyntax VisitSymbol(IGrammarSymbol sym, bool rewriting)
        {
            StatementSyntax statement = null;

            if (sym is NonTerminal)
            {
                var star = sym as NonTerminal;

                PropertyInfo propertyInfo = this.nameToIdentifier[star.Name];
                if (propertyInfo != null)
                {
                    if (star.Type.Equals(Common.StringName) || star.GrammarType.Equals(Common.DictionaryName))
                    {
                    }
                    else if (!propertyInfo.IsList)
                    {
                        statement =
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.NotEqualsExpression, 
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, 
                                        SyntaxFactory.IdentifierName(@"node"), 
                                        SyntaxFactory.IdentifierName(Common.ToProperCase(star.Name))), 
                                    SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)), 
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ExpressionStatement(CreateVisitChildStatement(star, rewriting)))));
                    }
                    else
                    {
                        statement = CreateIfForEach(star, rewriting);
                    }
                }
            }
            else if (sym is Star)
            {
                var star = sym as Star;
                var term = star.Symbol as NonTerminal;
                if (term != null && term.Type != Common.IdentifierName && term.Type != Common.StringName && term.Type != Common.NumberName && term.IsTypeFromGrammar == false)
                {
                    statement = CreateIfForEach(term, rewriting);
                }
            }
            else if (sym is Plus)
            {
                var star = sym as Plus;
                var term = star.Symbol as NonTerminal;
                if (term != null && term.Type != Common.IdentifierName && term.Type != Common.StringName && term.Type != Common.NumberName && term.IsTypeFromGrammar == false)
                {
                    statement = CreateIfForEach(term, rewriting);
                }
            }
            else if (sym is QuestionMark)
            {
                var question = sym as QuestionMark;
            }
            else if (sym is Alternative)
            {
                var alternative = sym as Alternative;
            }

            return statement;
        }

        internal class PropertyInfo
        {
            internal bool IsList { get; set; }
            internal PropertyDeclarationSyntax Syntax { get; set; }
        }
    }
}
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
    internal class JsonSerializerBuilder
    {
        public static SingleClassCompilationUnit BuildSerializer(
            Grammar grammar, 
            IEnumerable<string> declaredClasses, 
            Dictionary<string, string> classToSuperClass,
            List<DictionaryInformation> dictionaries )
        {
            var classCompilationUnit = new SingleClassCompilationUnit();

            classCompilationUnit.Name = grammar.Name + @"JsonSerializer";
            classCompilationUnit.Syntax =
                SyntaxFactory.CompilationUnit()
                    .WithUsings(BuildUsings(grammar))
                    .WithMembers(BuildClass(grammar, declaredClasses, classToSuperClass, dictionaries));

            return classCompilationUnit;
        }

        private static SyntaxList<MemberDeclarationSyntax> BuildClass(
            Grammar grammar, 
            IEnumerable<string> declaredClasses, 
            Dictionary<string, string> classToSuperClass,
            List<DictionaryInformation> dictionaries )
        {
            return
                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                    SyntaxFactory.NamespaceDeclaration(
                        SyntaxFactory.ParseName(grammar.GrammarNamespace))
                        .WithNamespaceKeyword(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(SyntaxFactory.Space), 
                                SyntaxKind.NamespaceKeyword, 
                                SyntaxFactory.TriviaList(SyntaxFactory.Space)))
                        .WithMembers(
                            SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                                SyntaxFactory.ClassDeclaration(grammar.Name + @"JsonSerializer")
                                    .WithModifiers(
                                        SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                    .WithBaseList(
                                        SyntaxFactory.BaseList(
                                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                SyntaxFactory.GenericName(
                                                    SyntaxFactory.Identifier(grammar.Name + @"Visitor"))
                                                    .WithTypeArgumentList(
                                                        SyntaxFactory.TypeArgumentList(
                                                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                                SyntaxFactory.PredefinedType(
                                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword))))))))
                                    .WithMembers(BuildMembers(grammar, declaredClasses, classToSuperClass, dictionaries)))));
        }

        private static IEnumerable<MemberDeclarationSyntax> BuildClassMethods(
            Grammar grammar, 
            IEnumerable<string> declaredClasses, 
            Dictionary<string, string> classToSuperClass)
        {
            var members = new SyntaxList<MemberDeclarationSyntax>();

            foreach (string className in declaredClasses)
            {
                if (classToSuperClass.Values.Contains(className))
                {
                    // Abstract
                    continue;
                }

                members = members.Add(BuildVisitor(grammar, className));
            }

            members = members.Add(BuildVisitIdentifier(grammar));

            return members;
        }

        private static MemberDeclarationSyntax BuildEncode()
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), 
                    SyntaxFactory.Identifier(@"Encode"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"node"))
                                    .WithType(SyntaxFactory.IdentifierName(@"Syntax")))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, 
                                            SyntaxFactory.ThisExpression(), 
                                            SyntaxFactory.IdentifierName(@"Visit")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"node")))))))));
        }

        private static IEnumerable<MemberDeclarationSyntax> BuildHelperMethods(List<DictionaryInformation> dictionaries)
        {
            List< MemberDeclarationSyntax > list = new List<MemberDeclarationSyntax>();
            HashSet<string> builtDictionaries = new HashSet<string>();
            list.Add(BuildWriteBeginObject());
            list.Add(BuildWriteEndObject());
            list.Add(BuildWriteCollection());

            foreach (DictionaryInformation di in dictionaries)
            {
                if(builtDictionaries.Add(di.ValueType))
                { 
                    list.Add(BuildWriteDictionary(di.ValueType));
                }
            }

            list.Add(BuildWriteLiteralCollection());
            list.Add(BuildWriteObjectCollection());
            list.Add(BuildWritePropertyString());
            list.Add(BuildWritePropertyInt());
            list.Add(BuildEncode());
            return list;
        }

        private static SyntaxList<MemberDeclarationSyntax> BuildMembers(
            Grammar grammar, 
            IEnumerable<string> declaredClasses, 
            Dictionary<string, string> classToSuperClass, 
            List<DictionaryInformation> dictionaries )
        {
            var members = new SyntaxList<MemberDeclarationSyntax>();

            members = members.AddRange(BuildHelperMethods(dictionaries));
            members = members.AddRange(BuildClassMethods(grammar, declaredClasses, classToSuperClass));
            return members;
        }

        private static LocalDeclarationStatementSyntax BuildStringBuilder()
        {
            return
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"var"))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"sb"))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.IdentifierName(@"StringBuilder"))
                                                .WithArgumentList(SyntaxFactory.ArgumentList()))))));
        }

        private static SyntaxList<UsingDirectiveSyntax> BuildUsings(Grammar grammar)
        {
            return
                SyntaxFactory.List(
                    new[]
                        {
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(@"System"))
                                .WithUsingKeyword(
                                    SyntaxFactory.Token(
                                        Common.GenerateFileHeader(grammar), 
                                        SyntaxKind.UsingKeyword, 
                                        SyntaxFactory.TriviaList(SyntaxFactory.Space)))
                                .WithSemicolonToken(
                                    SyntaxFactory.Token(
                                        SyntaxFactory.TriviaList(), 
                                        SyntaxKind.SemicolonToken, 
                                        SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturn, SyntaxFactory.LineFeed))), 
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(@"System.Collections.Generic")), 
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(@"System.Globalization")), 
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(@"System.Linq")), 
                            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(@"System.Text")), 
                            SyntaxFactory.UsingDirective(
                                SyntaxFactory.IdentifierName(grammar.GrammarNamespace))
                        });
        }

        private static MemberDeclarationSyntax BuildVisitIdentifier(Grammar grammar)
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), 
                    SyntaxFactory.Identifier(@"VisitIdentifier"))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            new[]
                                {
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), 
                                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword)
                                }))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"node"))
                                    .WithType(
                                        SyntaxFactory.QualifiedName(
                                            SyntaxFactory.IdentifierName(grammar.GrammarNamespace), 
                                            SyntaxFactory.IdentifierName(@"Identifier"))))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.List(
                                new StatementSyntax[]
                                    {
                                        SyntaxFactory.LocalDeclarationStatement(
                                            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"var"))
                                            .WithVariables(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"sb"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ObjectCreationExpression(
                                                        SyntaxFactory.IdentifierName(@"StringBuilder"))
                                            .WithArgumentList(SyntaxFactory.ArgumentList())))))), 
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.ThisExpression(), 
                                                    SyntaxFactory.IdentifierName(@"WriteBeginObject")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                            {
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.IdentifierName(@"sb")), 
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.IdentifierName(@"node"))
                                                            })))), 
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"Append")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
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
                                                                        SyntaxKind.StringLiteralExpression, 
                                                                        SyntaxFactory.Literal(
                                                                            SyntaxFactory.TriviaList(), 
                                                                            @"""name""", 
                                                                            @"""name""", 
                                                                            SyntaxFactory.TriviaList()))), 
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression, 
                                                                        SyntaxFactory.IdentifierName(@"node"), 
                                                                        SyntaxFactory.IdentifierName(@"Name")))
                                                            })))))))), 
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.ThisExpression(), 
                                                    SyntaxFactory.IdentifierName(@"WriteEndObject")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"sb")))))), 
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"ToString"))))
                                    })));
        }

        private static ReturnStatementSyntax BuildVisitReturn()
        {
            return
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, 
                            SyntaxFactory.IdentifierName(@"sb"), 
                            SyntaxFactory.IdentifierName(@"ToString"))));
        }

        private static MemberDeclarationSyntax BuildVisitor(Grammar grammar, string className)
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), 
                    SyntaxFactory.Identifier(@"Visit" + className))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            new[]
                                {
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), 
                                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword)
                                }))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"node"))
                                    .WithType(
                                        SyntaxFactory.QualifiedName(
                                            SyntaxFactory.IdentifierName(grammar.GrammarNamespace), 
                                            SyntaxFactory.IdentifierName(className))))))
                    .WithBody(SyntaxFactory.Block(SyntaxFactory.List(BuildVisitorStatements(grammar, className))));
        }

        private static SyntaxList<StatementSyntax> BuildVisitorStatements(Grammar grammar, string className)
        {
            var statements = new SyntaxList<StatementSyntax>();

            statements = statements.Add(BuildStringBuilder());
            statements = statements.Add(BuildWriteBegining());

            if (className != "Identifier")
            {
                // Identifier is a special case since it is not generated from the grammar
                IEnumerable<Production> result = from p in grammar.Productions where p.LHS.Name == className select p;
                Production production = result.First();

                Contract.Requires(!production.IsAlternatingClassDeclaration());
                RHSBuilder rhs = new RHSBuilder(production);
                IEnumerable<StatementSyntax> properties = rhs.ToSerializeProperties();
                if (properties.Any())
                {
                    statements = statements.AddRange(properties);
                }
            }

            statements =
                statements.Add(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression, 
                                SyntaxFactory.ThisExpression(), 
                                SyntaxFactory.IdentifierName(@"WriteEndObject")))
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"sb")))))));
            statements = statements.Add(BuildVisitReturn());

            return statements;
        }

        private static MemberDeclarationSyntax BuildWriteBeginObject()
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), 
                    SyntaxFactory.Identifier(@"WriteBeginObject"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]
                                    {
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"sb"))
                                            .WithType(SyntaxFactory.IdentifierName(@"StringBuilder")), 
                                        SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"node"))
                                            .WithType(SyntaxFactory.IdentifierName(@"Syntax"))
                                    })))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.List(
                                new StatementSyntax[]
                                    {
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"Append")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression, 
                                                                SyntaxFactory.Literal(
                                                                    SyntaxFactory.TriviaList(), 
                                                                    @"""{""", 
                                                                    @"""{""", 
                                                                    SyntaxFactory.TriviaList()))))))), 
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"Append")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
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
                                                                        SyntaxKind.StringLiteralExpression, 
                                                                        SyntaxFactory.Literal(
                                                                            SyntaxFactory.TriviaList(), 
                                                                            @"""__type""", 
                                                                            @"""__type""", 
                                                                            SyntaxFactory.TriviaList()))), 
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression, 
                                                                        SyntaxFactory.InvocationExpression(
                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                                                SyntaxFactory.IdentifierName(@"node"), 
                                                                                SyntaxFactory.IdentifierName(@"GetType"))), 
                                                                        SyntaxFactory.IdentifierName(@"Name")))
                                                            })))))))), 
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"Append")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression, 
                                                                SyntaxFactory.Literal(
                                                                    SyntaxFactory.TriviaList(), 
                                                                    @""", """, 
                                                                    @""", """, 
                                                                    SyntaxFactory.TriviaList()))))))), 
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"Append")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
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
                                                                        SyntaxKind.StringLiteralExpression, 
                                                                        SyntaxFactory.Literal(
                                                                            SyntaxFactory.TriviaList(), 
                                                                            @"""offset""", 
                                                                            @"""offset""", 
                                                                            SyntaxFactory.TriviaList()))), 
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression, 
                                                                        SyntaxFactory.IdentifierName(@"node"), 
                                                                        SyntaxFactory.IdentifierName(@"Offset")))
                                                            })))))))), 
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"Append")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression, 
                                                                SyntaxFactory.Literal(
                                                                    SyntaxFactory.TriviaList(), 
                                                                    @""", """, 
                                                                    @""", """, 
                                                                    SyntaxFactory.TriviaList()))))))), 
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"Append")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
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
                                                                        SyntaxKind.StringLiteralExpression, 
                                                                        SyntaxFactory.Literal(
                                                                            SyntaxFactory.TriviaList(), 
                                                                            @"""length""", 
                                                                            @"""length""", 
                                                                            SyntaxFactory.TriviaList()))), 
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression, 
                                                                        SyntaxFactory.IdentifierName(@"node"), 
                                                                        SyntaxFactory.IdentifierName(@"Length")))
                                                            }))))))))
                                    })));
        }

        private static ExpressionStatementSyntax BuildWriteBegining()
        {
            return
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, 
                            SyntaxFactory.ThisExpression(), 
                            SyntaxFactory.IdentifierName(@"WriteBeginObject")))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                        {
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"sb")), 
                                            SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"node"))
                                        }))));
        }

        private static MemberDeclarationSyntax BuildWriteCollection()
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), 
                    SyntaxFactory.Identifier(@"WriteCollection"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]
                                    {
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"type"))
                                            .WithType(
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword))), 
                                        SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"collection"))
                                            .WithType(
                                                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"IEnumerable"))
                                            .WithTypeArgumentList(
                                                SyntaxFactory.TypeArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                        SyntaxFactory.IdentifierName(@"Syntax")))))
                                    })))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.List(
                                new StatementSyntax[]
                                    {
                                        SyntaxFactory.LocalDeclarationStatement(
                                            SyntaxFactory.VariableDeclaration(
                                                SyntaxFactory.IdentifierName(@"StringBuilder"))
                                            .WithVariables(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"sb"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ObjectCreationExpression(
                                                        SyntaxFactory.IdentifierName(@"StringBuilder"))
                                            .WithArgumentList(SyntaxFactory.ArgumentList())))))), 
                                        SyntaxFactory.ForEachStatement(
                                            SyntaxFactory.IdentifierName(@"var"), 
                                            SyntaxFactory.Identifier(@"item"), 
                                            SyntaxFactory.IdentifierName(@"collection"), 
                                            SyntaxFactory.Block(
                                                SyntaxFactory.List(
                                                    new StatementSyntax[]
                                                        {
                                                            SyntaxFactory.ExpressionStatement(
                                                                SyntaxFactory.InvocationExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression, 
                                                                        SyntaxFactory.IdentifierName(@"sb"), 
                                                                        SyntaxFactory.IdentifierName(@"Append")))
                                                                .WithArgumentList(
                                                                    SyntaxFactory.ArgumentList(
                                                                        SyntaxFactory.SingletonSeparatedList(
                                                                            SyntaxFactory.Argument(
                                                                                SyntaxFactory.ConditionalExpression(
                                                                                    SyntaxFactory.BinaryExpression(
                                                                                        SyntaxKind.EqualsExpression, 
                                                                                        SyntaxFactory
                                                                .MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                                    SyntaxFactory.IdentifierName(@"Length")), 
                                                                                        SyntaxFactory.LiteralExpression(
                                                                                            SyntaxKind
                                                                .NumericLiteralExpression, 
                                                                                            SyntaxFactory.Literal(
                                                                                                SyntaxFactory.TriviaList
                                                                (), 
                                                                                                @"0", 
                                                                                                0, 
                                                                                                SyntaxFactory.TriviaList
                                                                ()))), 
                                                                                    SyntaxFactory.BinaryExpression(
                                                                                        SyntaxKind.AddExpression, 
                                                                                        SyntaxFactory.BinaryExpression(
                                                                                            SyntaxKind.AddExpression, 
                                                                                            SyntaxFactory
                                                                .LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression, 
                                                                    SyntaxFactory.Literal(
                                                                        SyntaxFactory.TriviaList(), 
                                                                        @""", \""""", 
                                                                        @""", \""""", 
                                                                        SyntaxFactory.TriviaList())), 
                                                                                            SyntaxFactory.IdentifierName
                                                                (@"type")), 
                                                                                        SyntaxFactory.LiteralExpression(
                                                                                            SyntaxKind
                                                                .StringLiteralExpression, 
                                                                                            SyntaxFactory.Literal(
                                                                                                SyntaxFactory.TriviaList
                                                                (), 
                                                                                                @"""\"" : [""", 
                                                                                                @"""\"" : [""", 
                                                                                                SyntaxFactory.TriviaList
                                                                ()))), 
                                                                                    SyntaxFactory.LiteralExpression(
                                                                                        SyntaxKind
                                                                .StringLiteralExpression, 
                                                                                        SyntaxFactory.Literal(
                                                                                            SyntaxFactory.TriviaList(), 
                                                                                            @""", """, 
                                                                                            @""", """, 
                                                                                            SyntaxFactory.TriviaList())))))))), 
                                                            SyntaxFactory.ExpressionStatement(
                                                                SyntaxFactory.InvocationExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression, 
                                                                        SyntaxFactory.IdentifierName(@"sb"), 
                                                                        SyntaxFactory.IdentifierName(@"Append")))
                                                                .WithArgumentList(
                                                                    SyntaxFactory.ArgumentList(
                                                                        SyntaxFactory.SingletonSeparatedList(
                                                                            SyntaxFactory.Argument(
                                                                                SyntaxFactory.InvocationExpression(
                                                                                    SyntaxFactory.MemberAccessExpression
                                                                (
                                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                                    SyntaxFactory.ThisExpression(), 
                                                                    SyntaxFactory.IdentifierName(@"Visit")))
                                                                .WithArgumentList(
                                                                    SyntaxFactory.ArgumentList(
                                                                        SyntaxFactory.SingletonSeparatedList(
                                                                            SyntaxFactory.Argument(
                                                                                SyntaxFactory.CastExpression(
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"Syntax"), 
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"item")))))))))))
                                                        }))), 
                                        SyntaxFactory.IfStatement(
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.GreaterThanExpression, 
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"Length")), 
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression, 
                                                    SyntaxFactory.Literal(
                                                        SyntaxFactory.TriviaList(), 
                                                        @"0", 
                                                        0, 
                                                        SyntaxFactory.TriviaList()))), 
                                            SyntaxFactory.Block(
                                                SyntaxFactory.SingletonList<StatementSyntax>(
                                                    SyntaxFactory.ExpressionStatement(
                                                        SyntaxFactory.InvocationExpression(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                                SyntaxFactory.IdentifierName(@"sb"), 
                                                                SyntaxFactory.IdentifierName(@"Append")))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression, 
                                                                SyntaxFactory.Literal(
                                                                    SyntaxFactory.TriviaList(), 
                                                                    @"""] """, 
                                                                    @"""] """, 
                                                                    SyntaxFactory.TriviaList())))))))))), 
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression, 
                                                    SyntaxFactory.IdentifierName(@"sb"), 
                                                    SyntaxFactory.IdentifierName(@"ToString"))))
                                    })));
        }

        private static MemberDeclarationSyntax BuildWriteEndObject()
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), 
                    SyntaxFactory.Identifier(@"WriteEndObject"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"sb"))
                                    .WithType(SyntaxFactory.IdentifierName(@"StringBuilder")))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, 
                                            SyntaxFactory.IdentifierName(@"sb"), 
                                            SyntaxFactory.IdentifierName(@"Append")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression, 
                                                            SyntaxFactory.Literal(
                                                                SyntaxFactory.TriviaList(), 
                                                                @"""}""", 
                                                                @"""}""", 
                                                                SyntaxFactory.TriviaList()))))))))));
        }

        private static MemberDeclarationSyntax BuildWriteLiteralCollection()
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                    SyntaxFactory.Identifier(@"WriteLiteralCollection"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]
                                    {
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"name"))
                                            .WithType(
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"collection"))
                                            .WithType(
                                                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"IEnumerable"))
                                            .WithTypeArgumentList(
                                                SyntaxFactory.TypeArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                        SyntaxFactory.IdentifierName(@"String")))))
                                    })))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.List<StatementSyntax>(
                                new StatementSyntax[]
                                    {
                                        SyntaxFactory.LocalDeclarationStatement(
                                            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"var"))
                                            .WithVariables(
                                                SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"sb"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ObjectCreationExpression(
                                                        SyntaxFactory.IdentifierName(@"StringBuilder"))
                                            .WithArgumentList(SyntaxFactory.ArgumentList())))))),
                                        SyntaxFactory.ForEachStatement(
                                            SyntaxFactory.IdentifierName(@"var"),
                                            SyntaxFactory.Identifier(@"item"),
                                            SyntaxFactory.IdentifierName(@"collection"),
                                            SyntaxFactory.Block(
                                                SyntaxFactory.List<StatementSyntax>(
                                                    new StatementSyntax[]
                                                        {
                                                            SyntaxFactory.ExpressionStatement(
                                                                SyntaxFactory.InvocationExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.IdentifierName(@"sb"),
                                                                        SyntaxFactory.IdentifierName(@"Append")))
                                                                .WithArgumentList(
                                                                    SyntaxFactory.ArgumentList(
                                                                        SyntaxFactory
                                                                .SingletonSeparatedList<ArgumentSyntax>(
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.ConditionalExpression(
                                                                            SyntaxFactory.BinaryExpression(
                                                                                SyntaxKind.EqualsExpression,
                                                                                SyntaxFactory.MemberAccessExpression(
                                                                                    SyntaxKind
                                                                .SimpleMemberAccessExpression,
                                                                                    SyntaxFactory.IdentifierName(@"sb"),
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"Length")),
                                                                                SyntaxFactory.LiteralExpression(
                                                                                    SyntaxKind.NumericLiteralExpression,
                                                                                    SyntaxFactory.Literal(
                                                                                        SyntaxFactory.TriviaList(),
                                                                                        @"0",
                                                                                        0,
                                                                                        SyntaxFactory.TriviaList()))),
                                                                            SyntaxFactory.BinaryExpression(
                                                                                SyntaxKind.AddExpression,
                                                                                SyntaxFactory.BinaryExpression(
                                                                                    SyntaxKind.AddExpression,
                                                                                    SyntaxFactory.LiteralExpression(
                                                                                        SyntaxKind
                                                                .StringLiteralExpression,
                                                                                        SyntaxFactory.Literal(
                                                                                            SyntaxFactory.TriviaList(),
                                                                                            @""", \""""",
                                                                                            @""", \""""",
                                                                                            SyntaxFactory.TriviaList())),
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"name")),
                                                                                SyntaxFactory.LiteralExpression(
                                                                                    SyntaxKind.StringLiteralExpression,
                                                                                    SyntaxFactory.Literal(
                                                                                        SyntaxFactory.TriviaList(),
                                                                                        @"""\"" : [""",
                                                                                        @"""\"" : [""",
                                                                                        SyntaxFactory.TriviaList()))),
                                                                            SyntaxFactory.LiteralExpression(
                                                                                SyntaxKind.StringLiteralExpression,
                                                                                SyntaxFactory.Literal(
                                                                                    SyntaxFactory.TriviaList(),
                                                                                    @""", """,
                                                                                    @""", """,
                                                                                    SyntaxFactory.TriviaList())))))))),
                                                            SyntaxFactory.ExpressionStatement(
                                                                SyntaxFactory.InvocationExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.IdentifierName(@"sb"),
                                                                        SyntaxFactory.IdentifierName(@"Append")))
                                                                .WithArgumentList(
                                                                    SyntaxFactory.ArgumentList(
                                                                        SyntaxFactory
                                                                .SingletonSeparatedList<ArgumentSyntax>(
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.BinaryExpression(
                                                                            SyntaxKind.AddExpression,
                                                                            SyntaxFactory.BinaryExpression(
                                                                                SyntaxKind.AddExpression,
                                                                                SyntaxFactory.LiteralExpression(
                                                                                    SyntaxKind.StringLiteralExpression,
                                                                                    SyntaxFactory.Literal(
                                                                                        SyntaxFactory.TriviaList(),
                                                                                        @"""\""""",
                                                                                        @"""\""""",
                                                                                        SyntaxFactory.TriviaList())),
                                                                                SyntaxFactory.InvocationExpression(
                                                                                    SyntaxFactory.MemberAccessExpression
                                                                (
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName(@"item"),
                                                                    SyntaxFactory.IdentifierName(@"Replace")))
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
                                                                                                SyntaxFactory.TriviaList
                                                                                        (),
                                                                                                @"""\""""",
                                                                                                @"""\""""",
                                                                                                SyntaxFactory.TriviaList
                                                                                        ()))),
                                                                                    SyntaxFactory.Token(
                                                                                        SyntaxKind.CommaToken),
                                                                                    SyntaxFactory.Argument(
                                                                                        SyntaxFactory.LiteralExpression(
                                                                                            SyntaxKind
                                                                                        .StringLiteralExpression,
                                                                                            SyntaxFactory.Literal(
                                                                                                SyntaxFactory.TriviaList
                                                                                        (),
                                                                                                @"""\\\""""",
                                                                                                @"""\\\""""",
                                                                                                SyntaxFactory.TriviaList
                                                                                        ())))
                                                                                })))),
                                                                            SyntaxFactory.LiteralExpression(
                                                                                SyntaxKind.StringLiteralExpression,
                                                                                SyntaxFactory.Literal(
                                                                                    SyntaxFactory.TriviaList(),
                                                                                    @"""\""""",
                                                                                    @"""\""""",
                                                                                    SyntaxFactory.TriviaList()))))))))
                                                        }))),
                                        SyntaxFactory.IfStatement(
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.GreaterThanExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(@"sb"),
                                                    SyntaxFactory.IdentifierName(@"Length")),
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal(
                                                        SyntaxFactory.TriviaList(),
                                                        @"0",
                                                        0,
                                                        SyntaxFactory.TriviaList()))),
                                            SyntaxFactory.Block(
                                                SyntaxFactory.SingletonList<StatementSyntax>(
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
                                                                    @"""] """,
                                                                    @"""] """,
                                                                    SyntaxFactory.TriviaList())))))))))),
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(@"sb"),
                                                    SyntaxFactory.IdentifierName(@"ToString"))))
                                    })));
        }

        private static MemberDeclarationSyntax BuildWriteDictionary(string objectType)
        {
            string typeName = Common.GrammarDictionary + "String_" + Common.ToProperCase(objectType.Replace(".",""));
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                    SyntaxFactory.Identifier(@"WriteDictionary"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]
                                    {
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"name"))
                                            .WithType(
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"collection"))
                                            .WithType(
                                                SyntaxFactory.GenericName(
                                                    SyntaxFactory.Identifier(Common.GrammarDictionary))
                                            .WithTypeArgumentList(
                                                SyntaxFactory.TypeArgumentList(
                                                    SyntaxFactory.SeparatedList<TypeSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                            {
                                                                SyntaxFactory.PredefinedType(
                                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                SyntaxFactory.IdentifierName(objectType)
                                                            }))))
                                    })))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.List<StatementSyntax>(
                                new StatementSyntax[]
                                    {
                                        SyntaxFactory.LocalDeclarationStatement(
                                            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"var"))
                                            .WithVariables(
                                                SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"sb"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ObjectCreationExpression(
                                                        SyntaxFactory.IdentifierName(@"StringBuilder"))
                                            .WithArgumentList(SyntaxFactory.ArgumentList())))))),
                                        SyntaxFactory.ForEachStatement(
                                            SyntaxFactory.IdentifierName(@"var"),
                                            SyntaxFactory.Identifier(@"item"),
                                            SyntaxFactory.IdentifierName(@"collection"),
                                            SyntaxFactory.Block(
                                                SyntaxFactory.List<StatementSyntax>(
                                                    new StatementSyntax[]
                                                        {
                                                            SyntaxFactory.IfStatement(
                                                                SyntaxFactory.BinaryExpression(
                                                                    SyntaxKind.EqualsExpression,
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.IdentifierName(@"sb"),
                                                                        SyntaxFactory.IdentifierName(@"Length")),
                                                                    SyntaxFactory.LiteralExpression(
                                                                        SyntaxKind.NumericLiteralExpression,
                                                                        SyntaxFactory.Literal(
                                                                            SyntaxFactory.TriviaList(),
                                                                            @"0",
                                                                            0,
                                                                            SyntaxFactory.TriviaList()))),
                                                                SyntaxFactory.Block(
                                                                    SyntaxFactory.List<StatementSyntax>(
                                                                        new StatementSyntax[]
                                                                            {
                                                                                SyntaxFactory.ExpressionStatement(
                                                                                    SyntaxFactory.InvocationExpression(
                                                                                        SyntaxFactory
                                                                                    .MemberAccessExpression(
                                                                                        SyntaxKind
                                                                                    .SimpleMemberAccessExpression,
                                                                                        SyntaxFactory.IdentifierName(
                                                                                            @"sb"),
                                                                                        SyntaxFactory.IdentifierName(
                                                                                            @"Append")))
                                                                                    .WithArgumentList(
                                                                                        SyntaxFactory.ArgumentList(
                                                                                            SyntaxFactory
                                                                                    .SingletonSeparatedList
                                                                                    <ArgumentSyntax>(
                                                                                        SyntaxFactory.Argument(
                                                                                            SyntaxFactory
                                                                                    .BinaryExpression(
                                                                                        SyntaxKind.AddExpression,
                                                                                        SyntaxFactory.BinaryExpression(
                                                                                            SyntaxKind.AddExpression,
                                                                                            SyntaxFactory
                                                                                    .LiteralExpression(
                                                                                        SyntaxKind
                                                                                    .StringLiteralExpression,
                                                                                        SyntaxFactory.Literal(
                                                                                            SyntaxFactory.TriviaList(),
                                                                                            @""", \""""",
                                                                                            @""", \""""",
                                                                                            SyntaxFactory.TriviaList())),
                                                                                            SyntaxFactory.IdentifierName
                                                                                    (@"name")),
                                                                                        SyntaxFactory.LiteralExpression(
                                                                                            SyntaxKind
                                                                                    .StringLiteralExpression,
                                                                                            SyntaxFactory.Literal(
                                                                                                SyntaxFactory.TriviaList
                                                                                    (),
                                                                                                @"""\"" : {""",
                                                                                                @"""\"" : {""",
                                                                                                SyntaxFactory.TriviaList
                                                                                    ())))))))),
                                                                                SyntaxFactory.ExpressionStatement(
                                                                                    SyntaxFactory.InvocationExpression(
                                                                                        SyntaxFactory
                                                                                    .MemberAccessExpression(
                                                                                        SyntaxKind
                                                                                    .SimpleMemberAccessExpression,
                                                                                        SyntaxFactory.IdentifierName(
                                                                                            @"sb"),
                                                                                        SyntaxFactory.IdentifierName(
                                                                                            @"Append")))
                                                                                    .WithArgumentList(
                                                                                        SyntaxFactory.ArgumentList(
                                                                                            SyntaxFactory
                                                                                    .SingletonSeparatedList
                                                                                    <ArgumentSyntax>(
                                                                                        SyntaxFactory.Argument(
                                                                                            SyntaxFactory
                                                                                    .LiteralExpression(
                                                                                        SyntaxKind
                                                                                    .StringLiteralExpression,
                                                                                        SyntaxFactory.Literal(
                                                                                            SyntaxFactory.TriviaList(),
                                                                                            @"""\""__type\"" : \""" + typeName + @"\""""",
                                                                                            @"""\""__type\"" : \""" + typeName + @"\""""",
                                                                                            SyntaxFactory.TriviaList())))))))
                                                                            }))),
                                                            SyntaxFactory.ExpressionStatement(
                                                                SyntaxFactory.InvocationExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.IdentifierName(@"sb"),
                                                                        SyntaxFactory.IdentifierName(@"Append")))
                                                                .WithArgumentList(
                                                                    SyntaxFactory.ArgumentList(
                                                                        SyntaxFactory
                                                                .SingletonSeparatedList<ArgumentSyntax>(
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.LiteralExpression(
                                                                            SyntaxKind.StringLiteralExpression,
                                                                            SyntaxFactory.Literal(
                                                                                SyntaxFactory.TriviaList(),
                                                                                @""", """,
                                                                                @""", """,
                                                                                SyntaxFactory.TriviaList()))))))),
                                                            SyntaxFactory.ExpressionStatement(
                                                                SyntaxFactory.InvocationExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.IdentifierName(@"sb"),
                                                                        SyntaxFactory.IdentifierName(@"Append")))
                                                                .WithArgumentList(
                                                                    SyntaxFactory.ArgumentList(
                                                                        SyntaxFactory
                                                                .SingletonSeparatedList<ArgumentSyntax>(
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.BinaryExpression(
                                                                            SyntaxKind.AddExpression,
                                                                            SyntaxFactory.BinaryExpression(
                                                                                SyntaxKind.AddExpression,
                                                                                SyntaxFactory.LiteralExpression(
                                                                                    SyntaxKind.StringLiteralExpression,
                                                                                    SyntaxFactory.Literal(
                                                                                        SyntaxFactory.TriviaList(),
                                                                                        @"""\""""",
                                                                                        @"""\""""",
                                                                                        SyntaxFactory.TriviaList())),
                                                                                SyntaxFactory.InvocationExpression(
                                                                                    SyntaxFactory.MemberAccessExpression
                                                                (
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.IdentifierName(@"item"),
                                                                        SyntaxFactory.IdentifierName(@"Key")),
                                                                    SyntaxFactory.IdentifierName(@"Replace")))
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
                                                                                                SyntaxFactory.TriviaList
                                                                                        (),
                                                                                                @"""\""""",
                                                                                                @"""\""""",
                                                                                                SyntaxFactory.TriviaList
                                                                                        ()))),
                                                                                    SyntaxFactory.Token(
                                                                                        SyntaxKind.CommaToken),
                                                                                    SyntaxFactory.Argument(
                                                                                        SyntaxFactory.LiteralExpression(
                                                                                            SyntaxKind
                                                                                        .StringLiteralExpression,
                                                                                            SyntaxFactory.Literal(
                                                                                                SyntaxFactory.TriviaList
                                                                                        (),
                                                                                                @"""\\\""""",
                                                                                                @"""\\\""""",
                                                                                                SyntaxFactory.TriviaList
                                                                                        ())))
                                                                                })))),
                                                                            SyntaxFactory.LiteralExpression(
                                                                                SyntaxKind.StringLiteralExpression,
                                                                                SyntaxFactory.Literal(
                                                                                    SyntaxFactory.TriviaList(),
                                                                                    @"""\"" : """,
                                                                                    @"""\"" : """,
                                                                                    SyntaxFactory.TriviaList())))))))),
                                                            SyntaxFactory.ExpressionStatement(
                                                                SyntaxFactory.InvocationExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.IdentifierName(@"sb"),
                                                                        SyntaxFactory.IdentifierName(@"Append")))
                                                                .WithArgumentList(
                                                                    SyntaxFactory.ArgumentList(
                                                                        SyntaxFactory
                                                                .SingletonSeparatedList<ArgumentSyntax>(
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.BinaryExpression(
                                                                            SyntaxKind.AddExpression,
                                                                            SyntaxFactory.BinaryExpression(
                                                                                SyntaxKind.AddExpression,
                                                                                SyntaxFactory.LiteralExpression(
                                                                                    SyntaxKind.StringLiteralExpression,
                                                                                    SyntaxFactory.Literal(
                                                                                        SyntaxFactory.TriviaList(),
                                                                                        @"""\""""",
                                                                                        @"""\""""",
                                                                                        SyntaxFactory.TriviaList())),
                                                                                SyntaxFactory.InvocationExpression(
                                                                                    SyntaxFactory.MemberAccessExpression
                                                                (
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.InvocationExpression(
                                                                        SyntaxFactory.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                SyntaxFactory.IdentifierName(@"item"),
                                                                                SyntaxFactory.IdentifierName(@"Value")),
                                                                            SyntaxFactory.IdentifierName(@"ToString"))),
                                                                    SyntaxFactory.IdentifierName(@"Replace")))
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
                                                                                                SyntaxFactory.TriviaList
                                                                                        (),
                                                                                                @"""\""""",
                                                                                                @"""\""""",
                                                                                                SyntaxFactory.TriviaList
                                                                                        ()))),
                                                                                    SyntaxFactory.Token(
                                                                                        SyntaxKind.CommaToken),
                                                                                    SyntaxFactory.Argument(
                                                                                        SyntaxFactory.LiteralExpression(
                                                                                            SyntaxKind
                                                                                        .StringLiteralExpression,
                                                                                            SyntaxFactory.Literal(
                                                                                                SyntaxFactory.TriviaList
                                                                                        (),
                                                                                                @"""\\\""""",
                                                                                                @"""\\\""""",
                                                                                                SyntaxFactory.TriviaList
                                                                                        ())))
                                                                                })))),
                                                                            SyntaxFactory.LiteralExpression(
                                                                                SyntaxKind.StringLiteralExpression,
                                                                                SyntaxFactory.Literal(
                                                                                    SyntaxFactory.TriviaList(),
                                                                                    @"""\""""",
                                                                                    @"""\""""",
                                                                                    SyntaxFactory.TriviaList()))))))))
                                                        }))),
                                        SyntaxFactory.IfStatement(
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.GreaterThanExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(@"sb"),
                                                    SyntaxFactory.IdentifierName(@"Length")),
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal(
                                                        SyntaxFactory.TriviaList(),
                                                        @"0",
                                                        0,
                                                        SyntaxFactory.TriviaList()))),
                                            SyntaxFactory.Block(
                                                SyntaxFactory.SingletonList<StatementSyntax>(
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
                                                                    @"""} """,
                                                                    @"""} """,
                                                                    SyntaxFactory.TriviaList())))))))))),
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(@"sb"),
                                                    SyntaxFactory.IdentifierName(@"ToString"))))
                                    })));

        }

        private static MemberDeclarationSyntax BuildWriteObjectCollection()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                SyntaxFactory.Identifier(@"WriteObjectCollection"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                                {
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"name"))
                                        .WithType(
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"collection"))
                                        .WithType(
                                            SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"IEnumerable"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                    SyntaxFactory.PredefinedType(
                                                        SyntaxFactory.Token(SyntaxKind.ObjectKeyword))))))
                                })))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.List<StatementSyntax>(
                            new StatementSyntax[]
                                {
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"var"))
                                        .WithVariables(
                                            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"sb"))
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.ObjectCreationExpression(
                                                    SyntaxFactory.IdentifierName(@"StringBuilder"))
                                        .WithArgumentList(SyntaxFactory.ArgumentList())))))),
                                    SyntaxFactory.ForEachStatement(
                                        SyntaxFactory.IdentifierName(@"var"),
                                        SyntaxFactory.Identifier(@"item"),
                                        SyntaxFactory.IdentifierName(@"collection"),
                                        SyntaxFactory.Block(
                                            SyntaxFactory.List<StatementSyntax>(
                                                new StatementSyntax[]
                                                    {
                                                        SyntaxFactory.LocalDeclarationStatement(
                                                            SyntaxFactory.VariableDeclaration(
                                                                SyntaxFactory.PredefinedType(
                                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                                                            .WithVariables(
                                                                SyntaxFactory
                                                            .SingletonSeparatedList<VariableDeclaratorSyntax>(
                                                                SyntaxFactory.VariableDeclarator(
                                                                    SyntaxFactory.Identifier(@"value"))
                                                            .WithInitializer(
                                                                SyntaxFactory.EqualsValueClause(
                                                                    SyntaxFactory.InvocationExpression(
                                                                        SyntaxFactory.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SyntaxFactory.IdentifierName(@"item"),
                                                                            SyntaxFactory.IdentifierName(@"ToString")))))))),
                                                        SyntaxFactory.ExpressionStatement(
                                                            SyntaxFactory.InvocationExpression(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName(@"sb"),
                                                                    SyntaxFactory.IdentifierName(@"Append")))
                                                            .WithArgumentList(
                                                                SyntaxFactory.ArgumentList(
                                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>
                                                            (
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.ConditionalExpression(
                                                                        SyntaxFactory.BinaryExpression(
                                                                            SyntaxKind.EqualsExpression,
                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                SyntaxFactory.IdentifierName(@"sb"),
                                                                                SyntaxFactory.IdentifierName(@"Length")),
                                                                            SyntaxFactory.LiteralExpression(
                                                                                SyntaxKind.NumericLiteralExpression,
                                                                                SyntaxFactory.Literal(
                                                                                    SyntaxFactory.TriviaList(),
                                                                                    @"0",
                                                                                    0,
                                                                                    SyntaxFactory.TriviaList()))),
                                                                        SyntaxFactory.BinaryExpression(
                                                                            SyntaxKind.AddExpression,
                                                                            SyntaxFactory.BinaryExpression(
                                                                                SyntaxKind.AddExpression,
                                                                                SyntaxFactory.LiteralExpression(
                                                                                    SyntaxKind.StringLiteralExpression,
                                                                                    SyntaxFactory.Literal(
                                                                                        SyntaxFactory.TriviaList(),
                                                                                        @""", \""""",
                                                                                        @""", \""""",
                                                                                        SyntaxFactory.TriviaList())),
                                                                                SyntaxFactory.IdentifierName(@"name")),
                                                                            SyntaxFactory.LiteralExpression(
                                                                                SyntaxKind.StringLiteralExpression,
                                                                                SyntaxFactory.Literal(
                                                                                    SyntaxFactory.TriviaList(),
                                                                                    @"""\"" : {""",
                                                                                    @"""\"" : {""",
                                                                                    SyntaxFactory.TriviaList()))),
                                                                        SyntaxFactory.LiteralExpression(
                                                                            SyntaxKind.StringLiteralExpression,
                                                                            SyntaxFactory.Literal(
                                                                                SyntaxFactory.TriviaList(),
                                                                                @""", """,
                                                                                @""", """,
                                                                                SyntaxFactory.TriviaList())))))))),
                                                        SyntaxFactory.ExpressionStatement(
                                                            SyntaxFactory.InvocationExpression(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName(@"sb"),
                                                                    SyntaxFactory.IdentifierName(@"Append")))
                                                            .WithArgumentList(
                                                                SyntaxFactory.ArgumentList(
                                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>
                                                            (
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.BinaryExpression(
                                                                        SyntaxKind.AddExpression,
                                                                        SyntaxFactory.BinaryExpression(
                                                                            SyntaxKind.AddExpression,
                                                                            SyntaxFactory.LiteralExpression(
                                                                                SyntaxKind.StringLiteralExpression,
                                                                                SyntaxFactory.Literal(
                                                                                    SyntaxFactory.TriviaList(),
                                                                                    @"""\""""",
                                                                                    @"""\""""",
                                                                                    SyntaxFactory.TriviaList())),
                                                                            SyntaxFactory.InvocationExpression(
                                                                                SyntaxFactory.MemberAccessExpression(
                                                                                    SyntaxKind
                                                            .SimpleMemberAccessExpression,
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"value"),
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"Replace")))
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
                                                                                            @"""\""""",
                                                                                            @"""\""""",
                                                                                            SyntaxFactory.TriviaList()))),
                                                                                SyntaxFactory.Token(
                                                                                    SyntaxKind.CommaToken),
                                                                                SyntaxFactory.Argument(
                                                                                    SyntaxFactory.LiteralExpression(
                                                                                        SyntaxKind
                                                                                    .StringLiteralExpression,
                                                                                        SyntaxFactory.Literal(
                                                                                            SyntaxFactory.TriviaList(),
                                                                                            @"""\\\""""",
                                                                                            @"""\\\""""",
                                                                                            SyntaxFactory.TriviaList())))
                                                                            })))),
                                                                        SyntaxFactory.LiteralExpression(
                                                                            SyntaxKind.StringLiteralExpression,
                                                                            SyntaxFactory.Literal(
                                                                                SyntaxFactory.TriviaList(),
                                                                                @"""\""""",
                                                                                @"""\""""",
                                                                                SyntaxFactory.TriviaList()))))))))
                                                    }))),
                                    SyntaxFactory.IfStatement(
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.GreaterThanExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName(@"sb"),
                                                SyntaxFactory.IdentifierName(@"Length")),
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(
                                                    SyntaxFactory.TriviaList(),
                                                    @"0",
                                                    0,
                                                    SyntaxFactory.TriviaList()))),
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
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
                                                                @"""} """,
                                                                @"""} """,
                                                                SyntaxFactory.TriviaList())))))))))),
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName(@"sb"),
                                                SyntaxFactory.IdentifierName(@"ToString"))))
                                })));
        }

        private static MemberDeclarationSyntax BuildWritePropertyInt()
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), 
                    SyntaxFactory.Identifier(@"WriteProperty"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]
                                    {
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"type"))
                                            .WithType(
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword))), 
                                        SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"property"))
                                            .WithType(
                                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                                    })))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.AddExpression, 
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.AddExpression, 
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.AddExpression, 
                                                SyntaxFactory.BinaryExpression(
                                                    SyntaxKind.AddExpression, 
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression, 
                                                        SyntaxFactory.Literal(
                                                            SyntaxFactory.TriviaList(), 
                                                            @"""\""""", 
                                                            @"""\""""", 
                                                            SyntaxFactory.TriviaList())), 
                                                    SyntaxFactory.IdentifierName(@"type")), 
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression, 
                                                    SyntaxFactory.Literal(
                                                        SyntaxFactory.TriviaList(), 
                                                        @"""\"" : """, 
                                                        @"""\"" : """, 
                                                        SyntaxFactory.TriviaList()))), 
                                            SyntaxFactory.IdentifierName(@"property")), 
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression, 
                                            SyntaxFactory.Literal(
                                                SyntaxFactory.TriviaList(), 
                                                @""" """, 
                                                @""" """, 
                                                SyntaxFactory.TriviaList())))))));
        }

        private static MemberDeclarationSyntax BuildWritePropertyString()
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), 
                    SyntaxFactory.Identifier(@"WriteProperty"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]
                                    {
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"type"))
                                            .WithType(
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword))), 
                                        SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"property"))
                                            .WithType(
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                                    })))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.AddExpression, 
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.AddExpression, 
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.AddExpression, 
                                                SyntaxFactory.BinaryExpression(
                                                    SyntaxKind.AddExpression, 
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression, 
                                                        SyntaxFactory.Literal(
                                                            SyntaxFactory.TriviaList(), 
                                                            @"""\""""", 
                                                            @"""\""""", 
                                                            SyntaxFactory.TriviaList())), 
                                                    SyntaxFactory.IdentifierName(@"type")), 
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression, 
                                                    SyntaxFactory.Literal(
                                                        SyntaxFactory.TriviaList(), 
                                                        @"""\"" : \""""", 
                                                        @"""\"" : \""""", 
                                                        SyntaxFactory.TriviaList()))), 
                                            SyntaxFactory.IdentifierName(@"property")), 
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression, 
                                            SyntaxFactory.Literal(
                                                SyntaxFactory.TriviaList(), 
                                                @"""\"" """, 
                                                @"""\"" """, 
                                                SyntaxFactory.TriviaList())))))));
        }
    }
}
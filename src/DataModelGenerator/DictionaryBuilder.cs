// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public class DictionaryBuilder
    {
        /// <summary>
        /// Builds the Generic Grammar Dictionary class
        /// </summary>
        /// <param name="grammar"></param>
        /// <returns></returns>
        public static MemberDeclarationSyntax MakeGrammarDictionaryClass(Grammar grammar)
        {
            return
                SyntaxFactory.ClassDeclaration(Common.GrammarDictionary)
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithTypeParameterList(
                        SyntaxFactory.TypeParameterList(
                            SyntaxFactory.SeparatedList<TypeParameterSyntax>(
                                new SyntaxNodeOrToken[]
                                    {
                                        SyntaxFactory.TypeParameter(SyntaxFactory.Identifier(@"TKey")),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.TypeParameter(SyntaxFactory.Identifier(@"TValue"))
                                    })))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                new SyntaxNodeOrToken[]
                                    {
                                        SyntaxFactory.IdentifierName(@"Syntax"),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"IDictionary"))
                                            .WithTypeArgumentList(
                                                SyntaxFactory.TypeArgumentList(
                                                    SyntaxFactory.SeparatedList<TypeSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                            {
                                                                SyntaxFactory.IdentifierName(@"TKey"),
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                SyntaxFactory.IdentifierName(@"TValue")
                                                            })))
                                    })))
                    .WithMembers(BuildAllMembers(grammar));

        }

        private static SyntaxList<MemberDeclarationSyntax> BuildAllMembers(Grammar grammar)
        {
            List<MemberDeclarationSyntax> list = new List<MemberDeclarationSyntax>();
            list.Add(BuildDictionaryField());
            list.Add(BuildConstructorMethod(grammar));
            list.Add(BuildConstructorMethodWithId(grammar));
            list.AddRange(BuildIDictionaryInterface());
            return SyntaxFactory.List<MemberDeclarationSyntax>(list);
        }

        private static ConstructorDeclarationSyntax BuildConstructorMethodWithId(Grammar grammar)
        {
            return SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(Common.GrammarDictionary))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"id"))
                                .WithType(SyntaxFactory.IdentifierName(grammar.Name + @"Kind")))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.List<StatementSyntax>(
                            new StatementSyntax[]
                                {
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(@"dictionary")),
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"Dictionary"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.IdentifierName(@"TKey"),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.IdentifierName(@"TValue")
                                                        }))))
                                        .WithArgumentList(SyntaxFactory.ArgumentList()))),
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(@"Id")),
                                            SyntaxFactory.IdentifierName(@"id")))
                                })));
        }

        private static IEnumerable<MemberDeclarationSyntax> BuildIDictionaryInterface()
        {
            List<MemberDeclarationSyntax> list = new List<MemberDeclarationSyntax>();
            list.Add(BuildCountProperty());
            list.Add(BuildKeysProperty());
            list.Add(BuildValuesProperty());
            list.Add(BuildIsReadOnlyProperty());
            list.Add(BuildIndexerProperty());
            list.Add(BuildAddKeyValueMethod());
            list.Add(BuildAddKeyValuePairMethod());
            list.Add(BuildClearMethod());
            list.Add(BuildContainsMethod());
            list.Add(BuildContainsKeyMethod());
            list.Add(BuildCopyToMethod());
            list.Add(BuildGetEnumeratorMethod());
            list.Add(BuildRemoveMethod());
            list.Add(BuildRemoveKeyValueMethod());
            list.Add(BuildSetPropertyMethod());
            list.Add(BuildTryGetValueMethod());
            list.Add(BuildIEnumerableGetEnumeratorMethod());
            list.Add(BuildSetPropertyInternalMethod());
            return list;
        }

        private static MethodDeclarationSyntax BuildSetPropertyInternalMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier(@"SetPropertyInternal"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                                {
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"name"))
                                        .WithType(SyntaxFactory.IdentifierName(@"TKey")),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"value"))
                                        .WithType(SyntaxFactory.IdentifierName(@"TValue"))
                                })))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.ElementAccessExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(@"dictionary")))
                                        .WithArgumentList(
                                            SyntaxFactory.BracketedArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"name"))))),
                                    SyntaxFactory.IdentifierName(@"value"))))));
        }

        private static MethodDeclarationSyntax BuildIEnumerableGetEnumeratorMethod()
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.IdentifierName(@"IEnumerator"),
                    SyntaxFactory.Identifier(@"GetEnumerator"))
                    .WithExplicitInterfaceSpecifier(
                        SyntaxFactory.ExplicitInterfaceSpecifier(SyntaxFactory.IdentifierName(@"IEnumerable")))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(@"dictionary")),
                                            SyntaxFactory.IdentifierName(@"GetEnumerator")))))));
        }

        private static MethodDeclarationSyntax BuildTryGetValueMethod()
        {
            return
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxFactory.Identifier(@"TryGetValue"))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SeparatedList<ParameterSyntax>(
                                new SyntaxNodeOrToken[]
                                    {
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"key"))
                                            .WithType(SyntaxFactory.IdentifierName(@"TKey")),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"value"))
                                            .WithModifiers(
                                                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.OutKeyword)))
                                            .WithType(SyntaxFactory.IdentifierName(@"TValue"))
                                    })))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(@"dictionary")),
                                            SyntaxFactory.IdentifierName(@"TryGetValue")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"key")),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"value"))
                                                                .WithRefOrOutKeyword(
                                                                    SyntaxFactory.Token(SyntaxKind.OutKeyword))
                                                        })))))));
        }

        private static MethodDeclarationSyntax BuildSetPropertyMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                SyntaxFactory.Identifier(@"SetProperty"))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        new[]
                            {
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                SyntaxFactory.Token(SyntaxKind.OverrideKeyword)
                            }))
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
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"value"))
                                        .WithType(
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                                })))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.List<StatementSyntax>(
                            new StatementSyntax[]
                                {
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"TKey"))
                                        .WithVariables(
                                            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"tKey"))))),
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"TValue"))
                                        .WithVariables(
                                            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                                SyntaxFactory.VariableDeclarator(
                                                    SyntaxFactory.Identifier(@"tValue"))))),
                                    SyntaxFactory.IfStatement(
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.EqualsExpression,
                                            SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(@"TKey")),
                                            SyntaxFactory.TypeOfExpression(
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword)))),
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.BinaryExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName(@"tKey"),
                                                        SyntaxFactory.CastExpression(
                                                            SyntaxFactory.IdentifierName(@"TKey"),
                                                            SyntaxFactory.InvocationExpression(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName(@"Convert"),
                                                                    SyntaxFactory.IdentifierName(@"ChangeType")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(@"name")),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.TypeOfExpression(
                                                                    SyntaxFactory.IdentifierName(@"TKey")))
                                                        })))))))))
                                        .WithElse(
                                            SyntaxFactory.ElseClause(
                                                SyntaxFactory.Block(
                                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                                        SyntaxFactory.ExpressionStatement(
                                                            SyntaxFactory.BinaryExpression(
                                                                SyntaxKind.SimpleAssignmentExpression,
                                                                SyntaxFactory.IdentifierName(@"tKey"),
                                                                SyntaxFactory.CastExpression(
                                                                    SyntaxFactory.IdentifierName(@"TKey"),
                                                                    SyntaxFactory.InvocationExpression(
                                                                        SyntaxFactory.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SyntaxFactory.IdentifierName(
                                                                                @"Activator"),
                                                                            SyntaxFactory.IdentifierName(
                                                                                @"CreateInstance")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.TypeOfExpression(
                                                                    SyntaxFactory.IdentifierName(@"TKey"))),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.ArrayCreationExpression(
                                                                    SyntaxFactory.ArrayType(
                                                                        SyntaxFactory.PredefinedType(
                                                                            SyntaxFactory.Token(
                                                                                SyntaxKind.ObjectKeyword)))
                                                                .WithRankSpecifiers(
                                                                    SyntaxFactory
                                                                .SingletonList<ArrayRankSpecifierSyntax>(
                                                                    SyntaxFactory.ArrayRankSpecifier(
                                                                        SyntaxFactory
                                                                .SingletonSeparatedList<ExpressionSyntax>(
                                                                    SyntaxFactory.OmittedArraySizeExpression())))))
                                                                .WithInitializer(
                                                                    SyntaxFactory.InitializerExpression(
                                                                        SyntaxKind.ArrayInitializerExpression,
                                                                        SyntaxFactory
                                                                .SingletonSeparatedList<ExpressionSyntax>(
                                                                    SyntaxFactory.IdentifierName(@"name")))))
                                                        })))))))))),
                                    SyntaxFactory.IfStatement(
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.EqualsExpression,
                                            SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(@"TValue")),
                                            SyntaxFactory.TypeOfExpression(
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword)))),
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.BinaryExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName(@"tValue"),
                                                        SyntaxFactory.CastExpression(
                                                            SyntaxFactory.IdentifierName(@"TValue"),
                                                            SyntaxFactory.InvocationExpression(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName(@"Convert"),
                                                                    SyntaxFactory.IdentifierName(@"ChangeType")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(@"name")),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.TypeOfExpression(
                                                                    SyntaxFactory.IdentifierName(@"TValue")))
                                                        })))))))))
                                        .WithElse(
                                            SyntaxFactory.ElseClause(
                                                SyntaxFactory.Block(
                                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                                        SyntaxFactory.ExpressionStatement(
                                                            SyntaxFactory.BinaryExpression(
                                                                SyntaxKind.SimpleAssignmentExpression,
                                                                SyntaxFactory.IdentifierName(@"tValue"),
                                                                SyntaxFactory.CastExpression(
                                                                    SyntaxFactory.IdentifierName(@"TValue"),
                                                                    SyntaxFactory.InvocationExpression(
                                                                        SyntaxFactory.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SyntaxFactory.IdentifierName(
                                                                                @"Activator"),
                                                                            SyntaxFactory.IdentifierName(
                                                                                @"CreateInstance")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.TypeOfExpression(
                                                                    SyntaxFactory.IdentifierName(@"TValue"))),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.ArrayCreationExpression(
                                                                    SyntaxFactory.ArrayType(
                                                                        SyntaxFactory.PredefinedType(
                                                                            SyntaxFactory.Token(
                                                                                SyntaxKind.ObjectKeyword)))
                                                                .WithRankSpecifiers(
                                                                    SyntaxFactory
                                                                .SingletonList<ArrayRankSpecifierSyntax>(
                                                                    SyntaxFactory.ArrayRankSpecifier(
                                                                        SyntaxFactory
                                                                .SingletonSeparatedList<ExpressionSyntax>(
                                                                    SyntaxFactory.OmittedArraySizeExpression())))))
                                                                .WithInitializer(
                                                                    SyntaxFactory.InitializerExpression(
                                                                        SyntaxKind.ArrayInitializerExpression,
                                                                        SyntaxFactory
                                                                .SingletonSeparatedList<ExpressionSyntax>(
                                                                    SyntaxFactory.IdentifierName(@"name")))))
                                                        })))))))))),
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(@"SetPropertyInternal")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(@"tKey")),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(@"tValue"))
                                                        })))),
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.BaseExpression(),
                                                SyntaxFactory.IdentifierName(@"SetProperty")))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(@"name")),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(@"value"))
                                                        }))))
                                })));
        }

        private static MethodDeclarationSyntax BuildRemoveKeyValueMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                SyntaxFactory.Identifier(@"Remove"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"item"))
                                .WithType(
                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"KeyValuePair"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.IdentifierName(@"TKey"),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.IdentifierName(@"TValue")
                                                        })))))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(@"dictionary")),
                                        SyntaxFactory.IdentifierName(@"Remove")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.IdentifierName(@"item"),
                                                        SyntaxFactory.IdentifierName(@"Key"))))))))));
        }

        private static MethodDeclarationSyntax BuildRemoveMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                SyntaxFactory.Identifier(@"Remove"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"key"))
                                .WithType(SyntaxFactory.IdentifierName(@"TKey")))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(@"dictionary")),
                                        SyntaxFactory.IdentifierName(@"Remove")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"key")))))))));
        }

        private static MethodDeclarationSyntax BuildGetEnumeratorMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"IEnumerator"))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"KeyValuePair"))
                                    .WithTypeArgumentList(
                                        SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                                new SyntaxNodeOrToken[]
                                                    {
                                                        SyntaxFactory.IdentifierName(@"TKey"),
                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                        SyntaxFactory.IdentifierName(@"TValue")
                                                    })))))),
                SyntaxFactory.Identifier(@"GetEnumerator"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(@"dictionary")),
                                        SyntaxFactory.IdentifierName(@"GetEnumerator")))))));
        }

        private static MethodDeclarationSyntax BuildCopyToMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier(@"CopyTo"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                                {
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"array"))
                                        .WithType(
                                            SyntaxFactory.ArrayType(
                                                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"KeyValuePair"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.IdentifierName(@"TKey"),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.IdentifierName(@"TValue")
                                                        }))))
                                        .WithRankSpecifiers(
                                            SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                                                SyntaxFactory.ArrayRankSpecifier(
                                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                        SyntaxFactory.OmittedArraySizeExpression()))))),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"arrayIndex"))
                                        .WithType(
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                                })))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ForEachStatement(
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"KeyValuePair"))
                                    .WithTypeArgumentList(
                                        SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                                new SyntaxNodeOrToken[]
                                                    {
                                                        SyntaxFactory.IdentifierName(@"TKey"),
                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                        SyntaxFactory.IdentifierName(@"TValue")
                                                    }))),
                                SyntaxFactory.Identifier(@"item"),
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxFactory.IdentifierName(@"dictionary")),
                                SyntaxFactory.Block(
                                    SyntaxFactory.List<StatementSyntax>(
                                        new StatementSyntax[]
                                            {
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.IdentifierName(@"array"),
                                                            SyntaxFactory.IdentifierName(@"SetValue")))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList(
                                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]
                                                                    {
                                                                        SyntaxFactory.Argument(
                                                                            SyntaxFactory.IdentifierName(@"item")),
                                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                        SyntaxFactory.Argument(
                                                                            SyntaxFactory.IdentifierName(
                                                                                @"arrayIndex"))
                                                                    })))),
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.PostfixUnaryExpression(
                                                        SyntaxKind.PostIncrementExpression,
                                                        SyntaxFactory.IdentifierName(@"arrayIndex")))
                                            }))))));
        }

        private static MethodDeclarationSyntax BuildContainsKeyMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                SyntaxFactory.Identifier(@"ContainsKey"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"key"))
                                .WithType(SyntaxFactory.IdentifierName(@"TKey")))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(@"dictionary")),
                                        SyntaxFactory.IdentifierName(@"ContainsKey")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"key")))))))));
        }

        private static MethodDeclarationSyntax BuildContainsMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                SyntaxFactory.Identifier(@"Contains"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"item"))
                                .WithType(
                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"KeyValuePair"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.IdentifierName(@"TKey"),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.IdentifierName(@"TValue")
                                                        })))))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(@"dictionary")),
                                        SyntaxFactory.IdentifierName(@"Contains")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"item")))))))));
        }

        private static MethodDeclarationSyntax BuildClearMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier(@"Clear"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(@"dictionary")),
                                        SyntaxFactory.IdentifierName(@"Clear")))))));
        }

        private static MethodDeclarationSyntax BuildAddKeyValuePairMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier(@"Add"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"item"))
                                .WithType(
                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"KeyValuePair"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.IdentifierName(@"TKey"),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.IdentifierName(@"TValue")
                                                        })))))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(@"dictionary")),
                                        SyntaxFactory.IdentifierName(@"Add")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                new SyntaxNodeOrToken[]
                                                    {
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.IdentifierName(@"item"),
                                                                SyntaxFactory.IdentifierName(@"Key"))),
                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.IdentifierName(@"item"),
                                                                SyntaxFactory.IdentifierName(@"Value")))
                                                    })))))));
        }

        private static MethodDeclarationSyntax BuildAddKeyValueMethod()
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier(@"Add"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                                {
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"key"))
                                        .WithType(SyntaxFactory.IdentifierName(@"TKey")),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"value"))
                                        .WithType(SyntaxFactory.IdentifierName(@"TValue"))
                                })))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.SingletonList<StatementSyntax>(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(@"dictionary")),
                                        SyntaxFactory.IdentifierName(@"Add")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                new SyntaxNodeOrToken[]
                                                    {
                                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"key")),
                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"value"))
                                                    })))))));
        }

        private static IndexerDeclarationSyntax BuildIndexerProperty()
        {
            return SyntaxFactory.IndexerDeclaration(SyntaxFactory.IdentifierName(@"TValue"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.BracketedParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"key"))
                                .WithType(SyntaxFactory.IdentifierName(@"TKey")))))
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.List<AccessorDeclarationSyntax>(
                            new AccessorDeclarationSyntax[]
                                {
                                    SyntaxFactory.AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration,
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
                                                SyntaxFactory.ReturnStatement(
                                                    SyntaxFactory.ElementAccessExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxFactory.IdentifierName(@"dictionary")))
                                        .WithArgumentList(
                                            SyntaxFactory.BracketedArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"key"))))))))),
                                    SyntaxFactory.AccessorDeclaration(
                                        SyntaxKind.SetAccessorDeclaration,
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.BinaryExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.ElementAccessExpression(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.ThisExpression(),
                                                                SyntaxFactory.IdentifierName(@"dictionary")))
                                        .WithArgumentList(
                                            SyntaxFactory.BracketedArgumentList(
                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"key"))))),
                                                        SyntaxFactory.IdentifierName(@"value"))))))
                                })));
        }

        private static PropertyDeclarationSyntax BuildIsReadOnlyProperty()
        {
            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                SyntaxFactory.Identifier(@"IsReadOnly"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.SingletonList<AccessorDeclarationSyntax>(
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))))))));
        }

        private static PropertyDeclarationSyntax BuildValuesProperty()
        {
            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"ICollection"))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.IdentifierName(@"TValue")))),
                SyntaxFactory.Identifier(@"Values"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.SingletonList<AccessorDeclarationSyntax>(
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxFactory.IdentifierName(@"dictionary")),
                                                SyntaxFactory.IdentifierName(@"Values")))))))));
        }

        private static PropertyDeclarationSyntax BuildKeysProperty()
        {
            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"ICollection"))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName(@"TKey")))),
                SyntaxFactory.Identifier(@"Keys"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.SingletonList<AccessorDeclarationSyntax>(
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxFactory.IdentifierName(@"dictionary")),
                                                SyntaxFactory.IdentifierName(@"Keys")))))))));
        }

        private static PropertyDeclarationSyntax BuildCountProperty()
        {
            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                SyntaxFactory.Identifier(@"Count"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.SingletonList<AccessorDeclarationSyntax>(
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxFactory.IdentifierName(@"dictionary")),
                                                SyntaxFactory.IdentifierName(@"Count")))))))));
        }

        private static MemberDeclarationSyntax BuildConstructorMethod(Grammar grammar)
        {
            return SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(Common.GrammarDictionary))
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.List<StatementSyntax>(
                            new StatementSyntax[]
                                {
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(@"dictionary")),
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.GenericName(
                                                    SyntaxFactory.Identifier(@"Dictionary"))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.IdentifierName(@"TKey"),
                                                            SyntaxFactory.Token(
                                                                SyntaxKind.CommaToken),
                                                            SyntaxFactory.IdentifierName(
                                                                @"TValue")
                                                        }))))
                                        .WithArgumentList(SyntaxFactory.ArgumentList()))),
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(@"Id")),
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName(
                                                    grammar.Name + @"Kind"),
                                                SyntaxFactory.IdentifierName(@"None"))))
                                })));
        }

        private static MemberDeclarationSyntax BuildDictionaryField()
        {
            return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"Dictionary"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                    new SyntaxNodeOrToken[]
                                        {
                                            SyntaxFactory.IdentifierName(@"TKey"),
                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                            SyntaxFactory.IdentifierName(@"TValue")
                                        }))))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(@"dictionary")))))
                .WithModifiers(
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
        }

    }
}
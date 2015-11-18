// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{

    public class CSharpBuilder
    {
        private readonly Dictionary<string, string> classToSuperClass;
        private List<DictionaryInformation> dictionaryList;
        private List<string> dictionaryTypesToMake;

        public CSharpBuilder(Grammar grammar)
        {
            Contract.Requires(grammar != null);
            this.Grammar = grammar;

            classToSuperClass = new Dictionary<string, string>();
            dictionaryList = new List<DictionaryInformation>();
            dictionaryTypesToMake = new List<string>();
        }

        public Grammar Grammar { get; set; }

        /// <summary>
        ///     Additions to the code
        /// </summary>
        /// <returns></returns>
        public List<SingleClassCompilationUnit> RunSeparate()
        {
            IEnumerable<string> properties = this.CollectProperties();
            var list = new List<SingleClassCompilationUnit>();

            list.Add(this.CreateCompilationUnit("Syntax", this.MakeSyntaxClass()));
            list.Add(this.CreateCompilationUnit(Common.GrammarDictionary, DictionaryBuilder.MakeGrammarDictionaryClass(this.Grammar)));
            foreach (Production prod in this.Grammar.Productions)
            {
                this.ExtractSublassDeclarations(prod);

                foreach (IGrammarSymbol property in prod.RHS)
                {
                    var nonTerminal = property as NonTerminal;
                    if (nonTerminal != null)
                    {
                        if (nonTerminal.GrammarType.Equals(Common.DictionaryName))
                        {
                            var di = new DictionaryInformation()
                                         {
                                             Name = nonTerminal.Name,
                                             KeyType ="string",
                                             ValueType = nonTerminal.Type.EndsWith(Common.DictionaryName) ? "string" : nonTerminal.Type
                                         };
                            this.dictionaryList.Add(di);

                            string dictionaryType = Common.GrammarDictionary + Common.ToProperCase(di.KeyType.Replace(".", string.Empty))
                                                   + "_" + Common.ToProperCase(di.ValueType.Replace(".", string.Empty));
                            if (!dictionaryTypesToMake.Contains(dictionaryType))
                            {
                                dictionaryTypesToMake.Add(dictionaryType);
                            }
                        }
                    }
                }
            }

            foreach (string property in properties)
            {
                list.Add(this.CreateCompilationUnit(property, this.MakeSingleClass(property)));
            }

            list.Add(this.CreateCompilationUnit("Identifier", this.MakeIdentifierClass()));
            list.Add(this.CreateCompilationUnit(this.Grammar.Name + "Kind", this.MakeEnums(properties)));
            list.Add(this.CreateCompilationUnit(this.Grammar.Name + "Visitor", this.MakeVisitor(properties, true)));
            list.Add(
                this.CreateCompilationUnit(
                    this.Grammar.Name + "RewritingVisitor", 
                    this.MakeVisitor(properties, true, true)));

            list.Add(
                this.CreateCompilationUnit(this.Grammar.Name + "JsonObjectFactory", this.MakeObjectFactory(properties)));
            list.Add(
                this.CreateCompilationUnit(
                    this.Grammar.Name + "JsonObjectPropertySetter",
                    this.MakeObjectPropertySetter()));

            list.Add(JsonSerializerBuilder.BuildSerializer(this.Grammar, properties, this.classToSuperClass, dictionaryList));

            return list;
        }

        private static SyntaxList<SwitchSectionSyntax> AddDictionaryToSwitch(
            SyntaxList<SwitchSectionSyntax> list,
            string className,
            string keyType,
            string valueType)
        {
            string quotedName = "\"" + className + "\"";
            list =
                list.Add(
                    SyntaxFactory.SwitchSection()
                        .WithLabels(
                            SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                                SyntaxFactory.SwitchLabel(
                                    SyntaxKind.CaseSwitchLabel,
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(
                                            SyntaxFactory.TriviaList(),
                                            quotedName,
                                            quotedName,
                                            SyntaxFactory.TriviaList())))))
                        .WithStatements(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.GenericName(SyntaxFactory.Identifier(Common.GrammarDictionary))
                                            .WithTypeArgumentList(
                                                SyntaxFactory.TypeArgumentList(
                                                    SyntaxFactory.SeparatedList<TypeSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                            {
                                                                SyntaxFactory.IdentifierName(keyType),
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                SyntaxFactory.IdentifierName(valueType)
                                                            }))))
                                        .WithArgumentList(SyntaxFactory.ArgumentList())))));
            return list;
        }

        private static SyntaxList<SwitchSectionSyntax> AddClassToSwitch(
            SyntaxList<SwitchSectionSyntax> list, 
            string className)
        {
            string quotedName = "\"" + className + "\"";
            list =
                list.Add(
                    SyntaxFactory.SwitchSection()
                        .WithLabels(
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.SwitchLabel(
                                    SyntaxKind.CaseSwitchLabel, 
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression, 
                                        SyntaxFactory.Literal(
                                            SyntaxFactory.TriviaList(), 
                                            quotedName, 
                                            quotedName, 
                                            SyntaxFactory.TriviaList())))))
                        .WithStatements(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(className))
                                        .WithArgumentList(SyntaxFactory.ArgumentList())))));
            return list;
        }

        private static NonTerminal GetSymbol(IGrammarSymbol alternative)
        {
            Contract.Requires(alternative != null);
            if (alternative is NonTerminal) { return alternative as NonTerminal; }
            if (alternative is Group && ((Group)alternative).Symbols.Count == 1)
            {
                return ((Group)alternative).Symbols.First() as NonTerminal;
            }

            return null;
        }

        private IEnumerable<string> CollectProperties()
        {
            return
                this.Grammar.Productions.Where(
                    p =>
                    !Common.IsReserved(p.LHS.Name) && !Common.IsLiteral(p.LHS.Type) && !Common.IsNumeric(p.LHS.Type))
                    .Select(p => p.LHS.Name);
        }

        private SyntaxList<MemberDeclarationSyntax> CreateAllMembers(Production production)
        {
            Contract.Requires(!production.IsAlternatingClassDeclaration());
            IEnumerable<RHSBuilder.PropertyInfo> propertyInfo = new RHSBuilder(production).ToProperties();
            SyntaxList<MemberDeclarationSyntax> list = SyntaxFactory.List<MemberDeclarationSyntax>(propertyInfo.Select(pi => pi.Syntax));
            SyntaxList<MemberDeclarationSyntax> constructors = this.CreateConstructors(production);
            SyntaxList<MemberDeclarationSyntax> setPropertyMethod = this.CreateSetProperty(production);

            list = list.AddRange(constructors);
            list = list.AddRange(setPropertyMethod);

            SyntaxList<MemberDeclarationSyntax> toStringMethods = this.CreateClassToString(production);
            if (toStringMethods.Count > 0)
            {
                list = list.AddRange(toStringMethods);
            }

            return list;
        }

        private SyntaxList<MemberDeclarationSyntax> CreateClassToString(Production production)
        {
            MemberDeclarationSyntax method =
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), 
                    SyntaxFactory.Identifier(@"ToString"))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            new[]
                                {
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), 
                                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword)
                                }))
                    .WithBody(SyntaxFactory.Block(this.CreateToStringStatements(production)));

            SyntaxList<MemberDeclarationSyntax> list = new SyntaxList<MemberDeclarationSyntax>();

            return list.Add(method);
        }

        private SingleClassCompilationUnit CreateCompilationUnit(string className, MemberDeclarationSyntax syntax)
        {
            SingleClassCompilationUnit classCompilationUnit = new SingleClassCompilationUnit();
            string namespaceToUse = String.Format("Microsoft.CodeAnalysis.{0}Grammar", this.Grammar.Name);
            namespaceToUse = this.Grammar.GrammarNamespace;

            classCompilationUnit.Name = className;
            classCompilationUnit.Syntax =
                SyntaxFactory.CompilationUnit()
                    .WithUsings(
                        SyntaxFactory.List(
                            new[]
                                {
                                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(@"System"))
                                        .WithUsingKeyword(
                                            SyntaxFactory.Token(
                                                Common.GenerateFileHeader(this.Grammar),
                                                SyntaxKind.UsingKeyword,
                                                SyntaxFactory.TriviaList(SyntaxFactory.Space)))
                                        .WithSemicolonToken(
                                            SyntaxFactory.Token(
                                                SyntaxFactory.TriviaList(),
                                                SyntaxKind.SemicolonToken,
                                                SyntaxFactory.TriviaList(
                                                    SyntaxFactory.CarriageReturn,
                                                    SyntaxFactory.LineFeed))),
                                    SyntaxFactory.UsingDirective(
                                        SyntaxFactory.IdentifierName(@"System.Collections")),
                                    SyntaxFactory.UsingDirective(
                                        SyntaxFactory.IdentifierName(@"System.Collections.Generic")),
                                    SyntaxFactory.UsingDirective(
                                        SyntaxFactory.IdentifierName(@"System.Globalization")),
                                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(@"System.Linq")),
                                    SyntaxFactory.UsingDirective(
                                        SyntaxFactory.IdentifierName("System.Runtime.CompilerServices")),
                                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(@"System.Runtime.Serialization")),
                                    SyntaxFactory.UsingDirective(
                                        SyntaxFactory.IdentifierName(@"Microsoft.CodeAnalysis.JsonGrammar"))
                                }))
                    .WithMembers(
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                            SyntaxFactory.NamespaceDeclaration(
                                SyntaxFactory.ParseName(namespaceToUse))
                                .WithNamespaceKeyword(
                                    SyntaxFactory.Token(
                                        SyntaxFactory.TriviaList(SyntaxFactory.Space),
                                        SyntaxKind.NamespaceKeyword,
                                        SyntaxFactory.TriviaList(SyntaxFactory.Space)))
                                .WithMembers(SyntaxFactory.SingletonList(syntax))))
                    .WithEndOfFileToken(SyntaxFactory.Token(SyntaxKind.EndOfFileToken));

            return classCompilationUnit;
        }

        private SyntaxList<MemberDeclarationSyntax> CreateConstructors(Production production)
        {
            SyntaxList<MemberDeclarationSyntax> list = new SyntaxList<MemberDeclarationSyntax>();

            MemberDeclarationSyntax method =
                SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(production.LHS.Name))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.SimpleAssignmentExpression, 
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, 
                                            SyntaxFactory.ThisExpression(), 
                                            SyntaxFactory.IdentifierName(@"Id")), 
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, 
                                            SyntaxFactory.IdentifierName(this.Grammar.EnumName), 
                                            SyntaxFactory.IdentifierName(production.LHS.Name)))))));

            return list.Add(method);
        }

        private InvocationExpressionSyntax CreateInvocationExpression(string propertyName, string methodName)
        {
            return
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression, 
                        SyntaxFactory.IdentifierName(propertyName), 
                        SyntaxFactory.IdentifierName(methodName)));
        }

        private StatementSyntax CreateReturnExpression(Production production)
        {
            SyntaxNodeOrToken[] args;
            var sb = new StringBuilder();
            sb.Append("\"");

            if (production.RHS.Count == 1 && production.RHS.First() is Alternative)
            {
                IGrammarSymbol element = production.RHS.First();
                var alternative = element as Alternative;
                if (RHSBuilder.IsConstantAlternative(alternative))
                {
                    sb.Append("{0}");
                }

                args = new SyntaxNodeOrToken[]
                           {
                               null, // patched later                                      
                               SyntaxFactory.Token(SyntaxKind.CommaToken), 
                               SyntaxFactory.Argument(
                                   this.CreateInvocationExpression(@"StringValue", @"ToString"))
                           };
            }
            else
            {
                int index = 0;
                foreach (var property in production.RHS)
                {
                    var prop = property as NonTerminal;
                    var propStruct = property as StructuralSymbol;
                    if (prop != null || (propStruct != null && propStruct.Symbol as NonTerminal != null))
                    {
                        sb.Append("{");
                        sb.Append(index++);
                        sb.Append("} ");
                    }
                    else
                    {
                        var term = property as Terminal;
                        if (term != null)
                        {
                            sb.Append(this.EscapeForStringFormat(term.Value));
                            sb.Append(" ");
                        }
                    }
                }

                args = new SyntaxNodeOrToken[index * 2 + 1];
                index = 1;
                int currentArgument = 0;
                foreach (IGrammarSymbol property in production.RHS)
                {
                    var prop = property as NonTerminal;
                    string propertyName = (prop != null) ? prop.Name : null;
                    if (prop == null)
                    {
                        var propStruct = property as StructuralSymbol;
                        if (propStruct != null)
                        {
                            prop = propStruct.Symbol as NonTerminal;
                            propertyName = (prop != null) ? prop.Name : String.Empty;
                        }
                    }

                    if (!String.IsNullOrEmpty(propertyName))
                    {
                        args[index++] = SyntaxFactory.Token(
                            SyntaxFactory.TriviaList(), 
                            SyntaxKind.CommaToken, 
                            SyntaxFactory.TriviaList());

                        args[index] = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"arg" + currentArgument++));
                        index++;
                    }
                }
            }

            sb.Append("\"");

            string format = sb.ToString();

            args[0] =
                SyntaxFactory.Argument(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression, 
                        SyntaxFactory.Literal(SyntaxFactory.TriviaList(), format, format, SyntaxFactory.TriviaList())));

            ReturnStatementSyntax returnStatement =
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(@"String"),
                            SyntaxFactory.IdentifierName(@"Format")))
                        .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(args))));
            return returnStatement;
        }

        private SyntaxList<MemberDeclarationSyntax> CreateSetProperty(Production production)
        {
            SyntaxList<MemberDeclarationSyntax> list = new SyntaxList<MemberDeclarationSyntax>();
            MemberDeclarationSyntax method =
                SyntaxFactory.MethodDeclaration(
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
                    .WithBody(SyntaxFactory.Block(this.CreateSetPropertyBody(production)));
            list = list.Add(method);
            return list;
        }

        private SyntaxList<StatementSyntax> CreateSetPropertyBody(Production production)
        {
            RHSBuilder rhs = new RHSBuilder(production);

            SyntaxList<StatementSyntax> list = rhs.CreateSetPropertyStatements();

            list =
                list.Add(
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
                                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"name")), 
                                                SyntaxFactory.Token(SyntaxKind.CommaToken), 
                                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"value"))
                                            })))));

            return list;
        }

        private SyntaxList<StatementSyntax> CreateSwitchBody(SwitchStatementSyntax switchStatement, bool rewriting)
        {
            SyntaxList<StatementSyntax> list =
                SyntaxFactory.List(
                    new StatementSyntax[]
                        {
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.EqualsExpression, 
                                    SyntaxFactory.IdentifierName(@"node"), 
                                    SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)), 
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ThrowStatement(
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.IdentifierName(@"ArgumentNullException"))
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression, 
                                                    SyntaxFactory.Literal(
                                                        SyntaxFactory.TriviaList(), 
                                                        @"""node""", 
                                                        @"""node""", 
                                                        SyntaxFactory.TriviaList())))))))))), 
                            switchStatement
                        });

            if (!rewriting)
            {
                list =
                    list.AddRange(
                        SyntaxFactory.List(
                            new StatementSyntax[]
                                {
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                                        .WithVariables(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"obj"))
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(@"node")))))), 
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"var"))
                                        .WithVariables(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"result"))
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.CastExpression(
                                                    SyntaxFactory.IdentifierName(@"T"), 
                                                    SyntaxFactory.IdentifierName(@"obj"))))))), 
                                    SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(@"result"))
                                }));
            }
            else
            {
                list =
                    list.AddRange(
                        SyntaxFactory.List(
                            new StatementSyntax[]
                                {
                                   SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(@"node")) 
                                }));
            }

            return list;
        }

        private IEnumerable<SwitchSectionSyntax> CreateSwitchForObjectFactory(IEnumerable<string> declaredClasses)
        {
            SyntaxList<SwitchSectionSyntax> list = new SyntaxList<SwitchSectionSyntax>();

            foreach (string className in declaredClasses)
            {
                if (this.IsAbstract(className))
                {
                    continue;
                }

                list = AddClassToSwitch(list, className);
            }

            list = AddClassToSwitch(list, "Identifier");

            HashSet<string> builtDictionary = new HashSet<string>();

            foreach (DictionaryInformation dictionaryInformation in dictionaryList)
            {
                string nameAndType = Common.GrammarDictionary + Common.ToProperCase(dictionaryInformation.KeyType.Replace(".",""))
                              + "_" + Common.ToProperCase(dictionaryInformation.ValueType.Replace(".",""));
                if (builtDictionary.Add(nameAndType))
                { 
                    list = AddDictionaryToSwitch(
                        list,
                        nameAndType,
                        dictionaryInformation.KeyType,
                        dictionaryInformation.ValueType);
                }
            }

            return list;
        }

        private SyntaxList<StatementSyntax> CreateTempArguments(Production production)
        {
            var statements = new SyntaxList<StatementSyntax>();
            int argumentIndex = 0;

            foreach (IGrammarSymbol property in production.RHS)
            {
                var prop = property as NonTerminal;
                string propertyName = (prop != null) ? prop.Name : null;

                if (prop == null)
                {
                    var propStruct = property as StructuralSymbol;
                    if (propStruct != null)
                    {
                        prop = propStruct.Symbol as NonTerminal;
                        propertyName = (prop != null) ? prop.Name : String.Empty;
                    }
                }

                string propertyType = (prop != null) ? prop.Type : String.Empty;

                if (!String.IsNullOrEmpty(propertyName))
                {
                    statements = Common.IsNumeric(propertyType)
                                     ? statements.Add(
                                         SyntaxFactory.LocalDeclarationStatement(
                                             SyntaxFactory.VariableDeclaration(
                                                 SyntaxFactory.PredefinedType(
                                                     SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                                           .WithVariables(
                                               SyntaxFactory.SingletonSeparatedList(
                                                   SyntaxFactory.VariableDeclarator(
                                                       SyntaxFactory.Identifier(@"arg" + argumentIndex++))
                                           .WithInitializer(
                                               SyntaxFactory.EqualsValueClause(
                                                   SyntaxFactory.InvocationExpression(
                                                       SyntaxFactory.MemberAccessExpression(
                                                           SyntaxKind.SimpleMemberAccessExpression,
                                                           SyntaxFactory.MemberAccessExpression(
                                                               SyntaxKind.SimpleMemberAccessExpression,
                                                               SyntaxFactory.ThisExpression(),
                                                               SyntaxFactory.IdentifierName(
                                                                   Common.ToProperCase(propertyName))),
                                                           SyntaxFactory.IdentifierName(@"ToString")))))))))
                                     : statements.Add(
                                         SyntaxFactory.LocalDeclarationStatement(
                                             SyntaxFactory.VariableDeclaration(
                                                 SyntaxFactory.PredefinedType(
                                                     SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                                           .WithVariables(
                                               SyntaxFactory.SingletonSeparatedList(
                                                   SyntaxFactory.VariableDeclarator(
                                                       SyntaxFactory.Identifier(@"arg" + argumentIndex++))
                                           .WithInitializer(
                                               SyntaxFactory.EqualsValueClause(
                                                   SyntaxFactory.ConditionalExpression(
                                                       SyntaxFactory.BinaryExpression(
                                                           SyntaxKind.NotEqualsExpression,
                                                           SyntaxFactory.MemberAccessExpression(
                                                               SyntaxKind.SimpleMemberAccessExpression,
                                                               SyntaxFactory.ThisExpression(),
                                                               SyntaxFactory.IdentifierName(
                                                                   Common.ToProperCase(propertyName))),
                                                           SyntaxFactory.LiteralExpression(
                                                               SyntaxKind.NullLiteralExpression)),
                                                       SyntaxFactory.InvocationExpression(
                                                           SyntaxFactory.MemberAccessExpression(
                                                               SyntaxKind.SimpleMemberAccessExpression,
                                                               SyntaxFactory.MemberAccessExpression(
                                                                   SyntaxKind.SimpleMemberAccessExpression,
                                                                   SyntaxFactory.ThisExpression(),
                                                                   SyntaxFactory.IdentifierName(
                                                                       Common.ToProperCase(propertyName))),
                                                               SyntaxFactory.IdentifierName(@"ToString"))),
                                                       SyntaxFactory.LiteralExpression(
                                                           SyntaxKind.StringLiteralExpression,
                                                           SyntaxFactory.Literal(
                                                               SyntaxFactory.TriviaList(),
                                                               "\"" + Common.ToProperCase(propertyName) + "(null)\"",
                                                               "\"" + Common.ToProperCase(propertyName) + "(null)\"",
                                                               SyntaxFactory.TriviaList())))))))));
                }
            }
            return statements;
        }

        private SyntaxList<StatementSyntax> CreateToStringStatements(Production production)
        {
            var statements = new SyntaxList<StatementSyntax>();

            SyntaxList<StatementSyntax> tempArgs = this.CreateTempArguments(production);
            if (tempArgs.Any())
            {
                statements = statements.AddRange(tempArgs);
            }

            statements = statements.Add(this.CreateReturnExpression(production));

            return statements;
        }

        private IEnumerable<StatementSyntax> CreateVisitProperties(string className, bool rewriting)
        {
            var list = new SyntaxList<StatementSyntax>();

            if (className != "Identifier")
            {
                // Identifier is a special case since it is not generated from the grammar
                IEnumerable<Production> result = from p in this.Grammar.Productions where p.LHS.Name == className select p;
                Production production = result.First();

                Contract.Requires(!production.IsAlternatingClassDeclaration());
                RHSBuilder rhs = new RHSBuilder(production);
                IEnumerable<StatementSyntax> properties = rhs.ToVisitProperties(rewriting);

                if (properties.Any())
                {
                    list = list.AddRange(properties);
                }
            }

            SyntaxList<StatementSyntax> returnStatement;
            if (!rewriting)
            {
                returnStatement =
                    SyntaxFactory.List(
                        new StatementSyntax[]
                            {
                                SyntaxFactory.LocalDeclarationStatement(
                                    SyntaxFactory.VariableDeclaration(
                                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                                    .WithVariables(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"obj"))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(@"node")))))), 
                                SyntaxFactory.LocalDeclarationStatement(
                                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"var"))
                                    .WithVariables(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"result"))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.CastExpression(
                                                SyntaxFactory.IdentifierName(@"T"), 
                                                SyntaxFactory.IdentifierName(@"obj"))))))), 
                                SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(@"result"))
                            });
            }
            else
            {
                returnStatement =
                    SyntaxFactory.List(
                        new StatementSyntax[] { SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(@"node")) });
            }

            list = list.AddRange(returnStatement);

            return list;
        }

        private string EscapeForStringFormat(string str)
        {
            return str.Replace("{", "{{").Replace("}", "}}");
        }

        private IEnumerable<SyntaxNodeOrToken> ExtractEnumMembers(IEnumerable<string> list)
        {
            var enumList = new List<string>();

            enumList.Add("Identifier");

            foreach(string typeName in dictionaryTypesToMake)
            {
                enumList.Add(typeName);
            }

            foreach (string className in list)
            {
                if (!enumList.Contains(className))
                {
                    enumList.Add(className);
                }
            }

            enumList.Sort();
            enumList.Insert(0, "None");

            var result = new List<SyntaxNodeOrToken>();

            foreach (string name in enumList)
            {
                result.Add(SyntaxFactory.EnumMemberDeclaration(SyntaxFactory.Identifier(name)));
                result.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
            }

            return result.ToArray();
        }

        /// <summary>
        ///     Side-effect: writes to this.classToSuperClass
        /// </summary>
        /// <param name="production"></param>
        /// <returns></returns>
        private void ExtractSublassDeclarations(Production production)
        {
            string className = production.LHS.Name;
            if (production.IsAlternatingClassDeclaration())
            {
                Alternative alternation = (Alternative)production.RHS.First();
                foreach (IGrammarSymbol alternative in alternation.Symbols)
                {
                    NonTerminal subclass = GetSymbol(alternative);
                    Contract.Assert(subclass != null);
                    this.classToSuperClass.Add(subclass.Name, className);
                }
            }
        }

        private MethodDeclarationSyntax ExtractSwitchMethod(
            IEnumerable<string> declaredClasses, 
            bool excludeClass, 
            bool rewriting)
        {
            SwitchStatementSyntax switchStatement = this.ExtractSwitchStatement(declaredClasses, excludeClass);
            TypeSyntax returnType = rewriting
                                 ? SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))
                                 : (TypeSyntax)SyntaxFactory.IdentifierName(@"T");

            return
                SyntaxFactory.MethodDeclaration(returnType, SyntaxFactory.Identifier("VisitActual"))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            new[]
                                {
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), 
                                    SyntaxFactory.Token(SyntaxKind.VirtualKeyword)
                                }))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"node"))
                                    .WithType(SyntaxFactory.IdentifierName(@"Syntax")))))
                    .WithBody(SyntaxFactory.Block(this.CreateSwitchBody(switchStatement, rewriting)));
        }

        private SwitchStatementSyntax ExtractSwitchStatement(
            IEnumerable<string> declaredClasses, 
            bool excludeClass = false)
        {
            IEnumerable<SwitchSectionSyntax> sections =
                declaredClasses.Select(
                    className =>
                    SyntaxFactory.SwitchSection()
                        .WithLabels(
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.SwitchLabel(
                                    SyntaxKind.CaseSwitchLabel, 
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, 
                                        SyntaxFactory.IdentifierName(this.Grammar.EnumName), 
                                        SyntaxFactory.IdentifierName(className)))))
                        .WithStatements(
                            SyntaxFactory.List(
                                new StatementSyntax[]
                                    {
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.IdentifierName("Visit" + className))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.CastExpression(
                                                                SyntaxFactory.QualifiedName(
                                                                    SyntaxFactory.IdentifierName(
                                                                        excludeClass
                                                                            ? this.Grammar.GrammarNamespace
                                                                            : this.Grammar.GrammarClass), 
                                                                    SyntaxFactory.IdentifierName(className)), 
                                                                SyntaxFactory.IdentifierName(@"node")))))))
                                    })));

            return
                SyntaxFactory.SwitchStatement(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression, 
                        SyntaxFactory.IdentifierName(@"node"), 
                        SyntaxFactory.IdentifierName(@"Id"))).WithSections(SyntaxFactory.List(sections));
        }

        private IEnumerable<MethodDeclarationSyntax> ExtractVisitorMethods(
            IEnumerable<string> declaredClasses, 
            bool excludeClass = false, 
            bool rewriting = false)
        {
            List<string> listToGen = new List<string>(declaredClasses);
            listToGen.Add("Identifier");
            listToGen.Sort();

            TypeSyntax visitReturnType = rewriting
                                      ? SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))
                                      : (TypeSyntax)SyntaxFactory.IdentifierName(@"T");

            yield return
                SyntaxFactory.MethodDeclaration(visitReturnType, SyntaxFactory.Identifier("Visit"))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            new[]
                                {
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), 
                                    SyntaxFactory.Token(SyntaxKind.VirtualKeyword)
                                }))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"node"))
                                    .WithType(SyntaxFactory.IdentifierName(@"Syntax")))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(@"VisitActual"))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(@"node")))))))))
                ;
            yield return this.ExtractSwitchMethod(listToGen, excludeClass, rewriting);

            foreach (string className in listToGen)
            {
                IdentifierNameSyntax returnType = rewriting
                                     ? SyntaxFactory.IdentifierName(className)
                                     : SyntaxFactory.IdentifierName(@"T");
                MethodDeclarationSyntax result =
                    SyntaxFactory.MethodDeclaration(returnType, SyntaxFactory.Identifier("Visit" + className))
                        .WithModifiers(
                            SyntaxFactory.TokenList(
                                new[]
                                    {
                                        SyntaxFactory.Token(SyntaxKind.PublicKeyword), 
                                        SyntaxFactory.Token(SyntaxKind.VirtualKeyword)
                                    }))
                        .WithParameterList(
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"node"))
                                        .WithType(
                                            SyntaxFactory.QualifiedName(
                                                SyntaxFactory.IdentifierName(
                                                    excludeClass
                                                        ? this.Grammar.GrammarNamespace
                                                        : this.Grammar.GrammarClass), 
                                                SyntaxFactory.IdentifierName(className))))));
                if (!this.IsAbstract(className))
                {
                    result = result.WithBody(SyntaxFactory.Block(this.CreateVisitProperties(className, rewriting)));
                }
                else
                {
                    SyntaxList<StatementSyntax> returnStatement;

                    if (!rewriting)
                    {
                        returnStatement =
                            SyntaxFactory.List(
                                new StatementSyntax[]
                                    {
                                        SyntaxFactory.LocalDeclarationStatement(
                                            SyntaxFactory.VariableDeclaration(
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                                            .WithVariables(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(@"obj"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(@"node")))))), 
                                        SyntaxFactory.LocalDeclarationStatement(
                                            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(@"var"))
                                            .WithVariables(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.VariableDeclarator(
                                                        SyntaxFactory.Identifier(@"result"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.CastExpression(
                                                        SyntaxFactory.IdentifierName(@"T"), 
                                                        SyntaxFactory.IdentifierName(@"obj"))))))), 
                                        SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(@"result"))
                                    });
                    }
                    else
                    {
                        returnStatement =
                            SyntaxFactory.List(
                                new StatementSyntax[]
                                    {
                                       SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(@"node")) 
                                    });
                    }

                    result = result.WithBody(SyntaxFactory.Block(returnStatement));
                }

                yield return result;
            }
        }

        private bool IsAbstract(string className)
        {
            return this.classToSuperClass.Values.Contains(className);
        }

        private MemberDeclarationSyntax MakeClass(Production production)
        {
            if (Common.IsReserved(production.LHS.Name) || Common.IsLiteral(production.LHS.Type)
                || Common.IsNumeric(production.LHS.Type))
            {
                return null;
            }

            string className = production.LHS.Name;

            // Check for special case of System.Enum or enum type. In this case we create an enum instead of a class
            if (production.RHS.Count == 1)
            {
                Alternative alternative = production.RHS[0] as Alternative;

                if (alternative != null && !alternative.Type.Equals("string"))
                {
                    var list = new List<SyntaxNodeOrToken>();

                    foreach (Group enumSymbol in alternative.Symbols)
                    {
                        if (enumSymbol.Symbols.Count == 1)
                        {

                            string name = ((Terminal)(enumSymbol.Symbols[0])).Value;
                            if (list.Count > 0)
                            {
                                list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                            }
                            list.Add(
                                SyntaxFactory.EnumMemberDeclaration(SyntaxFactory.Identifier(name))
                                    .WithAttributeLists(
                                        SyntaxFactory.SingletonList<AttributeListSyntax>(
                                            SyntaxFactory.AttributeList(
                                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(@"EnumMember")))))));
                        }
                        else
                        {
                            throw new InvalidDataException("Unexpected data found in grammar");
                        }
                    }

                    return
                        SyntaxFactory.EnumDeclaration(production.LHS.Name)
                            .WithAttributeLists(
                                SyntaxFactory.SingletonList<AttributeListSyntax>(
                                    SyntaxFactory.AttributeList(
                                        SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(@"DataContract"))))))
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                            .WithMembers(SyntaxFactory.SeparatedList<EnumMemberDeclarationSyntax>(list.ToArray()));
                }
            }

            string superClass;

            this.classToSuperClass.TryGetValue(className, out superClass);
            if (String.IsNullOrEmpty(superClass))
            {
                superClass = "Syntax";
            }

            var modifiers =
                new[]
                    {
                        SyntaxFactory.Token(
                            SyntaxFactory.TriviaList(SyntaxFactory.Space),
                            SyntaxKind.PublicKeyword,
                            SyntaxFactory.TriviaList(SyntaxFactory.Space))
                    }.ToList();
            bool isAbstract = this.IsAbstract(className);
            if (isAbstract)
            {
                modifiers.Add(
                    SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(SyntaxFactory.Space),
                        SyntaxKind.AbstractKeyword,
                        SyntaxFactory.TriviaList(SyntaxFactory.Space)));
            }

            ClassDeclarationSyntax result =
                SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(className))
                    .WithAttributeLists(
                        SyntaxFactory.SingletonList<AttributeListSyntax>(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SeparatedList<AttributeSyntax>(new[] {
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(@"DataContract")),
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(@"CompilerGenerated")),
                                }))))
                    .WithModifiers(SyntaxFactory.TokenList(modifiers))
                    .WithKeyword(
                        SyntaxFactory.Token(
                            SyntaxFactory.TriviaList(SyntaxFactory.Space),
                            SyntaxKind.ClassKeyword,
                            SyntaxFactory.TriviaList(SyntaxFactory.Space)))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName(superClass))))
                    .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken));
            if (!isAbstract)
            {
                result = result.WithMembers(this.CreateAllMembers(production));
            }

            result = result.WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
            return result;

        }

        private EnumDeclarationSyntax MakeEnums(IEnumerable<string> declaredClasses)
        {
            IEnumerable<SyntaxNodeOrToken> enumMembers = this.ExtractEnumMembers(declaredClasses);
            Contract.Assert(enumMembers != null);

            return
                SyntaxFactory.EnumDeclaration(this.Grammar.Name + "Kind")
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithMembers(SyntaxFactory.SeparatedList<EnumMemberDeclarationSyntax>(enumMembers));
        }

        private ClassDeclarationSyntax MakeIdentifierClass()
        {
            string superClass;

            this.classToSuperClass.TryGetValue(Common.IdentifierName, out superClass);

            if (String.IsNullOrEmpty(superClass))
            {
                superClass = "Syntax";
            }

            return
                SyntaxFactory.ClassDeclaration(@"Identifier")
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName(superClass))))
                    .WithMembers(
                        SyntaxFactory.List(
                            new MemberDeclarationSyntax[]
                                {
                                    SyntaxFactory.PropertyDeclaration(
                                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                        SyntaxFactory.Identifier(@"Name"))
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithAccessorList(
                                            SyntaxFactory.AccessorList(
                                                SyntaxFactory.List(
                                                    new[]
                                                        {
                                                            SyntaxFactory.AccessorDeclaration(
                                                                SyntaxKind.GetAccessorDeclaration)
                                                                .WithSemicolonToken(
                                                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                                            SyntaxFactory.AccessorDeclaration(
                                                                SyntaxKind.SetAccessorDeclaration)
                                                                .WithSemicolonToken(
                                                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                                        }))),
                                    SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(@"Identifier"))
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithBody(
                                            SyntaxFactory.Block(
                                                SyntaxFactory.SingletonList<StatementSyntax>(
                                                    SyntaxFactory.ExpressionStatement(
                                                        SyntaxFactory.BinaryExpression(
                                                            SyntaxKind.SimpleAssignmentExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.ThisExpression(),
                                                                SyntaxFactory.IdentifierName(@"Id")),
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.IdentifierName(this.Grammar.EnumName),
                                                                SyntaxFactory.IdentifierName(@"Identifier"))))))),
                                    SyntaxFactory.MethodDeclaration(
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
                                                SyntaxFactory.List(
                                                    new StatementSyntax[]
                                                        {
                                                            SyntaxFactory.IfStatement(
                                                                SyntaxFactory.BinaryExpression(
                                                                    SyntaxKind.EqualsExpression,
                                                                    SyntaxFactory.IdentifierName(@"name"),
                                                                    SyntaxFactory.LiteralExpression(
                                                                        SyntaxKind.StringLiteralExpression,
                                                                        SyntaxFactory.Literal(
                                                                            SyntaxFactory.TriviaList(),
                                                                            @"""name""",
                                                                            @"""name""",
                                                                            SyntaxFactory.TriviaList()))),
                                                                SyntaxFactory.Block(
                                                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                                                        SyntaxFactory.ExpressionStatement(
                                                                            SyntaxFactory.BinaryExpression(
                                                                                SyntaxKind.SimpleAssignmentExpression,
                                                                                SyntaxFactory.MemberAccessExpression(
                                                                                    SyntaxKind
                                                                .SimpleMemberAccessExpression,
                                                                                    SyntaxFactory.ThisExpression(),
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"Name")),
                                                                                SyntaxFactory.CastExpression(
                                                                                    SyntaxFactory.PredefinedType(
                                                                                        SyntaxFactory.Token(
                                                                                            SyntaxKind.StringKeyword)),
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"value"))))))),
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
                                                                                        SyntaxFactory.IdentifierName(
                                                                                            @"name")),
                                                                                    SyntaxFactory.Token(
                                                                                        SyntaxKind.CommaToken),
                                                                                    SyntaxFactory.Argument(
                                                                                        SyntaxFactory.IdentifierName(
                                                                                            @"value"))
                                                                                }))))
                                                        }))),
                                    SyntaxFactory.MethodDeclaration(
                                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                        SyntaxFactory.Identifier(@"ToString"))
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(
                                                new[]
                                                    {
                                                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                                        SyntaxFactory.Token(SyntaxKind.OverrideKeyword)
                                                    }))
                                        .WithBody(
                                            SyntaxFactory.Block(
                                                SyntaxFactory.SingletonList<StatementSyntax>(
                                                    SyntaxFactory.ReturnStatement(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxFactory.IdentifierName(@"Name"))))))
                                }));
        }

        private ClassDeclarationSyntax MakeObjectFactory(IEnumerable<string> declaredClasses)
        {
            ClassDeclarationSyntax result =
                SyntaxFactory.ClassDeclaration(@"JsonObjectFactory")
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"IGrammarNodeFactory"))
                                    .WithTypeArgumentList(
                                        SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                SyntaxFactory.IdentifierName(@"Syntax")))))))
                    .WithMembers(
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                            SyntaxFactory.MethodDeclaration(
                                SyntaxFactory.IdentifierName(@"Syntax"), 
                                SyntaxFactory.Identifier(@"CreateObjectFromJsonTypeName"))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .WithParameterList(
                                    SyntaxFactory.ParameterList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"typeName"))
                                                .WithType(
                                                    SyntaxFactory.PredefinedType(
                                                        SyntaxFactory.Token(SyntaxKind.StringKeyword))))))
                                .WithBody(
                                    SyntaxFactory.Block(
                                        SyntaxFactory.List(
                                            new StatementSyntax[]
                                                {
                                                    SyntaxFactory.SwitchStatement(
                                                        SyntaxFactory.IdentifierName(@"typeName"))
                                                        .WithSections(
                                                            SyntaxFactory.List(
                                                                this.CreateSwitchForObjectFactory(declaredClasses))), 
                                                    SyntaxFactory.ReturnStatement(
                                                        SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NullLiteralExpression))
                                                })))));

            return result;
        }

        private ClassDeclarationSyntax MakeObjectPropertySetter()
        {
            ClassDeclarationSyntax result =
                SyntaxFactory.ClassDeclaration(@"JsonObjectPropertySetter")
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.IdentifierName(@"IGrammarNodePropertySetter"))))
                    .WithMembers(
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                            SyntaxFactory.MethodDeclaration(
                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), 
                                SyntaxFactory.Identifier(@"SetProperty"))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .WithParameterList(
                                    SyntaxFactory.ParameterList(
                                        SyntaxFactory.SeparatedList<ParameterSyntax>(
                                            new SyntaxNodeOrToken[]
                                                {
                                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(@"node"))
                                                        .WithType(
                                                            SyntaxFactory.PredefinedType(
                                                                SyntaxFactory.Token(SyntaxKind.ObjectKeyword))), 
                                                    SyntaxFactory.Token(SyntaxKind.CommaToken), 
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
                                        SyntaxFactory.List(
                                            new StatementSyntax[]
                                                {
                                                    SyntaxFactory.LocalDeclarationStatement(
                                                        SyntaxFactory.VariableDeclaration(
                                                            SyntaxFactory.IdentifierName(@"var"))
                                                        .WithVariables(
                                                            SyntaxFactory.SingletonSeparatedList(
                                                                SyntaxFactory.VariableDeclarator(
                                                                    SyntaxFactory.Identifier(@"syntaxNode"))
                                                        .WithInitializer(
                                                            SyntaxFactory.EqualsValueClause(
                                                                SyntaxFactory.CastExpression(
                                                                    SyntaxFactory.IdentifierName(@"Syntax"), 
                                                                    SyntaxFactory.IdentifierName(@"node"))))))), 
                                                    SyntaxFactory.ReturnStatement(
                                                        SyntaxFactory.InvocationExpression(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                                SyntaxFactory.IdentifierName(@"syntaxNode"), 
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
                                                })))));

            return result;
        }

        private MemberDeclarationSyntax MakeSingleClass(string property)
        {
            IEnumerable<Production> result = from p in this.Grammar.Productions where p.LHS.Name == property select p;

            return this.MakeClass(result.First());
        }

        private MemberDeclarationSyntax MakeSyntaxClass()
        {
            return
                SyntaxFactory.ClassDeclaration(@"Syntax")
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithMembers(
                        SyntaxFactory.List(
                            new MemberDeclarationSyntax[]
                                {
                                    SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(@"Syntax"))
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)))
                                        .WithBody(SyntaxFactory.Block()),
                                    SyntaxFactory.MethodDeclaration(
                                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                                        SyntaxFactory.Identifier(@"SetProperty"))
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(
                                                new[]
                                                    {
                                                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                                        SyntaxFactory.Token(SyntaxKind.VirtualKeyword)
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
                                                SyntaxFactory.List(
                                                    new StatementSyntax[]
                                                        {
                                                            SyntaxFactory.IfStatement(
                                                                SyntaxFactory.BinaryExpression(
                                                                    SyntaxKind.EqualsExpression,
                                                                    SyntaxFactory.IdentifierName(@"name"),
                                                                    SyntaxFactory.LiteralExpression(
                                                                        SyntaxKind.StringLiteralExpression,
                                                                        SyntaxFactory.Literal(
                                                                            SyntaxFactory.TriviaList(),
                                                                            @"""offset""",
                                                                            @"""offset""",
                                                                            SyntaxFactory.TriviaList()))),
                                                                SyntaxFactory.Block(
                                                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                                                        SyntaxFactory.ExpressionStatement(
                                                                            SyntaxFactory.BinaryExpression(
                                                                                SyntaxKind.SimpleAssignmentExpression,
                                                                                SyntaxFactory.MemberAccessExpression(
                                                                                    SyntaxKind
                                                                .SimpleMemberAccessExpression,
                                                                                    SyntaxFactory.ThisExpression(),
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"Offset")),
                                                                                SyntaxFactory.CastExpression(
                                                                                    SyntaxFactory.PredefinedType(
                                                                                        SyntaxFactory.Token(
                                                                                            SyntaxKind.IntKeyword)),
                                                                                    SyntaxFactory.IdentifierName(
                                                                                        @"value")))))))
                                                                .WithElse(
                                                                    SyntaxFactory.ElseClause(
                                                                        SyntaxFactory.IfStatement(
                                                                            SyntaxFactory.BinaryExpression(
                                                                                SyntaxKind.EqualsExpression,
                                                                                SyntaxFactory.IdentifierName(@"name"),
                                                                                SyntaxFactory.LiteralExpression(
                                                                                    SyntaxKind.StringLiteralExpression,
                                                                                    SyntaxFactory.Literal(
                                                                                        SyntaxFactory.TriviaList(),
                                                                                        @"""length""",
                                                                                        @"""length""",
                                                                                        SyntaxFactory.TriviaList()))),
                                                                            SyntaxFactory.Block(
                                                                                SyntaxFactory
                                                                .SingletonList<StatementSyntax>(
                                                                    SyntaxFactory.ExpressionStatement(
                                                                        SyntaxFactory.BinaryExpression(
                                                                            SyntaxKind.SimpleAssignmentExpression,
                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                SyntaxFactory.ThisExpression(),
                                                                                SyntaxFactory.IdentifierName(@"Length")),
                                                                            SyntaxFactory.CastExpression(
                                                                                SyntaxFactory.PredefinedType(
                                                                                    SyntaxFactory.Token(
                                                                                        SyntaxKind.IntKeyword)),
                                                                                SyntaxFactory.IdentifierName(@"value"))))))))),
                                                            SyntaxFactory.ReturnStatement(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.FalseLiteralExpression))
                                                        }))),
                                    SyntaxFactory.PropertyDeclaration(
                                        SyntaxFactory.IdentifierName(this.Grammar.EnumName),
                                        SyntaxFactory.Identifier(@"Id"))
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithAccessorList(
                                            SyntaxFactory.AccessorList(
                                                SyntaxFactory.List(
                                                    new[]
                                                        {
                                                            SyntaxFactory.AccessorDeclaration(
                                                                SyntaxKind.GetAccessorDeclaration)
                                                                .WithSemicolonToken(
                                                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                                            SyntaxFactory.AccessorDeclaration(
                                                                SyntaxKind.SetAccessorDeclaration)
                                                                .WithModifiers(
                                                                    SyntaxFactory.TokenList(
                                                                        SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)))
                                                                .WithSemicolonToken(
                                                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                                        }))),
                                    SyntaxFactory.PropertyDeclaration(
                                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                                        SyntaxFactory.Identifier(@"Offset"))
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithAccessorList(
                                            SyntaxFactory.AccessorList(
                                                SyntaxFactory.List(
                                                    new[]
                                                        {
                                                            SyntaxFactory.AccessorDeclaration(
                                                                SyntaxKind.GetAccessorDeclaration)
                                                                .WithSemicolonToken(
                                                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                                            SyntaxFactory.AccessorDeclaration(
                                                                SyntaxKind.SetAccessorDeclaration)
                                                                .WithSemicolonToken(
                                                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                                        }))),
                                    SyntaxFactory.PropertyDeclaration(
                                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                                        SyntaxFactory.Identifier(@"Length"))
                                        .WithModifiers(
                                            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                        .WithAccessorList(
                                            SyntaxFactory.AccessorList(
                                                SyntaxFactory.List(
                                                    new[]
                                                        {
                                                            SyntaxFactory.AccessorDeclaration(
                                                                SyntaxKind.GetAccessorDeclaration)
                                                                .WithSemicolonToken(
                                                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                                            SyntaxFactory.AccessorDeclaration(
                                                                SyntaxKind.SetAccessorDeclaration)
                                                                .WithSemicolonToken(
                                                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                                        })))
                                }));
        }

        private ClassDeclarationSyntax MakeVisitor(
            IEnumerable<string> declaredClasses, 
            bool excludeClass = false, 
            bool rewriting = false)
        {
            MethodDeclarationSyntax[] methods = this.ExtractVisitorMethods(declaredClasses, excludeClass, rewriting).ToArray();
            Contract.Assert(methods != null);
            string visitorName = rewriting ? "RewritingVisitor" : "Visitor";
            ClassDeclarationSyntax result =
                SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(this.Grammar.Name + visitorName))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(SyntaxFactory.Space), 
                                SyntaxKind.PublicKeyword, 
                                SyntaxFactory.TriviaList(SyntaxFactory.Space))))
                    .WithKeyword(
                        SyntaxFactory.Token(
                            SyntaxFactory.TriviaList(SyntaxFactory.Space), 
                            SyntaxKind.ClassKeyword, 
                            SyntaxFactory.TriviaList(SyntaxFactory.Space)));
            if (!rewriting)
            {
                result =
                    result.WithTypeParameterList(
                        SyntaxFactory.TypeParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.TypeParameter(SyntaxFactory.Identifier(@"T")))));
            }

            result =
                result.WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                    .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(methods))
                    .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));

            return result;
        }
    }
}
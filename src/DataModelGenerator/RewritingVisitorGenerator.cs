// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Generator that makes rewriting visitors for data models.</summary>
    internal static class RewritingVisitorGenerator
    {
        /// <summary>Generates a rewriting visitor class for the supplied model.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="codeWriter">The code writer into which the rewriting visitor shall be written.</param>
        /// <param name="model">The model for which a rewriting visitor shall be generated.</param>
        public static void Generate(CodeWriter codeWriter, DataModel model)
        {
            codeWriter.WriteLine("/// <summary>Rewriting visitor for a {0} tree.</summary>", model.Name);
            codeWriter.OpenBrace("public abstract class {0}RewritingVisitor", model.Name);

            codeWriter.WriteLine("/// <summary>Starts a rewriting visit of a {0} tree.</summary>", model.Name);
            codeWriter.WriteLine("/// <param name=\"node\">The node to rewrite.</param>");
            codeWriter.OpenBrace("public virtual object Visit({0} node)", CodeGenerator.CommonInterfaceName);
            codeWriter.WriteLine("return this.VisitActual(node);");
            codeWriter.CloseBrace();
            codeWriter.WriteLine();

            codeWriter.WriteLine("/// <summary>Executes a visit of a {0} tree.</summary>", model.Name);
            codeWriter.WriteLine("/// <param name=\"node\">The node to visit.</param>");
            codeWriter.OpenBrace("public virtual object VisitActual({0} node)", CodeGenerator.CommonInterfaceName);
            codeWriter.OpenBrace("if (node == null)");
            codeWriter.WriteLine("throw new ArgumentNullException(\"node\");");
            codeWriter.CloseBrace(); // if node == null
            codeWriter.WriteLine();

            codeWriter.OpenBrace("switch (node.SyntaxKind)");

            foreach (DataModelType type in model.Types)
            {
                if (type.Kind == DataModelTypeKind.Leaf)
                {
                    string csName = type.CSharpName;
                    codeWriter.DecrementIndentLevel();
                    codeWriter.WriteLine("case {0}Kind.{1}:", model.Name, csName);
                    codeWriter.IncrementIndentLevel();
                    codeWriter.WriteLine("return this.Visit{0}(({0})node);", csName);
                }
            }

            codeWriter.DecrementIndentLevel();
            codeWriter.WriteLine("default:");
            codeWriter.IncrementIndentLevel();
            codeWriter.WriteLine("return node;");
            codeWriter.CloseBrace(); // switch
            codeWriter.CloseBrace(); // VisitActual

            codeWriter.WriteLine();
            codeWriter.WriteLine("private T VisitNullChecked<T>(T node)", CodeGenerator.CommonInterfaceName);
            codeWriter.IncrementIndentLevel();
            codeWriter.WriteLine("where T : class, " + CodeGenerator.CommonInterfaceName);
            codeWriter.DecrementIndentLevel();
            codeWriter.OpenBrace();
            codeWriter.OpenBrace("if (node == null)");
            codeWriter.WriteLine("return null;");
            codeWriter.CloseBrace(); // node == null
            codeWriter.WriteLine();
            codeWriter.WriteLine("return (T)this.Visit(node);");
            codeWriter.CloseBrace(); // VisitNullChecked

            foreach (DataModelType type in model.Types)
            {
                if (type.Kind == DataModelTypeKind.Leaf)
                {
                    string csName = type.CSharpName;
                    codeWriter.WriteLine();
                    codeWriter.WriteLine("/// <summary>Rewrites a {0} node in a {1} tree.</summary>", csName, model.Name);
                    codeWriter.WriteLine("/// <param name=\"node\">A {0} node to visit.</param>", csName);
                    codeWriter.OpenBrace("public virtual {0} Visit{0}({0} node)", csName);
                    codeWriter.OpenBrace("if (node != null)");
                    foreach (DataModelMember member in type.Members)
                    {
                        WriteVisitMember(codeWriter, model, member);
                    }

                    codeWriter.CloseBrace();
                    codeWriter.WriteLine();
                    codeWriter.WriteLine("return node;");
                    codeWriter.CloseBrace(); // Visit{TNode}
                }
            }

            codeWriter.CloseBrace(); // class
        }

        private static void WriteVisitMember(CodeWriter codeWriter, DataModel model, DataModelMember member)
        {
            DataModelType memberType = model.GetTypeForMember(member);
            switch (memberType.Kind)
            {
                case DataModelTypeKind.BuiltInNumber:
                case DataModelTypeKind.BuiltInString:
                case DataModelTypeKind.BuiltInDictionary:
                case DataModelTypeKind.BuiltInBoolean:
                case DataModelTypeKind.BuiltInUri:
                case DataModelTypeKind.Enum:
                case DataModelTypeKind.BuiltInVersion:
                // Don't visit builtins; overrides would inspect those directly.
                return;
                case DataModelTypeKind.Leaf:
                case DataModelTypeKind.Base:
                    // Visit this member
                    break;
                case DataModelTypeKind.Default:
                default:
                    Debug.Fail("Unexpected DataModelTypeKind");
                    return;
            }

            string sourceVariable = "node." + member.CSharpName;
            if (member.Rank == 0)
            {
                codeWriter.WriteLine("{0} = this.VisitNullChecked({0});", sourceVariable);
                return;
            }

            var indexNamer = new LocalVariableNamer("index");
            var valueNamer = new LocalVariableNamer("value");

            string indexName = null;
            for (int idx = 1; idx < member.Rank; ++idx)
            {
                string valueName = valueNamer.MakeName();
                indexName = indexNamer.MakeName();
                codeWriter.OpenBrace("if ({0} != null)", sourceVariable);
                codeWriter.OpenBrace("for (int {0} = 0; {0} < {1}.Count; ++{0})", indexName, sourceVariable);
                codeWriter.WriteLine("var {0} = {1}[{2}];", valueName, sourceVariable, indexName);
                sourceVariable = valueName;
            }

            indexName = indexNamer.MakeName();
            codeWriter.OpenBrace("if ({0} != null)", sourceVariable);
            codeWriter.OpenBrace("for (int {0} = 0; {0} < {1}.Count; ++{0})", indexName, sourceVariable);
            codeWriter.WriteLine("{0}[{1}] = this.VisitNullChecked({0}[{1}]);", sourceVariable, indexName);

            for (int idx = 0; idx < member.Rank; ++idx)
            {
                codeWriter.CloseBrace(); // if (source != null)
                codeWriter.CloseBrace(); // for
            }

            codeWriter.WriteLine();
        }
    }
}

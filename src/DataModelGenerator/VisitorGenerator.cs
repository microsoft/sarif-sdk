// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Generator that makes visitors for data models.</summary>
    internal static class VisitorGenerator
    {
        /// <summary>Generates a visitor class for the supplied model.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="codeWriter">The code writer into which the visitor shall be written.</param>
        /// <param name="model">The model for which a visitor shall be generated.</param>
        public static void Generate(CodeWriter codeWriter, DataModel model)
        {
            codeWriter.WriteLine("/// <summary>Visitor for a {0} tree.</summary>", model.Name);
            codeWriter.OpenBrace("public abstract class {0}Visitor<T>", model.Name);

            codeWriter.WriteLine("/// <summary>Starts a visit of a {0} tree.</summary>", model.Name);
            codeWriter.WriteLine("/// <param name=\"node\">The node to visit.</param>");
            codeWriter.OpenBrace("public virtual T Visit({0} node)", CodeGenerator.CommonInterfaceName);
            codeWriter.WriteLine("return this.VisitActual(node);");
            codeWriter.CloseBrace();
            codeWriter.WriteLine();

            codeWriter.WriteLine("/// <summary>Executes a visit of a {0} tree.</summary>", model.Name);
            codeWriter.WriteLine("/// <param name=\"node\">The node to visit.</param>");
            codeWriter.OpenBrace("public virtual T VisitActual({0} node)", CodeGenerator.CommonInterfaceName);
            codeWriter.OpenBrace("if (node == null)");
            codeWriter.WriteLine("throw new ArgumentNullException(\"node\");");
            codeWriter.CloseBrace(); // if node == null
            codeWriter.WriteLine();

            codeWriter.OpenBrace("switch (node.Kind)");

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
            codeWriter.WriteLine("return (T)(object)node;");
            codeWriter.CloseBrace(); // switch
            codeWriter.CloseBrace(); // VisitActual

            codeWriter.WriteLine();
            codeWriter.OpenBrace("private void VisitNullChecked({0} node)", CodeGenerator.CommonInterfaceName);
            codeWriter.OpenBrace("if (node != null)");
            codeWriter.WriteLine("this.Visit(node);");
            codeWriter.CloseBrace(); // node != null
            codeWriter.CloseBrace(); // VisitNullChecked

            foreach (DataModelType type in model.Types)
            {
                if (type.Kind == DataModelTypeKind.Leaf)
                {
                    string csName = type.CSharpName;
                    codeWriter.WriteLine();
                    codeWriter.WriteLine("/// <summary>Visits a {0} node in a {1} tree.</summary>", csName, model.Name);
                    codeWriter.WriteLine("/// <param name=\"node\">A {0} node to visit.</param>", csName);
                    codeWriter.OpenBrace("public virtual T Visit{0}({0} node)", csName);
                    codeWriter.OpenBrace("if (node != null)");
                    foreach (DataModelMember member in type.Members)
                    {
                        WriteVisitMemmber(codeWriter, model, member);
                    }

                    codeWriter.CloseBrace();
                    codeWriter.WriteLine();
                    codeWriter.WriteLine("return (T)(object)node;");
                    codeWriter.CloseBrace(); // Visit{TNode}
                }
            }

            codeWriter.CloseBrace(); // class
        }

        private static void WriteVisitMemmber(CodeWriter codeWriter, DataModel model, DataModelMember member)
        {
            DataModelType memberType = model.GetTypeForMember(member);
            switch (memberType.Kind)
            {
                case DataModelTypeKind.BuiltInNumber:
                case DataModelTypeKind.BuiltInString:
                case DataModelTypeKind.BuiltInDictionary:
                case DataModelTypeKind.BuiltInBoolean:
                case DataModelTypeKind.BuiltInUri:
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

            var valueNamer = new LocalVariableNamer("value");

            string sourceVariable = "node." + member.CSharpName;
            for (int idx = 0; idx < member.Rank; ++idx)
            {
                string valueName = valueNamer.MakeName();
                codeWriter.OpenBrace("if ({0} != null)", sourceVariable);
                codeWriter.OpenBrace("foreach (var {0} in {1})", valueName, sourceVariable);
                sourceVariable = valueName;
            }

            codeWriter.WriteLine("this.VisitNullChecked({0});", sourceVariable);

            if (member.Rank == 0)
            {
                return;
            }

            for (int idx = 0; idx < member.Rank; ++idx)
            {
                codeWriter.CloseBrace(); // if (source != null)
                codeWriter.CloseBrace(); // foreach
            }

            codeWriter.WriteLine();
        }
    }
}

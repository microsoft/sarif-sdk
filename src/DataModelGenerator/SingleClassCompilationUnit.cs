// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public class SingleClassCompilationUnit
    {
        public string Name { get; set; }
        public CompilationUnitSyntax Syntax { get; set; }
    }
}
// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************
namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public abstract class StructuralSymbol
    {
        public IGrammarSymbol Symbol { get; internal set; }
    }
}
// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public class Plus : StructuralSymbol, IGrammarSymbol
    {
        public override string ToString()
        {
            return this.Symbol + "+";
        }
    }
}
// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public class QuestionMark : StructuralSymbol, IGrammarSymbol
    {
        public override string ToString()
        {
            return this.Symbol + "?";
        }
    }
}
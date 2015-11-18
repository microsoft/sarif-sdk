// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public class Group : IGrammarSymbol
    {
        public Group()
        {
            this.Symbols = new List<IGrammarSymbol>();
        }

        public IList<IGrammarSymbol> Symbols { get; internal set; }

        public override string ToString()
        {
            return String.Format("({0})", String.Join(" ", this.Symbols));
        }
    }
}
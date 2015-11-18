// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.SecurityDevelopmentLifecycle.SdlCommon;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public class Production
    {
        public OffsetInfo Location;

        public IList<IGrammarSymbol> RHS;

        public Production()
        {
            this.RHS = new List<IGrammarSymbol>();
        }

        public NonTerminal LHS { get; set; }

        public override string ToString()
        {
            return String.Format("{0}\t{1}\t:\n\t{2} ;", this.Location, this.LHS, String.Join(" ", this.RHS));
        }

        internal bool IsAlternatingClassDeclaration()
        {
            if (this.RHS.Count != 1) { return false; }
            if (!(this.RHS[0] is Alternative)) { return false; }
            Alternative alternative = this.RHS[0] as Alternative;
            Contract.Requires(alternative.Symbols.Count > 0);
            foreach (IGrammarSymbol sym in alternative.Symbols)
            {
                if (!(sym is NonTerminal)
                    && !(sym is Group && ((Group)sym).Symbols.Count == 1 && ((Group)sym).Symbols.First() is NonTerminal))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
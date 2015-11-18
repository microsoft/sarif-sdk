// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>
    /// Represents an alternative token produced from a production in the form of X | Y | Z ...
    /// </summary>
    public class Alternative : IGrammarSymbol
    {
        public Alternative()
        {
            this.Symbols = new List<IGrammarSymbol>();
        }

        public IList<IGrammarSymbol> Symbols { get; private set; }

        public IDictionary<string, string> Annotations { get; set; }

        public string Type { get; set; }

        public override string ToString()
        {
            return String.Format("{0}", String.Join(" |\n\t", this.Symbols));
        }
    }
}
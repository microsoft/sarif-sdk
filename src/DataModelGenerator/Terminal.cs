// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    public class Terminal : IGrammarSymbol
    {
        public string Value { get; internal set; }
        public IDictionary<string, string> Annotations { get; set; }

        public override string ToString()
        {
            return "'" + this.Value + "'";
        }
    }
}
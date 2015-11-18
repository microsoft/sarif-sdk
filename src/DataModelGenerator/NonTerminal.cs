// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{

    public class NonTerminal : IGrammarSymbol
    {
        public string Name { get; internal set; }
        public string Type { get; set; }
        public string GrammarType { get; set; }
        public bool IsTypeFromGrammar { get; internal set; }
        public IDictionary<string,string> Annotations { get; set; }

        public override bool Equals(object obj)
        {
            NonTerminal that = obj as NonTerminal;
            if (that == null) { return false; }

            return that.Name.Equals(this.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override string ToString()
        {
            if (this.Type != this.Name)
            {
                return "/*[" + this.Name + "]*/" + this.Type;
            }

            return this.Name;
        }
    }
}
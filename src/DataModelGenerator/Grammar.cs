// *********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// *********************************************************

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{

    public class Grammar
    {
        private readonly Dictionary<string, TypeInformation> typeMap;
        private ImmutableList<Production> productions;

        private string grammarNamespace;

        public Grammar()
        {
            this.typeMap = new Dictionary<string, TypeInformation>();
            this.productions = ImmutableList<Production>.Empty;
        }

        public string EnumName
        {
            get
            {
                return this.Name + "Kind";
            }
        }

        public string GrammarClass
        {
            get
            {
                return this.Name;
            }
        }

        public string GrammarNamespace
        {
            get
            {
                // Preserve older behavior if namespace was not defined
                if (this.grammarNamespace == "Undefined")
                {
                    return this.Name + "Grammar";
                }

                return this.grammarNamespace;
            }

            set
            {
                this.grammarNamespace = value;
            }
        }

        public string Name { get; set; }

        public string SourcePath { get; set; }

        public void AddProduction(Production productionToAdd)
        {
            this.productions = this.productions.Add(productionToAdd);
            this.AddProductionToTypeMap(productionToAdd);
        }

        public void AddProductionRaw(Production productionToAdd)
        {
            this.productions = this.productions.Add(productionToAdd);
        }

        public TypeInformation LookupType(string name)
        {
            TypeInformation result;
            if (this.typeMap.TryGetValue(name, out result))
            {
                return result;
            }

            return null;
        }

        public TypeInformation LookupType(Production production)
        {
            return this.LookupType(production.LHS.Name);
        }

        public TypeInformation LookupType(NonTerminal nonTerminal)
        {
            return this.LookupType(nonTerminal.Type);
        }

        public IImmutableList<Production> Productions
        {
            get
            {
                return this.productions;
            }
        }

        public IEnumerable<KeyValuePair<Production, TypeInformation>> TypedProductions
        {
            get
            {
                foreach (Production production in this.productions)
                {
                    TypeInformation typeInformation = this.LookupType(production);
                    if (typeInformation != null)
                    {
                        yield return new KeyValuePair<Production, TypeInformation>(production, typeInformation);
                    }
                }
            }
        }

        private void AddProductionToTypeMap(Production production)
        {
            bool isNullable = true;
            bool isBaseType = false;
            bool isEnum = false;
            string className;
            if (!production.LHS.Annotations.TryGetValue("className", out className))
            {
                className = production.LHS.Name;
            }

            string serializedName;
            if (!production.LHS.Annotations.TryGetValue("serializedName", out serializedName))
            {
                serializedName = production.LHS.Name;
            }

            string cSharpTypeName = className;
            if (production.RHS.Count == 1)
            {
                NonTerminal type = production.RHS[0] as NonTerminal;
                if (type != null && Common.IsReserved(type.GrammarType))
                {
                    if (type.GrammarType.Equals(Common.DictionaryName))
                    {
                        // Dictionaries are treated as custom types 
                        if (type.Type.Equals(Common.DictionaryName))
                        {
                            cSharpTypeName = "Dictionary<string, string>";
                        }
                        else
                        {
                            cSharpTypeName = "Dictionary<string, " + type.Type + ">";
                        }
                    }
                    else
                    {
                        switch (type.Type)
                        {
                            case Common.BooleanName:
                                cSharpTypeName = "bool";
                                isBaseType = true;
                                isNullable = false;
                                break;
                            case Common.DictionaryName:
                                // default to a string to string dictionary
                                cSharpTypeName = "Dictionary<string, string>";
                                break;
                            case Common.IdentifierName:
                                cSharpTypeName = "string";
                                isBaseType = true;
                                break;
                            case Common.RegExName:
                                cSharpTypeName = "Regex";
                                isBaseType = true;
                                break;
                            case Common.StringName:
                                cSharpTypeName = "string";
                                isBaseType = true;
                                break;
                            case Common.NumberName:
                                cSharpTypeName = "int";
                                isNullable = false;
                                isBaseType = true;
                                break;
                            default:
                                cSharpTypeName = type.Type;
                                isBaseType = true;
                                break;
                        }
                    }
                }

                if (production.RHS[0] is Alternative)
                {
                    isBaseType = true;
                    isEnum = true;
                    isNullable = false;
                }
            }

            this.typeMap.Add(production.LHS.Name, new TypeInformation(
                className,
                cSharpTypeName,
                serializedName,
                isBaseType,
                isEnum,
                isNullable
            ));
        }

        public override string ToString()
        {
            return string.Format("grammar {0};" + Environment.NewLine + "{1}", this.Name, string.Join(Environment.NewLine, this.productions));
        }
    }
}
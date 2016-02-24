// Copyright            CreateBuiltin("URI", "global::System.Uri", DataModelTypeKind.BuiltInUri), (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    internal class DataModelBuilder
    {
        private static readonly ImmutableArray<DataModelType> s_basicBuiltinTypes = ImmutableArray.Create(
            CreateBuiltin("STRING", "string", DataModelTypeKind.BuiltInString),
            CreateBuiltin("NUMBER", "double", DataModelTypeKind.BuiltInNumber),
            CreateBuiltin("BOOLEAN", "bool", DataModelTypeKind.BuiltInBoolean),
            CreateBuiltin("ID", "string", DataModelTypeKind.BuiltInString),
            CreateBuiltin("INTEGER", "int", DataModelTypeKind.BuiltInNumber),
            CreateBuiltin("DICTIONARY", "global::System.Collections.Generic.Dictionary<string, string>", DataModelTypeKind.BuiltInDictionary),
            CreateBuiltin("URI", "global::System.Uri", DataModelTypeKind.BuiltInUri),
            CreateBuiltin("DATETIME", "global::System.DateTime", DataModelTypeKind.BuiltInDateTime),
            CreateBuiltin("VERSION", "global::System.Version", DataModelTypeKind.BuiltInVersion)
            );

        private static DataModelType CreateBuiltin(string g4Name, string cSharpName, DataModelTypeKind kind)
        {
            return new DataModelType(
                false,
                null,
                null,
                g4Name,
                cSharpName,
                null,
                ImmutableArray<DataModelMember>.Empty,
                ImmutableArray<string>.Empty,
                ImmutableArray<string>.Empty,
                ImmutableArray<ToStringEntry>.Empty,
                null,
                kind
                );
        }

        private readonly Dictionary<string, DataModelLeafTypeBuilder> _compiledTypes = new Dictionary<string, DataModelLeafTypeBuilder>();
        private readonly Dictionary<string, DataModelBaseTypeBuilder> _compiledBases = new Dictionary<string, DataModelBaseTypeBuilder>();

        private void AddCompiledType(DataModelLeafTypeBuilder builder)
        {
            _compiledTypes.Add(builder.G4DeclaredName, builder);
        }

        public void CompileProduction(GrammarSymbol production)
        {
            Debug.Assert(production.Kind == SymbolKind.Production);
            GrammarSymbol decl = production[0];
            Debug.Assert(decl.Kind == SymbolKind.ProductionDecl);
            if (decl.FirstToken != production.FirstToken)
            {
                // Honestly I don't know why "fragment" productions are even
                // in the grammar if they're not supposed to be produced; but this is what
                // the original MSR code did and we need to parse their grammar files.
                Debug.Assert(production.FirstToken.GetText() == "fragment");
                return;
            }

            string productionName = decl.GetLogicalText();
            if (s_basicBuiltinTypes.Any(type => type.G4DeclaredName == productionName))
            {
                // Builtins don't matter in the grammar.
                return;
            }

            GrammarSymbol productionIs = production[1];
            switch (productionIs.Kind)
            {
                case SymbolKind.String:
                    this.CompileEnumValueType(decl, productionIs.FirstToken);
                    break;
                case SymbolKind.Identifier:
                case SymbolKind.ZeroOrMoreQuantifier:
                case SymbolKind.OneOrMoreQuantifier:
                case SymbolKind.ZeroOrOneQuantifier:
                    this.CompileStandardType(decl, ImmutableArray.Create(productionIs));
                    break;
                case SymbolKind.Group:
                    this.CompileStandardType(decl, productionIs.Children);
                    break;
                case SymbolKind.Alternation:
                    if (productionIs.Children.All(child => child.Kind == SymbolKind.String))
                    {
                        this.CompileEnumValueType(decl, productionIs.Children);
                        break;
                    }
                    else if (productionIs.Children.All(child => child.Kind == SymbolKind.Identifier))
                    {
                        this.CompileBaseType(decl, productionIs.Children);
                        break;
                    }

                    goto default;
                default:
                    throw new G4ParseFailureException(production.GetLocation(), Strings.UnrecognizedDataModel, productionName);
            }
        }

        public DataModel Link(string sourceFilePath, DataModelMetadata metaData)
        {
            this.AnnotateDerivedTypes();
            if (metaData.GenerateLocations)
            {
                this.AddLocationsToTypes();
            }

            return new DataModel(
                sourceFilePath,
                metaData,
                this.BuildTypes()
                );
        }

        private void AddLocationsToTypes()
        {
            foreach (DataModelLeafTypeBuilder builder in _compiledTypes.Values)
            {
                bool generateLength = true;
                bool generateOffset = true;

                foreach (DataModelMember member in builder.Members)
                {
                    if (member.CSharpName == "Length")
                    {
                        generateLength = false;
                    }

                    if (member.CSharpName == "Offset")
                    {
                        generateOffset = false;
                    }
                }

                if (generateLength)
                {
                    DataModelMember newMember = new DataModelMember(
                        "INTEGER",
                        "Length",
                        "length",
                        "The length of this object in a source text file.",
                        "length",
                        null,
                        null,
                        null,
                        null,
                        null, 
                        0,
                        false
                        );

                    builder.Members.Add(newMember);
                    builder.ToStringEntries.Add(new ToStringEntry(newMember));
                }

                if (generateOffset)
                {
                    DataModelMember newMember = new DataModelMember(
                        "INTEGER",
                        "Offset",
                        "offset",
                        "The offset of this object in a source text file.",
                        "offset",
                        null,
                        null,
                        null,
                        null,
                        null,
                        0,
                        false
                        );

                    builder.Members.Add(newMember);
                    builder.ToStringEntries.Add(new ToStringEntry(newMember));
                }
            }
        }

        private IEnumerable<DataModelType> BuildTypes()
        {
            var bases = (IEnumerable<DataModelTypeBuilder>)_compiledBases.Values;
            return s_basicBuiltinTypes.Concat(bases
               .Concat(_compiledTypes.Values)
               .Select(typeSpec => typeSpec.ToImmutable()));
        }

        private void AnnotateDerivedTypes()
        {
            foreach (DataModelBaseTypeBuilder baseSpec in _compiledBases.Values)
            {
                foreach (string derivedName in baseSpec.DerivedDecls)
                {
                    this.AnnotateDerivedTypes(baseSpec, derivedName);
                }
            }
        }

        private void AnnotateDerivedTypes(DataModelBaseTypeBuilder baseBuilder, string derivedName)
        {
            if (!this.TryAnnotateDerivedType(_compiledTypes, baseBuilder, derivedName) && !this.TryAnnotateDerivedType(_compiledBases, baseBuilder, derivedName))
            {
                throw new G4ParseFailureException(baseBuilder.DeclSymbol.GetLocation(), Strings.UnresolvedDerivedType, derivedName);
            }
        }

        private bool TryAnnotateDerivedType<T>(Dictionary<string, T> lookupTable, DataModelBaseTypeBuilder baseBuilder, string derivedName)
            where T : DataModelTypeBuilder
        {
            T derived;
            if (lookupTable.TryGetValue(derivedName, out derived))
            {
                string existingBase = derived.Base;
                if (existingBase != null && existingBase != baseBuilder.CSharpName)
                {
                    DataModelBaseTypeBuilder existingBaseBuilder = _compiledBases.Values.First(compiledBase => compiledBase.CSharpName == baseBuilder.CSharpName);

                    throw new G4ParseFailureException(baseBuilder.DeclSymbol.GetLocation(), Strings.TypeAlreadyTaken,
                        derived.G4DeclaredName, existingBaseBuilder.G4DeclaredName, baseBuilder.G4DeclaredName);
                }

                derived.Base = baseBuilder.CSharpName;
                return true;
            }

            return false;
        }

        private void CompileBaseType(GrammarSymbol decl, ImmutableArray<GrammarSymbol> children)
        {
            _compiledBases.Add(decl.GetLogicalText(),
                new DataModelBaseTypeBuilder(decl, children.Select(child => child.GetLogicalText())));
        }

        private void CompileEnumValueType(GrammarSymbol decl, Token token)
        {
            var result = new DataModelLeafTypeBuilder(decl);

            var declaredValues = new List<string>();
            declaredValues.Add(token.GetText());
            result.G4DeclaredValues = declaredValues;
            this.AddCompiledType(result);
        }

        private void CompileEnumValueType(GrammarSymbol decl, ImmutableArray<GrammarSymbol> enumMembers)
        {
            var result = new DataModelLeafTypeBuilder(decl);

            var declaredValues = new List<string>();
            foreach (GrammarSymbol symbol in enumMembers)
            {
                declaredValues.Add(symbol.FirstToken.GetText());
            }

            result.G4DeclaredValues = declaredValues;
            this.AddCompiledType(result);
        }

        private void CompileStringValueType(GrammarSymbol decl)
        {
            var result = new DataModelLeafTypeBuilder(decl);
            var member = new DataModelMember("STRING", "StringValue", "stringValue", null, "stringValue", null, null, null, null, null, 0, false);
            result.Members.Add(member);
            result.ToStringEntries.Add(new ToStringEntry(member));
            this.AddCompiledType(result);
        }

        private void CompileStandardType(GrammarSymbol decl, ImmutableArray<GrammarSymbol> groupList)
        {
            var result = new DataModelLeafTypeBuilder(decl);
            int idx = 0;
            while (idx < groupList.Length)
            {
                List<string> staticStrings = GetStringValuesAfter(groupList, idx);
                foreach (string staticString in staticStrings)
                {
                    result.ToStringEntries.Add(new ToStringEntry(staticString, null));
                }

                idx += staticStrings.Count;
                if (idx >= groupList.Length)
                {
                    break;
                }

                GrammarSymbol currentSymbol = groupList[idx];
                GrammarSymbol memberDeclSymbol = GetDeclaringIdentifierSymbol(currentSymbol, result.G4DeclaredName);
                GrammarSymbol lookAheadSymbol = GetSymbolOrEmpty(groupList, idx + 1);

                // Check for patterns:
                if (MatchesDelimitedCollectionPattern(currentSymbol, memberDeclSymbol, lookAheadSymbol))
                {
                    // x ('delimiter' x)*
                    string delimeter = lookAheadSymbol[0][0].GetLogicalText();
                    result.AddMember(memberDeclSymbol, 1, currentSymbol.Kind == SymbolKind.Identifier, delimeter);
                    idx += 2;
                    continue;
                }

                int rank;
                bool required;
                switch (currentSymbol.Kind)
                {
                    case SymbolKind.Identifier:
                        // x
                        rank = 0;
                        required = true;
                        break;
                    case SymbolKind.ZeroOrOneQuantifier:
                        // x?
                        rank = 0;
                        required = false;
                        break;
                    case SymbolKind.ZeroOrMoreQuantifier:
                        // x*
                        rank = 1;
                        required = false;
                        break;
                    case SymbolKind.OneOrMoreQuantifier:
                        // x+
                        rank = 1;
                        required = true;
                        break;
                    default:
                        throw new G4ParseFailureException(currentSymbol.GetLocation(), Strings.UnrecognizedDataModel, result.G4DeclaredName);
                }

                ++idx;
                result.AddMember(memberDeclSymbol, rank, required);
            }

            this.AddCompiledType(result);
        }

        private static GrammarSymbol GetDeclaringIdentifierSymbol(GrammarSymbol currentSymbol, string productionName)
        {
            GrammarSymbol declSymbol;
            switch (currentSymbol.Kind)
            {
                case SymbolKind.ZeroOrOneQuantifier:
                case SymbolKind.ZeroOrMoreQuantifier:
                case SymbolKind.OneOrMoreQuantifier:
                    declSymbol = currentSymbol[0];
                    break;
                case SymbolKind.Identifier:
                    declSymbol = currentSymbol;
                    break;
                default:
                    declSymbol = GrammarSymbol.Empty;
                    break;
            }

            if (declSymbol.Kind != SymbolKind.Identifier)
            {
                throw new G4ParseFailureException(currentSymbol.GetLocation(), Strings.UnrecognizedDataModel, productionName);
            }

            return declSymbol;
        }

        private static bool MatchesDelimitedCollectionPattern(GrammarSymbol currentSymbol, GrammarSymbol memberDeclSymbol, GrammarSymbol lookAheadSymbol)
        {
            // x ('delimiter' x)*
            // or
            // x? ('delimiter' x)*
            return (currentSymbol.Kind == SymbolKind.ZeroOrOneQuantifier || currentSymbol.Kind == SymbolKind.Identifier) &&
                   lookAheadSymbol.Kind == SymbolKind.ZeroOrMoreQuantifier &&
                   lookAheadSymbol[0].Kind == SymbolKind.Group &&
                   lookAheadSymbol[0][0].Kind == SymbolKind.String &&
                   lookAheadSymbol[0][1].Kind == SymbolKind.Identifier &&
                   memberDeclSymbol.FirstToken.TextEqual(lookAheadSymbol[0][1].FirstToken);
        }

        private static GrammarSymbol GetSymbolOrEmpty(ImmutableArray<GrammarSymbol> groupMemberList, int idx)
        {
            if (idx < groupMemberList.Length)
            {
                return groupMemberList[idx];
            }

            return GrammarSymbol.Empty;
        }

        private static List<string> GetStringValuesAfter(ImmutableArray<GrammarSymbol> groupMemberList, int idx)
        {
            var result = new List<string>();
            for (; idx < groupMemberList.Length; ++idx)
            {
                GrammarSymbol current = groupMemberList[idx];
                if (current.Kind != SymbolKind.String)
                {
                    break;
                }

                result.Add(current.GetLogicalText());
            }

            return result;
        }
    }
}

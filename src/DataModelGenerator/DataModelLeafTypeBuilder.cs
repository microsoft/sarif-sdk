// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    internal sealed class DataModelLeafTypeBuilder : DataModelTypeBuilder
    {
        public ImmutableArray<DataModelMember>.Builder Members;
        public List<ToStringEntry> ToStringEntries;

        public DataModelLeafTypeBuilder(GrammarSymbol decl)
            : base(decl)
        {
            this.Members = ImmutableArray.CreateBuilder<DataModelMember>();
            this.ToStringEntries = new List<ToStringEntry>();
        }

        public override DataModelType ToImmutable()
        {
            return new DataModelType(
                this.SummaryText,
                this.RemarksText,
                this.G4DeclaredName,
                this.CSharpName,
                this.Members.ToImmutable(),
                ToStringEntry.Coalesce(this.ToStringEntries),
                this.Base,
                DataModelTypeKind.Leaf
                );
        }

        internal void AddMember(GrammarSymbol declSymbol, int rank, bool required, string delimeter = null)
        {
            string declaredName = declSymbol.GetLogicalText();
            string annotationName = declSymbol.Annotations.GetAnnotationValue("name");
            string cSharpName = annotationName ?? LinguisticTransformer.ToCSharpName(declaredName);
            string serializedName = declSymbol.Annotations.GetAnnotationValue("serializedName");
            if (serializedName == null)
            {
                serializedName = LinguisticTransformer.ToJsonName(cSharpName);
            }

            if (serializedName == null)
            {
                serializedName = LinguisticTransformer.ToJsonName(declaredName);
            }

            string argumentName = declSymbol.Annotations.GetAnnotationValue("argumentName");
            if (argumentName == null)
            {
                if (annotationName == null)
                {
                    argumentName = LinguisticTransformer.ToArgumentName(declaredName);
                }
                else
                {
                    argumentName = LinguisticTransformer.ToArgumentName(annotationName);
                }
            }

            foreach (DataModelMember existingMember in this.Members)
            {
                if (existingMember.CSharpName == cSharpName)
                {
                    throw new G4ParseFailureException(declSymbol.GetLocation(), Strings.DuplicateCSharpMemberName, cSharpName);
                }

                if (existingMember.SerializedName == serializedName)
                {
                    throw new G4ParseFailureException(declSymbol.GetLocation(), Strings.DuplicateJsonMemberName, serializedName);
                }

                if (existingMember.ArgumentName == argumentName)
                {
                    throw new G4ParseFailureException(declSymbol.GetLocation(), Strings.DuplicateArgumentName, argumentName);
                }
            }

            DataModelMember newMember = new DataModelMember(
                declaredName,
                cSharpName,
                serializedName,
                declSymbol.Annotations.GetAnnotationValue("summary"),
                argumentName,
                rank,
                required
                );


            this.Members.Add(newMember);
            this.ToStringEntries.Add(new ToStringEntry(delimeter, newMember));
        }
    }
}
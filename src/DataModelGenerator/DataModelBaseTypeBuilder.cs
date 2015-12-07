// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    internal sealed class DataModelBaseTypeBuilder : DataModelTypeBuilder
    {
        public readonly ImmutableArray<string> DerivedDecls;

        public DataModelBaseTypeBuilder(GrammarSymbol decl, IEnumerable<string> derivedDecls)
            : base(decl)
        {
            this.DerivedDecls = derivedDecls.ToImmutableArray();
        }

        public override DataModelType ToImmutable()
        {
            return new DataModelType(
                this.RootObject,
                this.SummaryText,
                this.RemarksText,
                this.G4DeclaredName,
                this.CSharpName,
                ImmutableArray<DataModelMember>.Empty,
                ImmutableArray<string>.Empty,
                ImmutableArray<string>.Empty,
                ImmutableArray<ToStringEntry>.Empty,
                this.Base,
                DataModelTypeKind.Base
                );
        }
    }
}
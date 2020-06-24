// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif;

namespace BSOA.Column
{
    internal class ArtifactLocationColumn : WrappingColumn<ArtifactLocation, int>
    {
        private ArtifactLocationTable _table;

        public ArtifactLocationColumn(SarifLogDatabase db) : base(new RefColumn(nameof(ArtifactLocation)))
        {
            _table = db.ArtifactLocation;
        }

        public override ArtifactLocation this[int index] 
        { 
            get => new ArtifactLocation(_table, Inner[index]);
            set => Inner[index] = _table.LocalIndex(value);
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif;

namespace BSOA.Column
{
    internal class ArtifactLocationColumn : WrappingColumn<ArtifactLocation, int>
    {
        private readonly SarifLogDatabase _database;

        public ArtifactLocationColumn(SarifLogDatabase database, RefColumn inner) : base(inner)
        {
            _database = database;
        }

        public override ArtifactLocation this[int index] 
        { 
            get => new ArtifactLocation(_database.ArtifactLocation, Inner[index]);
            set => Inner[index] = _database.ArtifactLocation.LocalIndex(value);
        }
    }
}

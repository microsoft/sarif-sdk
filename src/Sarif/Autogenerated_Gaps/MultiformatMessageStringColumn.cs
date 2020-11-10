// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif;

namespace BSOA.Column
{
    internal class MultiformatMessageStringColumn : WrappingColumn<MultiformatMessageString, int>
    {
        private readonly SarifLogDatabase _database;

        public MultiformatMessageStringColumn(SarifLogDatabase database, RefColumn inner) : base(inner)
        {
            _database = database;
        }

        public override MultiformatMessageString this[int index] 
        {
            get => new MultiformatMessageString(_database.MultiformatMessageString, Inner[index]);
            set => Inner[index] = _database.MultiformatMessageString.LocalIndex(value);
        }
    }
}

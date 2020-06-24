// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif;

namespace BSOA.Column
{
    internal class MultiformatMessageStringColumn : WrappingColumn<MultiformatMessageString, int>
    {
        private MultiformatMessageStringTable _table;

        public MultiformatMessageStringColumn(SarifLogDatabase db) : base(new RefColumn(nameof(MultiformatMessageString)))
        {
            _table = db.MultiformatMessageString;
        }

        public override MultiformatMessageString this[int index] 
        {
            get => new MultiformatMessageString(_table, Inner[index]);
            set => Inner[index] = _table.LocalIndex(value);
        }
    }
}

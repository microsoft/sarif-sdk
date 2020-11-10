// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Run
    {
        // BSOA: Expose root object (SarifLog) containing this run to enable creating new objects in the correct instance.
        internal SarifLog Log => _table.Database.SarifLog[0];

        partial void Init()
        {
            Language = "en-US";
            ColumnKind = ColumnKind.Utf16CodeUnits;
        }
    }
}

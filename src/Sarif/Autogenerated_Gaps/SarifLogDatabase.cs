// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using BSOA.Column;
using BSOA.Model;

namespace Microsoft.CodeAnalysis.Sarif
{
    internal partial class SarifLogDatabase
    {
        public override IColumn BuildColumn(string tableName, string columnName, Type type, object defaultValue = null)
        {
            if (type == typeof(IDictionary<String, MultiformatMessageString>))
            {
                RefColumn valuesInner = new RefColumn(nameof(MultiformatMessageString));

                return new RefDictionaryColumn<String, MultiformatMessageString>(
                    new DistinctColumn<string>(new StringColumn()),
                    new MultiformatMessageStringColumn(this, valuesInner),
                    valuesInner);
            }
            else if(type == typeof(IDictionary<String, ArtifactLocation>))
            {
                RefColumn valuesInner = new RefColumn(nameof(ArtifactLocation));

                return new RefDictionaryColumn<String, ArtifactLocation>(
                    new DistinctColumn<string>(new StringColumn()),
                    new ArtifactLocationColumn(this, valuesInner),
                    valuesInner);
            }
            else if (type == typeof(IDictionary<String, SerializedPropertyInfo>))
            {
                // SerializedPropertyInfo is a composed data type, not a reference, so no RefColumn wrapping needed.
                return new DictionaryColumn<String, SerializedPropertyInfo>(
                    new DistinctColumn<string>(new StringColumn()),
                    new SerializedPropertyInfoColumn());
            }
            else
            {
                return ColumnFactory.Build(type, defaultValue, recurseTo: (t, d) => BuildColumn(tableName, columnName, t, d));
            }
        }
    }
}

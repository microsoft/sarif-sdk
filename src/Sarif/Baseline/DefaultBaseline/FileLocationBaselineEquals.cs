// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class FileLocationBaselineEquals : IEqualityComparer<FileLocation>
    {
        public static readonly FileLocationBaselineEquals Instance = new FileLocationBaselineEquals();
        public bool Equals(FileLocation x, FileLocation y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                if (x == null || y == null)
                {
                    return false;
                }

                // Only compare URI (so file path/relative file path).  UriBaseId may change run over run.
                if (x.Uri != y.Uri)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(FileLocation obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }
            else
            {
                return obj.Uri.GetNullCheckedHashCode();
            }
        }
    }
}

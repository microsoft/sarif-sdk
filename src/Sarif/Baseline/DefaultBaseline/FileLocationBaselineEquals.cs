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

                if (x.Uri != y.Uri || x.UriBaseId != y.UriBaseId)
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

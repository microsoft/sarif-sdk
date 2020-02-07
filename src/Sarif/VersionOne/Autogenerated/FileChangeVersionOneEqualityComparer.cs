// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type FileChangeVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class FileChangeVersionOneEqualityComparer : IEqualityComparer<FileChangeVersionOne>
    {
        internal static readonly FileChangeVersionOneEqualityComparer Instance = new FileChangeVersionOneEqualityComparer();

        public bool Equals(FileChangeVersionOne left, FileChangeVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Uri != right.Uri)
            {
                return false;
            }

            if (left.UriBaseId != right.UriBaseId)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Replacements, right.Replacements))
            {
                if (left.Replacements == null || right.Replacements == null)
                {
                    return false;
                }

                if (left.Replacements.Count != right.Replacements.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Replacements.Count; ++index_0)
                {
                    if (!ReplacementVersionOne.ValueComparer.Equals(left.Replacements[index_0], right.Replacements[index_0]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(FileChangeVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Uri != null)
                {
                    result = (result * 31) + obj.Uri.GetHashCode();
                }

                if (obj.UriBaseId != null)
                {
                    result = (result * 31) + obj.UriBaseId.GetHashCode();
                }

                if (obj.Replacements != null)
                {
                    foreach (var value_0 in obj.Replacements)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.ValueGetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}
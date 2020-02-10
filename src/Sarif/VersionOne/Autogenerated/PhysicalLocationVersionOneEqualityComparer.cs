// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type PhysicalLocationVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class PhysicalLocationVersionOneEqualityComparer : IEqualityComparer<PhysicalLocationVersionOne>
    {
        internal static readonly PhysicalLocationVersionOneEqualityComparer Instance = new PhysicalLocationVersionOneEqualityComparer();

        public bool Equals(PhysicalLocationVersionOne left, PhysicalLocationVersionOne right)
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

            if (!RegionVersionOne.ValueComparer.Equals(left.Region, right.Region))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(PhysicalLocationVersionOne obj)
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

                if (obj.Region != null)
                {
                    result = (result * 31) + obj.Region.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}
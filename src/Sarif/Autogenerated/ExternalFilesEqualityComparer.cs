// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ExternalFiles for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
    internal sealed class ExternalFilesEqualityComparer : IEqualityComparer<ExternalFiles>
    {
        internal static readonly ExternalFilesEqualityComparer Instance = new ExternalFilesEqualityComparer();

        public bool Equals(ExternalFiles left, ExternalFiles right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.Conversion, right.Conversion))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.Files, right.Files))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.Graphs, right.Graphs))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.Invocations, right.Invocations))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.LogicalLocations, right.LogicalLocations))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.Resources, right.Resources))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Results, right.Results))
            {
                if (left.Results == null || right.Results == null)
                {
                    return false;
                }

                if (left.Results.Count != right.Results.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Results.Count; ++index_0)
                {
                    if (!FileLocation.ValueComparer.Equals(left.Results[index_0], right.Results[index_0]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(ExternalFiles obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Conversion != null)
                {
                    result = (result * 31) + obj.Conversion.ValueGetHashCode();
                }

                if (obj.Files != null)
                {
                    result = (result * 31) + obj.Files.ValueGetHashCode();
                }

                if (obj.Graphs != null)
                {
                    result = (result * 31) + obj.Graphs.ValueGetHashCode();
                }

                if (obj.Invocations != null)
                {
                    result = (result * 31) + obj.Invocations.ValueGetHashCode();
                }

                if (obj.LogicalLocations != null)
                {
                    result = (result * 31) + obj.LogicalLocations.ValueGetHashCode();
                }

                if (obj.Resources != null)
                {
                    result = (result * 31) + obj.Resources.ValueGetHashCode();
                }

                if (obj.Results != null)
                {
                    foreach (var value_0 in obj.Results)
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
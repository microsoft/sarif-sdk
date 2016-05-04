// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type SarifLog for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public sealed class SarifLogEqualityComparer : IEqualityComparer<SarifLog>
    {
        internal static readonly SarifLogEqualityComparer Instance = new SarifLogEqualityComparer();

        public bool Equals(SarifLog left, SarifLog right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.SchemaUri != right.SchemaUri)
            {
                return false;
            }

            if (left.Version != right.Version)
            {
                return false;
            }

            if (!Object.ReferenceEquals(left.Runs, right.Runs))
            {
                if (left.Runs == null || right.Runs == null)
                {
                    return false;
                }

                if (left.Runs.Count != right.Runs.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Runs.Count; ++index_0)
                {
                    if (!Run.ValueComparer.Equals(left.Runs[index_0], right.Runs[index_0]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(SarifLog obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.SchemaUri != null)
                {
                    result = (result * 31) + obj.SchemaUri.GetHashCode();
                }

                result = (result * 31) + obj.Version.GetHashCode();
                if (obj.Runs != null)
                {
                    foreach (var value_0 in obj.Runs)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}
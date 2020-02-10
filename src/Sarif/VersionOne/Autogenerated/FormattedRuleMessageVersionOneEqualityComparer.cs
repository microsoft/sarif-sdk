// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type FormattedRuleMessageVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class FormattedRuleMessageVersionOneEqualityComparer : IEqualityComparer<FormattedRuleMessageVersionOne>
    {
        internal static readonly FormattedRuleMessageVersionOneEqualityComparer Instance = new FormattedRuleMessageVersionOneEqualityComparer();

        public bool Equals(FormattedRuleMessageVersionOne left, FormattedRuleMessageVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.FormatId != right.FormatId)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Arguments, right.Arguments))
            {
                if (left.Arguments == null || right.Arguments == null)
                {
                    return false;
                }

                if (left.Arguments.Count != right.Arguments.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Arguments.Count; ++index_0)
                {
                    if (left.Arguments[index_0] != right.Arguments[index_0])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(FormattedRuleMessageVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.FormatId != null)
                {
                    result = (result * 31) + obj.FormatId.GetHashCode();
                }

                if (obj.Arguments != null)
                {
                    foreach (var value_0 in obj.Arguments)
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
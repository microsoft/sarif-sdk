// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type FormattedRuleMessage for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    internal sealed class FormattedRuleMessageEqualityComparer : IEqualityComparer<FormattedRuleMessage>
    {
        internal static readonly FormattedRuleMessageEqualityComparer Instance = new FormattedRuleMessageEqualityComparer();

        public bool Equals(FormattedRuleMessage left, FormattedRuleMessage right)
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

            if (!Object.ReferenceEquals(left.Arguments, right.Arguments))
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

        public int GetHashCode(FormattedRuleMessage obj)
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
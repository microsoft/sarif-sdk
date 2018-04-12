// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type RuleConfiguration for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class RuleConfigurationEqualityComparer : IEqualityComparer<RuleConfiguration>
    {
        internal static readonly RuleConfigurationEqualityComparer Instance = new RuleConfigurationEqualityComparer();

        public bool Equals(RuleConfiguration left, RuleConfiguration right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Enabled != right.Enabled)
            {
                return false;
            }

            if (left.DefaultLevel != right.DefaultLevel)
            {
                return false;
            }

            if (!object.Equals(left.Parameters, right.Parameters))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(RuleConfiguration obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Enabled.GetHashCode();
                result = (result * 31) + obj.DefaultLevel.GetHashCode();
                if (obj.Parameters != null)
                {
                    result = (result * 31) + obj.Parameters.GetHashCode();
                }
            }

            return result;
        }
    }
}
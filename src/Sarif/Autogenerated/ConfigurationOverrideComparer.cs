// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ConfigurationOverride for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class ConfigurationOverrideComparer : IComparer<ConfigurationOverride>
    {
        internal static readonly ConfigurationOverrideComparer Instance = new ConfigurationOverrideComparer();

        public int Compare(ConfigurationOverride left, ConfigurationOverride right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = ReportingConfigurationComparer.Instance.Compare(left.Configuration, right.Configuration);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = ReportingDescriptorReferenceComparer.Instance.Compare(left.Descriptor, right.Descriptor);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Properties.DictionaryCompares(right.Properties, SerializedPropertyInfoComparer.Instance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
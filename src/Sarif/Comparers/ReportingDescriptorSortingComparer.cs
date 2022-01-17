// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

/// <summary>
/// All Comparer implementations should be replaced by auto-generated codes by JSchema as 
/// part of EqualityComparer in a planned comprehensive solution.
/// Tracking by issue: https://github.com/microsoft/jschema/issues/141
/// </summary>
namespace Microsoft.CodeAnalysis.Sarif
{
    internal class ReportingDescriptorSortingComparer : IComparer<ReportingDescriptor>
    {
        internal static readonly ReportingDescriptorSortingComparer Instance = new ReportingDescriptorSortingComparer();

        public int Compare(ReportingDescriptor left, ReportingDescriptor right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            int compareResult = 0;
            compareResult = string.Compare(left.Id, right.Id);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.DeprecatedIds, right.DeprecatedIds))
            {
                if (left.DeprecatedIds == null)
                {
                    return -1;
                }

                if (right.DeprecatedIds == null)
                {
                    return 1;
                }

                compareResult = left.DeprecatedIds.Count.CompareTo(right.DeprecatedIds.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.DeprecatedIds.Count; ++i)
                {
                    compareResult = string.Compare(left.DeprecatedIds[i], right.DeprecatedIds[i]);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            compareResult = string.Compare(left.Guid, right.Guid);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.DeprecatedGuids, right.DeprecatedGuids))
            {
                if (left.DeprecatedGuids == null)
                {
                    return -1;
                }

                if (right.DeprecatedGuids == null)
                {
                    return 1;
                }

                compareResult = left.DeprecatedGuids.Count.CompareTo(right.DeprecatedGuids.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.DeprecatedGuids.Count; ++i)
                {
                    compareResult = string.Compare(left.DeprecatedGuids[i], right.DeprecatedGuids[i]);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            compareResult = string.Compare(left.Name, right.Name);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.DeprecatedNames, right.DeprecatedNames))
            {
                if (left.DeprecatedNames == null)
                {
                    return -1;
                }

                if (right.DeprecatedNames == null)
                {
                    return 1;
                }

                compareResult = left.DeprecatedNames.Count.CompareTo(right.DeprecatedNames.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.DeprecatedNames.Count; ++i)
                {
                    compareResult = string.Compare(left.DeprecatedNames[i], right.DeprecatedNames[i]);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            compareResult = MultiformatMessageStringSortingComparer.Instance.Compare(left.ShortDescription, right.ShortDescription);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = MultiformatMessageStringSortingComparer.Instance.Compare(left.FullDescription, right.FullDescription);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = ReportingConfigurationSortingComparer.Instance.Compare(left.DefaultConfiguration, right.DefaultConfiguration);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.HelpUri.OriginalString, right.HelpUri.OriginalString);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = MultiformatMessageStringSortingComparer.Instance.Compare(left.Help, right.Help);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}

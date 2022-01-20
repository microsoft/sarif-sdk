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
    internal class MessageSortingComparer : IComparer<Message>
    {
        internal static readonly MessageSortingComparer Instance = new MessageSortingComparer();

        public int Compare(Message left, Message right)
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
            compareResult = string.Compare(left.Text, right.Text);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Markdown, right.Markdown);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Id, right.Id);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.Arguments, right.Arguments))
            {
                if (left.Arguments == null)
                {
                    return -1;
                }

                if (right.Arguments == null)
                {
                    return 1;
                }

                compareResult = left.Arguments.Count.CompareTo(right.Arguments.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.Arguments.Count; ++i)
                {
                    compareResult = string.Compare(left.Arguments[i], right.Arguments[i]);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            return compareResult;
        }
    }
}

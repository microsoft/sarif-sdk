// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Message for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class MessageComparer : IComparer<Message>
    {
        internal static readonly MessageComparer Instance = new MessageComparer();

        public int Compare(Message left, Message right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

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

            compareResult = left.Arguments.ListCompares(right.Arguments);
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
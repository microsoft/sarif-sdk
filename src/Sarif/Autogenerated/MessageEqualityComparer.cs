// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Message for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
    internal sealed class MessageEqualityComparer : IEqualityComparer<Message>
    {
        internal static readonly MessageEqualityComparer Instance = new MessageEqualityComparer();

        public bool Equals(Message left, Message right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Text != right.Text)
            {
                return false;
            }

            if (left.MessageId != right.MessageId)
            {
                return false;
            }

            if (left.RichText != right.RichText)
            {
                return false;
            }

            if (left.RichMessageId != right.RichMessageId)
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

        public int GetHashCode(Message obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Text != null)
                {
                    result = (result * 31) + obj.Text.GetHashCode();
                }

                if (obj.MessageId != null)
                {
                    result = (result * 31) + obj.MessageId.GetHashCode();
                }

                if (obj.RichText != null)
                {
                    result = (result * 31) + obj.RichText.GetHashCode();
                }

                if (obj.RichMessageId != null)
                {
                    result = (result * 31) + obj.RichMessageId.GetHashCode();
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
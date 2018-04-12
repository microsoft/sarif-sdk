// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Attachment for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class AttachmentEqualityComparer : IEqualityComparer<Attachment>
    {
        internal static readonly AttachmentEqualityComparer Instance = new AttachmentEqualityComparer();

        public bool Equals(Attachment left, Attachment right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Description, right.Description))
            {
                return false;
            }

            if (!FileLocation.ValueComparer.Equals(left.FileLocation, right.FileLocation))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Attachment obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Description != null)
                {
                    result = (result * 31) + obj.Description.ValueGetHashCode();
                }

                if (obj.FileLocation != null)
                {
                    result = (result * 31) + obj.FileLocation.ValueGetHashCode();
                }
            }

            return result;
        }
    }
}
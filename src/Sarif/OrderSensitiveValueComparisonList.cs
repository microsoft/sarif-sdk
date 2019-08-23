// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class OrderSensitiveValueComparisonList<T> : List<T>, IEqualityComparer<List<T>>
    {
        private readonly IEqualityComparer<T> _equalityComparer;

        public OrderSensitiveValueComparisonList(IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer;
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj as List<T>);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public bool Equals(List<T> left, List<T> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (!_equalityComparer.Equals(left[i], right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(List<T> obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Count.GetHashCode();

                for (int i = 0; i < obj.Count; i++)
                {
                    result = (result * 31) + _equalityComparer.GetHashCode(obj[i]);
                }
            }
            return result;
        }
    }
}

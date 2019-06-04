// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Query
{
    /// <summary>
    ///  StringSlice represents a portion of a string and can wrap different
    ///  string subsets without allocations.
    /// </summary>
    internal struct StringSlice
    {
        private string Value { get; set; }
        private int Index { get; set; }
        public int Length { get; private set; }
        public char this[int index] => Value[Index + index];

        public StringSlice(string value, int index, int length)
        {
            if (index < 0 || length < 0 || index + length > (value?.Length ?? 0)) { throw new ArgumentOutOfRangeException($"StringSlice for index {index}, length {length} out of range for string length {value?.Length ?? 0}"); }
            Value = value;
            Index = index;
            Length = length;
        }

        public StringSlice(string value) : this(value, 0, value?.Length ?? 0)
        { }

        public static implicit operator StringSlice(string value)
        {
            return new StringSlice(value, 0, value?.Length ?? 0);
        }

        public StringSlice Substring(int index)
        {
            if (index == 0) { return this; }
            return Substring(index, Length - index);
        }

        public StringSlice Substring(int index, int length)
        {
            return new StringSlice(Value, Index + index, length);
        }

        public int CompareTo(StringSlice other, StringComparison comparison = StringComparison.Ordinal)
        {
            int length = Math.Min(this.Length, other.Length);
            int compareResult = String.Compare(this.Value, this.Index, other.Value, other.Index, length, comparison);
            return (compareResult != 0 ? compareResult : this.Length.CompareTo(other.Length));
        }

        public bool StartsWith(StringSlice prefix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (this.Length < prefix.Length) { return false; }
            return String.Compare(this.Value, this.Index, prefix.Value, prefix.Index, prefix.Length, comparison) == 0;
        }

        public bool StartsWith(char c)
        {
            if (this.Length == 0) { return false; }
            return (this[0] == c);
        }

        public void AppendTo(StringBuilder builder)
        {
            if (Length > 0)
            {
                builder.Append(Value, Index, Length);
            }
        }

        public override string ToString()
        {
            return Value?.Substring(Index, Length) ?? "";
        }
    }
}

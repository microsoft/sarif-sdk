// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>A part which should be used in generating a type's ToString overload.</summary>
    internal struct ToStringEntry : IEquatable<ToStringEntry>
    {
        /// <summary>The text which should be used when generating the overload. Note that in the case of
        /// delimited collections this will be the delimiter to use.</summary> 
        public readonly string Text;
        /// <summary>The variable to reference when generating the overload.</summary> 
        public readonly DataModelMember Variable;

        /// <summary>Initializes a new instance of the <see cref="ToStringEntry"/> struct.</summary>
        /// <param name="variable">The variable to reference when generating the overload.</param>
        public ToStringEntry(DataModelMember variable)
            : this(null, variable)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ToStringEntry"/> struct.</summary>
        /// <param name="text">The text which should be used when generating the overload. Note that in
        /// the case of delimited collections this will be the delimiter to use.</param>
        /// <param name="variable">The variable to reference when generating the overload.</param>
        public ToStringEntry(string text, DataModelMember variable)
        {
            Debug.Assert(text != null || variable != null, "Bad ToStringEntry with no data");
            this.Text = text;
            this.Variable = variable;
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ToStringEntry)
            {
                return this.Equals((ToStringEntry)obj);
            }

            return false;
        }

        /// <summary>Tests if this ToStringEntry is considered equal to another.</summary>
        /// <param name="other">to string entry to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(ToStringEntry other)
        {
            return Object.Equals(this.Text, other.Text)
                && Object.Equals(this.Variable, other.Variable);
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.Text);
            hash.Add(this.Variable);
            return hash.GetHashCode();
        }

        /// <summary>Convert this instance into a string representation.</summary>
        /// <returns>A string that represents this instance.</returns>
        public override string ToString()
        {
            return String.Concat(
                "Text: ", this.Text, Environment.NewLine,
                "Var : [", this.Variable, "]"
                );
        }

        /// <summary>
        /// Coalesces the given <see cref="ToStringEntry"/> instances to remove redundant objects.
        /// </summary>
        /// <param name="sourceEntries">Source entries to coalesce.</param>
        /// <returns>
        /// An ImmutableArray&lt;ToStringEntry&gt; with adjacent strings and similar smashed together.
        /// </returns>
        public static ImmutableArray<ToStringEntry> Coalesce(IEnumerable<ToStringEntry> sourceEntries)
        {
            ImmutableArray<ToStringEntry>.Builder builder = ImmutableArray.CreateBuilder<ToStringEntry>();
            var strings = new List<string>();

            foreach (ToStringEntry entry in sourceEntries)
            {
                if (entry.Variable == null)
                {
                    strings.Add(entry.Text);
                    continue;
                }

                if (strings.Count != 0)
                {
                    builder.Add(new ToStringEntry(String.Join(" ", strings), null));
                    strings.Clear();
                }

                builder.Add(entry);
            }

            if (strings.Count != 0)
            {
                builder.Add(new ToStringEntry(String.Join(" ", strings), null));
            }

            return builder.ToImmutable();
        }
    }
}

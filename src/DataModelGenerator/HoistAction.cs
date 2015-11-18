// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Microsoft.CodeAnalysis.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>
    /// A record indicating what the replacement type and rank should be for a given type.
    /// </summary>
    /// <seealso cref="T:IEquatable{HoistAction}"/>
    internal struct HoistAction : IEquatable<HoistAction>
    {
        /// <summary>The declared name of the new type.</summary>
        public readonly string Becomes;
        /// <summary>The number of ranks to add to all members of the old type.</summary>
        public readonly int AddedRanks;

        /// <summary>Initializes a new instance of the <see cref="HoistAction"/> struct.</summary>
        /// <param name="becomes">The declared name of the new type.</param>
        /// <param name="addedRank">The number of ranks to add to all members of the old type.</param>
        public HoistAction(string becomes, int addedRank)
        {
            this.Becomes = becomes;
            this.AddedRanks = addedRank;
        }

        /// <summary>Convert this instance into a string representation.</summary>
        /// <returns>A string that represents this instance.</returns>
        public override string ToString()
        {
            return "becomes '" + this.Becomes + "' (" + this.AddedRanks.ToString(CultureInfo.InvariantCulture) + " ranks)";
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.Becomes);
            hash.Add(this.AddedRanks);
            return hash.GetHashCode();
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public override bool Equals(object obj)
        {
            return obj is HoistAction && this.Equals((HoistAction)obj);
        }

        /// <summary>Tests if this HoistAction is considered equal to another.</summary>
        /// <param name="other">The hoist action to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(HoistAction other)
        {
            return this.Becomes == other.Becomes
                && this.AddedRanks == other.AddedRanks;
        }
    }
}

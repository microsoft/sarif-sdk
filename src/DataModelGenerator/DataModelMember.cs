// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>A member of a leaf type in a data model.</summary>
    /// <seealso cref="T:System.IEquatable{Microsoft.CodeAnalysis.DataModelGenerator.DataModelMember}"/>
    internal class DataModelMember : IEquatable<DataModelMember>
    {
        /// <summary>The name declaring this member in the source G4 file.</summary>
        public readonly string DeclaredName;
        /// <summary>The C# property name of this member.</summary>
        public readonly string CSharpName;
        /// <summary>The name serialized into JSON for this property.</summary>
        public readonly string SerializedName;
        /// <summary>The summary text emitted on this member property.</summary>
        public readonly string SummaryText;
        /// <summary>The argument name used when passing this member across function call boundaries.</summary>
        public readonly string ArgumentName;
        /// <summary>The rank of the property; e.g. 0 = int, 1 = List{int}, 2 = List{List{int}}.</summary>
        public readonly int Rank;
        /// <summary>true if required in JSON.</summary>
        public readonly bool Required;
        /// <summary>Regex to apply for member value validation.</summary>
        public readonly string Pattern;
        /// <summary>Minimum value for member. The value should be parseable as an integer or number.</summary>
        public readonly string Minimum;
        /// <summary>Minimum count of elements for array members.</summary>
        public readonly string MinItems;
        /// <summary>If present, true or false value that indicates whether all array items should be unique.</summary>
        public readonly string UniqueItems;
        /// <summary>A default value associated with the member. String defaults must be encapsulated by quotes in the G4</summary>
        public readonly string Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataModelMember"/> class.
        /// </summary>
        /// <param name="declaredName">The name declaring this member in the source G4 file.</param>
        /// <param name="rank">
        /// The rank of the property; e.g. 0 = int, 1 = List{int}, 2 = List{List{int}}.
        /// </param>
        /// <param name="required">true if required in JSON.</param>
        public DataModelMember(string declaredName, int rank, bool required)
            : this(
                declaredName,
                LinguisticTransformer.ToCSharpName(declaredName),
                LinguisticTransformer.ToJsonName(declaredName),
                String.Empty,
                LinguisticTransformer.ToArgumentName(declaredName),
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                rank,
                required
                )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataModelMember"/> class.
        /// </summary>
        /// <param name="declaredName">The name declaring this member in the source G4 file.</param>
        /// <param name="cSharpName">The C# property name of this member.</param>
        /// <param name="serializedName">The name serialized into JSON for this property.</param>
        /// <param name="summaryText">The summary text emitted on this member property.</param>
        /// <param name="argumentName">
        /// The argument name used when passing this member across function call boundaries.
        /// </param>
        /// <param name="rank">
        /// The rank of the property; e.g. 0 = int, 1 = List{int}, 2 = List{List{int}}.
        /// </param>
        /// <param name="required">true if required in JSON.</param>
        public DataModelMember(string declaredName, string cSharpName, string serializedName, string summaryText, string argumentName, string pattern, string minimum, string minItems, string uniqueItems, string defaultValue, int rank, bool required)
        {
            this.DeclaredName = declaredName;
            this.CSharpName = cSharpName;
            this.SerializedName = serializedName;
            this.SummaryText = summaryText;
            this.ArgumentName = argumentName;
            this.Pattern = pattern;
            this.Minimum = minimum;
            this.MinItems = minItems;
            this.UniqueItems = uniqueItems;
            this.Default = defaultValue;
            this.Rank = rank;
            this.Required = required;
        }

        /// <summary>Convert this instance into a string representation.</summary>
        /// <returns>A string that represents this instance.</returns>
        public override string ToString()
        {
            return String.Join(Environment.NewLine, new[] {
                "G4: " + this.DeclaredName,
                "C#: " + this.CSharpName + "(" + this.ArgumentName + ")",
                "JS: " + this.SerializedName,
                "S : " + this.SummaryText,
                "R : " + this.Rank.ToString(CultureInfo.InvariantCulture),
                "Rq: " + this.Required.ToString(CultureInfo.InvariantCulture)
                });
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.DeclaredName);
            hash.Add(this.CSharpName);
            hash.Add(this.SerializedName);
            hash.Add(this.SummaryText);
            hash.Add(this.ArgumentName);
            hash.Add(this.Rank);
            hash.Add(this.Required ? 1 : 0);
            return hash.GetHashCode();
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as DataModelMember);
        }

        /// <summary>Tests if this DataModelMember is considered equal to another.</summary>
        /// <param name="other">The data model member to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(DataModelMember other)
        {
            return other != null
                && this.DeclaredName == other.DeclaredName
                && this.CSharpName == other.CSharpName
                && this.SerializedName == other.SerializedName
                && this.SummaryText == other.SummaryText
                && this.ArgumentName == other.ArgumentName
                && this.Rank == other.Rank
                && this.Required == other.Required;
        }
    }
}

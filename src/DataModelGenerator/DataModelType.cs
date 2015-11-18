// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>A generated type in a data model.</summary>
    /// <seealso cref="T:System.IEquatable{Microsoft.CodeAnalysis.DataModelGenerator.DataModelType}"/>
    internal class DataModelType : IEquatable<DataModelType>
    {
        /// <summary>The summary text to emit for this type declaration.</summary>
        public readonly string SummaryText;
        /// <summary>The remarks text to emit for this type declaration.</summary>
        public readonly string RemarksText;
        /// <summary>The name used in the G4 file to declare this type.</summary>
        public readonly string G4DeclaredName;
        /// <summary>Name of the type in C#.</summary>
        public readonly string CSharpName;
        /// <summary>The members of this type, if any.</summary>
        public readonly ImmutableArray<DataModelMember> Members;
        /// <summary>Records describing the "ToString" override to generate for this type.</summary>
        public readonly ImmutableArray<ToStringEntry> ToStringEntries;
        /// <summary>The C# name of the base class of this type.</summary>
        public readonly string Base;
        /// <summary>The kind of type this is.</summary>
        public readonly DataModelTypeKind Kind;

        /// <summary>Initializes a new instance of the <see cref="DataModelType"/> class.</summary>
        /// <param name="g4Name">Declaring name in the G4 file.</param>
        /// <param name="members">The members of this type, if any.</param>
        /// <param name="toStringEntries">
        /// Records describing the "ToString" override to generate for this type.
        /// </param>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="kind">The kind of type this is.</param>
        public DataModelType(
            string g4Name,
            ImmutableArray<DataModelMember> members,
            ImmutableArray<ToStringEntry> toStringEntries,
            string baseType,
            DataModelTypeKind kind
            ) : this(String.Empty, String.Empty, g4Name, LinguisticTransformer.ToCSharpName(g4Name), members, toStringEntries, baseType, kind)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DataModelType"/> class.</summary>
        /// <param name="summaryText">The summary text to emit for this type declaration.</param>
        /// <param name="remarksText">The remarks text to emit for this type declaration.</param>
        /// <param name="g4Name">Declaring name in the G4 file.</param>
        /// <param name="cSharpName">Name of the type in C#.</param>
        /// <param name="members">The members of this type, if any.</param>
        /// <param name="toStringEntries">
        /// Records describing the "ToString" override to generate for this type.
        /// </param>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="kind">The kind of type this is.</param>
        public DataModelType(
            string summaryText,
            string remarksText,
            string g4Name,
            string cSharpName,
            ImmutableArray<DataModelMember> members,
            ImmutableArray<ToStringEntry> toStringEntries,
            string baseType,
            DataModelTypeKind kind
            )
        {
            this.SummaryText = summaryText;
            this.RemarksText = remarksText;
            this.G4DeclaredName = g4Name;
            this.CSharpName = cSharpName;
            this.Members = members;
            this.ToStringEntries = toStringEntries;
            this.Base = baseType;
            this.Kind = kind;
        }

        /// <summary>Gets a value indicating whether this instance has a base.</summary>
        /// <value>true if this instance has a base, false if not.</value>
        public bool HasBase
        {
            get
            {
                return !String.IsNullOrEmpty(this.Base);
            }
        }

        /// <summary>Gets a value indicating whether this instance is nullable.</summary>
        /// <value>true if this instance is nullable, false if not.</value>
        public bool IsNullable
        {
            get
            {
                switch (this.Kind)
                {
                    case DataModelTypeKind.BuiltInBoolean:
                    case DataModelTypeKind.BuiltInNumber:
                        return false;
                    default:
                        return true;
                }
            }
        }

        /// <summary>Convert this instance into a string representation.</summary>
        /// <returns>A string that represents this instance.</returns>
        public override string ToString()
        {
            return "G4: " + this.G4DeclaredName + " C#: " + this.CSharpName +
                "M :" + Environment.NewLine + String.Join(Environment.NewLine, this.Members)
                    .Replace(Environment.NewLine, Environment.NewLine + "    ") +
                "TS:" + Environment.NewLine + String.Join(Environment.NewLine, this.ToStringEntries)
                    .Replace(Environment.NewLine, Environment.NewLine + "    ");
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DataModelType);
        }

        /// <summary>Tests if this DataModelType is considered equal to another.</summary>
        /// <param name="other">The data model type to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(DataModelType other)
        {
            return other != null
                && this.SummaryText == other.SummaryText
                && this.RemarksText == other.RemarksText
                && this.G4DeclaredName == other.G4DeclaredName
                && this.CSharpName == other.CSharpName
                && this.Base == other.Base
                && this.Kind == other.Kind
                && this.Members.SequenceEqual(other.Members)
                && this.ToStringEntries.SequenceEqual(other.ToStringEntries);
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.SummaryText);
            hash.Add(this.RemarksText);
            hash.Add(this.G4DeclaredName);
            hash.Add(this.CSharpName);
            hash.Add(this.Base);
            hash.Add((int)this.Kind);
            hash.AddRange(this.Members);
            hash.AddRange(this.ToStringEntries);
            return hash.GetHashCode();
        }
    }
}

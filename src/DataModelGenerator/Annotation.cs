// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>An annotation in a .g4 file; e.g. /** @name {value} */.</summary>
    /// <seealso cref="T:System.IEquatable{Microsoft.CodeAnalysis.DataModelGenerator.Annotation}"/>
    internal class Annotation : IEquatable<Annotation>
    {
        /// <summary>The name of the annotation. (The part after the @)</summary>
        public readonly string Name;
        /// <summary>The value of the annotation. (The part in {}s).</summary>
        public readonly string Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Annotation"/> class.
        /// </summary>
        /// <param name="name">The name of the annotation. (The part after the @)</param>
        /// <param name="value">The value of the annotation, if any. (The part in {}s).</param>
        public Annotation(string name, string value)
        {
            Debug.Assert(name != null);

            this.Name = name;
            this.Value = value ?? "";
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance.</returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.Name);
            hash.Add(this.Value);
            return hash.GetHashCode();
        }

        /// <summary>Tests if this object is considered equal to another.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        /// <seealso cref="M:System.Object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Annotation);
        }

        /// <summary>Convert this instance into a string representation.</summary>
        /// <returns>A string that represents this instance.</returns>
        /// <seealso cref="M:System.Object.ToString()"/>
        public override string ToString()
        {
            if (this.Value == null)
            {
                return String.Concat("@", this.Name);
            }
            else
            {
                return String.Concat("@", this.Name, " {", this.Value, "}");
            }
        }

        /// <summary>Tests if this Annotation is considered equal to another.</summary>
        /// <param name="other">The annotation to compare to this instance.</param>
        /// <returns>true if the objects are considered equal, false if they are not.</returns>
        public bool Equals(Annotation other)
        {
            return other != null
                && this.Name == other.Name
                && this.Value == other.Value;
        }
    }

    internal static class AnnotationExtensions
    {
        /// <summary>
        /// An ImmutableArray&lt;Annotation&gt; extension method that gets annotation value for a given key.
        /// </summary>
        /// <param name="annotations">The annotations to act on.</param>
        /// <param name="name">The annotation name to look for.</param>
        /// <returns>The annotation value if the annotation is present; otherwise, null.</returns>
        public static string GetAnnotationValue(this ImmutableArray<Annotation> annotations, string name)
        {
            foreach (Annotation a in annotations)
            {
                if (a.Name == name)
                {
                    return a.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// An ImmutableArray&lt;Annotation&gt; extension method that query if
        /// <paramref name="annotations"/> has an annotation of the supplied name.
        /// </summary>
        /// <param name="annotations">The annotations to act on.</param>
        /// <param name="name">The annotation name to look for.</param>
        /// <returns>true if an annotation with the supplied name is present; otherwise, false.</returns>
        public static bool HasAnnotation(this ImmutableArray<Annotation> annotations, string name)
        {
            foreach (Annotation a in annotations)
            {
                if (a.Name == name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

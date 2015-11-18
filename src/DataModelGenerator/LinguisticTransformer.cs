// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>Linguistic transformations such as case transformations and similar for G4 files.</summary>
    internal static class LinguisticTransformer
    {
        /// <summary>Converts a G4 identifier name to a C# name.</summary>
        /// <param name="g4Name">The G4 identifier name.</param>
        /// <returns><paramref name="g4Name"/> converted to an ideomatic C# name, that is, with the first
        /// character in uppercase.</returns>
        public static string ToCSharpName(string g4Name)
        {
            string firstTextElement = StringInfo.GetNextTextElement(g4Name);
            string toUpper = firstTextElement.ToUpper(CultureInfo.InvariantCulture);
            if (toUpper == firstTextElement)
            {
                // Avoid allocating...
                return g4Name;
            }
            else
            {
                return String.Concat(toUpper, g4Name.Remove(0, firstTextElement.Length));
            }
        }

        /// <summary>Converts a G4 identifier name to a C# argument name.</summary>
        /// <param name="g4Name">The G4 identifier name.</param>
        /// <returns><paramref name="g4Name"/> converted to an ideomatic C# argument name, that is, with the first
        /// character in lowercase suffixed by "Arg".</returns>
        public static string ToArgumentName(string g4Name)
        {
            return ToJsonName(g4Name) + "Arg";
        }

        /// <summary>Converts a G4 identifier name to a JSON name.</summary>
        /// <param name="g4Name">The G4 identifier name.</param>
        /// <returns><paramref name="g4Name"/> converted to an ideomatic JSON name, that is, with the first
        /// character in lowercase.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public static string ToJsonName(string g4Name)
        {
            if (IsUpper(g4Name))
            {
                return g4Name.ToLower(CultureInfo.InvariantCulture);
            }

            string firstTextElement = StringInfo.GetNextTextElement(g4Name);
            string toLower = firstTextElement.ToLower(CultureInfo.InvariantCulture);
            if (toLower == firstTextElement)
            {
                // Avoid allocating...
                return g4Name;
            }
            else
            {
                return String.Concat(toLower, g4Name.Remove(0, firstTextElement.Length));
            }
        }

        private static bool IsUpper(string s)
        {
            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(s);
            while (enumerator.MoveNext())
            {
                if (Char.IsLower(s, enumerator.ElementIndex))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Extensions
    {
        /// <summary>Retrieves a property value if it exists, or null.</summary>
        /// <param name="properties">A properties object from which the property shall be
        /// retrieved, or null.</param>
        /// <param name="key">The property name / key.</param>
        /// <returns>
        /// If <paramref name="properties"/> is not null and an entry for the supplied key exists, the
        /// value associated with that key; otherwise, null.
        /// </returns>
        internal static string PropertyValue(this Dictionary<string, string> properties, string key)
        {
            string propValue;

            if (properties != null &&
                properties.TryGetValue(key, out propValue))
            {
                return propValue;
            }

            return null;
        }

        /// <summary>Checks if a character is a newline.</summary>
        /// <param name="testedCharacter">The character to check.</param>
        /// <returns>true if newline, false if not.</returns>
        internal static bool IsNewline(char testedCharacter)
        {
            return testedCharacter == '\r'
                || testedCharacter == '\n'
                || testedCharacter == '\u2028'  // Unicode line separator
                || testedCharacter == '\u2029'; // Unicode paragraph separator
        }

        /// <summary>
        /// Returns whether or not the range [<paramref name="startIndex"/>,
        /// <paramref name="startIndex"/> + <paramref name="target"/><c>.Length</c>) is equal to the
        /// supplied string.
        /// </summary>
        /// <param name="array">The array to check.</param>
        /// <param name="startIndex">The start index in the array to check.</param>
        /// <param name="target">Target string to look for in the array.</param>
        /// <returns>
        /// true if the range [<paramref name="startIndex"/>, <paramref name="startIndex"/> +
        /// <paramref name="target"/><c>.Length</c>) is equal to
        /// <paramref name="target"/>. If the range is undefined in the bounds of the array, false.
        /// </returns>
        internal static bool ArrayMatches(char[] array, int startIndex, string target)
        {
            if (startIndex < 0)
            {
                return false;
            }

            int targetLength = target.Length;
            if (targetLength + startIndex >= array.Length)
            {
                return false;
            }

            for (int idx = 0; idx < targetLength; ++idx)
            {
                if (array[idx + startIndex] != target[idx])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Consumes content from an XML reader until the end element of the element at endElementDepth
        /// <paramref name="endElementDepth"/>, including the end element.
        /// </summary>
        /// <param name="xmlReader">The <see cref="XmlReader"/> whose contents shall be consumed.</param>
        /// <param name="endElementDepth">The endElementDepth of node to consume.</param>
        internal static void ConsumeElementOfDepth(this XmlReader xmlReader, int endElementDepth)
        {
            int enteringReaderDepth = xmlReader.Depth;

            if (enteringReaderDepth < endElementDepth)
            {
                return;
            }

            if (enteringReaderDepth == endElementDepth)
            {
                // Move to the following element
                xmlReader.Read();
            }

            while (xmlReader.Depth > endElementDepth && xmlReader.Read()) { }

            if (xmlReader.NodeType == XmlNodeType.EndElement)
            {
                // Consume the end element
                xmlReader.Read();
            }
        }
    }
}
